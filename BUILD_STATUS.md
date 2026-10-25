# Build and test status (2026-10-25)

## SoundHub / Vinu Vinu (recommended)

- Windows GitHub Actions run: https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40174419924
- .NET 10 WPF app compiled on GitHub-hosted Windows runner: **passed**, sky-blue UI and Night Sky/Day Sky toggle compiled.
- Three design-regression tests checked named controls, sky-blue theme keys, white-on-blue contrast, and primary action markup: **passed**.
- Automated test suite: **17 passed, 0 failed, 0 skipped**.
- Published self-contained win-x64 executable `VinuVinu.exe`: **passed**.
- Artifact `VinuVinu-win-x64` uploaded: **passed**.
- The artifact has not been run with physical Bluetooth headsets. Real playback, clock drift and acoustic latency remain **unverified**.

## SyncCast legacy prototype

- Original SyncCast application compiled on GitHub-hosted Windows runner: **passed**.
- Managed smoke tests: **passed**.
- Win-x64 publish and artifact upload: **passed** (workflow run 40170010636).
- Physical Bluetooth hardware validation remains **unverified**.

Compiled and unit-tested software is not proof of synchronized physical playback.

## Independent Windows hardware-verifier tool

- [VinuVerifier CI run 40171365667](https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40171365667): Windows compilation and self-contained publish **passed**.
- Five synthetic tone-detector self-tests **passed** (signal detection, amplitude, silence, unrelated tone rejection, stereo float buffer).
- Output available as Actions artifact `VinuVerifier-win-x64`.
- Actual local Bluetooth endpoint tests **await execution on Windows hardware**. The verifier saves a JSON result under `%LOCALAPPDATA%\\VinuVinu\\verification\\`.
