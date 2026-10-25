# Vinu Vinu architecture

## Primary application

\`src/VinuVinu/\` is a .NET 10 WPF program. It is the **production-intended code path**. Its historical internal namespaces use \`SoundSync\` so existing WPF XAML, settings, and service interfaces still match.

Main elements:

- \`MainWindow.xaml(.cs)\`: Windows controls, connection commands, local status, saved profile handling and day/night theme.
- \`Models/DeviceItem.cs\`: Windows endpoint model with individual volume, EQ, delay and status.
- \`Services/AudioEngine.cs\`: capture the default playback mix (process-loopback where supported, WASAPI fallback); fan out to selected render endpoints with \`WasapiOut\`.
- \`Services/FormatAdapter.cs\` and \`Services/Providers/\`: sample-rate/channel format adaptation, sample metering, output EQ, manual delay.
- \`Services/MirrorBufferPolicy.cs\`: bounded PCM queue recovery; avoids per-callback artificial silence padding. A queue reset can be audible, and reported counts are **not physical synchronization metrics**.
- \`Services/SpeechPreset.cs\`: mild playback EQ, not microphone noise cancellation.
- \`Services/AudioEnvironmentWatcher.cs\`: device and Windows session notifications used for recovery.
- \`Services/NetworkStreamer.cs\` and \`LinkAuth.cs\`: local-network stream; treat shared URLs/tokens as credentials. Remote control is opt-in for fresh configurations.
- \`Features/AudioHubFeature.cs\`: endpoint inventory, privacy-minimized JSON export, saved speaker groups.

The **default output remains the source**. Every additional speaker independently buffers and plays its own rendered stream. Consequently, different Bluetooth clocks and codec pipelines can drift apart. Manual delay and a bounded queue do not solve long-term acoustic alignment.

## Companion tool

\`tools/VinuVerifier/\` sends a known test tone to the default Windows output, watches the digital loopback of selected Windows endpoints, records results and asks the user whether each device was audible. It **cannot** measure actual acoustic latency or guarantee Bluetooth sync.

## Reference implementation

\`legacy/SyncCast/\` and \`legacy/SyncCast.SmokeTests/\` contain the older independent WinForms prototype. It is preserved for history and comparison; it is **not** the entry point for new feature contributions.

## Suggested contribution boundaries

Keep audio callbacks predictable and allocation-conscious. Put pure policy in small testable classes. Prefer independent, explicit per-output diagnostics over unverified "supports N devices" counters. Write negative tests for missing/removed endpoints, wrong sample formats, and unstable buffers. Test security-sensitive network changes separately. Preserve upstream notices and never commit captured personal audio, output secrets, or credentials.
