using System;
using System.Threading;
using System.Web;

namespace Ionic.ToolsAndTests;

public class MyAspNetHost : MarshalByRefObject
{
    public AppDomain GetAppDomain()
    {
        return Thread.GetDomain();
    }

    public void ProcessRequest(string url)
    {
        var strArray = url.Split('?');
        HttpRuntime.ProcessRequest(new BinaryCapableRequest(strArray[0], strArray.Length > 1 ? strArray[1] : null,
            Console.OpenStandardOutput()));
    }
}