# Windows build and verification status

**Date:** October 25, 2026

## Vinu Vinu — current open-source layout

- [Windows app and test run 40185401698](https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40185401698): **passed**.
- .NET 10 WPF primary app compiled under \`src/VinuVinu/\`; output project now \`VinuVinu.csproj\`.
- .NET managed tests under \`tests/VinuVinu.Tests/\`: **26 passed, 0 failed, 0 skipped**.
- Self-contained Windows x64 application compile/publish: **passed**.
- Original SoundSync MIT notice: retained under \`src/VinuVinu/UPSTREAM_LICENSE\`.
- The project was made the primary root \`VinuVinu.slnx\` source-first project; no official character artwork included.

## Hardware verifier

- [Verifier CI run 40185401620](https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40185401620): **passed**, including five synthetic analyzer checks and Windows packaging.
- No actual Bluetooth devices were attached to GitHub CI; loopback tests on a user's machine are different from acoustic hearing/synchronization measurements.

## Legacy prototype

- [Legacy SyncCast run 40185401622](https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40185401622): **passed** after moving it to \`legacy/\`.
- The legacy prototype is available for reference and not the primary app.

**Outstanding hardware evidence:** Real two-or-more-headset acoustic playback counts, packet-drop artifacts, long-run drift and exact synchronization must be measured on actual Windows hardware. Do not use these CI results as proof of those properties.

Source build and contributor steps live in [README.md](README.md), [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/TESTING.md](docs/TESTING.md).
