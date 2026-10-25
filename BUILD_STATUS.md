# Build and test status (2026-10-25)

## SoundHub / Vinu Vinu (recommended)

- Windows GitHub Actions run: https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40170010630
- .NET 10 WPF app compiled on GitHub-hosted Windows runner: **passed**.
- Automated test suite: **14 passed, 0 failed, 0 skipped**.
- Published self-contained win-x64 executable `VinuVinu.exe`: **passed**.
- Artifact `VinuVinu-win-x64` uploaded: **passed**.
- The artifact has not been run with physical Bluetooth headsets. Real playback, clock drift and acoustic latency remain **unverified**.

## SyncCast legacy prototype

- Original SyncCast application compiled on GitHub-hosted Windows runner: **passed**.
- Managed smoke tests: **passed**.
- Win-x64 publish and artifact upload: **passed** (workflow run 40170010636).
- Physical Bluetooth hardware validation remains **unverified**.

Compiled and unit-tested software is not proof of synchronized physical playback.
