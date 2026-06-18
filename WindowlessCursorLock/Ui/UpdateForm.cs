using System.Diagnostics;
using WindowlessCursorLock.Updates;

namespace WindowlessCursorLock.Ui;

public sealed class UpdateForm : Form
{
    private readonly UpdateInfo _update;
    private readonly Label _titleLabel;
    private readonly Label _infoLabel;
    private readonly Button _downloadButton;
    private readonly Button _viewButton;
    private readonly Button _closeButton;

    public UpdateForm(UpdateInfo update)
    {
        _update = update;
        Text = "Update Available";
        Width = 400;
        Height = 200;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        _titleLabel = new Label
        {
            Text = $"Version {update.LatestVersion} is available",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Padding(12, 12, 0, 0)
        };
        _infoLabel = new Label
        {
            Text = $"You are currently running version {update.CurrentVersion}.\n\nChoose an option below:",
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(12, 0, 12, 0)
        };
        FlowLayoutPanel buttonPanel = new()
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(12)
        };
        _downloadButton = new Button { Text = "Download Installer", Width = 130 };
        _downloadButton.Click += (_, _) => DownloadInstaller();
        _viewButton = new Button { Text = "View Release Notes", Width = 130 };
        _viewButton.Click += (_, _) => ViewReleaseNotes();
        _closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel };
        buttonPanel.Controls.Add(_downloadButton);
        buttonPanel.Controls.Add(_viewButton);
        buttonPanel.Controls.Add(_closeButton);
        Controls.Add(_infoLabel);
        Controls.Add(_titleLabel);
        Controls.Add(buttonPanel);
        AcceptButton = _downloadButton;
        CancelButton = _closeButton;
    }

    private void DownloadInstaller()
    {
        if (string.IsNullOrEmpty(_update.InstallerUrl))
        {
            ViewReleaseNotes();
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _update.InstallerUrl,
                UseShellExecute = true
            });
        }
        catch { MessageBox.Show("Could not open the download URL. Please try downloading from the release page.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ViewReleaseNotes()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _update.ReleaseNotesUrl,
                UseShellExecute = true
            });
        }
        catch { MessageBox.Show("Could not open the release notes URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}