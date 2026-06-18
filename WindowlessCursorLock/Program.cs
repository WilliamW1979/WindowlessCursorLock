using System.Reflection;
using WindowlessCursorLock.Ui;

namespace WindowlessCursorLock;

internal static class Program
{
    private const string FirstRunKey = "Software\\WindowlessCursorLock";
    private const string FirstRunValue = "FirstRunCompleted";

    [STAThread]
    private static void Main()
    {
        using Mutex singleInstanceMutex = new(initiallyOwned: true, "WindowlessCursorLock.SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Windowless Cursor Lock is already running in the system tray.", "Windowless Cursor Lock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        ApplicationConfiguration.Initialize();
        bool isFirstRun = !IsFirstRunCompleted();
        if (isFirstRun)
        {
            ShowSplashScreen();
            MarkFirstRunCompleted();
        }
        Application.Run(new TrayApplicationContext(isFirstRun: isFirstRun));
    }

    private static bool IsFirstRunCompleted()
    {
        using Microsoft.Win32.RegistryKey? key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(FirstRunKey);
        return key?.GetValue(FirstRunValue) is not null;
    }

    private static void MarkFirstRunCompleted()
    {
        using Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(FirstRunKey);
        key.SetValue(FirstRunValue, 1);
    }

    private static void ShowSplashScreen()
    {
        Icon? appIcon = AppIcon.Load();
        Image? logo = LoadSplashLogo();
        using SplashForm splash = new(logo, 5000);
        splash.ShowDialog();
        appIcon?.Dispose();
        logo?.Dispose();
    }

    private static Image? LoadSplashLogo()
    {
        try
        {
            string resourceName = "WindowlessCursorLock.WardShield.png";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            return Image.FromStream(ms);
        }
        catch { return null; }
    }
}