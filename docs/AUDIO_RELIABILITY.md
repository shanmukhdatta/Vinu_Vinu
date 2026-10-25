# Audio reliability and voice clarity (experimental update)

## What changed

The previous capture callback forced each mirrored output toward **30 milliseconds** by padding silence when short and removing frames when long. That can create discontinuities and waste allocations on busy multi-device setups.

This version uses an original, conservative **high-water queue recovery policy**:

- Buffered output capacity: 300 ms
- Leave normal queue fluctuation alone; never insert artificial silence to maintain an exact queue target
- Only discard old, frame-aligned samples after queued plus incoming audio passes 140 ms, reducing toward 65 ms
- Reuse scratch memory for recovery instead of allocating a fresh byte array every audio packet
- Display **Queue recoveries** in the LIVE STATUS bar while streaming; increasing counts are a diagnostic warning, not a sync measurement

A recovery still discards part of the waveform and can be audible. This is a stability improvement to test, not a claim that clock drift has been solved. True continuous drift correction would need a measured clock difference and controlled adaptive resampling, and needs separate hardware research.

The existing **CLEAR VOCALS** sound preset has been revised to **VOICE CLARITY**: a mild EQ of -3 dB bass, +2 dB mids, -1 dB highs on each chosen mirrored output. It can make dialogue stand out over bass-heavy content, depending on headphones and source. It is **not** microphone denoising, noise cancellation, speech isolation, or a universal fix for dropouts. It does not alter the Windows default source output.

## Open-source design references and licensing

- [NAudio](https://github.com/naudio/NAudio) — Windows WASAPI capture/output and resampling library already used by Vinu Vinu. Existing project targets NAudio **2.3.0**. Moving to NAudio 3 is a separate migration that needs extensive regression testing.
- [Snapcast](https://github.com/Snapcast/snapcast) — example of timestamped, clock-corrected *network* clients. Its GPL-3.0 source was **not** copied or linked into the MIT-based Vinu Vinu app. Snapcast cannot make unsupported classic Bluetooth A2DP endpoints appear in Windows.
- Original [SoundSync](https://github.com/sugumar247/SoundSync) source retained under its MIT license, with local changes documented by this repository.

## Verification levels

1. **Unit tests / Windows CI:** 5 buffer/EQ tests plus 4 more parameterized long-running synthetic cases (1, 2, 4 and 6 simulated independent PCM output queues). These verify frame alignment, bounded buffering and correct configurations, not Bluetooth hardware.
2. **Digital signal check:** [VinuVerifier](../tools/VinuVerifier/README.md) monitors a known tone at the Windows audio endpoints using loopback. It records whether the endpoint's digital audio mix receives the expected signal. It does not prove physical acoustic playback.
3. **Real Windows Bluetooth check:** Pair/activate the specific headsets in Windows. Start with two, then increase one at a time only if Windows exposes them as separate active outputs. Play non-DRM dialogue and music with the usual moderate listening volume. Run for 10 minutes per configuration. Note any crackles, interruptions, connection drops and how much the **Queue recoveries** counter increases. Repeat Voice Clarity on/off. If a headset disappears, preserve diagnostic reports. Never claim a higher output count until tested on actual devices.
4. **Synchronization check:** Record simultaneous acoustic output with a suitable external microphone/reference and compare timing and its drift over time. The app's measured buffer delay is not an acoustic measurement.

## Conservative acceptance criteria for a pilot

- Windows CI compilation and all automated tests pass.
- Two or more actual connected playback endpoints send audible non-DRM sound at the same time.
- No unexpected silence, distortion or disconnections for a user-observed 10-minute test.
- No continuously rising queue recovery count.
- The voice preset changes dialogue intelligibility by listener preference without obvious clipping.
- Exact hardware latency and synchronization are **not verified** until measured.

Keep the previously working build if a physical playback regression is noticed. Record your Windows version, audio driver version, active output count, and VinuVerifier JSON report (review device names before sharing).
