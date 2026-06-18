using WindowlessCursorLock.Confinement;
using WindowlessCursorLock.Settings;
using WindowlessCursorLock.Ui;
using WindowlessCursorLock.Updates;

namespace WindowlessCursorLock;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly SettingsStore _store;
    private readonly AppSettings _settings;
    private readonly CursorConfiner _confiner;
    private readonly UpdateChecker _updateChecker;
    private readonly ToolStripMenuItem _pauseResumeItem;
    private readonly Icon? _appIcon;
    private SettingsForm? _openSettingsForm;

    public TrayApplicationContext(bool isFirstRun = false)
    {
        _store = new SettingsStore();
        _settings = _store.Load();
        _confiner = new CursorConfiner(_settings.PollIntervalMs)
        {
            WatchedExecutableNames = _settings.WatchedExecutableNames,
            AutoDetectBorderless = _settings.AutoDetectBorderless
        };
        _appIcon = AppIcon.Load();
        _pauseResumeItem = new ToolStripMenuItem("Pause", null, (_, _) => TogglePause());
        ToolStripMenuItem settingsItem = new("Settings...", null, (_, _) => OpenSettings());
        ToolStripMenuItem checkUpdatesItem = new("Check for Updates...", null, async (_, _) => await CheckForUpdatesAsync());
        ToolStripMenuItem exitItem = new("Exit", null, (_, _) => ExitApplication());
        ContextMenuStrip menu = new();
        menu.Items.Add(settingsItem);
        menu.Items.Add(checkUpdatesItem);
        menu.Items.Add(_pauseResumeItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);
        _trayIcon = new NotifyIcon
        {
            Icon = _appIcon ?? System.Drawing.SystemIcons.Application,
            Text = "Windowless Cursor Lock",
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => OpenSettings();
        _updateChecker = new UpdateChecker(_settings);
        _updateChecker.UpdateAvailable += OnUpdateAvailable;
        _confiner.Start();
        if (!isFirstRun)
            _ = _updateChecker.CheckAsync();
    }

    private void TogglePause()
    {
        if (_confiner.IsActive)
        {
            _confiner.Stop();
            _pauseResumeItem.Text = "Resume";
            _trayIcon.Text = "Windowless Cursor Lock (paused)";
        }
        else
        {
            _confiner.Start();
            _pauseResumeItem.Text = "Pause";
            _trayIcon.Text = "Windowless Cursor Lock";
        }
    }

    private void OpenSettings()
    {
        if (_openSettingsForm is not null)
        {
            _openSettingsForm.Activate();
            return;
        }
        _openSettingsForm = new SettingsForm(_settings, _store, _appIcon);
        _openSettingsForm.SettingsChanged += (_, _) =>
        {
            _confiner.WatchedExecutableNames = _settings.WatchedExecutableNames;
            _confiner.AutoDetectBorderless = _settings.AutoDetectBorderless;
        };
        _openSettingsForm.FormClosed += (_, _) => _openSettingsForm = null;
        _openSettingsForm.Show();
    }

    private async Task CheckForUpdatesAsync()
    {
        UpdateInfo? update = await _updateChecker.CheckNowAsync();
        if (update is null)
        {
            MessageBox.Show("You are running the latest version.", "Windowless Cursor Lock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        ShowUpdatePrompt(update);
    }

    private void OnUpdateAvailable(object? sender, UpdateInfo update)
    {
        _trayIcon.BalloonTipTitle = "Update Available";
        _trayIcon.BalloonTipText = $"Version {update.LatestVersion} is available.";
        _trayIcon.ShowBalloonTip(5000);
    }

    private void ShowUpdatePrompt(UpdateInfo update)
    {
        using UpdateForm form = new(update);
        form.ShowDialog();
    }

    private void ExitApplication()
    {
        _updateChecker.Dispose();
        _confiner.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _appIcon?.Dispose();
        ExitThread();
    }
}