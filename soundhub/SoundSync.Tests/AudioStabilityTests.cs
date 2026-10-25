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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        public void IndependentQueueSimulation_DoesNotOverflowAtSeveralSyntheticClocks(int endpoints)
        {
            // A simulated 50-second PCM stream at 48kHz with output clocks
            // shifted by a few percent. This is NOT a Bluetooth hardware test.
            var providers = new NAudio.Wave.BufferedWaveProvider[endpoints];
            int[] rates = { 1888, 1904, 1920, 1936, 1952, 1968 };
            int[] recoveries = new int[endpoints];
            int packet = B(10);
            var samples = new byte[packet];
            var discard = new byte[B(250)];
            for (int i = 0; i < endpoints; i++)
            {
                providers[i] = new NAudio.Wave.BufferedWaveProvider(Format)
                {
                    BufferDuration = TimeSpan.FromMilliseconds(MirrorBufferPolicy.CapacityMs),
                    ReadFully = true,
                    DiscardOnBufferOverflow = false
                };
            }

            for (int tick = 0; tick < 5000; tick++)
            {
                for (int i = 0; i < endpoints; i++)
                {
                    var queue = providers[i];
                    int trim = MirrorBufferPolicy.BytesToDiscard(queue.BufferedBytes, packet, Format);
                    if (trim > 0)
                    {
                        Assert.True(trim <= discard.Length);
                        queue.Read(discard, 0, trim);
                        recoveries[i]++;
                    }
                    queue.AddSamples(samples, 0, samples.Length);
                    queue.Read(discard, 0, rates[i]);
                    Assert.InRange(queue.BufferedBytes, 0, B(MirrorBufferPolicy.CapacityMs));
                }
            }
            if (endpoints >= 1) Assert.True(recoveries[0] > 0);
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
