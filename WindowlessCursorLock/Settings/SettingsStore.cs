using Tomlyn;

namespace WindowlessCursorLock.Settings;

public sealed class SettingsStore
{
    private readonly string _filePath;
    private static readonly TomlSerializerOptions Options = new() { WriteIndented = true };
    public SettingsStore() : this(GetDefaultFilePath()) { }
    public SettingsStore(string filePath) => _filePath = filePath;

    private static string GetDefaultFilePath()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WindowlessCursorLock");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "settings.ini");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_filePath)) return new AppSettings();
        try
        {
            string toml = File.ReadAllText(_filePath);
            return TomlSerializer.Deserialize<AppSettings>(toml, Options) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or TomlException or UnauthorizedAccessException) { return new AppSettings(); }
    }

    public void Save(AppSettings settings)
    {
        string toml = TomlSerializer.Serialize(settings, Options);
        string tempPath = _filePath + ".tmp";
        File.WriteAllText(tempPath, toml);
        File.Move(tempPath, _filePath, overwrite: true);
    }
}