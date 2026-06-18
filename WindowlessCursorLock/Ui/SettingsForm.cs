using System.Diagnostics;
using System.Reflection;
using WindowlessCursorLock.Settings;

namespace WindowlessCursorLock.Ui;

public sealed class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly SettingsStore _store;
    private readonly Icon? _icon;
    private readonly CheckBox _autoDetectBox;
    private readonly ListBox _exeListBox;
    private readonly CheckBox _startWithWindowsBox;
    private readonly CheckBox _startMinimizedBox;
    private readonly ComboBox _updateModeBox;
    public event EventHandler? SettingsChanged;

    public SettingsForm(AppSettings settings, SettingsStore store, Icon? icon)
    {
        _settings = settings;
        _store = store;
        _icon = icon;
        Text = "Windowless Cursor Lock Settings";
        Width = 440;
        Height = 620;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        if (icon is not null) Icon = icon;
        _autoDetectBox = new CheckBox
        {
            Text = "Auto-detect borderless fullscreen windows",
            Dock = DockStyle.Top,
            Height = 28,
            Checked = _settings.AutoDetectBorderless,
            Padding = new Padding(8, 8, 0, 0)
        };
        Label autoDetectHint = new()
        {
            Text = "When enabled, any borderless fullscreen window is confined automatically — no need to add it below.",
            Dock = DockStyle.Top,
            Height = 32,
            Padding = new Padding(8, 0, 8, 8),
            ForeColor = SystemColors.GrayText
        };
        Label listLabel = new() { Text = "Force-Watch These Executables (optional):", Dock = DockStyle.Top, Height = 24, Padding = new Padding(8, 4, 0, 0) };
        _exeListBox = new ListBox { Dock = DockStyle.Top, Height = 140 };
        _exeListBox.Items.AddRange(_settings.WatchedExecutableNames.ToArray());
        FlowLayoutPanel buttonPanel = new() { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8) };
        Button addButton = new() { Text = "Add..." };
        addButton.Click += (_, _) => AddExecutable();
        Button removeButton = new() { Text = "Remove" };
        removeButton.Click += (_, _) => RemoveSelectedExecutable();
        buttonPanel.Controls.Add(addButton);
        buttonPanel.Controls.Add(removeButton);
        _startWithWindowsBox = new CheckBox { Text = "Start with Windows", Dock = DockStyle.Top, Height = 28, Checked = _settings.StartWithWindows, Padding = new Padding(8, 4, 0, 0) };
        _startMinimizedBox = new CheckBox { Text = "Start minimized to tray", Dock = DockStyle.Top, Height = 28, Checked = _settings.StartMinimized, Padding = new Padding(8, 4, 0, 0) };
        GroupBox updateGroup = new() { Text = "Update Settings", Dock = DockStyle.Top, Height = 80, Padding = new Padding(8) };
        Label updateLabel = new() { Text = "When updates are available:", Dock = DockStyle.Top, Height = 20, Padding = new Padding(0, 4, 0, 0) };
        _updateModeBox = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 28 };
        _updateModeBox.Items.AddRange(["Notify me only", "Download automatically (manual install)", "Install automatically"]);
        _updateModeBox.SelectedIndex = (int)_settings.UpdateMode;
        updateGroup.Controls.Add(_updateModeBox);
        updateGroup.Controls.Add(updateLabel);
        Panel linksPanel = new()
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(8, 8, 8, 4)
        };
        Assembly assembly = Assembly.GetExecutingAssembly();
        PictureBox githubpicture = new()
        {
            Image = Image.FromStream(assembly.GetManifestResourceStream("WindowlessCursorLock.github.jpg")!),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(57, 32),
            Location = new Point(8, 4),
            Cursor = Cursors.Hand
        };
        githubpicture.Click += (_, _) => OpenUrl("https://github.com/WilliamW1979");
        ToolTip tooltip = new();
        tooltip.SetToolTip(githubpicture, "Visit my Github");
        linksPanel.Controls.Add(githubpicture);
        PictureBox kofipicture = new()
        {
            Image = Image.FromStream(assembly.GetManifestResourceStream("WindowlessCursorLock.Kofi.png")!),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(39, 32),
            Location = new Point(75, 4),
            Cursor = Cursors.Hand
        };
        kofipicture.Click += (_, _) => OpenUrl("https://ko-fi.com/williamw1979");
        tooltip = new();
        tooltip.SetToolTip(kofipicture, "Donate please");
        linksPanel.Controls.Add(kofipicture);
        Button saveButton = new() { Text = "Save", Dock = DockStyle.Bottom };
        saveButton.Click += (_, _) => SaveAndClose();
        Button closeButton = new() { Text = "Close", Dock = DockStyle.Bottom };
        closeButton.Click += (_, _) => Close();
        Controls.Add(linksPanel);
        Controls.Add(updateGroup);
        Controls.Add(_startMinimizedBox);
        Controls.Add(_startWithWindowsBox);
        Controls.Add(buttonPanel);
        Controls.Add(_exeListBox);
        Controls.Add(listLabel);
        Controls.Add(autoDetectHint);
        Controls.Add(_autoDetectBox);
        Controls.Add(saveButton);
        Controls.Add(closeButton);
    }

    private void AddExecutable()
    {
        using AddExeForm form = new(_icon);
        if (form.ShowDialog(this) != DialogResult.OK || form.SelectedExeName is null) return;
        if (_exeListBox.Items.Cast<string>().Any(x => string.Equals(x, form.SelectedExeName, StringComparison.OrdinalIgnoreCase))) return;
        _exeListBox.Items.Add(form.SelectedExeName);
    }

    private void RemoveSelectedExecutable()
    {
        if (_exeListBox.SelectedItem is null) return;
        _exeListBox.Items.Remove(_exeListBox.SelectedItem);
    }

    private void SaveAndClose()
    {
        _settings.AutoDetectBorderless = _autoDetectBox.Checked;
        _settings.WatchedExecutableNames = _exeListBox.Items.Cast<string>().ToList();
        _settings.StartWithWindows = _startWithWindowsBox.Checked;
        _settings.StartMinimized = _startMinimizedBox.Checked;
        _settings.UpdateMode = (UpdateMode)_updateModeBox.SelectedIndex;
        _store.Save(_settings);
        StartupRegistration.Apply(_settings.StartWithWindows);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
        catch { }
    }
}