# Contributing to Vinu Vinu

Thank you for helping to build an accessible, reliable and genuinely open-source Windows audio-sharing tool.

Vinu Vinu is a public MIT-licensed project maintained by [@shanmukhdatta](https://github.com/shanmukhdatta). Both human and AI-assisted contributions are welcome through regular GitHub issues and PRs. Contributors retain attribution through Git history. The main app derives from SoundSync; its original author and MIT notice must remain credited.

## Before starting

- Search existing [issues](https://github.com/shanmukhdatta/Vinu_Vinu/issues) and [pull requests](https://github.com/shanmukhdatta/Vinu_Vinu/pulls).
- For a new feature or large architectural change, open a proposal issue first.
- For bugs, provide exact Windows version, device types, expected versus actual behavior, relevant user-visible messages and reproducible steps. **Redact endpoint IDs, tokens and personal information.**
- Read [ARCHITECTURE.md](docs/ARCHITECTURE.md), [TESTING.md](docs/TESTING.md), [ROADMAP.md](docs/ROADMAP.md) and [AGENTS.md](AGENTS.md) if using coding assistants.

## Work from the source

1. Fork the repository and create a topic branch from \`main\`.
2. Make one logically scoped change in \`src/VinuVinu/\`, or update docs/tests where appropriate.
3. Add a focused regression test for changed behavior; mock or simulate errors rather than requiring a Bluetooth headset in automated CI.
4. Build and test **on Windows with the .NET 10 SDK**:

   ~~~powershell
   dotnet restore VinuVinu.slnx
   dotnet build src/VinuVinu/VinuVinu.csproj -c Release -p:PublishSingleFile=false
   dotnet test tests/VinuVinu.Tests/VinuVinu.Tests.csproj -c Release
   dotnet run --project tools/VinuVerifier/VinuVerifier.csproj -c Release -- --self-test
   ~~~

5. Open a PR using the provided template. Include test commands and actual results, a clear risk statement and screenshots for visual changes (with personal information removed). CI needs to be green before merge.

## Special policies

- **Do not invent test results.** Managed tests are not physical Bluetooth or acoustic-synchronization tests.
- **Do not upload captured audio**, bearer tokens, private driver/device identifiers or unsanitized logs.
- **Preserve MIT notices and attribution.** Check third-party license compatibility before copying code. Please do not paste incompatible GPL material.
- For network features, preserve access-token checks, remote-control opt-in and bounded buffering.
- For WPF features, support keyboard navigation, high contrast, readable scaling, and preserve working XAML handlers.
- Legacy SyncCast stays under \`legacy/\` as reference material.

The project maintainer may ask for smaller PRs, additional tests or changes to unsafe proposals. By submitting a PR, you agree your contribution may be distributed under this repository's MIT license along with required third-party notices.

For community expectations see [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). For sensitive reports see [SECURITY.md](SECURITY.md).
