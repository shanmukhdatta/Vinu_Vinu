using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SyncCast.Audio;

/// <summary>
/// Capture system audio from ONE render endpoint (WASAPI loopback) and fan it
/// out to N independent render endpoints. Selected outputs cannot include source.
/// </summary>
public sealed class AudioEngine : IDisposable
{
    private readonly object _gate = new();
    private readonly List<AudioSink> _sinks = new();
    private WasapiLoopbackCapture? _capture;
    private volatile bool _running;

    public event Action<string>? Faulted;
    public bool IsRunning => _running;

    public static List<AudioDeviceInfo> GetActiveRenderDevices()
    {
        using var enumerator = new MMDeviceEnumerator();
        return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
            .Select(device => new AudioDeviceInfo(device.ID, device.FriendlyName))
            .OrderBy(device => device.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public void Start(string sourceId, IReadOnlyList<OutputSettings> settings)
    {
        if (_running) throw new InvalidOperationException("Already sharing audio.");
        if (settings.Count == 0) throw new ArgumentException("Choose at least one output.");
        if (settings.Any(x => x.DeviceId == sourceId))
            throw new ArgumentException("Source and destination cannot be the same device (feedback loop).");
        if (settings.GroupBy(x => x.DeviceId).Any(group => group.Count() > 1))
            throw new ArgumentException("The same destination was selected twice.");
        if (settings.Any(x => x.VolumePercent is < 0 or > 100 || x.ExtraDelayMs is < 0 or > 1000))
            throw new ArgumentException("Volume must be 0–100; extra delay must be 0–1000ms.");

        using var enumerator = new MMDeviceEnumerator();
        var capture = new WasapiLoopbackCapture(enumerator.GetDevice(sourceId));
        try
        {
            foreach (var settingsItem in settings)
            {
                var sink = new AudioSink(enumerator.GetDevice(settingsItem.DeviceId),
                                         capture.WaveFormat, settingsItem);
                _sinks.Add(sink);
            }
            _capture = capture;
            capture.DataAvailable += OnDataAvailable;
            capture.RecordingStopped += OnRecordingStopped;
            // Start every destination before capturing, so they have a common
            // approximate start boundary. Hardware output clocks remain independent.
            foreach (var sink in _sinks) sink.Start();
            _running = true;
            capture.StartRecording();
        }
        catch
        {
            _running = false;
            capture.DataAvailable -= OnDataAvailable;
            capture.RecordingStopped -= OnRecordingStopped;
            try { capture.StopRecording(); } catch { /* capture not started */ }
            capture.Dispose();
            _capture = null;
            lock (_gate)
            {
                foreach (var sink in _sinks) sink.Dispose();
                _sinks.Clear();
            }
            throw;
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_running) return;
        try
        {
            lock (_gate)
            {
                if (!_running) return;
                foreach (var sink in _sinks) sink.Push(e.Buffer, e.BytesRecorded);
            }
        }
        catch (Exception exception)
        {
            Faulted?.Invoke($"Audio forwarding error: {exception.Message}");
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (_running)
            Faulted?.Invoke(e.Exception == null
                ? "Audio capture stopped. Check the source audio device."
                : $"Audio capture stopped: {e.Exception.Message}");
    }

    public IReadOnlyList<SinkStatus> GetStatuses()
    {
        lock (_gate) return _sinks.Select(sink => sink.Status).ToArray();
    }

    public void Stop()
    {
        if (!_running && _capture == null && _sinks.Count == 0) return;
        _running = false;
        var capture = _capture;
        _capture = null;
        if (capture != null)
        {
            capture.DataAvailable -= OnDataAvailable;
            capture.RecordingStopped -= OnRecordingStopped;
            try { capture.StopRecording(); } catch { /* removed device */ }
            capture.Dispose();
        }
        lock (_gate)
        {
            foreach (var sink in _sinks) sink.Dispose();
            _sinks.Clear();
        }
    }

    public void Dispose() => Stop();
}
