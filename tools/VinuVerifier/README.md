# VinuVerifier: physical-device test companion for Vinu Vinu

**Purpose.** On your own Windows laptop, run a repeatable test of whether Vinu Vinu's 997 Hz test signal reaches the digital WASAPI loopback of every selected Windows audio output. An active endpoint does not necessarily mean Bluetooth connectivity is physically working. Loopback cannot confirm real acoustic audio, hardware latency, multi-headset synchronization or receiver clock drift. Manual "did you hear it" answers are recorded separately.

## How to run

1. Pair Bluetooth headphones/speakers through Windows **Settings > Bluetooth & devices**. Make sure each appears separately in Windows playback settings.
2. Open \`VinuVinu.exe\`. In its main window, tick the outputs you want, then press **START AUDIO SHARING**. Leave it running. Leave the original Windows default output as the source (do not check it as a mirror).
3. Download the GitHub Actions artifact called \`VinuVerifier-win-x64\` from a successful run of **Vinu Vinu hardware verifier**. Extract the archive, then **double-click \`VinuVerifier.exe\`**.
4. The verifier lists active playback devices with numbers. Enter the numbers of the *mirrored* outputs checked in Vinu Vinu (e.g. \`2,3\`). Do not select the default device; the verifier monitors it automatically.
5. Stop other music, set moderate headset volume, confirm the app is routing, and press Enter. The verifier captures a brief baseline, plays a quiet 997 Hz tone for 6 seconds through the default Windows device, and checks the source and each mirror independently.
6. After it plays, answer whether you really **heard** it in each headset. The automated check and manual hearing confirmation are reported separately.
7. Read the output and local JSON report saved under \`%LOCALAPPDATA%\VinuVinu\verification\`. The JSON includes endpoint friendly names and Windows version; review before sharing. There is no raw audio, Bluetooth address, device ID, or authentication token in the report.

## What PASS means

- \`SIGNAL_DETECTED\`: a clear 997 Hz component reached the *Windows audio engine's digital endpoint loopback*. **Not proof** that speakers physically produced it, nor that two Bluetooth devices emitted at the same time.
- \`SIGNAL_NOT_DETECTED\`: no identifiable tone; could be disabled routing, a muted output, volume too low, wrong selected devices, unsupported/changed Windows default source, or Bluetooth driver limitations.
- \`NO_LOOPBACK_SAMPLES\`: no enough active capture audio in the test interval.
- \`CAPTURE_ERROR\` / \`MONITOR_OPEN_FAILED\`: the OS/audio device could not be read. These must not be counted as successful tests.
- \`DIGITAL_ENDPOINT_PASS\`: default source plus all selected mirrors reported signal detected. The user still needs to confirm hearing and, for quantitative sync, use external acoustic recording equipment.

If Vinu Vinu refuses to open multiple Bluetooth endpoints, capture that message or screenshot separately. The verifier cannot force multiple A2DP Bluetooth streams to work on a laptop that does not support them.

## Automated CI

\`\`\`powershell
dotnet run --project tools/VinuVerifier/VinuVerifier.csproj -- --self-test
dotnet publish tools/VinuVerifier/VinuVerifier.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/VinuVerifier
\`\`\`

GitHub Actions runs synthetic signal-detector tests on a Windows runner (no real Bluetooth accessories), compiles the CLI, and publishes a self-contained ZIP you can download. **No remote bot can verify your own physical headphones without a local hardware test or a Windows machine with those devices connected.**
