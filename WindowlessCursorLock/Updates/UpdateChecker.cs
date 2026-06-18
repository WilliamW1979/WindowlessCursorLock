using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using WindowlessCursorLock.Settings;

namespace WindowlessCursorLock.Updates;

public sealed class UpdateChecker : IDisposable
{
    private const string GitHubRepo = "WilliamW1979/WindowlessCursorLock";
    private const string GitHubApiUrl = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
    private static readonly HttpClient _httpClient = CreateHttpClient();
    private readonly AppSettings _settings;
    private readonly System.Threading.Timer? _periodicTimer;
    private bool _disposed;
    public event EventHandler<UpdateInfo>? UpdateAvailable;
    public UpdateInfo? LastUpdate { get; private set; }

    public UpdateChecker(AppSettings settings)
    {
        _settings = settings;
        if (settings.UpdateCheckIntervalHours > 0)
            _periodicTimer = new System.Threading.Timer(_ => _ = CheckAsync(), null, TimeSpan.Zero, TimeSpan.FromHours(settings.UpdateCheckIntervalHours));
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new() { Timeout = TimeSpan.FromSeconds(30) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WindowlessCursorLock/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        return client;
    }

    public async Task<UpdateInfo?> CheckNowAsync() => await CheckInternalAsync(forceNotify: true);

    public async Task CheckAsync() => await CheckInternalAsync(forceNotify: false);

    private async Task<UpdateInfo?> CheckInternalAsync(bool forceNotify)
    {
        if (_disposed) return null;
        try
        {
            GitHubReleaseResponse? release = await _httpClient.GetFromJsonAsync<GitHubReleaseResponse>(GitHubApiUrl);
            if (release is null || string.IsNullOrEmpty(release.TagName)) return null;
            string currentVersion = GetCurrentVersion();
            string latestVersion = release.TagName.TrimStart('v');
            string installerUrl = release.Assets?.FirstOrDefault(a => a.Name!.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))?.BrowserDownloadUrl ?? string.Empty;
            UpdateInfo update = new()
            {
                LatestVersion = latestVersion,
                CurrentVersion = currentVersion,
                DownloadUrl = release.HtmlUrl ?? $"https://github.com/{GitHubRepo}/releases/latest",
                ReleaseNotesUrl = release.HtmlUrl ?? $"https://github.com/{GitHubRepo}/releases/latest",
                InstallerUrl = installerUrl
            };
            if (!update.IsNewer) return null;
            LastUpdate = update;
            if (forceNotify) UpdateAvailable?.Invoke(this, update);
            else HandleUpdateMode(update);
            return update;
        }
        catch { return null; }
    }

    private void HandleUpdateMode(UpdateInfo update)
    {
        switch (_settings.UpdateMode)
        {
            case UpdateMode.AutoUpdate:
                if (!string.IsNullOrEmpty(update.InstallerUrl)) _ = DownloadAndInstallAsync(update);
                else UpdateAvailable?.Invoke(this, update);
                break;
            case UpdateMode.AutoDownload:
                if (!string.IsNullOrEmpty(update.InstallerUrl)) _ = DownloadAsync(update);
                else UpdateAvailable?.Invoke(this, update);
                break;
            case UpdateMode.NotifyOnly:
            default:
                UpdateAvailable?.Invoke(this, update);
                break;
        }
    }

    private async Task DownloadAndInstallAsync(UpdateInfo update)
    {
        string installerPath = await DownloadAsync(update);
        if (!string.IsNullOrEmpty(installerPath) && File.Exists(installerPath))
            try
            {
                Process.Start(new ProcessStartInfo { FileName = installerPath, UseShellExecute = true });
                Environment.Exit(0);
            }
            catch { UpdateAvailable?.Invoke(this, update); }
    }

    private async Task<string> DownloadAsync(UpdateInfo update)
    {
        if (string.IsNullOrEmpty(update.InstallerUrl)) return string.Empty;
        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "WindowlessCursorLock_Update");
            Directory.CreateDirectory(tempDir);
            string fileName = Path.GetFileName(new Uri(update.InstallerUrl).LocalPath);
            string filePath = Path.Combine(tempDir, fileName);
            byte[] data = await _httpClient.GetByteArrayAsync(update.InstallerUrl);
            await File.WriteAllBytesAsync(filePath, data);
            return filePath;
        }
        catch { return string.Empty; }
    }

    private static string GetCurrentVersion() => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _periodicTimer?.Dispose();
    }

    private sealed class GitHubReleaseResponse
    {
        public string? TagName { get; set; }
        public string? HtmlUrl { get; set; }
        public List<GitHubAssetResponse>? Assets { get; set; }
    }

    private sealed class GitHubAssetResponse
    {
        public string? Name { get; set; }
        public string? BrowserDownloadUrl { get; set; }
    }
}