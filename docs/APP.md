# Vinu Vinu desktop app

The public application is built from [src/VinuVinu](../src/VinuVinu). Its C# namespace is still \`SoundSync\` internally for compatibility, but its executable assembly is \`VinuVinu\`.

It uses one Windows default playback endpoint as a capture source and opens independent WASAPI output streams for **additional endpoints already exposed by Windows**. Per-output providers adapt audio format, apply volume, optional EQ, delay and buffer management. The source endpoint plays through Windows and cannot be moved earlier by an app delay control. Audio streaming to a phone/browser is an additional feature with token-based access.

Build from source using the steps in [README.md](../README.md). The application is **not** a pairing service, a driver, a microphone denoiser, or a sample-accurate synchronization system.

See [ARCHITECTURE.md](ARCHITECTURE.md) and [TESTING.md](TESTING.md) for development and verification. Main application improvements must be reflected in the primary solution, Windows CI and contributor documentation.
