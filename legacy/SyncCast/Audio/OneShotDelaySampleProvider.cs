using NAudio.Wave;

namespace SyncCast.Audio;

/// <summary>
/// Adds a fixed offset at stream startup. This is a manual alignment aid, NOT
/// automatic latency measurement or a guarantee of perpetual clock sync.
/// Each output has its own instance.
/// </summary>
internal sealed class OneShotDelaySampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private long _silenceSamplesRemaining;

    public OneShotDelaySampleProvider(ISampleProvider source, int delayMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(delayMilliseconds);
        _source = source;
        // Compute whole sample FRAMES first. Multiplying channels before
        // integer division could produce an odd number of interleaved
        // stereo samples, accidentally shifting the right channel.
        long silenceFrames = (long)source.WaveFormat.SampleRate
            * delayMilliseconds / 1000;
        _silenceSamplesRemaining = silenceFrames * source.WaveFormat.Channels;
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int zeros = (int)Math.Min(count, _silenceSamplesRemaining);
        if (zeros > 0)
        {
            Array.Clear(buffer, offset, zeros);
            _silenceSamplesRemaining -= zeros;
        }
        if (zeros == count) return count;
        return zeros + _source.Read(buffer, offset + zeros, count - zeros);
    }
}
