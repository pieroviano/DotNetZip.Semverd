using System;

namespace Ionic.CopyData;

public class DataReceivedEventArgs : EventArgs
{
    internal DataReceivedEventArgs(string channelName, object data, DateTime sent)
    {
        this.ChannelName = channelName;
        this.Data = data;
        this.Sent = sent;
        Received = DateTime.Now;
    }

    public string ChannelName { get; } = "";

    public object Data { get; } = null;

    public DateTime Received { get; }

    public DateTime Sent { get; }
}