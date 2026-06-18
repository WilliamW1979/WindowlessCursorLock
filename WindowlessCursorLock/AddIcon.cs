using System.Reflection;

namespace WindowlessCursorLock;

internal static class AppIcon
{
    private const string ResourceName = "WindowlessCursorLock.windowlesscursorlock.ico";
    private const string ResourceLogo = "WindowlessCursorLock.WardShield.png";

    internal static Icon? Load()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(ResourceName);
        return stream is null ? null : new Icon(stream);
    }
}