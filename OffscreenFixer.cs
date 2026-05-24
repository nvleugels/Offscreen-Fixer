using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

internal static class Program
{
    private const int MinWindowSize = 50;
    private const int BaseOffset = 20;
    private const int CascadeStep = 30;

    private static int moveCount;
    private static List<Rectangle> monitorBounds;
    private static Rectangle primaryMonitorBounds;

    [STAThread]
    private static int Main()
    {
        // Capture the active monitor layout once so every window is evaluated against the current screen setup.
        monitorBounds = new List<Rectangle>();
        foreach (Screen screen in Screen.AllScreens)
        {
            Rectangle bounds = screen.Bounds;
            monitorBounds.Add(bounds);

            string primaryMark = screen.Primary ? " (primary)" : string.Empty;
            Console.WriteLine("Monitor{0}: {1} ({2},{3})-({4},{5}) {6}x{7}",
                primaryMark,
                screen.DeviceName,
                bounds.Left,
                bounds.Top,
                bounds.Right,
                bounds.Bottom,
                bounds.Width,
                bounds.Height);

            if (screen.Primary)
            {
                primaryMonitorBounds = bounds;
            }
        }

        if (monitorBounds.Count == 0)
        {
            Console.WriteLine("No monitors found. Exiting.");
            return 1;
        }

        if (primaryMonitorBounds.Width == 0 && primaryMonitorBounds.Height == 0)
        {
            primaryMonitorBounds = monitorBounds[0];
        }

        Console.WriteLine();
        Console.WriteLine("Windows:");

        // Enumerate every visible top-level window, not just process main windows.
        NativeMethods.EnumWindows(EnumerateWindow, IntPtr.Zero);

        Console.WriteLine("Done.");
        return 0;
    }

    private static bool EnumerateWindow(IntPtr hWnd, IntPtr lParam)
    {
        if (!NativeMethods.IsWindowVisible(hWnd))
        {
            return true;
        }

        string title = GetWindowTitle(hWnd);
        if (string.IsNullOrWhiteSpace(title))
        {
            return true;
        }

        NativeMethods.RECT rect;
        if (!NativeMethods.GetWindowRect(hWnd, out rect))
        {
            return true;
        }

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width < MinWindowSize || height < MinWindowSize)
        {
            return true;
        }

        int centerX = (rect.Left + rect.Right) / 2;
        int centerY = (rect.Top + rect.Bottom) / 2;
        bool onMonitor = IsPointOnAnyMonitor(centerX, centerY);
        bool minimized = NativeMethods.IsIconic(hWnd);

        string state = onMonitor ? "on-screen" : "off-screen";
        if (minimized)
        {
            state += ", minimized";
        }

        Console.WriteLine("Window: '{0}' | {1} | Rect=({2},{3})-({4},{5}) | Center=({6},{7})",
            title,
            state,
            rect.Left,
            rect.Top,
            rect.Right,
            rect.Bottom,
            centerX,
            centerY);

        if (onMonitor && minimized)
        {
            Console.WriteLine("  Skipped: minimized but already on a monitor");
            return true;
        }

        if (!onMonitor)
        {
            moveCount++;
            int offset = (moveCount - 1) * CascadeStep;

            // Restore first so minimized windows become visible before we move them.
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);

            int targetX = primaryMonitorBounds.Left + BaseOffset + offset;
            int targetY = primaryMonitorBounds.Top + BaseOffset + offset;

            bool moved = NativeMethods.SetWindowPos(
                hWnd,
                NativeMethods.HWND_TOP,
                targetX,
                targetY,
                width,
                height,
                NativeMethods.SWP_NOZORDER | NativeMethods.SWP_SHOWWINDOW);

            if (!moved)
            {
                moved = NativeMethods.MoveWindow(hWnd, targetX, targetY, width, height, true);
            }

            if (moved)
            {
                // Bring the window forward so the user sees the recovered app immediately.
                NativeMethods.SetForegroundWindow(hWnd);
                Console.WriteLine("  Moved to primary monitor at ({0},{1}) and brought to the foreground", targetX, targetY);
            }
            else
            {
                Console.WriteLine("  Move failed");
            }
        }

        return true;
    }

    private static bool IsPointOnAnyMonitor(int x, int y)
    {
        foreach (Rectangle bounds in monitorBounds)
        {
            if (x >= bounds.Left && x < bounds.Right && y >= bounds.Top && y < bounds.Bottom)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        int length = NativeMethods.GetWindowTextLength(hWnd);
        if (length <= 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString().Trim();
    }

    private static class NativeMethods
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static readonly IntPtr HWND_TOP = IntPtr.Zero;

        public const int SW_RESTORE = 9;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}