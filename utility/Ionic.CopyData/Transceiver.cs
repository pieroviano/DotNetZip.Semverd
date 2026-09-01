using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Ionic.CopyData;

public class Transceiver : NativeWindow, IDisposable
{
    private const int WM_COPYDATA = 74;
    private const int WM_DESTROY = 2;
    private CopyDataChannel _channel;
    private string _channelName;
    private bool disposed;

    public string Channel
    {
        get => _channelName;
        set
        {
            _channelName = value;
            _channel = value == null ? null : new CopyDataChannel(this, value);
        }
    }

    public bool CanSend => _channelName != null;

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_channel != null)
        {
            _channel.Dispose();
        }

        _channel = null;
        disposed = true;
        GC.SuppressFinalize(this);
    }

    public event EventHandler<DataReceivedEventArgs> DataReceived;

    public void Send(object msg)
    {
        if (_channelName == null || _channel == null)
        {
            throw new InvalidOperationException();
        }

        _channel.Send(msg);
    }

    protected void OnDataReceived(DataReceivedEventArgs e)
    {
        DataReceived(this, e);
    }

    protected override void OnHandleChange()
    {
        if (_channel != null)
        {
            _channel.OnHandleChange();
        }

        base.OnHandleChange();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 74)
        {
            if (_channel != null)
            {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
                var copydatastruct = new COPYDATASTRUCT();
#pragma warning restore CS0219 // Variable is assigned but its value is never used
                var structure = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                if (structure.cbData > 0)
                {
                    var numArray = new byte[structure.cbData];
                    Marshal.Copy(structure.lpData, numArray, 0, structure.cbData);
                    var copyDataObjectData =
                        (CopyDataObjectData)new BinaryFormatter().Deserialize(new MemoryStream(numArray));
                    if (_channelName == copyDataObjectData.Channel)
                    {
                        OnDataReceived(new DataReceivedEventArgs(copyDataObjectData.Channel, copyDataObjectData.Data,
                            copyDataObjectData.Sent));
                        m.Result = (IntPtr)1;
                    }
                }
            }
        }
        else if (m.Msg == 2)
        {
            OnHandleChange();
        }

        base.WndProc(ref m);
    }

    ~Transceiver()
    {
        Dispose();
    }

    private struct COPYDATASTRUCT
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public IntPtr dwData;
        public int cbData;
        public IntPtr lpData;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }
}