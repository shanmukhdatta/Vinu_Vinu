# Testing and verification

## Automated Windows validation

Use Windows 10/11 with the .NET 10 SDK:

~~~powershell
dotnet restore VinuVinu.slnx
dotnet build src/VinuVinu/VinuVinu.csproj -c Release -p:PublishSingleFile=false
dotnet test tests/VinuVinu.Tests/VinuVinu.Tests.csproj -c Release
dotnet run --project tools/VinuVerifier/VinuVerifier.csproj -c Release -- --self-test
~~~

Or run \`.\scripts\verify-windows.ps1\`. Pull requests automatically run the [Windows app workflow](../.github/workflows/windows-app.yml) and [verifier workflow](../.github/workflows/hardware-verifier.yml). CI tests use simulated audio; **no Bluetooth headsets are attached**.

## Manual Windows endpoint test

1. Connect a wired/USB headset and Bluetooth output using Windows Settings. Verify that each is an **active playback endpoint** in Sound settings.
2. Start Vinu Vinu, select the additional endpoint as an output, and click **Start Audio Sharing**.
3. Play ordinary non-DRM dialogue and music at a reasonable volume. Confirm audible sound from the default source and each selected output.
4. Test **Voice Clarity** against the **Reset Flat** preset. This applies only to mirrored outputs.
5. Observe **Queue recoveries** while playing for at least 10 minutes. Record the starting and final counts and any clicks, silence, distortion or disconnections.
6. Run \`VinuVerifier.exe\` for a digital signal report, then confirm actual hearing. Its report is stored under \`%LOCALAPPDATA%\VinuVinu\verification\`. Review device-friendly names/OS details before sharing.
7. Add additional devices **one at a time**, only if Windows exposes them as usable separate endpoints. Note actual model, Windows driver, output count and whether playback remained stable.

For synchronization measurements, use external simultaneous acoustic measurement across endpoints and track changes in delay over time. Windows-buffer estimates, synthetic test output and loopback tests cannot establish exact emitted-audio timing.

## Do not confuse these evidence levels

- **Build passed**: code compiles on Windows.
- **Unit tests passed**: tested software logic behaved as expected under inputs provided.
- **Digital endpoint passed**: Windows output mix contains the test tone.
- **Manual hearing passed**: someone genuinely heard it on the specified hardware.
- **Acoustic synchronization passed**: measured with suitable external recording equipment and documented procedures.

A contributor should always state **which** was tested. Do not claim tests, headsets or results that weren't actually used. For the optional old prototype, use \`.\scripts\build-legacy.ps1\`.
