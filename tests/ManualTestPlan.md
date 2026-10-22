# SyncCast manual acceptance plan (Windows 10/11)

No lab audio hardware was accessible during project generation. The cases below MUST be run on a Windows PC before claiming the app works on a particular Bluetooth chipset.

## Build test

1. Install the .NET 8 SDK.
2. Run `powershell -ExecutionPolicy Bypass -File .\scripts\build-windows.ps1` from the project root.
3. Confirm `dist/win-x64/SyncCast.exe` exists and launches.

## Functional test matrix

| ID | Source | Destination(s) | Expected behavior | Result |
|---|---|---|---|---|
| A | built-in speakers | wired headset | Same unprotected audio at both; source remains normally audible | Not run |
| B | built-in speakers | BT headset | Secondary audio is audible, but may lag behind source | Not run |
| C | virtual cable | 2 BT headsets | Both selected destinations audible (if Windows exposes both) | Not run |
| D | virtual cable | 1 USB + 1 BT | Both selected destinations audible | Not run |
| E | virtual cable | 3 devices | All three receive the same mono/stereo mix | Not run |
| F | virtual cable | two devices, volume 50/100 | Device at 50% plays quieter, other unchanged | Not run |
| G | virtual cable | two devices, delay 0/250ms | Second stream has ~250ms additional initial delay (not guaranteed audible offset because hardware latency also contributes) | Not run |
| H | source same as destination | same endpoint | UI must never allow this mapping | Static guard |
| I | virtual cable | unplug destination | UI should report a stopped output, but auto-reconnect is not implemented | Not run |
| J | virtual cable | local file, pause/restart | Forwarding resumes; any accumulated buffer drift/dropouts documented | Not run |
| K | virtual cable | protected streaming content | Must acknowledge silence/restriction without bypass | Not run |

## Quality measurements for future development

- Record two audio outputs with an external dual-channel recorder/microphone and cross-correlate sample onset timestamps. An initial 0ms UI offset is NOT evidence of physical synchronicity.
- Measure latency jitter (p50, p95, p99) on each output over at least 30 minutes of playback.
- Measure clock drift between 44.1k and 48k endpoints, Bluetooth reconnect, codec switch to headset telephony mode, video lip-sync and underflow/overflow rates.
- Testing should use short, unprotected test tones first; video streaming services can introduce DRM or app routing restrictions.

## Original unprotected test audio

Run `powershell -ExecutionPolicy Bypass -File .\scripts\generate-test-tone.ps1`, open the generated `test-audio.wav` in a local player, then route the sound to the chosen SyncCast source. Start at a comfortable volume. This test requires no streaming account, network, or protected media. The script creates the file locally; it is not bundled into the source ZIP.

## Diagnostics

App log: `%LOCALAPPDATA%\SyncCast\logs\synccast.log`. The per-device Queue column displays recent buffer depth and how many last-resort queue resets occurred. A nonzero reset counter may correspond to an audible click; it is not evidence of adaptive synchronization.
