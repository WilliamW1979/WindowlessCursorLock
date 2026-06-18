using WindowlessCursorLock.Native;

namespace WindowlessCursorLock.Confinement;

public sealed class BorderlessWindowDetector
{
    private const int DefaultTolerancePixels = 8;

    public int TolerancePixels { get; set; } = DefaultTolerancePixels;

    internal bool IsBorderlessFullscreen(IntPtr windowHandle, RECT windowBounds, RECT monitorBounds)
    {
        if (NativeMethods.IsZoomed(windowHandle)) return false;
        if (HasStandardWindowChrome(windowHandle)) return false;
        return BoundsMatchWithinTolerance(windowBounds, monitorBounds);
    }

    private static bool HasStandardWindowChrome(IntPtr windowHandle)
    {
        long style = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GWL_STYLE).ToInt64();
        bool hasCaption = (style & NativeMethods.WS_CAPTION) != 0;
        bool hasThickFrame = (style & NativeMethods.WS_THICKFRAME) != 0;
        bool hasBorder = (style & NativeMethods.WS_BORDER) != 0;
        return hasCaption || hasThickFrame || hasBorder;
    }

    private bool BoundsMatchWithinTolerance(RECT windowBounds, RECT monitorBounds) => Math.Abs(windowBounds.Left - monitorBounds.Left) <= TolerancePixels && Math.Abs(windowBounds.Top - monitorBounds.Top) <= TolerancePixels && Math.Abs(windowBounds.Right - monitorBounds.Right) <= TolerancePixels && Math.Abs(windowBounds.Bottom - monitorBounds.Bottom) <= TolerancePixels;
}