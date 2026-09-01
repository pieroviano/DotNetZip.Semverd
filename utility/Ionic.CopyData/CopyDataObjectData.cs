using System;

namespace Ionic.CopyData;

[Serializable]
internal class CopyDataObjectData
{
    public string Channel;
    public object Data;
    public DateTime Sent;

    public CopyDataObjectData(object data, string channel)
    {
        Data = data;
        if (!data.GetType().IsSerializable)
        {
            throw new ArgumentException("Data object must be serializable.", nameof(data));
        }

        Channel = channel;
        Sent = DateTime.Now;
    }
}