# SysVue

A lightweight Windows system monitor: live CPU, GPU, memory, storage and network readings in one window, built with Avalonia and [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor).

## Features

- **Overview** — a card per component with the key number and a short detail line, updated every second.
- **CPU / GPU / Memory / Storage** — live load and temperature charts, plus a details table (model, cores, cache, clocks, power draw and more, depending on what the hardware exposes).
- **Network** — download and upload speed on one chart, no fill, two colors.
- Runs without administrator rights; elevating unlocks storage temperature and disk model detection.
- Light and dark theme, English and Russian localization.

## Download

Grab the latest build from the [Releases](../../releases) page — a single `SysVue.exe`, no installation required.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build
dotnet run
```

A self-contained single-file build:

```
dotnet publish -p:PublishProfile=win-x64
```

## License

SysVue's own code is licensed under the [MIT License](LICENSE).

The app bundles the **Onest** font (SIL Open Font License 1.1), **Lucide** icons (ISC / MIT) and **LibreHardwareMonitorLib** (Mozilla Public License 2.0). Full license texts and copyright notices are shown in the app's About screen.