namespace SyncCast.Audio;

public sealed record AudioDeviceInfo(string Id, string Name)
{
    public override string ToString() => Name;
}

public sealed record OutputSettings(string DeviceId, int VolumePercent = 100, int ExtraDelayMs = 0);

public sealed record SinkStatus(string DeviceId, int BufferedMilliseconds, bool IsPlaying, int ResyncCount);
