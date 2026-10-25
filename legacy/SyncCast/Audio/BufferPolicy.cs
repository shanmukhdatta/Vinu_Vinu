using NAudio.Wave;

namespace SyncCast.Audio;

/// <summary>
/// Bounds runaway queues on independently clocked output devices. This is
/// deliberately a last-resort resync, not phase alignment or drift correction.
/// </summary>
internal static class BufferPolicy
{
    public const int MaxBufferedMilliseconds = 500;
    public const int TargetBufferedMilliseconds = 120;

    /// <returns>Complete interleaved frames expressed as an aligned byte count.</returns>
    public static int BytesToDiscard(int queuedMilliseconds, WaveFormat format)
    {
        if (queuedMilliseconds < MaxBufferedMilliseconds) return 0;
        if (format.SampleRate <= 0 || format.BlockAlign <= 0)
            throw new ArgumentException("Invalid audio format.", nameof(format));
        long excessMilliseconds = queuedMilliseconds - TargetBufferedMilliseconds;
        long frames = excessMilliseconds * format.SampleRate / 1000;
        long bytes = frames * format.BlockAlign;
        return (int)Math.Min(bytes, int.MaxValue / format.BlockAlign * format.BlockAlign);
    }
}
