using System.IO;
using System.Web.Hosting;

namespace Ionic.ToolsAndTests;

public class BinaryCapableRequest : SimpleWorkerRequest
{
    private readonly Stream _outStream;

    public BinaryCapableRequest(string page, string query, Stream output)
        : base(page, query, null)
    {
        _outStream = output;
    }

    public override void SendResponseFromMemory(byte[] data, int length)
    {
        _outStream.Write(data, 0, length);
    }
}