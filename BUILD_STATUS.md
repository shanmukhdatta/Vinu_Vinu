# Build and test status

- Prepared complete source and launch/build scripts.
- Checked C# source file structure, project XML files and workflow YAML in Linux.
- .NET SDK is **not installed** in this environment; **compilation not performed** here.
- No Windows WASAPI or Bluetooth endpoints available; **audio playback not tested** here.
- `tests/SyncCast.SmokeTests` provides hardware-independent managed tests. Run those on Windows.
- `.github/workflows/windows-build.yml` enables GitHub-hosted Windows build/smoke checks after pushing to a repository; **no CI run has been executed**.
- `ANTIGRAVITY_TEST_PROMPT.md` details how to build, debug, test and report actual results.

Do not confuse static inspection with a successful compilation, Windows device validation, or Bluetooth synchronization measurement.
