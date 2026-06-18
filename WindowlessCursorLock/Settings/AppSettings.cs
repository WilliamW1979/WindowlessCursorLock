using Tomlyn.Serialization;

namespace WindowlessCursorLock.Settings;

[TomlSerializable(typeof(AppSettings))]
internal sealed partial class SettingsTomlContext : TomlSerializerContext { }

public enum UpdateMode
{
    AutoUpdate,
    AutoDownload,
    NotifyOnly
}

public sealed class AppSettings
{
    public List<string> WatchedExecutableNames { get; set; } = [];
    public bool AutoDetectBorderless { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool StartMinimized { get; set; } = true;
    public int PollIntervalMs { get; set; } = 250;
    public UpdateMode UpdateMode { get; set; } = UpdateMode.NotifyOnly;
    public int UpdateCheckIntervalHours { get; set; } = 24;
}