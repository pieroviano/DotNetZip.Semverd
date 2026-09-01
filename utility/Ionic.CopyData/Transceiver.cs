using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

#nullable disable
namespace Ionic.CopyData;

public class Transceiver : NativeWindow, IDisposable
{
  private const int WM_COPYDATA = 74;
  private const int WM_DESTROY = 2;
  private CopyDataChannel _channel;
  private string _channelName;
  private bool disposed;

  public event EventHandler<DataReceivedEventArgs> DataReceived;

  public string Channel
  {
    set
    {
      this._channelName = value;
      this._channel = value == null ? (CopyDataChannel) null : new CopyDataChannel((NativeWindow) this, value);
    }
    get => this._channelName;
  }

  public bool CanSend => this._channelName != null;

  protected void OnDataReceived(DataReceivedEventArgs e) => this.DataReceived((object) this, e);

  protected override void OnHandleChange()
  {
    if (this._channel != null)
      this._channel.OnHandleChange();
    base.OnHandleChange();
  }

  public void Send(object msg)
  {
    if (this._channelName == null || this._channel == null)
      throw new InvalidOperationException();
    this._channel.Send(msg);
  }

  protected override void WndProc(ref Message m)
  {
    if (m.Msg == 74)
    {
      if (this._channel != null)
      {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
          Transceiver.COPYDATASTRUCT copydatastruct = new Transceiver.COPYDATASTRUCT();
#pragma warning restore CS0219 // Variable is assigned but its value is never used
          Transceiver.COPYDATASTRUCT structure = (Transceiver.COPYDATASTRUCT) Marshal.PtrToStructure(m.LParam, typeof (Transceiver.COPYDATASTRUCT));
        if (structure.cbData > 0)
        {
          byte[] numArray = new byte[structure.cbData];
          Marshal.Copy(structure.lpData, numArray, 0, structure.cbData);
          CopyDataObjectData copyDataObjectData = (CopyDataObjectData) new BinaryFormatter().Deserialize((Stream) new MemoryStream(numArray));
          if (this._channelName == copyDataObjectData.Channel)
          {
            this.OnDataReceived(new DataReceivedEventArgs(copyDataObjectData.Channel, copyDataObjectData.Data, copyDataObjectData.Sent));
            m.Result = (IntPtr) 1;
          }
        }
      }
    }
    else if (m.Msg == 2)
      this.OnHandleChange();
    base.WndProc(ref m);
  }

  public void Dispose()
  {
    if (this.disposed)
      return;
    if (this._channel != null)
      this._channel.Dispose();
    this._channel = (CopyDataChannel) null;
    this.disposed = true;
    GC.SuppressFinalize((object) this);
  }

  ~Transceiver() => this.Dispose();

  private struct COPYDATASTRUCT
  {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
      public IntPtr dwData;
      public int cbData;
    public IntPtr lpData;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
  }
}
