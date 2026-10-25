using NAudio.Wave;
using SyncCast.Audio;

int passed = 0;
void Verify(bool condition, string explanation)
{
    if (!condition) throw new Exception("FAILED: " + explanation);
    Console.WriteLine("PASS: " + explanation);
    passed++;
}

// Delay is measured in complete interleaved frames. At 44.1kHz, 5ms has
// 220 frames, i.e. exactly 440 stereo sample values, never 441 values.
var fake = new ConstantSampleProvider(44100, 2);
var delayed = new OneShotDelaySampleProvider(fake, 5);
var signal = new float[512];
int count = delayed.Read(signal, 0, signal.Length);
Verify(count == 512, "A filled audio request returns its full sample count");
Verify(signal.Take(440).All(v => v == 0f), "Delay emits 220 silent stereo frames");
Verify(signal.Skip(440).All(v => v == 0.75f), "Sound begins on an aligned stereo boundary");

var noDelay = new OneShotDelaySampleProvider(fake, 0);
var direct = new float[256];
noDelay.Read(direct, 0, direct.Length);
Verify(direct.All(v => v == 0.75f), "Zero delay passes sound immediately");

var longDelay = new OneShotDelaySampleProvider(fake, 50);
var frames1 = new float[200];
var frames2 = new float[5000];
longDelay.Read(frames1, 0, frames1.Length);
longDelay.Read(frames2, 0, frames2.Length);
Verify(frames1.All(v => v == 0f), "Long delay remains silent over successive reads");
Verify(frames2.Take(4210).All(v => v == 0f), "Long delay carries silence correctly between reads");
Verify(frames2.Skip(4210).All(v => v == 0.75f), "Long delay eventually forwards sound");

try
{
    _ = new OneShotDelaySampleProvider(fake, -1);
    throw new Exception("Negative delay was accepted");
}
catch (ArgumentOutOfRangeException)
{
    Verify(true, "Negative startup delay is rejected");
}

var fmt = new WaveFormat(48000, 16, 2);
Verify(BufferPolicy.BytesToDiscard(499, fmt) == 0, "Queues below threshold are kept");
int trimmed = BufferPolicy.BytesToDiscard(500, fmt);
Verify(trimmed > 0 && trimmed % fmt.BlockAlign == 0, "Queue trimming respects frame boundaries");
Verify(trimmed == 380 * fmt.AverageBytesPerSecond / 1000,
    "Queue policy drops from 500ms to its 120ms target");
Console.WriteLine($"All {passed} managed smoke tests passed (no hardware required).");

file sealed class ConstantSampleProvider : ISampleProvider
{
    public ConstantSampleProvider(int sampleRate, int channels) =>
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
    public WaveFormat WaveFormat { get; }
    public int Read(float[] buffer, int offset, int count)
    {
        Array.Fill(buffer, 0.75f, offset, count);
        return count;
    }
}
