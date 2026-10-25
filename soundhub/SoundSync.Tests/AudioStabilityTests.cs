using System;
using NAudio.Wave;
using SoundSync.Services;
using SoundSync.Models;
using Xunit;

namespace SoundSync.Tests
{
    public sealed class AudioStabilityTests
    {
        private static readonly WaveFormat Format = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
        private static int B(int ms) => (int)((long)Format.AverageBytesPerSecond * ms / 1000);

        [Fact]
        public void NormalJitter_IsNotTrimmed()
        {
            Assert.Equal(0, MirrorBufferPolicy.BytesToDiscard(B(30), B(10), Format));
            Assert.Equal(0, MirrorBufferPolicy.BytesToDiscard(B(100), B(20), Format));
            Assert.Equal(0, MirrorBufferPolicy.BytesToDiscard(B(130), B(10), Format));
        }

        [Fact]
        public void ExcessBacklog_TrimsOnlyWholeOldFrames()
        {
            int trim = MirrorBufferPolicy.BytesToDiscard(B(150), B(20), Format);
            Assert.Equal(B(105), trim);
            Assert.Equal(0, trim % Format.BlockAlign);
            Assert.True(trim <= B(150));
        }

        [Fact]
        public void Recovery_NeverDropsFutureSamples()
        {
            Assert.Equal(B(10), MirrorBufferPolicy.BytesToDiscard(B(10), B(220), Format));
        }

        [Fact]
        public void InvalidInputsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MirrorBufferPolicy.BytesToDiscard(-1, 0, Format));
            Assert.Throws<ArgumentOutOfRangeException>(() => MirrorBufferPolicy.BytesToDiscard(0, -1, Format));
            Assert.Equal(0, MirrorBufferPolicy.BytesToDiscard(0, 0, Format));
        }

        [Fact]
        public void VoicePreset_ChangesEqualizerNotRoutingSettings()
        {
            var device = new DeviceItem { IsSelected = true, Delay = 100 };
            SpeechPreset.Apply(device);
            Assert.Equal(-3f, device.Bass);
            Assert.Equal(2f, device.Mid);
            Assert.Equal(-1f, device.Treble);
            Assert.Equal(100, device.Delay);
            Assert.True(device.IsSelected);
        }
    }
}
