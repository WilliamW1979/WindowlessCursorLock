namespace WindowlessCursorLock.Updates;

public sealed class UpdateInfo
{
    public required string LatestVersion { get; init; }
    public required string CurrentVersion { get; init; }
    public required string DownloadUrl { get; init; }
    public required string ReleaseNotesUrl { get; init; }
    public required string InstallerUrl { get; init; }
    public bool IsNewer => CompareVersions(LatestVersion, CurrentVersion) > 0;

    private static int CompareVersions(string v1, string v2)
    {
        Version ver1 = Version.TryParse(v1.TrimStart('v', 'V'), out Version? p1) ? p1 : new Version(0, 0);
        Version ver2 = Version.TryParse(v2.TrimStart('v', 'V'), out Version? p2) ? p2 : new Version(0, 0);
        return ver1.CompareTo(ver2);
    }
}
