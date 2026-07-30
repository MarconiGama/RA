using System;
using NUnit.Framework;
using UnityEngine;

public sealed class EngagementContentTests
{
    private const string LegacyJson =
        "{\"id\":\"legacy\",\"version\":\"1.0.0\",\"items\":[" +
        "{\"id\":\"letter-a\",\"target\":\"A\",\"title\":\"A\",\"prefabResource\":\"Models/A\"}]}";

    [Test]
    public void Schema1_RemainsValid()
    {
        var pack = ContentRepository.LoadFromJson(LegacyJson);
        Assert.AreEqual("1.0.0", pack.version);
        Assert.AreEqual("A", pack.items[0].EffectiveLetterName);
    }

    [Test]
    public void Schema2_IsValidAndLetterAIsComplete()
    {
        var pack = LoadCurrent();
        Assert.AreEqual("2.0.0", pack.version);
        var item = ContentRepository.FindByTarget(pack, "A");
        Assert.NotNull(item);
        Assert.IsTrue(item.HasExtendedExperience);
        Assert.AreEqual("Arara", item.word);
        Assert.AreEqual("tap", item.interactionType);
        Assert.AreEqual("single-successful-tap", item.completionRule);
        Assert.IsTrue(item.pedagogicalReviewRequired);
    }

    [Test]
    public void Schema2_PreservesAllIdsAndTargets()
    {
        var pack = LoadCurrent();
        Assert.AreEqual(26, pack.items.Length);
        for (var index = 0; index < 26; index++)
        {
            var letter = ((char)('A' + index)).ToString();
            Assert.AreEqual(letter, pack.items[index].target);
            Assert.AreEqual("letter-" + letter.ToLowerInvariant(), pack.items[index].id);
        }
    }

    [Test]
    public void Schema2_LettersBThroughZHaveBasicFallbacks()
    {
        var pack = LoadCurrent();
        for (var index = 1; index < pack.items.Length; index++)
        {
            Assert.IsFalse(pack.items[index].HasExtendedExperience, pack.items[index].target);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pack.items[index].title));
            Assert.IsFalse(string.IsNullOrWhiteSpace(pack.items[index].prefabResource));
        }
    }

    [Test]
    public void MissingOptionalAudio_DoesNotInvalidateContent()
    {
        var pack = ContentRepository.LoadFromJson(LegacyJson);
        pack.items[0].audioResource = string.Empty;
        pack.items[0].narrationLetterResource = string.Empty;
        Assert.DoesNotThrow(() => ContentRepository.Validate(pack));
    }

    private static LearningContentPack LoadCurrent()
    {
        var asset = Resources.Load<TextAsset>("Content/alphabet-pt-br");
        Assert.NotNull(asset);
        return ContentRepository.LoadFromJson(asset.text);
    }
}
