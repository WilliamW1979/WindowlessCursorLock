using WindowlessCursorLock.Native;

namespace WindowlessCursorLock.Confinement;

public sealed class MonitorLocator
{
    internal bool TryGetMonitorBounds(IntPtr windowHandle, out RECT monitorBounds)
    {
        monitorBounds = default;
        IntPtr monitorHandle = NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MONITOR_DEFAULTTONEAREST);
        if (monitorHandle == IntPtr.Zero) return false;
        MONITORINFO info = MONITORINFO.Create();
        if (!NativeMethods.GetMonitorInfo(monitorHandle, ref info)) return false;
        monitorBounds = info.rcMonitor;
        return true;
    }
}