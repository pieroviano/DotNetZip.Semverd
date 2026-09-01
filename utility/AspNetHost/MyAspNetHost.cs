using System;
using System.Threading;
using System.Web;

#nullable disable
namespace Ionic.ToolsAndTests;

public class MyAspNetHost : MarshalByRefObject
{
  public void ProcessRequest(string url)
  {
    string[] strArray = url.Split('?');
    HttpRuntime.ProcessRequest((HttpWorkerRequest) new BinaryCapableRequest(strArray[0], strArray.Length > 1 ? strArray[1] : (string) null, Console.OpenStandardOutput()));
  }

  public AppDomain GetAppDomain() => Thread.GetDomain();
}
