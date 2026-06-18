using System.Diagnostics;

namespace WindowlessCursorLock.Ui;

public sealed class AddExeForm : Form
{
    private readonly TabControl _tabs;
    private readonly ListView _processList;
    private readonly TextBox _browsePathBox;
    private readonly Button _okButton;

    public string? SelectedExeName { get; private set; }

    public AddExeForm(Icon? icon)
    {
        Text = "Add Watched Executable";
        Width = 480;
        Height = 420;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        if (icon is not null) Icon = icon;
        _tabs = new TabControl { Dock = DockStyle.Top, Height = 320 };
        TabPage runningTab = new("Running Processes");
        _processList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false
        };
        _processList.Columns.Add("Process", 200);
        _processList.Columns.Add("Window Title", 220);
        _processList.SelectedIndexChanged += (_, _) => UpdateOkButtonState();
        Button refreshButton = new() { Text = "Refresh", Dock = DockStyle.Bottom };
        refreshButton.Click += (_, _) => PopulateRunningProcesses();
        runningTab.Controls.Add(_processList);
        runningTab.Controls.Add(refreshButton);
        TabPage browseTab = new("Browse Disk");
        _browsePathBox = new TextBox { Dock = DockStyle.Top, ReadOnly = true };
        Button browseButton = new() { Text = "Browse...", Dock = DockStyle.Top };
        browseButton.Click += (_, _) => BrowseForExecutable();
        browseTab.Controls.Add(_browsePathBox);
        browseTab.Controls.Add(browseButton);
        _tabs.TabPages.Add(runningTab);
        _tabs.TabPages.Add(browseTab);
        _tabs.SelectedIndexChanged += (_, _) => UpdateOkButtonState();
        _okButton = new Button { Text = "Add", Dock = DockStyle.Bottom, Enabled = false };
        _okButton.Click += (_, _) => ConfirmSelection();
        Button cancelButton = new() { Text = "Cancel", Dock = DockStyle.Bottom, DialogResult = DialogResult.Cancel };
        Controls.Add(_tabs);
        Controls.Add(_okButton);
        Controls.Add(cancelButton);
        PopulateRunningProcesses();
    }

    private void PopulateRunningProcesses()
    {
        _processList.Items.Clear();
        foreach (Process process in Process.GetProcesses().OrderBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase))
            using (process)
            {
                string title = TryGetMainWindowTitle(process);
                if (string.IsNullOrWhiteSpace(title)) continue;
                ListViewItem item = new(process.ProcessName + ".exe");
                item.SubItems.Add(title);
                _processList.Items.Add(item);
            }
    }

    private static string TryGetMainWindowTitle(Process process)
    {
        try { return process.MainWindowTitle; }
        catch (InvalidOperationException) { return string.Empty; }
        catch (System.ComponentModel.Win32Exception) { return string.Empty; }
    }

    private void BrowseForExecutable()
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "Executable files (*.exe)|*.exe",
            Title = "Select an executable"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _browsePathBox.Text = dialog.FileName;
            UpdateOkButtonState();
        }
    }

    private void UpdateOkButtonState() => _okButton.Enabled = _tabs.SelectedIndex == 0 ? _processList.SelectedItems.Count > 0 : !string.IsNullOrWhiteSpace(_browsePathBox.Text);

    private void ConfirmSelection()
    {
        SelectedExeName = _tabs.SelectedIndex == 0 ? _processList.SelectedItems[0].Text : Path.GetFileName(_browsePathBox.Text);
        DialogResult = DialogResult.OK;
        Close();
    }
}