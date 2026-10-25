# Community roadmap

This is an invitation to contribute, **not** a set of completed features.

## Good first contributions

- Improve error messages when Windows hides or refuses Bluetooth endpoints.
- Add tests for device disappearance, buffer backlog, sample-rate mismatch and malformed diagnostics.
- Improve keyboard navigation, window scaling, screen-reader labels and theme contrast.
- Document Windows 10/11 audio setup with reproducible troubleshooting instructions.
- Add privacy-safe telemetry **only with explicit opt-in** and a clear data-retention design.

## Areas needing hardware research

- Repeatable two-device, three-device and mixed wired/Bluetooth playback matrices across Windows drivers.
- Audible click/dropout investigation, robust reconnect, stable longer-term buffer tracking.
- Adaptive resampling for independent output clock drift and measured reference timing.
- Network listener streaming reliability with backpressure, bounded memory and proper access controls.
- Clear speech processing with objective tests and opt-in audio processing; don't describe a basic EQ as noise cancellation.

## Before proposing large refactors

Open a GitHub Issue to explain the problem, proposed design, tradeoffs and reproducible acceptance criteria. Keep backward compatibility and clearly label experimental behavior. Preserve the MIT notices; review external code licenses first. Small verified PRs are preferred to large untested rewrites.
