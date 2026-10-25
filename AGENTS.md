# AI agent contribution instructions — Vinu Vinu

This file applies to the entire repository. AI-generated or AI-assisted changes are welcome when the human operator has authorized the specific task.

## Repository map

- **Main product:** \`src/VinuVinu/\` — .NET 10 Windows WPF, C# namespaces historically \`SoundSync\`, executable \`VinuVinu.exe\`.
- **Main tests:** \`tests/VinuVinu.Tests/\`.
- **Windows endpoint verifier:** \`tools/VinuVerifier/\`.
- **Old reference prototype:** \`legacy/SyncCast/\`; do not add new features here except scoped maintenance.
- **Primary solution:** \`VinuVinu.slnx\`.
- **CI:** \`.github/workflows/windows-app.yml\`, \`hardware-verifier.yml\` and the separately maintained old-prototype workflow.

## Mandatory safeguards

1. **Do not fabricate verification.** Windows CI proves builds and tests, not physical multi-Bluetooth playback, number of simultaneous A2DP streams or acoustic synchronization. Mark hardware claims unverified until properly measured.
2. Preserve original SoundSync MIT license in \`src/VinuVinu/UPSTREAM_LICENSE\`, the root MIT license, and [docs/UPSTREAM.md](docs/UPSTREAM.md). Attribute copied code and check license compatibility before importing from other repositories. Do not silently pull GPL implementations into an MIT app.
3. Keep UI XAML control names/events and the historical internal C# namespaces intact unless their changes have thorough WPF compilation and regression tests. Avoid changing a working audio pipeline just for stylistic cleanliness.
4. Avoid blocking, sleep, networking calls or heavy allocation in \`AudioEngine\` capture callbacks. Ensure all circular/PCM buffer operations use complete frames, bounded capacity and explicit recovery metrics.
5. Do not log or commit Bluetooth unique identifiers, users' locally stored secrets, audio recordings, streaming tokens, or personal information. Diagnostic exports must remain privacy-minimized.
6. Treat network listener links as bearer credentials. Do not enable unrestricted remote control by default or disable access checks.
7. For any agent-authored pull request, identify files changed, reasoning and tradeoffs, tests genuinely run and not run, existing risks and remaining manual checks. Ask contributors to review before merging.

## Commands (Windows with .NET 10)

~~~powershell
dotnet restore VinuVinu.slnx
dotnet build src/VinuVinu/VinuVinu.csproj -c Release -p:PublishSingleFile=false
dotnet test tests/VinuVinu.Tests/VinuVinu.Tests.csproj -c Release
dotnet run --project tools/VinuVerifier/VinuVerifier.csproj -c Release -- --self-test
~~~

If the agent runs on Linux, it may inspect the files and run platform-independent analyses, but **WPF and actual WASAPI/Bluetooth validation require Windows**. CI results only count after the actual workflow completes successfully.

## Suggested change process

Read [CONTRIBUTING.md](CONTRIBUTING.md) and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), search relevant open issues, make a small reversible branch, add automated edge-case tests, and open a draft PR with evidence. The maintainer and contributors decide what to merge. AGENTS.md is guidance, **not** an autonomous bot or an instruction to contact users.
