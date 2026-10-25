using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Globalization;
using System.Text.Json;
using VinuVerifier;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.Title = "Vinu Vinu Hardware Verifier";
        if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
            return RunSelfTests();
        if (args.Contains("--help", StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine("Usage: VinuVerifier.exe [--list | --self-test]");
            return 0;
        }

        Console.WriteLine("VINU VINU - LIVE WINDOWS AUDIO ENDPOINT VERIFICATION");
        Console.WriteLine("Checks a test tone in each endpoint's Windows WASAPI digital loopback.");
        Console.WriteLine("This is NOT an acoustic or Bluetooth clock-synchronization measurement.");
        Console.WriteLine();

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            var endpoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .OrderBy(x => x.FriendlyName, StringComparer.CurrentCultureIgnoreCase).ToList();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (endpoints.Count == 0) { Console.WriteLine("No active Windows playback endpoints."); return 2; }

            Console.WriteLine("ACTIVE WINDOWS OUTPUTS (already paired/connected in Windows):");
            for (int i = 0; i < endpoints.Count; i++)
                Console.WriteLine($"  {i + 1,2}. {endpoints[i].FriendlyName}" +
                    (endpoints[i].ID == defaultDevice.ID ? " [DEFAULT SOURCE]" : ""));

            if (args.Contains("--list", StringComparer.OrdinalIgnoreCase)) return 0;
            Console.WriteLine();
            Console.WriteLine("1. Run VinuVinu.exe and START audio sharing using the output checkboxes.");
            Console.WriteLine("2. Select below the SAME mirror outputs checked in Vinu Vinu.");
            Console.WriteLine("3. Stop other music/video while this test runs. Use moderate listening volume.");
            Console.WriteLine("   The verifier plays a quiet 997 Hz tone through Windows' default output.");
            Console.WriteLine("   It NEVER pairs Bluetooth devices or changes your Windows default device.");
            Console.WriteLine();
            Console.Write("Enter mirror device numbers separated by commas (e.g. 2,3): ");
            var line = Console.ReadLine();
            if (line == null) { Console.WriteLine("Input unavailable. Run this interactively."); return 2; }

            var mirrorIndices = new List<int>();
            foreach (var part in line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                    || value < 1 || value > endpoints.Count)
                {
                    Console.WriteLine($"Invalid output number: {part}"); return 2;
                }
                if (endpoints[value - 1].ID == defaultDevice.ID)
                {
                    Console.WriteLine("The default output is already the SOURCE; do not select it as a mirror."); return 2;
                }
                if (!mirrorIndices.Contains(value - 1)) mirrorIndices.Add(value - 1);
            }
            if (mirrorIndices.Count == 0) { Console.WriteLine("Select at least one additional output."); return 2; }

            Console.WriteLine($"Selected: {mirrorIndices.Count} mirror(s) + one default Windows source.");
            Console.Write("Confirm Vinu Vinu audio sharing is running, then press Enter to begin: ");
            if (Console.ReadLine() == null) return 2;

            var report = new VerificationReport
            {
                Source = defaultDevice.FriendlyName,
                AvailablePlaybackEndpoints = endpoints.Count,
                AvailableRecordingEndpoints = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).Count
            };

            var monitored = new List<LoopbackMonitor>();
            try
            {
                var targets = new List<MMDevice> { defaultDevice };
                targets.AddRange(mirrorIndices.Select(i => endpoints[i]));
                foreach (var device in targets)
                {
                    try
                    {
                        var monitor = new LoopbackMonitor(device, device.ID == defaultDevice.ID);
                        monitor.Start();
                        monitored.Add(monitor);
                        Console.WriteLine("Monitoring: " + device.FriendlyName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not monitor {device.FriendlyName}: {ex.Message}");
                        report.Endpoints.Add(new EndpointResult {
                            Endpoint = device.FriendlyName,
                            IsDefault = device.ID == defaultDevice.ID,
                            DigitalSignal = "MONITOR_OPEN_FAILED", CaptureError = ex.Message
                        });
                    }
                }

                Console.WriteLine("Measuring 1 second of baseline (no test tone)...");
                await Task.Delay(TimeSpan.FromSeconds(1.2));
                foreach (var monitor in monitored) monitor.StartTonePhase();

                using var player = new WaveOutEvent { DeviceNumber = -1, DesiredLatency = 130 };
                player.Init(new SineToneProvider().ToWaveProvider());
                Console.WriteLine("Playing 997 Hz for 6 seconds; keep Vinu Vinu connected...");
                player.Play();
                await Task.Delay(TimeSpan.FromSeconds(6));
                player.Stop();
                await Task.Delay(200);
            }
            catch (Exception ex) { report.TestError = ex.Message; }
            finally
            {
                foreach (var monitor in monitored) monitor.Dispose();
                report.Endpoints.AddRange(monitored.Select(m => m.Finish()));
            }

            if (report.TestError != null) Console.WriteLine("Test error: " + report.TestError);
            Console.WriteLine();
            Console.WriteLine("DIGITAL LOOPBACK RESULTS:");
            foreach (var device in report.Endpoints)
            {
                Console.WriteLine($"  {device.Endpoint}");
                Console.WriteLine($"    {device.DigitalSignal}: test-tone peak {device.PeakToneAmplitude:F4}, " +
                    $"baseline {device.BaselineToneAmplitude:F4}, strong windows {device.WindowsWithStrongTone}/{device.WindowsAnalyzed}");
                if (device.CaptureError != null) Console.WriteLine("    Details: " + device.CaptureError);
            }

            var source = report.Endpoints.FirstOrDefault(x => x.IsDefault);
            bool sourceOk = source?.DigitalSignal == "SIGNAL_DETECTED";
            bool allMirrorOk = report.Endpoints.Count(x => !x.IsDefault) == mirrorIndices.Count
                && report.Endpoints.Where(x => !x.IsDefault).All(x => x.DigitalSignal == "SIGNAL_DETECTED");
            if (sourceOk && allMirrorOk) report.OverallResult = "DIGITAL_ENDPOINT_PASS";
            else if (!sourceOk) report.OverallResult = "INCONCLUSIVE_SOURCE_NOT_DETECTED";
            else report.OverallResult = "MIRROR_SIGNAL_NOT_DETECTED";

            Console.WriteLine();
            Console.WriteLine("MANUAL HEARING CONFIRMATION (required for physical sound evidence)");
            foreach (var device in report.Endpoints.Where(x => !x.IsDefault))
            {
                Console.Write($"Did you actually hear the tone on '{device.Endpoint}'? [y/n/skip]: ");
                var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                device.HeardByUser = answer == "y" || answer == "yes" ? "YES" :
                    answer == "n" || answer == "no" ? "NO" : "NOT_CONFIRMED";
            }
            Console.WriteLine("Reminder: even hearing the tone does NOT establish millisecond synchronization.");

            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VinuVinu", "verification");
            Directory.CreateDirectory(directory);
            var reportPath = Path.Combine(directory, "verification-" +
                DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".json");
            File.WriteAllText(reportPath, JsonSerializer.Serialize(report,
                new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine();
            Console.WriteLine("Overall result: " + report.OverallResult);
            Console.WriteLine("Saved JSON diagnostic report: " + reportPath);
            Console.WriteLine("No device IDs, pairing credentials or audio recordings are saved.");
            Console.WriteLine("Share the report only after reviewing endpoint names and system information.");
            Console.WriteLine();
            Console.Write("Press Enter to close...");
            Console.ReadLine();
            return report.OverallResult == "DIGITAL_ENDPOINT_PASS" ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Verification could not start: " + ex.Message);
            return 2;
        }
    }

    private static int RunSelfTests()
    {
        int passed = 0;
        static void Assert(bool ok, string label, ref int count)
        {
            if (!ok) throw new InvalidOperationException("FAILED: " + label);
            Console.WriteLine("PASS: " + label);
            count++;
        }
        try
        {
            var a = new ToneAnalysis(48000);
            a.AddSamples(new float[8192]);
            a.StartTonePhase();
            var sine = new float[48000];
            for (int i = 0; i < sine.Length; i++)
                sine[i] = (float)(0.08 * Math.Sin(2 * Math.PI * 997 * i / 48000));
            a.AddSamples(sine);
            var stats = a.Snapshot();
            Assert(ToneAnalysis.Evaluate(stats) == "SIGNAL_DETECTED",
                "997 Hz target detected", ref passed);
            Assert(stats.MaxToneAmplitude is > 0.070 and < 0.085,
                "997 Hz amplitude estimated accurately", ref passed);

            var missing = new ToneAnalysis(48000);
            missing.StartTonePhase();
            missing.AddSamples(new float[48000]);
            Assert(ToneAnalysis.Evaluate(missing.Snapshot()) == "SIGNAL_NOT_DETECTED",
                "Silence correctly rejected", ref passed);

            var incorrect = new ToneAnalysis(48000);
            incorrect.StartTonePhase();
            var other = new float[48000];
            for (int i = 0; i < other.Length; i++)
                other[i] = (float)(0.08 * Math.Sin(2 * Math.PI * 440 * i / 48000));
            incorrect.AddSamples(other);
            Assert(ToneAnalysis.Evaluate(incorrect.Snapshot()) == "SIGNAL_NOT_DETECTED",
                "Unrelated 440 Hz sound rejected", ref passed);

            var decoded = new ToneAnalysis(48000);
            decoded.StartTonePhase();
            float[] stereo = new float[sine.Length * 2];
            for (int i = 0; i < sine.Length; i++)
            {
                stereo[2 * i] = sine[i];
                stereo[2 * i + 1] = sine[i];
            }
            var bytes = new byte[stereo.Length * sizeof(float)];
            Buffer.BlockCopy(stereo, 0, bytes, 0, bytes.Length);
            decoded.AddBuffer(bytes, bytes.Length, WaveFormat.CreateIeeeFloatWaveFormat(48000, 2));
            Assert(ToneAnalysis.Evaluate(decoded.Snapshot()) == "SIGNAL_DETECTED",
                "Stereo float WASAPI buffer detected", ref passed);

            Console.WriteLine($"All {passed} analyzer self-tests passed (synthetic, no Bluetooth hardware).");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}

internal sealed class SineToneProvider : ISampleProvider
{
    private double _phase;
    private readonly double _increment = 2 * Math.PI * 997 / 48000;
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
    public int Read(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < count; i += 2)
        {
            var sample = (float)(0.09 * Math.Sin(_phase));
            buffer[offset + i] = sample;
            if (i + 1 < count) buffer[offset + i + 1] = sample;
            _phase += _increment;
            if (_phase > 2 * Math.PI) _phase -= 2 * Math.PI;
        }
        return count;
    }
}
