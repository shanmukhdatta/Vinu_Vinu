# Coding guidance for GitHub Copilot and AI contributors

Follow the root [AGENTS.md](../AGENTS.md) and [CONTRIBUTING.md](../CONTRIBUTING.md) for this public MIT-licensed repository.

- Work in \`src/VinuVinu/\` for the primary WPF .NET 10 app; preserve explicit \`SoundSync\` namespaces and bindings unless refactoring is requested.
- Add tests in \`tests/VinuVinu.Tests/\` and verify with Windows GitHub Actions.
- Keep the original SoundSync author and MIT notice at \`src/VinuVinu/UPSTREAM_LICENSE\`.
- Never make unsupported claims about number of Bluetooth outputs or sample-accurate synchronization.
- Avoid copying GPL-source implementations, hardcoded streaming tokens, personal audio samples or identifying device logs.
- Respect the \`legacy/\` directory as historical source; new features belong in the primary app.
- Describe real test evidence, uncertainties and limitations in every PR. Physical Bluetooth tests need actual hardware.
