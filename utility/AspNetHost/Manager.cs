using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Hosting;

#nullable disable
namespace Ionic.ToolsAndTests;

public class Manager
{
  [DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLinkW", CharSet = CharSet.Unicode)]
  public static extern int CreateSymbolicLink(
    string lpSymlinkFileName,
    string lpTargetFileName,
    int dwFlags);

  public void Run(string[] pages)
  {
    bool flag = false;
    MyAspNetHost myAspNetHost = (MyAspNetHost) null;
    string virtualDir = "/foo";
    string currentDirectory = Directory.GetCurrentDirectory();
    try
    {
      if (!Directory.Exists("bin"))
      {
        flag = true;
        Manager.CreateSymbolicLink("bin", ".", 1);
      }
      myAspNetHost = (MyAspNetHost) ApplicationHost.CreateApplicationHost(typeof (MyAspNetHost), virtualDir, currentDirectory);
      foreach (string page in pages)
        myAspNetHost.ProcessRequest(page);
    }
    finally
    {
      if (myAspNetHost != null)
      {
        AppDomain.Unload(myAspNetHost.GetAppDomain());
        if (flag)
          Directory.Delete("bin");
      }
    }
  }

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
        new Manager().Run(args);
      }
      catch (Exception ex)
      {
        Console.WriteLine("{0}: {1}\n{2}", (object) ex.GetType().ToString(), (object) ex.Message, (object) ex.StackTrace);
      }
    }
  }
}
