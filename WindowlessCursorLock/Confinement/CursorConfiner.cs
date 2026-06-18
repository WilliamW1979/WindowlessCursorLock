using System.Diagnostics;
using WindowlessCursorLock.Native;

namespace WindowlessCursorLock.Confinement;

public sealed class CursorConfiner : IDisposable
{
    private readonly System.Windows.Forms.Timer _timer;
    private readonly MonitorLocator _monitorLocator = new();
    private readonly BorderlessWindowDetector _borderlessDetector = new();
    private bool _isCurrentlyClipped;
    private string? _lastConfinedExeName;

    public IReadOnlyList<string> WatchedExecutableNames { get; set; } = [];
    public bool AutoDetectBorderless { get; set; } = true;
    public bool IsActive { get; private set; }

    public event EventHandler<string?>? ConfinementStateChanged;

    public CursorConfiner(int pollIntervalMs)
    {
        _timer = new System.Windows.Forms.Timer { Interval = Math.Max(50, pollIntervalMs) };
        _timer.Tick += (_, _) => Poll();
    }

    public void Start()
    {
        IsActive = true;
        _timer.Start();
    }

    public void Stop()
    {
        IsActive = false;
        _timer.Stop();
        ReleaseClip();
    }

    private void Poll()
    {
        IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
        if (foregroundWindow == IntPtr.Zero)
        {
            ReleaseClip();
            return;
        }
        if (!NativeMethods.GetWindowRect(foregroundWindow, out RECT windowBounds))
        {
            ReleaseClip();
            return;
        }
        if (!_monitorLocator.TryGetMonitorBounds(foregroundWindow, out RECT monitorBounds))
        {
            ReleaseClip();
            return;
        }
        string? exeName = TryGetProcessExeName(foregroundWindow);
        bool isExplicitlyWatched = exeName is not null && WatchedExecutableNames.Any(w => string.Equals(w, exeName, StringComparison.OrdinalIgnoreCase));
        bool isAutoDetectedBorderless = AutoDetectBorderless && _borderlessDetector.IsBorderlessFullscreen(foregroundWindow, windowBounds, monitorBounds);
        if (!isExplicitlyWatched && !isAutoDetectedBorderless)
        {
            ReleaseClip();
            return;
        }
        ApplyClip(monitorBounds, exeName);
    }

    private static string? TryGetProcessExeName(IntPtr windowHandle)
    {
        NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId);
        if (processId == 0) return null;
        try
        {
            using Process process = Process.GetProcessById((int)processId);
            return process.ProcessName + ".exe";
        }
        catch (ArgumentException) { return null; }
        catch (InvalidOperationException) { return null; }
    }

    private void ApplyClip(RECT bounds, string? exeName)
    {
        NativeMethods.ClipCursor(ref bounds);
        _isCurrentlyClipped = true;
        if (!string.Equals(_lastConfinedExeName, exeName, StringComparison.OrdinalIgnoreCase))
        {
            _lastConfinedExeName = exeName;
            ConfinementStateChanged?.Invoke(this, exeName);
        }
    }

    private void ReleaseClip()
    {
        if (!_isCurrentlyClipped) return;
        NativeMethods.ClipCursor(IntPtr.Zero);
        _isCurrentlyClipped = false;
        _lastConfinedExeName = null;
        ConfinementStateChanged?.Invoke(this, null);
    }

    public void Dispose()
    {
        Stop();
        _timer.Dispose();
    }
}