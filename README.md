# Windowless Cursor Lock

Confines your mouse cursor to whichever monitor your chosen game (or any application) currently occupies — so a borderless-windowed game can never lose your cursor to a second monitor mid-fight.

## How it works

Windowless Cursor Lock sits in your system tray. Every 250ms (configurable) it checks which window is currently focused.

By default, **auto-detect** is on: any window that looks borderless-fullscreen (no title bar/resize border, and its bounds match its monitor's full bounds within a small pixel tolerance) gets the cursor confined to that monitor automatically — no setup needed for most games. Maximized normal windows are correctly excluded, since they're a different window state than true borderless-fullscreen.

You can also force-watch specific executables in Settings — useful for an app that auto-detect wouldn't catch (e.g. a windowed-mode game you still want confined). Either condition is enough to trigger confinement.

If you `Win+Shift+Left/Right` the game to a different monitor, the clip follows it automatically on the next check (every 250ms by default). The moment you alt-tab away, the clip is released — your cursor is never restricted system-wide, only while a matching window is focused.

## Features

- System tray app with Settings / Pause / Exit context menu
- Auto-detects borderless-fullscreen windows with no manual configuration
- Optionally force-watch specific executables by picking from currently running processes, or by browsing to an .exe on disk
- Auto-detects which monitor the focused app is on — no manual monitor selection needed
- Optional launch at Windows startup
- Settings persisted to `%AppData%\WindowlessCursorLock\settings.toml`
- **Splash screen** on first run with logo display
- **Automatic updates** from GitHub releases (configurable: notify, auto-download, or auto-install)
- **Proper uninstaller** with option to preserve or remove settings

## Requirements

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (to build from source)

## Building from source

```bash
git clone https://github.com/WilliamW1979/WindowlessCursorLock.git
cd WindowlessCursorLock
dotnet build WindowlessCursorLock.sln -c Release
```

To run directly without installing:

```bash
dotnet run --project src\WindowlessCursorLock\WindowlessCursorLock.csproj
```

## Building the installers

Both installer options publish a self-contained build first:

```bash
dotnet publish src\WindowlessCursorLock\WindowlessCursorLock.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Option 1: EXE installer (Inno Setup)

1. Install [Inno Setup](https://jrsoftware.org/isinfo.php) (free).
2. Open `installer\innosetup\WindowlessCursorLock.iss` in the Inno Setup Compiler and click Build, or run:
   ```bash
   iscc installer\innosetup\WindowlessCursorLock.iss
   ```
3. Output: `installer\innosetup\Output\WindowlessCursorLockSetup.exe`

### Option 2: MSI installer (WiX v4)

1. Install the WiX CLI: `dotnet tool install --global wix`
2. From `installer\wix`, run:
   ```bash
   wix build WindowlessCursorLock.wxs -d PublishDir=..\..\src\WindowlessCursorLock\bin\Release\net10.0-windows\win-x64\publish -arch x64 -o WindowlessCursorLock.msi
   ```

Both installers place the app in `Program Files\Windowless Cursor Lock`, add a Start Menu entry, and register a proper uninstaller. Settings always live in `%AppData%\WindowlessCursorLock`, untouched by uninstall unless you choose otherwise.

## Customizing the Splash Screen

To add a custom splash screen image, place a `splash.png` file in `src\WindowlessCursorLock\`. It will be copied to the output directory and displayed on first run.

## Adding an icon

The project currently uses the default Windows application icon. To add your own, drop a `windowlesscursorlock.ico` into `src\WindowlessCursorLock\`, then add `<ApplicationIcon>windowlesscursorlock.ico</ApplicationIcon>` to the `<PropertyGroup>` in `WindowlessCursorLock.csproj`.

## License

MIT — see `LICENSE`.

## Contributing

PRs welcome. A few known areas for improvement if anyone wants to pick them up:
- Swap the polling loop for `SetWinEventHook` for lower latency / lower CPU usage
- Per-monitor DPI edge cases on mixed-DPI multi-monitor setups
- A proper app icon
- Make the borderless-detection pixel tolerance user-configurable in Settings
