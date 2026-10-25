using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VinuVerifier;

internal sealed class LoopbackMonitor : IDisposable
{
    private readonly WasapiLoopbackCapture _capture;
    private readonly ToneAnalysis _analysis;
    private string? _captureError;
    private bool _started;
    public string DeviceName { get; }
    public bool IsDefault { get; }

    public LoopbackMonitor(MMDevice endpoint, bool isDefault)
    {
        DeviceName = endpoint.FriendlyName;
        IsDefault = isDefault;
        _capture = new WasapiLoopbackCapture(endpoint);
        _analysis = new ToneAnalysis(_capture.WaveFormat.SampleRate);
        _capture.DataAvailable += (_, e) =>
        {
            try { _analysis.AddBuffer(e.Buffer, e.BytesRecorded, _capture.WaveFormat); }
            catch (Exception ex) { _captureError ??= ex.Message; }
        };
        _capture.RecordingStopped += (_, e) =>
        {
            if (e.Exception != null) _captureError ??= e.Exception.Message;
        };
    }

    public void Start() { _capture.StartRecording(); _started = true; }
    public void StartTonePhase() => _analysis.StartTonePhase();

    public EndpointResult Finish()
    {
        var stats = _analysis.Snapshot();
        return new EndpointResult
        {
            Endpoint = DeviceName, IsDefault = IsDefault,
            DigitalSignal = _captureError != null ? "CAPTURE_ERROR" : ToneAnalysis.Evaluate(stats),
            CaptureError = _captureError ?? stats.DecodeError,
            PeakToneAmplitude = Math.Round(stats.MaxToneAmplitude, 5),
            BaselineToneAmplitude = Math.Round(stats.MaxBaselineAmplitude, 5),
            WindowsAnalyzed = stats.TestWindows, WindowsWithStrongTone = stats.StrongWindows,
            CapturedFrames = stats.FramesSeen, HeardByUser = "NOT_ASKED"
        };
    }

    public void Dispose()
    {
        if (_started) { try { _capture.StopRecording(); } catch { } }
        _capture.Dispose();
    }
}

internal sealed class EndpointResult
{
    public string Endpoint { get; set; } = "";
    public bool IsDefault { get; set; }
    public string DigitalSignal { get; set; } = "";
    public string? CaptureError { get; set; }
    public double PeakToneAmplitude { get; set; }
    public double BaselineToneAmplitude { get; set; }
    public int WindowsAnalyzed { get; set; }
    public int WindowsWithStrongTone { get; set; }
    public long CapturedFrames { get; set; }
    public string HeardByUser { get; set; } = "NOT_ASKED";
}

internal sealed class VerificationReport
{
    public string TestName { get; set; } = "Vinu Vinu hardware-in-the-loop endpoint check";
    public DateTimeOffset UtcTime { get; set; } = DateTimeOffset.UtcNow;
    public string ComputerOs { get; set; } = Environment.OSVersion.VersionString;
    public string Source { get; set; } = "";
    public int AvailablePlaybackEndpoints { get; set; }
    public int AvailableRecordingEndpoints { get; set; }
    public int TestToneHz { get; set; } = 997;
    public string Scope { get; set; } =
        "Digital Windows WASAPI loopback only. Cannot prove actual acoustic sound, simultaneous Bluetooth hardware operation or end-to-end synchronization.";
    public List<EndpointResult> Endpoints { get; set; } = new();
    public string OverallResult { get; set; } = "INCONCLUSIVE";
    public string? TestError { get; set; }
}
