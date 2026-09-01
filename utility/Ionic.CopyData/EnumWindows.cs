using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

#nullable disable
namespace Ionic.CopyData;

public class EnumWindows
{
  private List<EnumWindowsItem> items;

  public void GetWindows()
  {
    this.items = new List<EnumWindowsItem>();
    EnumWindows.NativeMethods.EnumWindows(new EnumWindows.EnumWindowsProc(this.WindowEnum), 0);
  }

  public void GetWindows(IntPtr hWndParent)
  {
    this.items = new List<EnumWindowsItem>();
    EnumWindows.NativeMethods.EnumChildWindows(hWndParent, new EnumWindows.EnumWindowsProc(this.WindowEnum), 0);
  }

  protected virtual bool OnWindowEnum(IntPtr hWnd)
  {
    this.items.Add(new EnumWindowsItem(hWnd));
    return true;
  }

  private int WindowEnum(IntPtr hWnd, int lParam) => this.OnWindowEnum(hWnd) ? 1 : 0;

  public ReadOnlyCollection<EnumWindowsItem> Items => this.items.AsReadOnly();

  private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);

  private class NativeMethods
  {
    [DllImport("user32")]
    public static extern int EnumChildWindows(
      IntPtr hWndParent,
      EnumWindows.EnumWindowsProc lpEnumFunc,
      int lParam);

    [DllImport("user32")]
    public static extern int EnumWindows(EnumWindows.EnumWindowsProc lpEnumFunc, int lParam);
  }
}
