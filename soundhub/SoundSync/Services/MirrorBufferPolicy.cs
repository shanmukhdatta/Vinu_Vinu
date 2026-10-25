using System;
using NAudio.Wave;

namespace SoundSync.Services
{
    /// <summary>
    /// Bounded queue recovery for independent WASAPI outputs. Unlike a strict
    /// per-callback queue servo, normal jitter is left untouched. Resynchronizing
    /// still discards old audio and can click; it is not clock-drift correction.
    /// </summary>
    public static class MirrorBufferPolicy
    {
        public const int RecoveryThresholdMs = 140;
        public const int RecoveryTargetMs = 65;
        public const int CapacityMs = 300;

        public static int BytesToDiscard(int queuedBytes, int incomingBytes, WaveFormat format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (format.BlockAlign <= 0 || format.AverageBytesPerSecond <= 0)
                throw new ArgumentException("Invalid wave format.", nameof(format));
            if (queuedBytes < 0 || incomingBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(queuedBytes));

            int align = format.BlockAlign;
            long predicted = (long)queuedBytes + incomingBytes;
            long highWater = AlignDown((long)format.AverageBytesPerSecond * RecoveryThresholdMs / 1000, align);
            if (predicted <= highWater) return 0;

            long recoveryTarget = AlignDown((long)format.AverageBytesPerSecond * RecoveryTargetMs / 1000, align);
            long wholeQueuedBytes = AlignDown(queuedBytes, align);
            long requested = AlignDown(Math.Max(0, predicted - recoveryTarget), align);
            return (int)Math.Min(wholeQueuedBytes, requested);
        }

        private static long AlignDown(long value, int frameSize) => value - value % frameSize;
    }
}
