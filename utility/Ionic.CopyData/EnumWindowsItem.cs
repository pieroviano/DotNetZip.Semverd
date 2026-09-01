using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Ionic.CopyData;

public class EnumWindowsItem
{
    public EnumWindowsItem(IntPtr hWnd)
    {
        this.Handle = hWnd;
    }

    public string ClassName
    {
        get
        {
            var lpClassName = new StringBuilder(260, 260);
            UnManagedMethods.GetClassName(Handle, lpClassName, lpClassName.Capacity);
            return lpClassName.ToString();
        }
    }

    public ExtendedWindowStyleFlags ExtendedWindowStyle =>
        (ExtendedWindowStyleFlags)UnManagedMethods.GetWindowLong(Handle, -20);

    public IntPtr Handle { get; }

    public bool Iconic
    {
        get => UnManagedMethods.IsIconic(Handle) != 0;
        set => UnManagedMethods.SendMessage(Handle, 274, (IntPtr)61472, IntPtr.Zero);
    }

    public Point Location
    {
        get
        {
            var rect = Rect;
            return new Point(rect.Left, rect.Top);
        }
    }

    public bool Maximised
    {
        get => UnManagedMethods.IsZoomed(Handle) != 0;
        set => UnManagedMethods.SendMessage(Handle, 274, (IntPtr)61488, IntPtr.Zero);
    }

    public Rectangle Rect
    {
        get
        {
            var lpRect = new RECT();
            UnManagedMethods.GetWindowRect(Handle, ref lpRect);
            return new Rectangle(lpRect.Left, lpRect.Top, lpRect.Right - lpRect.Left, lpRect.Bottom - lpRect.Top);
        }
    }

    public Size Size
    {
        get
        {
            var rect = Rect;
            return new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
    }

    public string Text
    {
        get
        {
            var lpString = new StringBuilder(260, 260);
            UnManagedMethods.GetWindowText(Handle, lpString, lpString.Capacity);
            return lpString.ToString();
        }
    }

    public bool Visible => UnManagedMethods.IsWindowVisible(Handle) != 0;

    public WindowStyleFlags WindowStyle => (WindowStyleFlags)UnManagedMethods.GetWindowLong(Handle, -16);

    public override int GetHashCode()
    {
        return (int)Handle;
    }

    public void Restore()
    {
        if (Iconic)
        {
            UnManagedMethods.SendMessage(Handle, 274, (IntPtr)61728, IntPtr.Zero);
        }

        UnManagedMethods.BringWindowToTop(Handle);
        UnManagedMethods.SetForegroundWindow(Handle);
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
        public static extern int FlashWindow(IntPtr hWnd, ref FLASHWINFO pwfi);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32")]
        public static extern int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

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