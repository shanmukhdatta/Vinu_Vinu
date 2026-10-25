# Vinu Vinu — Windows Multi-Device Audio Sharing

**New recommended app:** [Vinu Vinu SoundHub](soundhub/README.md), a Windows .NET 10 WPF application based on the MIT-licensed [SoundSync](https://github.com/sugumar247/SoundSync) project.

- **[Windows build and test automation](.github/workflows/soundhub-windows.yml)** — on each push and pull request, GitHub Actions builds, tests, publishes and uploads a portable Windows x64 EXE.
- **[Download app from a successful workflow run](https://github.com/shanmukhdatta/Vinu_Vinu/actions/workflows/soundhub-windows.yml)** — open the latest successful run, then download the `VinuVinu-win-x64` artifact. Extract it and run `VinuVinu.exe`.
- Features include selecting multiple Windows audio outputs, audio input/output inventory, independent volume and delay (from SoundSync), named speaker groups, and a privacy-minimized diagnostic export.
- **Verified on GitHub Windows CI:** source compilation, 14 managed unit tests passed, and standalone Windows x64 packaging (run [40170010630](https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40170010630)).
- **Not yet verified:** physical two-headset Bluetooth playback, long-term drift, or acoustic synchronization. Ordinary Bluetooth output counts are constrained by Windows drivers and radio hardware.

The earlier independent **SyncCast** prototype remains under [src/SyncCast](src/SyncCast) and has its own [Windows build workflow](.github/workflows/windows-build.yml). Its original documentation is preserved below.

---

# SyncCast — Multi-Device Windows Audio Sharing

> Source-complete **Windows 10/11 desktop prototype**, not a hardware-validated or DRM-compatible product.

Capture one Windows playback endpoint and mirror its audio to any number of **already-connected Windows output endpoints**, including wired headphones, USB headsets, Bluetooth speakers and Bluetooth earbuds. The selected devices each receive the same audio samples rather than a different portion of the track.

## Important: What this can and cannot do

- ✅ Enumerate available Windows render endpoints.
- ✅ Select one source and one or more destinations.
- ✅ Send the same stereo/audio mix to all selected endpoints via WASAPI shared mode.
- ✅ Per-destination volume (0–100%).
- ✅ Per-destination manual startup offset (0–1000 ms, in complete audio frames).
- ✅ Show approximate per-device audio-buffer depths and queue reset counters.
- ✅ Prevent a direct source→same-output feedback loop; limit backlog if one destination falls behind (with a reset counter).
- ⚠️ **Does not pair Bluetooth devices**; Windows must connect/expose them first. Some Bluetooth chipsets/drivers cannot sustain two A2DP outputs concurrently.
- ⚠️ **Does not guarantee zero latency or sample-perfect alignment.** Independent Bluetooth codecs, device buffers, clock drift and RF interference can cause varying delay. Start offset is manual; buffer trimming prevents unbounded delay but can cause a short audible discontinuity if triggered.
- ⚠️ **Does not bypass DRM.** Windows can restrict WASAPI loopback for protected streams, so Netflix and other DRM services may play silence or refuse capture.
- ⚠️ Not automatic live delay estimation, reconnect, tray app or production-grade sync. Requires Windows testing.

## Tech stack

- C# / .NET 8 / WinForms
- [NAudio 2.2.1](https://www.nuget.org/packages/NAudio/2.2.1) (MIT), Windows Core Audio (WASAPI)
- Audio fan-out: `WasapiLoopbackCapture` -> one bounded `BufferedWaveProvider` per output -> format normalization -> `VolumeSampleProvider` -> `OneShotDelaySampleProvider` -> `WasapiOut`.

## Quick-start A: one output is the source

Works without a virtual cable, but the source device plays normally and cannot be delayed by the app.

1. Pair/connect two headsets in Windows Settings > Bluetooth & devices (or connect wired/USB).
2. In Windows Sound settings choose **Headset A** as the Windows default OUTPUT.
3. Play non-DRM audio through Headset A.
4. Launch SyncCast, select Headset A as SOURCE, check Headset B as DESTINATION, click **Start sharing**.
5. You now hear the source output on A, and duplicated audio on B. A is not app-controlled and may not align in time with B.

## Quick-start B: best multi-headphone setup (recommended)

For two or more independent controllable destinations, use a virtual playback endpoint as the source so no physical device plays outside SyncCast.

1. Install the optional **VB-CABLE** driver from the [official vendor](https://vb-audio.com/Cable/index.htm) and restart Windows when prompted.
2. In Windows Settings > System > Sound set **CABLE Input** as the DEFAULT OUTPUT. (The input endpoint is in the playback-device list despite its name.)
3. Start a non-DRM audio track. Windows sends it to the virtual playback endpoint; it is not audible until SyncCast forwards it.
4. In SyncCast select **CABLE Input** as source.
5. Select **Headset A**, **Headset B** and any other physically connected Windows playback devices in the table. Set volume 100% and delay 0ms to start.
6. Click **Start sharing**. If one headset plays ahead of another, STOP, add extra delay to the **faster** headset's row (for example 120ms), and START again. Repeat by ear for acceptable alignment.
7. Before shutting down SyncCast, you may restore the original Windows default output or system audio will keep routing to the cable.

Do not select the same endpoint as source and destination. Source and outputs should be ordinary playback/render endpoints; Bluetooth microphones are not used.

## Building on Windows

Install the **.NET 8 SDK** (or Visual Studio with .NET desktop tools). In PowerShell:

```powershell
cd SyncCast-Windows
.\scripts\build-windows.ps1
```

This restores NuGet dependencies, builds the app and publishes a self-contained `dist/win-x64/SyncCast.exe`. To run in development:

```powershell
dotnet run --project src/SyncCast/SyncCast.csproj
```

No driver installation is done by the source project. Windows may show usual application trust prompts for an unsigned locally-built executable.

## Layout

```text
src/SyncCast/
  Program.cs                           # Windows app entry point
  SyncCast.csproj                      # Project and NAudio dependency
  UI/MainForm.cs                       # Device picker, settings, telemetry
  Audio/AudioDeviceInfo.cs             # Immutable endpoint/config models
  Audio/AudioEngine.cs                 # Loopback capture and audio fan-out
  Audio/AudioSink.cs                   # Independent bounded buffers and outputs
  Audio/OneShotDelaySampleProvider.cs  # Manual startup delay samples
  Audio/BufferPolicy.cs                # Bounded queue reset policy
  Diagnostics/AppLog.cs                # Local troubleshooting logs
tests/SyncCast.SmokeTests/             # Managed audio logic tests
.github/workflows/windows-build.yml    # Windows CI build and artifacts
ANTIGRAVITY_TEST_PROMPT.md             # Detailed local test / repair instructions
scripts/build-windows.ps1             # Restore / build / publish
```

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Only one earbud appears | Verify Windows actually exposes both as *active playback endpoints*; reconnect in Windows Sound settings |
| Silence everywhere | Play an unprotected local file first; verify chosen source is the same Windows output carrying audio |
| Netflix silent | Likely DRM/protected-path restriction; don't attempt to bypass it |
| Mic makes audio poor | Windows Bluetooth HFP headset mode can replace A2DP stereo when the headset microphone is active; disable microphone use |
| Feedback/echo | Make source a virtual cable, not a destination; use headphones rather than closely placed speakers |
| Crackling | Try disabling audio enhancements; reduce other Bluetooth traffic; avoid low latency assumptions |
| Increasing latency | Different hardware output clocks drift; buffers are bounded but this MVP lacks continuous clock-drift correction |

## Engineering roadmap (NOT part of prototype)

1. Real audio delay measurement via calibrated microphone-loopback fixtures, initial offsets, and ongoing drift estimation (per-device clocks).
2. Adaptively resample each sink by a small amount to correct clock drift without buffer-drop pops.
3. Automatic reconnect after Bluetooth sleep/disconnection, safe device hot-plug handling, tray mode, saved presets, signed installer.
4. Optional per-application audio capture using Windows process-loopback APIs on compatible Windows versions, respecting DRM restrictions.
5. Automated Windows integration test harness with loopback fixtures and latency/jitter traces.

## Windows log file

Check `%LOCALAPPDATA%\SyncCast\logs\synccast.log` after running; do not publish the log without reviewing whether it contains your headphone device names.

## Online Windows build

If you put this folder in a GitHub repository, the included GitHub Actions workflow compiles and publishes an executable artifact on a Windows runner and runs managed audio unit/smoke checks. It cannot prove two physical Bluetooth devices are in sync; that still requires real hardware testing. Do not upload copyrighted protected audio or private logs to CI.

## Testing status

The source and file structure were prepared and statically audited in a Linux sandbox. **No Windows WASAPI devices or .NET SDK were available in the Linux container**, so the executable was not compiled, device-tested or benchmarked. Run the PowerShell build on Windows and report any output/driver errors before treating this as tested software.

## Recommended Antigravity workflow

Open this folder in Antigravity and paste the complete contents of `ANTIGRAVITY_TEST_PROMPT.md` into the coding-agent chat. For local validation, run `powershell -ExecutionPolicy Bypass -File .\scripts\verify-windows.ps1` from the project root. This first tests pure audio logic, then compiles and publishes the app.
