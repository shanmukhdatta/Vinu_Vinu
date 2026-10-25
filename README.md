# Vinu Vinu 🎧

**An open-source Windows application for sharing one PC's audio with multiple output devices.**

[![Windows app CI](https://github.com/shanmukhdatta/Vinu_Vinu/actions/workflows/windows-app.yml/badge.svg)](https://github.com/shanmukhdatta/Vinu_Vinu/actions/workflows/windows-app.yml)
[![Hardware verifier CI](https://github.com/shanmukhdatta/Vinu_Vinu/actions/workflows/hardware-verifier.yml/badge.svg)](https://github.com/shanmukhdatta/Vinu_Vinu/actions/workflows/hardware-verifier.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Vinu Vinu is maintained as a **public, source-first** project by [@shanmukhdatta](https://github.com/shanmukhdatta), with contributions welcome. The primary desktop app is a .NET 10 WPF application built on the MIT-licensed [SoundSync](https://github.com/sugumar247/SoundSync) project by Sugumar S. All upstream attribution and the original MIT notice are preserved in [src/VinuVinu/UPSTREAM_LICENSE](src/VinuVinu/UPSTREAM_LICENSE) and [docs/UPSTREAM.md](docs/UPSTREAM.md).

## Public website and Windows app download

The project website is built from the fully public [website/](website/) folder: original responsive HTML/CSS/JS, no analytics, cookies or external build service. It explains the project, limitations, features, setup and how to contribute. The [GitHub Pages workflow](.github/workflows/pages.yml) publishes it after Pages is configured for the repository.

- **Website after deployment:** https://shanmukhdatta.github.io/Vinu_Vinu/ (check the Pages workflow for live status).
- **Permanent Windows releases:** [GitHub Releases](https://github.com/shanmukhdatta/Vinu_Vinu/releases). Published releases include `VinuVinu-win-x64.zip`, both MIT notices and SHA256 checksums.
- **How publishing works:** [website instructions](website/README.md). Website downloads resolve via the public GitHub Releases API; they fall back to the releases overview rather than inventing an EXE URL when a release is unavailable.
- **Everyday CI artifacts:** [Windows app CI](.github/workflows/windows-app.yml) still tests and publishes temporary build archives; these are different from permanent Releases.

## What the app does

- Lets you select multiple **already connected Windows playback endpoints** (Bluetooth headphones, speakers, USB or wired outputs) and mirror the Windows default playback stream to them.
- Provides per-output volume, manual audio delay, equalizer controls and a gentle **Voice Clarity** preset.
- Offers a sky-blue, rounded desktop interface with Day Sky and Night Sky themes.
- Supports saved speaker groups, Windows endpoint inventory and a local phone/browser audio link (authentication token; remote control is opt-in).
- Displays a **queue recoveries** diagnostic when an output becomes heavily backed up. This is not an acoustic sync measurement.
- Includes [VinuVerifier](tools/VinuVerifier), which checks a known test tone in individual Windows **digital** output loops.

**Limits:** Windows and Bluetooth drivers decide which endpoints can actually run simultaneously. The app does not pair devices, bypass DRM, promise unlimited Bluetooth connections, or guarantee zero-latency synchronized sound. WPF unit tests and CI cannot prove that two physical headsets are playing at the same time. Use the manual hardware test in [docs/TESTING.md](docs/TESTING.md).

## Get started from the source

Requires **Windows 10/11 (x64)**, **.NET 10 SDK**, and Git.

~~~powershell
git clone https://github.com/shanmukhdatta/Vinu_Vinu.git
cd Vinu_Vinu
dotnet restore VinuVinu.slnx
dotnet build src/VinuVinu/VinuVinu.csproj -c Release -p:PublishSingleFile=false
dotnet run --project src/VinuVinu/VinuVinu.csproj -c Release
~~~

Alternatively, run **\`.\scripts\build-windows.ps1\`** in PowerShell to compile and publish the locally built app from source. To run the automated checks: **\`.\scripts\verify-windows.ps1\`**.

Pair headphones in Windows settings *first*, set a suitable default playback device, select additional outputs in Vinu Vinu, then choose **Start Audio Sharing**. Keep volume at a comfortable level. For multiple-device connection problems see [docs/TESTING.md](docs/TESTING.md).

## Project structure

~~~text
Vinu_Vinu/
├── src/VinuVinu/                 Main .NET 10 WPF desktop app
│   ├── Features/                Audio Hub and speaker group UI
│   ├── Models/                  Settings and device models
│   ├── Services/                WASAPI capture, playback, networking, processing
│   ├── ViewModels/               WPF UI state
│   └── UPSTREAM_LICENSE          Original SoundSync MIT notice
├── tests/VinuVinu.Tests/        Unit and regression tests
├── tools/VinuVerifier/          Windows endpoint signal-test tool
├── legacy/                      Original independently authored SyncCast prototype
├── scripts/                     Local build and verification commands
├── docs/                        Architecture, testing, provenance, roadmap
├── .github/                     Windows CI, issues and PR templates
├── AGENTS.md                    Instructions for AI coding agents
├── CONTRIBUTING.md              How to contribute
└── VinuVinu.slnx                Primary solution
~~~

**Compatibility note:** Some existing source code and XAML still use the historical internal C# namespace \`SoundSync\`. The public application and executable are **Vinu Vinu**. Renaming namespaces safely is a separate refactor, not a reason to break the running application.

## Contributing (including AI-assisted work)

Ideas, reproducible bug reports, documentation, tests and code are welcome through **[GitHub Issues](https://github.com/shanmukhdatta/Vinu_Vinu/issues)** and **[Pull Requests](https://github.com/shanmukhdatta/Vinu_Vinu/pulls)**. Read [CONTRIBUTING.md](CONTRIBUTING.md), [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md), and [AGENTS.md](AGENTS.md). AI coding agents can read the public repository and open PRs when authorized by their operators, but a public repo and AGENTS.md alone do **not** automatically send suggestions or grant an agent permissions.

Current priorities: reliable two-output playback, actionable diagnostics, consistent reconnection, measured drift correction, accessibility and contributor-friendly tests. See [docs/ROADMAP.md](docs/ROADMAP.md). Please provide logs and CI links when making performance claims; never describe synthetic runs as verified Bluetooth hardware tests.

## Testing and status

GitHub Actions performs Windows compilation, .NET managed tests and packaging. The prior app version passed **26 automated tests** ([recorded run](https://github.com/shanmukhdatta/Vinu_Vinu/actions/runs/40178237505)); the latest live status is the **Windows app CI** badge at the top. The [hardware verifier](tools/VinuVerifier/README.md) checks digital endpoint loopback; manual listening and external measurements remain necessary for real headphones and timing.

## Licenses and credits

Vinu Vinu, the independent SyncCast prototype and associated changes are distributed under the [MIT License](LICENSE), subject to the original SoundSync MIT notice in [src/VinuVinu/UPSTREAM_LICENSE](src/VinuVinu/UPSTREAM_LICENSE). Do not remove original credits or claim upstream work was authored by the Vinu Vinu maintainer. The app uses [NAudio](https://github.com/naudio/NAudio) and other packages under their respective licenses. Design colors are inspired by friendly cartoon styling; **no official Doraemon character art is included**.

For provenance see [docs/UPSTREAM.md](docs/UPSTREAM.md) and [NOTICE.md](NOTICE.md).
