using System;
using System.Windows.Forms;

#nullable disable
namespace UnitTestProgressMonitor;

internal static class Program
{
  [STAThread]
  private static void Main(string[] args)
  {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run((Form) new Form1(args));
  }
}
