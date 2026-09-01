using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Ionic.CopyData;

public class CopyDataChannel : IDisposable
{
    private const int WM_COPYDATA = 74;
    private static readonly TimeSpan threshold = new(0, 0, 0, 0, 85);
    private bool disposed;
    private DateTime lastSend;
    private readonly NativeWindow owner = null;
    private bool recreateChannel;

    internal CopyDataChannel(NativeWindow owner, string channelName)
    {
        this.owner = owner;
        this.ChannelName = channelName;
        addChannel();
        lastSend = DateTime.FromFileTimeUtc(0L);
    }

    public string ChannelName { get; private set; } = "";

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (ChannelName.Length > 0)
        {
            removeChannel();
        }

        ChannelName = "";
        disposed = true;
        GC.SuppressFinalize(this);
    }

    public override bool Equals(object obj)
    {
        return obj != null && GetType() == obj.GetType() && Equals((CopyDataChannel)obj);
    }

    public bool Equals(CopyDataChannel cdc)
    {
        return cdc != null && owner.Handle == cdc.owner.Handle && ChannelName.Equals(cdc.ChannelName);
    }

    public override int GetHashCode()
    {
        return (int)((uint)(int)owner.Handle ^ ChannelName.GetHashCode());
    }

    public void OnHandleChange()
    {
        removeChannel();
        recreateChannel = true;
    }

    public int Send(object obj)
    {
        var num1 = 0;
        if (disposed)
        {
            throw new InvalidOperationException("Object has been disposed");
        }

        if (recreateChannel)
        {
            addChannel();
        }

        var graph = new CopyDataObjectData(obj, ChannelName);
        var binaryFormatter = new BinaryFormatter();
        var serializationStream = new MemoryStream();
        binaryFormatter.Serialize(serializationStream, graph);
        serializationStream.Flush();
        DateTime utcNow;
        for (utcNow = DateTime.UtcNow; utcNow - lastSend < threshold; utcNow = DateTime.UtcNow)
        {
            Thread.Sleep(15);
        }

        lastSend = utcNow;
        var length = (int)serializationStream.Length;
        if (length > 0)
        {
            var numArray = new byte[length];
            serializationStream.Seek(0L, SeekOrigin.Begin);
            serializationStream.Read(numArray, 0, length);
            var num2 = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(numArray, 0, num2, length);
            var enumWindows = new EnumWindows();
            enumWindows.GetWindows();
            foreach (var enumWindowsItem in enumWindows.Items)
            {
                if (!enumWindowsItem.Handle.Equals(owner.Handle) && GetProp(enumWindowsItem.Handle, ChannelName) != 0)
                {
                    var t = new COPYDATASTRUCT
                    {
                        cbData = length,
                        dwData = IntPtr.Zero,
                        lpData = num2
                    };
                    SendMessage(enumWindowsItem.Handle, 74, (int)owner.Handle, ref t);
                    num1 += Marshal.GetLastWin32Error() == 0 ? 1 : 0;
                }
            }

            Marshal.FreeCoTaskMem(num2);
        }

        serializationStream.Close();
        return num1;
    }

    private void addChannel()
    {
        SetProp(owner.Handle, ChannelName, (int)owner.Handle);
    }

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int GetProp(IntPtr hwnd, string lpString);

    private void removeChannel()
    {
        RemoveProp(owner.Handle, ChannelName);
    }

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int RemoveProp(IntPtr hwnd, string lpString);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int SendMessage(
        IntPtr hwnd,
        int wMsg,
        int wParam,
        ref COPYDATASTRUCT lParam);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int SetProp(IntPtr hwnd, string lpString, int hData);

    ~CopyDataChannel()
    {
        Dispose();
    }

    private struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
    }
}