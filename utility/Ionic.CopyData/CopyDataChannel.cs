using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

#nullable disable
namespace Ionic.CopyData;

public class CopyDataChannel : IDisposable
{
    private const int WM_COPYDATA = 74;
    private string channelName = "";
    private bool disposed = false;
    private NativeWindow owner = (NativeWindow)null;
    private bool recreateChannel = false;
    private DateTime lastSend;
    private static TimeSpan threshold = new TimeSpan(0, 0, 0, 0, 85);

    internal CopyDataChannel(NativeWindow owner, string channelName)
    {
        this.owner = owner;
        this.channelName = channelName;
        this.addChannel();
        this.lastSend = DateTime.FromFileTimeUtc(0L);
    }

    private void addChannel()
    {
        CopyDataChannel.SetProp(this.owner.Handle, this.channelName, (int)this.owner.Handle);
    }

    public void Dispose()
    {
        if (this.disposed)
            return;
        if (this.channelName.Length > 0)
            this.removeChannel();
        this.channelName = "";
        this.disposed = true;
        GC.SuppressFinalize((object)this);
    }

    ~CopyDataChannel() => this.Dispose();

    public override bool Equals(object obj)
    {
        return obj != null && this.GetType() == obj.GetType() && this.Equals((CopyDataChannel)obj);
    }

    public bool Equals(CopyDataChannel cdc)
    {
        return cdc != null && this.owner.Handle == cdc.owner.Handle && this.channelName.Equals(cdc.channelName);
    }

    public override int GetHashCode()
    {
        return (int)((long)(uint)(int)this.owner.Handle ^ (long)this.channelName.GetHashCode());
    }

    public void OnHandleChange()
    {
        this.removeChannel();
        this.recreateChannel = true;
    }

    private void removeChannel() => CopyDataChannel.RemoveProp(this.owner.Handle, this.channelName);

    public int Send(object obj)
    {
        int num1 = 0;
        if (this.disposed)
            throw new InvalidOperationException("Object has been disposed");
        if (this.recreateChannel)
            this.addChannel();
        CopyDataObjectData graph = new CopyDataObjectData(obj, this.channelName);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream serializationStream = new MemoryStream();
        binaryFormatter.Serialize((Stream)serializationStream, (object)graph);
        serializationStream.Flush();
        DateTime utcNow;
        for (utcNow = DateTime.UtcNow; utcNow - this.lastSend < CopyDataChannel.threshold; utcNow = DateTime.UtcNow)
            Thread.Sleep(15);
        this.lastSend = utcNow;
        int length = (int)serializationStream.Length;
        if (length > 0)
        {
            byte[] numArray = new byte[length];
            serializationStream.Seek(0L, SeekOrigin.Begin);
            serializationStream.Read(numArray, 0, length);
            IntPtr num2 = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(numArray, 0, num2, length);
            EnumWindows enumWindows = new EnumWindows();
            enumWindows.GetWindows();
            foreach (EnumWindowsItem enumWindowsItem in enumWindows.Items)
            {
                if (!enumWindowsItem.Handle.Equals((object)this.owner.Handle) && CopyDataChannel.GetProp(enumWindowsItem.Handle, this.channelName) != 0)
                {
                    var t = new CopyDataChannel.COPYDATASTRUCT()
                    {
                        cbData = length,
                        dwData = IntPtr.Zero,
                        lpData = num2
                    };
                    CopyDataChannel.SendMessage(enumWindowsItem.Handle, 74, (int)this.owner.Handle, ref t);
                    num1 += Marshal.GetLastWin32Error() == 0 ? 1 : 0;
                }
            }
            Marshal.FreeCoTaskMem(num2);
        }
        serializationStream.Close();
        return num1;
    }

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int SendMessage(
      IntPtr hwnd,
      int wMsg,
      int wParam,
      ref CopyDataChannel.COPYDATASTRUCT lParam);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int SetProp(IntPtr hwnd, string lpString, int hData);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int GetProp(IntPtr hwnd, string lpString);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int RemoveProp(IntPtr hwnd, string lpString);

    public string ChannelName => this.channelName;

    private struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
    }
}
