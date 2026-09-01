using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Ionic.IO;

public static class JunctionPoint
{
    private const int ERROR_NOT_A_REPARSE_POINT = 4390;
    private const int ERROR_REPARSE_ATTRIBUTE_CONFLICT = 4391;
    private const int ERROR_INVALID_REPARSE_DATA = 4392;
    private const int ERROR_REPARSE_TAG_INVALID = 4393;
    private const int ERROR_REPARSE_TAG_MISMATCH = 4394;
    private const int FSCTL_SET_REPARSE_POINT = 589988;
    private const int FSCTL_GET_REPARSE_POINT = 589992;
    private const int FSCTL_DELETE_REPARSE_POINT = 589996;
    private const uint IO_REPARSE_TAG_MOUNT_POINT = 2684354563 /*0xA0000003*/;
    private const uint IO_REPARSE_TAG_SYMLINK = 2684354572 /*0xA000000C*/;
    private const string NonInterpretedPathPrefix = "\\??\\";

    public static ReparsePointInfo Create(string path, string targetDir)
    {
        return Create(path, targetDir, false);
    }

    public static ReparsePointInfo Create(string path, string targetDir, bool overwrite)
    {
        targetDir = Path.GetFullPath(targetDir);
        if (!Directory.Exists(targetDir))
        {
            throw new IOException("Target path does not exist or is not a directory.");
        }

        if (Directory.Exists(path))
        {
            if (!overwrite)
            {
                throw new IOException("Directory already exists and overwrite parameter is false.");
            }
        }
        else
        {
            Directory.CreateDirectory(path);
        }

        using (var safeFileHandle = OpenReparsePoint(path, EFileAccess.GenericWrite))
        {
            var sourceArray = targetDir.StartsWith("\\??\\")
                ? Encoding.Unicode.GetBytes(Path.GetFullPath(targetDir))
                : Encoding.Unicode.GetBytes("\\??\\" + Path.GetFullPath(targetDir));
            var structure = new REPARSE_SET_BUFFER
            {
                ReparseTag = 2684354563 /*0xA0000003*/,
                ReparseDataLength = (ushort)(sourceArray.Length + 12),
                TargetLength = (ushort)sourceArray.Length,
                TargetMaxLength = (ushort)(sourceArray.Length + 2),
                PathBuffer = new byte[16368]
            };
            Array.Copy(sourceArray, structure.PathBuffer, sourceArray.Length);
            var num = Marshal.AllocHGlobal(Marshal.SizeOf((object)structure));
            try
            {
                Marshal.StructureToPtr((object)structure, num, false);
                if (!DeviceIoControl(safeFileHandle.DangerousGetHandle(), 589988U, num, sourceArray.Length + 20,
                        IntPtr.Zero, 0, out var _, IntPtr.Zero))
                {
                    ThrowLastWin32Error("Unable to create junction point.");
                }

                return GetInfo(path);
            }
            finally
            {
                Marshal.FreeHGlobal(num);
            }
        }
    }

    public static void Delete(string junctionPoint)
    {
        if (!Directory.Exists(junctionPoint))
        {
            if (File.Exists(junctionPoint))
            {
                throw new IOException("Path is not a junction point.");
            }
        }
        else
        {
            using (var safeFileHandle = OpenReparsePoint(junctionPoint, EFileAccess.GenericWrite))
            {
                var structure = new REPARSE_GET_BUFFER
                {
                    ReparseTag = 2684354563 /*0xA0000003*/,
                    ReparseDataLength = 0,
                    PathBuffer = new byte[16368]
                };
                var num = Marshal.AllocHGlobal(Marshal.SizeOf((object)structure));
                try
                {
                    Marshal.StructureToPtr((object)structure, num, false);
                    if (!DeviceIoControl(safeFileHandle.DangerousGetHandle(), 589996U, num, 8, IntPtr.Zero, 0,
                            out var _, IntPtr.Zero))
                    {
                        ThrowLastWin32Error("Unable to delete junction point.");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(num);
                }

                try
                {
                    Directory.Delete(junctionPoint);
                }
                catch (IOException ex)
                {
                    throw new IOException("Unable to delete junction point.", ex);
                }
            }
        }
    }

    public static bool Exists(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        using (var handle = OpenReparsePoint(path, EFileAccess.GenericRead))
        {
            return InternalGetTarget(handle, path).Flavor != ReparsePointFlavor.NotaReparsePoint;
        }
    }

    public static ReparsePointInfo GetInfo(string path)
    {
        using (var handle = OpenReparsePoint(path, EFileAccess.GenericRead))
        {
            return InternalGetTarget(handle, path);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        EFileAccess dwDesiredAccess,
        EFileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        ECreationDisposition dwCreationDisposition,
        EFileAttributes dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        IntPtr InBuffer,
        int nInBufferSize,
        IntPtr OutBuffer,
        int nOutBufferSize,
        out int pBytesReturned,
        IntPtr lpOverlapped);

    private static ReparsePointInfo InternalGetTarget(SafeFileHandle handle, string path)
    {
        var num1 = Marshal.SizeOf(typeof(REPARSE_GET_BUFFER));
        var num2 = Marshal.AllocHGlobal(num1);
        var target = new ReparsePointInfo(path);
        try
        {
            if (!DeviceIoControl(handle.DangerousGetHandle(), 589992U, IntPtr.Zero, 0, num2, num1, out var _,
                    IntPtr.Zero))
            {
                if (Marshal.GetLastWin32Error() == 4390)
                {
                    return null;
                }

                ThrowLastWin32Error("Unable to get information about junction point.");
            }

            var structure = (REPARSE_GET_BUFFER)Marshal.PtrToStructure(num2, typeof(REPARSE_GET_BUFFER));
            target.Target = Encoding.Unicode.GetString(structure.PathBuffer, structure.SubstituteNameOffset,
                structure.SubstituteNameLength);
            target.Flavor = structure.ReparseTag != 2684354572U /*0xA000000C*/
                ? structure.ReparseTag != 2684354563U /*0xA0000003*/ ? ReparsePointFlavor.NotaReparsePoint :
                target.Target.StartsWith("\\??\\Volume") ? ReparsePointFlavor.MountPoint :
                ReparsePointFlavor.JunctionPoint
                : ReparsePointFlavor.SymbolicLink;
            if (target.Flavor != ReparsePointFlavor.NotaReparsePoint)
            {
                if (target.Target.StartsWith("\\??\\"))
                {
                    target.Target = target.Target.Substring("\\??\\".Length);
                }

                target.PrintName = Encoding.Unicode.GetString(structure.PathBuffer, structure.PrintNameOffset,
                    structure.PrintNameLength);
            }

            return target;
        }
        finally
        {
            Marshal.FreeHGlobal(num2);
        }
    }

    private static SafeFileHandle OpenReparsePoint(
        string reparsePoint,
        EFileAccess accessMode)
    {
        var file = CreateFile(reparsePoint, accessMode, EFileShare.Read | EFileShare.Write | EFileShare.Delete,
            IntPtr.Zero, ECreationDisposition.OpenExisting,
            EFileAttributes.BackupSemantics | EFileAttributes.OpenReparsePoint, IntPtr.Zero);
        if (Marshal.GetLastWin32Error() != 0)
        {
            ThrowLastWin32Error("Unable to open reparse point.");
        }

        var safeFileHandle = new SafeFileHandle(file, true);
        if (Marshal.GetLastWin32Error() != 0)
        {
            ThrowLastWin32Error("Unable to open reparse point.");
        }

        return safeFileHandle;
    }

    private static void ThrowLastWin32Error(string message)
    {
        throw new IOException(message, Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
    }

    [Flags]
    private enum EFileAccess : uint
    {
        GenericRead = 2147483648, // 0x80000000
        GenericWrite = 1073741824, // 0x40000000
        GenericExecute = 536870912, // 0x20000000
        GenericAll = 268435456 // 0x10000000
    }

    [Flags]
    private enum EFileShare : uint
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4
    }

    private enum ECreationDisposition : uint
    {
        New = 1,
        CreateAlways = 2,
        OpenExisting = 3,
        OpenAlways = 4,
        TruncateExisting = 5
    }

    [Flags]
    private enum EFileAttributes : uint
    {
        Readonly = 1,
        Hidden = 2,
        System = 4,
        Directory = 16, // 0x00000010
        Archive = 32, // 0x00000020
        Device = 64, // 0x00000040
        Normal = 128, // 0x00000080
        Temporary = 256, // 0x00000100
        SparseFile = 512, // 0x00000200
        ReparsePoint = 1024, // 0x00000400
        Compressed = 2048, // 0x00000800
        Offline = 4096, // 0x00001000
        NotContentIndexed = 8192, // 0x00002000
        Encrypted = 16384, // 0x00004000
        Write_Through = 2147483648, // 0x80000000
        Overlapped = 1073741824, // 0x40000000
        NoBuffering = 536870912, // 0x20000000
        RandomAccess = 268435456, // 0x10000000
        SequentialScan = 134217728, // 0x08000000
        DeleteOnClose = 67108864, // 0x04000000
        BackupSemantics = 33554432, // 0x02000000
        PosixSemantics = 16777216, // 0x01000000
        OpenReparsePoint = 2097152, // 0x00200000
        OpenNoRecall = 1048576, // 0x00100000
        FirstPipeInstance = 524288 // 0x00080000
    }

    private struct REPARSE_GET_BUFFER
    {
        public uint ReparseTag;
        public ushort ReparseDataLength;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public ushort Reserved;
        public ushort SubstituteNameOffset;
        public ushort SubstituteNameLength;
        public ushort PrintNameOffset;
        public ushort PrintNameLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16368)]
        public byte[] PathBuffer;
    }

    private struct REPARSE_SET_BUFFER
    {
        public uint ReparseTag;
        public ushort ReparseDataLength;
        public ushort Reserved1;
        public ushort Reserved2;
        public ushort TargetLength;
        public ushort TargetMaxLength;
        public ushort Reserved3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16368)]
        public byte[] PathBuffer;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }
}