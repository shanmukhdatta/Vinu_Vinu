using System;
using SoundSync.Models;

namespace SoundSync.Services
{
    /// <summary>
    /// Mild dialogue equalization for mirrored playback endpoints.
    /// Not microphone denoising, ANC, or a guarantee of intelligibility.
    /// </summary>
    public static class SpeechPreset
    {
        public const float BassDb = -3.0f;
        public const float MidDb = 2.0f;
        public const float TrebleDb = -1.0f;

        public static void Apply(DeviceItem device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            device.Bass = BassDb;
            device.Mid = MidDb;
            device.Treble = TrebleDb;
        }
    }
}
