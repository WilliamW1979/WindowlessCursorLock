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

## Requirements

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (to build from source)

## License

MIT — see `LICENSE`.
