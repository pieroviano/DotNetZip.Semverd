using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

#nullable disable
namespace Ionic.CopyData;

public class EnumWindowsItem
{
  private IntPtr hWnd;

  public EnumWindowsItem(IntPtr hWnd) => this.hWnd = hWnd;

  public override int GetHashCode() => (int) this.hWnd;

  public void Restore()
  {
    if (this.Iconic)
      EnumWindowsItem.UnManagedMethods.SendMessage(this.hWnd, 274, (IntPtr) 61728, IntPtr.Zero);
    EnumWindowsItem.UnManagedMethods.BringWindowToTop(this.hWnd);
    EnumWindowsItem.UnManagedMethods.SetForegroundWindow(this.hWnd);
  }

  public string ClassName
  {
    get
    {
      StringBuilder lpClassName = new StringBuilder(260, 260);
      EnumWindowsItem.UnManagedMethods.GetClassName(this.hWnd, lpClassName, lpClassName.Capacity);
      return lpClassName.ToString();
    }
  }

  public ExtendedWindowStyleFlags ExtendedWindowStyle
  {
    get
    {
      return (ExtendedWindowStyleFlags) EnumWindowsItem.UnManagedMethods.GetWindowLong(this.hWnd, -20);
    }
  }

  public IntPtr Handle => this.hWnd;

  public bool Iconic
  {
    get => EnumWindowsItem.UnManagedMethods.IsIconic(this.hWnd) != 0;
    set
    {
      EnumWindowsItem.UnManagedMethods.SendMessage(this.hWnd, 274, (IntPtr) 61472, IntPtr.Zero);
    }
  }

  public Point Location
  {
    get
    {
      Rectangle rect = this.Rect;
      return new Point(rect.Left, rect.Top);
    }
  }

  public bool Maximised
  {
    get => EnumWindowsItem.UnManagedMethods.IsZoomed(this.hWnd) != 0;
    set
    {
      EnumWindowsItem.UnManagedMethods.SendMessage(this.hWnd, 274, (IntPtr) 61488, IntPtr.Zero);
    }
  }

  public Rectangle Rect
  {
    get
    {
      EnumWindowsItem.RECT lpRect = new EnumWindowsItem.RECT();
      EnumWindowsItem.UnManagedMethods.GetWindowRect(this.hWnd, ref lpRect);
      return new Rectangle(lpRect.Left, lpRect.Top, lpRect.Right - lpRect.Left, lpRect.Bottom - lpRect.Top);
    }
  }

  public Size Size
  {
    get
    {
      Rectangle rect = this.Rect;
      return new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
    }
  }

  public string Text
  {
    get
    {
      StringBuilder lpString = new StringBuilder(260, 260);
      EnumWindowsItem.UnManagedMethods.GetWindowText(this.hWnd, lpString, lpString.Capacity);
      return lpString.ToString();
    }
  }

  public bool Visible => EnumWindowsItem.UnManagedMethods.IsWindowVisible(this.hWnd) != 0;

  public WindowStyleFlags WindowStyle
  {
    get => (WindowStyleFlags) EnumWindowsItem.UnManagedMethods.GetWindowLong(this.hWnd, -16);
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  private struct FLASHWINFO
  {
    public int cbSize;
    public IntPtr hwnd;
    public int dwFlags;
    public int uCount;
    public int dwTimeout;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  private struct RECT
  {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }

  private class UnManagedMethods
  {
    public const int FLASHW_ALL = 3;
    public const int FLASHW_CAPTION = 1;
    public const int FLASHW_STOP = 0;
    public const int FLASHW_TIMER = 4;
    public const int FLASHW_TIMERNOFG = 12;
    public const int FLASHW_TRAY = 2;
    public const int GWL_EXSTYLE = -20;
    public const int GWL_STYLE = -16;
    public const int SC_CLOSE = 61536;
    public const int SC_MAXIMIZE = 61488;
    public const int SC_MINIMIZE = 61472;
    public const int SC_RESTORE = 61728;
    public const int WM_COMMAND = 273;
    public const int WM_SYSCOMMAND = 274;

    [DllImport("user32")]
    public static extern int BringWindowToTop(IntPtr hWnd);

    [DllImport("user32")]
    public static extern int FlashWindow(IntPtr hWnd, ref EnumWindowsItem.FLASHWINFO pwfi);

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

    [DllImport("user32")]
    public static extern int GetWindowRect(IntPtr hWnd, ref EnumWindowsItem.RECT lpRect);

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int cch);

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32")]
    public static extern int IsIconic(IntPtr hWnd);

    [DllImport("user32")]
    public static extern int IsWindowVisible(IntPtr hWnd);

    [DllImport("user32")]
    public static extern int IsZoomed(IntPtr hwnd);

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32")]
    public static extern int SetForegroundWindow(IntPtr hWnd);
  }
}
