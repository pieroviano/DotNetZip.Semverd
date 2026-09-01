using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Hosting;

namespace Ionic.ToolsAndTests;

public class Program
{
    [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode)]
    public static extern int CreateSymbolicLink(
        string lpSymlinkFileName,
        string lpTargetFileName,
        int dwFlags);

    public static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine("Usage:  AspNetHost <aspx url> ...");
        }
        else
        {
            try
            {
                new Program().Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: {1}\n{2}", ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
        }
    }

    public void Run(string[] pages)
    {
        var flag = false;
        var myAspNetHost = (MyAspNetHost)null;
        var virtualDir = "/foo";
        var currentDirectory = Directory.GetCurrentDirectory();
        try
        {
            if (!Directory.Exists("bin"))
            {
                flag = true;
                CreateSymbolicLink("bin", ".", 1);
            }

            myAspNetHost =
                (MyAspNetHost)ApplicationHost.CreateApplicationHost(typeof(MyAspNetHost), virtualDir, currentDirectory);
            foreach (var page in pages)
            {
                myAspNetHost.ProcessRequest(page);
            }
        }
        finally
        {
            if (myAspNetHost != null)
            {
                AppDomain.Unload(myAspNetHost.GetAppDomain());
                if (flag)
                {
                    Directory.Delete("bin");
                }
            }
        }
    }
}