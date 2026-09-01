using System.IO;
using System.Web.Hosting;

#nullable disable
namespace Ionic.ToolsAndTests;

public class BinaryCapableRequest : SimpleWorkerRequest
{
  private Stream _outStream;

  public BinaryCapableRequest(string page, string query, Stream output)
    : base(page, query, (TextWriter) null)
  {
    this._outStream = output;
  }

  public override void SendResponseFromMemory(byte[] data, int length)
  {
    this._outStream.Write(data, 0, length);
  }
}
