using System;

#nullable disable
namespace Ionic.CopyData;

[Serializable]
internal class CopyDataObjectData
{
  public string Channel;
  public object Data;
  public DateTime Sent;

  public CopyDataObjectData(object data, string channel)
  {
    this.Data = data;
    if (!data.GetType().IsSerializable)
      throw new ArgumentException("Data object must be serializable.", nameof (data));
    this.Channel = channel;
    this.Sent = DateTime.Now;
  }
}
