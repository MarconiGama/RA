using System;
using NUnit.Framework;

public sealed class EngagementPersistenceTests
{
    [Test]
    public void CalmProfile_IsDefault()
    {
        var settings = new SensorySettingsService(false);
        Assert.AreEqual(SensoryProfile.Calm, settings.Current.profile);
        Assert.IsTrue(settings.Current.captionsEnabled);
        Assert.IsFalse(settings.Current.ambientEnabled);
        Assert.IsFalse(settings.Current.autoPlayNarration);
    }

    [Test]
    public void MissingAudio_DoesNotBreakLegacyService()
    {
        var service = new AudioService(null);
        Assert.DoesNotThrow(() => Assert.IsFalse(service.Play("Audio/DoesNotExist")));
        Assert.DoesNotThrow(service.Stop);
    }

    [Test]
    public void LegacyBinaryProgress_RemainsReadableByEngagementProgress()
    {
        var profile = "test-" + Guid.NewGuid().ToString("N");
        var service = new ProgressService();
        service.MarkCompleted(profile, "letter-a");
        var progress = service.GetEngagementProgress(profile, "letter-a");
        Assert.IsTrue(service.IsCompleted(profile, "letter-a"));
        Assert.IsTrue(progress.completed);
    }

    [Test]
    public void Telemetry_IsBoundedAndContainsNoPersonalFields()
    {
        var service = new LocalTelemetryService();
        service.ExportAndClear();
        var sensory = SensoryPreferences.Create(SensoryProfile.Calm);
        for (var index = 0; index < LocalTelemetryService.MaxQueueLength + 5; index++)
        {
            service.TrackEngagement(EngagementTelemetryEvents.TargetAcquired, "letter-a", sensory);
        }
        var payload = service.Export();
        Assert.AreEqual(LocalTelemetryService.MaxQueueLength, service.Count);
        StringAssert.DoesNotContain("\"profileId\"", payload);
        StringAssert.DoesNotContain("\"name\"", payload);
        StringAssert.DoesNotContain("\"image\"", payload);
        StringAssert.DoesNotContain("\"video\"", payload);
        StringAssert.DoesNotContain("\"voice\"", payload);
        StringAssert.DoesNotContain("\"location\"", payload);
        StringAssert.DoesNotContain("\"email\"", payload);
        StringAssert.DoesNotContain("\"phone\"", payload);
        StringAssert.DoesNotContain("\"advertisingId\"", payload);
        StringAssert.DoesNotContain("\"biometric\"", payload);
        service.ExportAndClear();
    }
}
