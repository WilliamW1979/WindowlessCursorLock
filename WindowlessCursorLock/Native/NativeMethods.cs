using System.Runtime.InteropServices;

namespace WindowlessCursorLock.Native;

internal static class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool ClipCursor(ref RECT lpRect);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool ClipCursor(System.IntPtr lpRect);
    [DllImport("user32.dll")] internal static extern System.IntPtr GetForegroundWindow();
    [DllImport("user32.dll", SetLastError = true)] internal static extern uint GetWindowThreadProcessId(System.IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool GetWindowRect(System.IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] internal static extern System.IntPtr MonitorFromWindow(System.IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll")] internal static extern bool GetMonitorInfo(System.IntPtr hMonitor, ref MONITORINFO lpmi);
    [DllImport("user32.dll", SetLastError = true)] internal static extern System.IntPtr GetWindowLongPtr(System.IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] internal static extern bool IsZoomed(System.IntPtr hWnd);
    internal const uint MONITOR_DEFAULTTONEAREST = 2;
    internal const int GWL_STYLE = -16;
    internal const long WS_CAPTION = 0x00C00000L;
    internal const long WS_THICKFRAME = 0x00040000L;
    internal const long WS_BORDER = 0x00800000L;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    internal int Left;
    internal int Top;
    internal int Right;
    internal int Bottom;
    internal int Width => Right - Left;
    internal int Height => Bottom - Top;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MONITORINFO
{
    internal uint cbSize;
    internal RECT rcMonitor;
    internal RECT rcWork;
    internal uint dwFlags;
    internal static MONITORINFO Create() => new() { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
}