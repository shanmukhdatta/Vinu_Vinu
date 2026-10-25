using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SyncCast.Audio;

/// <summary>
/// Each destination owns an independent WASAPI stream, ring buffer and volume.
/// No destination ever consumes audio from another destination's buffer.
/// </summary>
internal sealed class AudioSink : IDisposable
{
    private readonly BufferedWaveProvider _buffer;
    private readonly WasapiOut _player;
    private readonly WaveFormat _captureFormat;
    private byte[] _discard = Array.Empty<byte>();
    private bool _disposed;
    private int _resyncCount;

    public string DeviceId { get; }

    public AudioSink(MMDevice device, WaveFormat captureFormat, OutputSettings settings)
    {
        DeviceId = settings.DeviceId;
        _captureFormat = captureFormat;
        _buffer = new BufferedWaveProvider(captureFormat)
        {
            BufferDuration = TimeSpan.FromSeconds(2),
            DiscardOnBufferOverflow = true,
            ReadFully = true
        };

        // NAudio 2.2.1 requires PCM or IEEE-float encoding for ToSampleProvider.
        // WASAPI often reports an Extensible-format wrapper around these bytes.
        IWaveProvider normalized = new FormatOverrideWaveProvider(
            _buffer, captureFormat.AsStandardWaveFormat());

        var volume = new VolumeSampleProvider(normalized.ToSampleProvider())
        {
            Volume = settings.VolumePercent / 100f
        };
        var delayed = new OneShotDelaySampleProvider(volume, settings.ExtraDelayMs);

        // Shared WASAPI output performs sample-rate/channel adaptation when required.
        _player = new WasapiOut(device, AudioClientShareMode.Shared, true, 80);
        try
        {
            _player.Init(delayed.ToWaveProvider());
        }
        catch
        {
            _player.Dispose();
            throw;
        }
    }

    public SinkStatus Status => new(DeviceId,
        (int)_buffer.BufferedDuration.TotalMilliseconds,
        _player.PlaybackState == PlaybackState.Playing,
        _resyncCount);

    public void Start() => _player.Play();

    public void Push(byte[] source, int count)
    {
        if (_disposed || count <= 0) return;

        // Bluetooth devices have independent clocks. An overloaded queue is
        // reduced rather than allowing its latency to grow indefinitely.
        // This may create a click; adaptive resampling is future work.
        int queuedMs = (int)_buffer.BufferedDuration.TotalMilliseconds;
        int bytesToDiscard = BufferPolicy.BytesToDiscard(queuedMs, _captureFormat);
        if (bytesToDiscard > 0)
        {
            bytesToDiscard = Math.Min(bytesToDiscard,
                _buffer.BufferedBytes / _captureFormat.BlockAlign * _captureFormat.BlockAlign);
            if (bytesToDiscard > 0)
            {
                if (_discard.Length < bytesToDiscard)
                    _discard = new byte[bytesToDiscard];
                _buffer.Read(_discard, 0, bytesToDiscard);
                _resyncCount++;
            }
        }
        _buffer.AddSamples(source, 0, count);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _player.Stop(); } catch { /* device may have unplugged */ }
        _player.Dispose();
    }

    private sealed class FormatOverrideWaveProvider : IWaveProvider
    {
        private readonly IWaveProvider _inner;
        public FormatOverrideWaveProvider(IWaveProvider inner, WaveFormat format)
        {
            _inner = inner;
            WaveFormat = format;
            if (inner.WaveFormat.BlockAlign != format.BlockAlign ||
                inner.WaveFormat.SampleRate != format.SampleRate ||
                inner.WaveFormat.Channels != format.Channels)
                throw new InvalidOperationException("Unsupported source format conversion.");
        }
        public WaveFormat WaveFormat { get; }
        public int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    }
}
