namespace WindowlessCursorLock.Ui;

public sealed class SplashForm : Form
{
    private readonly PictureBox _logoBox;

    public SplashForm(Image? logo, int displayMs = 2000)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.White;
        _logoBox = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.AutoSize,
            Dock = DockStyle.Fill
        };
        if (logo is not null)
        {
            _logoBox.Image = logo;
            ClientSize = logo.Size;
        }
        else
        {
            ClientSize = new Size(300, 200);
            Label label = new()
            {
                Text = "Windowless Cursor Lock",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };
            Controls.Add(label);
        }
        Controls.Add(_logoBox);
        Shown += async (_, _) => await CloseAfterDelay(displayMs);
    }

    private async Task CloseAfterDelay(int delayMs)
    {
        await Task.Delay(delayMs);
        Close();
    }
}