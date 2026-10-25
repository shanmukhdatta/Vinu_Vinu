# Source origins and license preservation

Vinu Vinu is maintained by [@shanmukhdatta](https://github.com/shanmukhdatta/Vinu_Vinu). It contains distinct work from more than one origin. **Properly attribute both.**

## Main desktop app

The source at \`src/VinuVinu/\` was originally imported and then substantially adapted from [SoundSync](https://github.com/sugumar247/SoundSync), by **Sugumar S**. The pinned imported upstream commit was \`858f54a5b91051a547b226701225fb7e9c87069b\` (2026-08-26).

The upstream MIT notice has been preserved **unaltered** at [\`src/VinuVinu/UPSTREAM_LICENSE\`](../src/VinuVinu/UPSTREAM_LICENSE). The source was imported as a snapshot, so this repository is **not** a fork preserving SoundSync's original Git history. Consult SoundSync itself for the upstream contribution history. The original app includes NAudio and other dependency code via NuGet under their own licenses.

Vinu Vinu changes include custom branding and rounded sky-blue themes, a speaker-group and endpoint inventory feature, security-friendly remote-control defaults, per-output diagnostics, new tests, an external verifier, packaging and open-source project setup. The current code is not an untouched copy of SoundSync, but should never be described as entirely original.

## Earlier independent prototype

The earlier \`SyncCast\` WinForms proof-of-concept remains under [\`legacy/SyncCast/\`](../legacy/SyncCast/), with its smoke tests. Its root [MIT license](../LICENSE) preserves the \`SyncCast contributors\` notice.

## Attribution rules for contributors

Keep both MIT notices, preserve third-party notices in copied/modified work, and identify the source when borrowing code. Review licenses **before** adopting another repository. In particular, [Snapcast](https://github.com/Snapcast/snapcast) is discussed only as a networking/synchronization architecture reference; its GPL source is **not** imported. Ask maintainers before introducing incompatible third-party dependencies or artwork.

Internal namespaces still using \`SoundSync\` are retained to avoid changing the tested WPF bindings in the same step as the directory reorganization.
