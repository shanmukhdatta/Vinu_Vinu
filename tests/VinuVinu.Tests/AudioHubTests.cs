using System;
using System.IO;
using System.Linq;
using SoundSync.Features;
using SoundSync.Models;
using Xunit;

namespace SoundSync.Tests;

public class AudioHubTests
{
    [Fact]
    public void SpeakerGroups_RoundTripAndCaseInsensitiveReplace()
    {
        var directory = Path.Combine(Path.GetTempPath(), "vinu-tests-" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(directory, "groups.json");
        try
        {
            var store = new SpeakerGroupStore(file);
            Assert.Empty(store.Load());
            store.Upsert("Earbuds", new[] { new SavedDeviceSettings { DeviceId = "A", IsSelected = true, Delay = 40 } });
            Assert.Single(store.Load());
            store.Upsert("earbuds", new[] { new SavedDeviceSettings { DeviceId = "B", IsSelected = true, Delay = 20 } });
            var entry = Assert.Single(store.Load());
            Assert.Equal("earbuds", entry.Name);
            Assert.Equal("B", Assert.Single(entry.Devices).DeviceId);
            store.Delete("EARBUDS");
            Assert.Empty(store.Load());
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public void SpeakerGroups_RejectInvalidNameAndDoNotOverwriteBrokenData()
    {
        var directory = Path.Combine(Path.GetTempPath(), "vinu-tests-" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(directory, "groups.json");
        Directory.CreateDirectory(directory);
        try
        {
            var store = new SpeakerGroupStore(file);
            Assert.Throws<ArgumentException>(() => store.Upsert("  ", Array.Empty<SavedDeviceSettings>()));
            File.WriteAllText(file, "{ not json");
            Assert.Throws<InvalidDataException>(() => store.Upsert("Example", Array.Empty<SavedDeviceSettings>()));
            Assert.Equal("{ not json", File.ReadAllText(file));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void AudioEndpointRows_DoNotContainPrivateDeviceIdentifiers()
    {
        var propertyNames = typeof(AudioEndpointRow).GetProperties().Select(x => x.Name).ToArray();
        Assert.DoesNotContain("DeviceId", propertyNames);
        Assert.DoesNotContain("Id", propertyNames);
    }
}