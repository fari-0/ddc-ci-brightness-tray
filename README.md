# ddc-ci-brightness-tray

A tiny Windows tray utility that controls the brightness of external monitors over **DDC/CI** — no installation, no dependencies, single ~100 KB executable.

![screenshot-placeholder](docs/screenshot.png)

## Features

- Lives in the system tray with a dynamic icon showing current brightness
- **Right-click** the tray icon to open a flyout panel with a smooth, Windows 11–style slider (amber accent)
- Drag, click anywhere on the track, or use the **mouse wheel** (1 notch = ±1%)
- **Left-click** toggles the app between active and paused (paused = no DDC/CI commands are sent, icon turns gray)
- Footer of the flyout: *"Başlangıçta çalıştır"* toggle (run at Windows startup) and *Çıkış* (exit)
- Monitors are re-scanned automatically on hot-plug / display changes
- Brightness commands are debounced (~120 ms) and dispatched in the background so the UI never blocks

## How it works

The app talks to displays using the VESA **DDC/CI** protocol (VCP code `0x10` – Luminance) via `dxva2.dll`. Only monitors that respond to a luminance query are listed; laptop internal panels do not use this protocol and are skipped automatically.

## Compatibility

- Works with virtually any external monitor connected via HDMI / DisplayPort / DVI from the last ~15+ years (any brand: AOC, LG, Samsung, Dell, BenQ…), as long as **DDC/CI** is enabled in the monitor's OSD menu (on by default for most models)
- Does **not** control laptop built-in panels
- Requires .NET Framework 4.x (preinstalled on Windows 10/11) — nothing else

## Build

Run:

```powershell
.\build.ps1
```

This compiles `bin\BrightnessTray.exe` using the C# compiler that ships with Windows.

## Usage

| Action | Result |
|---|---|
| Right-click tray icon | Open brightness flyout |
| Left-click tray icon | Toggle active / paused |
| Slider drag / track click | Set brightness |
| Mouse wheel over slider | ±1% per notch |
| Esc or click outside | Close flyout |

> Note: UI text is in Turkish. English strings may be added later.

## Project structure

```
src/
├── Infrastructure/NativeMethods.cs   # All Win32 P/Invoke declarations
├── Core/
│   ├── IMonitorBrightness.cs         # Abstraction over one display's brightness
│   ├── DdcCiMonitor.cs               # Adapter: dxva2/DDC-CI implementation
│   ├── MonitorScanner.cs             # Finds DDC/CI-capable displays
│   └── BrightnessService.cs          # Facade: debounce + background dispatch + pause gate
├── UI/
│   ├── BrightnessSlider.cs           # Custom-drawn Windows-style slider
│   ├── FlyoutForm.cs                 # Popup panel view
│   ├── TrayIconView.cs               # NotifyIcon wrapper
│   └── IconPainter.cs                # Tray icon rendering
└── App/
    ├── TrayAppController.cs          # Coordinates tray, state and services
    └── StartupManager.cs             # HKCU Run registry entry
```

## License

[MIT](LICENSE)
