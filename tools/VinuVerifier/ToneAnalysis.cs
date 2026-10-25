using NAudio.Wave;

namespace VinuVerifier;

/// <summary>
/// Analyzes WASAPI endpoint-loopback samples, not microphone or acoustic output.
/// The peak amplitude of a known 997-Hz test tone provides a conservative digital signal check.
/// </summary>
internal sealed class ToneAnalysis
{
    private const double TestFrequency = 997.0;
    private const int WindowSize = 4096;
    private readonly object _gate = new();
    private readonly float[] _window = new float[WindowSize];
    private readonly int _sampleRate;
    private int _position;
    private bool _tonePhase;
    private double _maxBaseline;
    private double _maxTone;
    private int _testWindows;
    private int _strongWindows;
    private long _framesSeen;
    private string? _decodeError;
    public ToneAnalysis(int sampleRate) => _sampleRate = sampleRate;

    public void StartTonePhase()
    {
        lock (_gate)
        {
            _tonePhase = true;
            _position = 0;
        }
    }

    public void AddSamples(ReadOnlySpan<float> mono)
    {
        lock (_gate)
        {
            foreach (var value in mono) AddSampleLocked(value);
        }
    }

    public void AddBuffer(byte[] bytes, int length, WaveFormat format)
    {
        if (format.Channels <= 0 || format.BlockAlign <= 0 || length < 0) return;
        bool floating = format.Encoding == WaveFormatEncoding.IeeeFloat ||
            (format is WaveFormatExtensible ext &&
                ext.SubFormat == new Guid("00000003-0000-0010-8000-00aa00389b71"));
        bool pcm = format.Encoding == WaveFormatEncoding.Pcm ||
            (format is WaveFormatExtensible extensible &&
                extensible.SubFormat == new Guid("00000001-0000-0010-8000-00aa00389b71"));
        int bits = format.BitsPerSample;
        if (!(floating && bits == 32) && !(pcm && (bits == 8 || bits == 16 || bits == 24 || bits == 32)))
        {
            lock (_gate) _decodeError ??= $"Unsupported capture format: {format.Encoding}, {bits} bits";
            return;
        }

        var frames = Math.Min(length, bytes.Length) / format.BlockAlign;
        lock (_gate)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                int offset = frame * format.BlockAlign;
                if (offset + bits / 8 > bytes.Length) break;
                float sample = (floating, bits) switch
                {
                    (true, 32) => BitConverter.ToSingle(bytes, offset),
                    (false, 8) => (bytes[offset] - 128f) / 128f,
                    (false, 16) => BitConverter.ToInt16(bytes, offset) / 32768f,
                    (false, 24) => ToSigned24(bytes, offset) / 8388608f,
                    (false, 32) => BitConverter.ToInt32(bytes, offset) / 2147483648f,
                    _ => 0
                };
                AddSampleLocked(float.IsFinite(sample) ? sample : 0);
            }
        }
    }

    private static int ToSigned24(byte[] b, int offset)
    {
        int raw = b[offset] | b[offset + 1] << 8 | b[offset + 2] << 16;
        return (raw & 0x800000) != 0 ? raw | unchecked((int)0xff000000) : raw;
    }

    private void AddSampleLocked(float sample)
    {
        _framesSeen++;
        _window[_position++] = sample;
        if (_position != WindowSize) return;
        _position = 0;

        // Frequency-specific amplitude estimate from the DFT projection.
        // This checks digital endpoint-loopback delivery; it does NOT measure audible sound.
        double re = 0, im = 0;
        double omega = 2 * Math.PI * TestFrequency / _sampleRate;
        for (int i = 0; i < WindowSize; i++)
        {
            double angle = omega * i;
            re += _window[i] * Math.Cos(angle);
            im -= _window[i] * Math.Sin(angle);
        }
        double amplitude = 2 * Math.Sqrt(re * re + im * im) / WindowSize;
        if (_tonePhase)
        {
            _testWindows++;
            _maxTone = Math.Max(_maxTone, amplitude);
            if (amplitude >= 0.007) _strongWindows++;
        }
        else _maxBaseline = Math.Max(_maxBaseline, amplitude);
    }

    public ToneStats Snapshot()
    {
        lock (_gate)
            return new ToneStats(_maxBaseline, _maxTone, _testWindows,
                _strongWindows, _framesSeen, _decodeError);
    }

    public static string Evaluate(ToneStats data)
    {
        if (data.DecodeError != null) return "UNSUPPORTED_AUDIO_FORMAT";
        if (data.TestWindows < 3) return "NO_LOOPBACK_SAMPLES";
        if (data.MaxToneAmplitude >= Math.Max(0.007, data.MaxBaselineAmplitude * 3 + 0.003)
            && data.StrongWindows >= 3) return "SIGNAL_DETECTED";
        return "SIGNAL_NOT_DETECTED";
    }
}

internal sealed record ToneStats(
    double MaxBaselineAmplitude,
    double MaxToneAmplitude,
    int TestWindows,
    int StrongWindows,
    long FramesSeen,
    string? DecodeError);
