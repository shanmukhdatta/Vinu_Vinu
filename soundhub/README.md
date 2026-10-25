# Vinu Vinu Audio Hub (Windows)

This is the enhanced [SoundSync](https://github.com/sugumar247/SoundSync) desktop program based on pinned commit `858f54a5b91051a547b226701225fb7e9c87069b` under the MIT license (see `LICENSE` in this directory). It coexists with the earlier independent SyncCast prototype in `src/SyncCast`; do not mix their builds.

## Features

- Show Windows active and inactive playback and recording endpoints, selected output count and Windows default output
- Route the same audio mix from the Windows default playback device to any additional outputs Windows exposes (inherited upstream)
- Save named speaker groups, per-device equalization/delay configuration, and apply groups while stopped
- Export privacy-minimized device and routing diagnostics as JSON
- Reuse SoundSync's independent volume, device switching, local-network streaming and delay control
- Browser remote control starts off, not on, for fresh settings

## Download and run

Go to the Actions tab in `shanmukhdatta/Vinu_Vinu`, select **Vinu Vinu Windows app build and tests**, open a successful run and download the `VinuVinu-win-x64` artifact. Extract the ZIP on Windows 10/11 and run `VinuVinu.exe`. It is an unsigned app, not a signed installer. The executable is only available if Windows CI completes successfully.

## Build locally

Requires Windows and .NET 10 SDK:

```powershell
dotnet restore soundhub/SoundSync/SoundSync.csproj
dotnet test soundhub/SoundSync.Tests/SoundSync.Tests.csproj -c Release
dotnet publish soundhub/SoundSync/SoundSync.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/VinuVinu
```

In Windows Sound Settings, connect and activate the devices first. Play audio through the default output, select additional mirror endpoints in Vinu Vinu, then click Start. The default Windows device is the original output and should not be reselected as a mirror. Open **AUDIO HUB** for input/output diagnostics and saved speaker groups.

## Limitations and tests

The app cannot connect unlimited ordinary Bluetooth devices: adapter/driver limits decide how many Windows render endpoints work simultaneously. Delay sliders do not measure true acoustic latency; endpoint buffers and Bluetooth codecs create drift. DRM audio might not be capturable. GitHub Actions compiles and tests code in Windows and publishes an artifact, but GitHub-hosted CI does **not** verify simultaneous playback using two physical Bluetooth headsets. Real hardware tests remain necessary before treating the product as production-ready. The source is not signed or security-audited.

Upstream attribution: SoundSync copyright (c) 2026 Sugumar S (MIT). The app uses the original source with modifications and does not claim upstream work as its own.
