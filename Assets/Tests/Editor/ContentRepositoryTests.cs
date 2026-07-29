using NUnit.Framework;

public sealed class ContentRepositoryTests
{
    [Test]
    public void Validate_AcceptsValidPack()
    {
        var pack = new LearningContentPack
        {
            id = "alphabet",
            items = new[]
            {
                new LearningContentItem { id = "letter-a", target = "A" },
                new LearningContentItem { id = "letter-b", target = "B" }
            }
        };

        Assert.DoesNotThrow(() => ContentRepository.Validate(pack));
    }

    [Test]
    public void Validate_RejectsDuplicateTargets()
    {
        var pack = new LearningContentPack
        {
            id = "alphabet",
            items = new[]
            {
                new LearningContentItem { id = "letter-a", target = "A" },
                new LearningContentItem { id = "letter-a-2", target = "A" }
            }
        };

        Assert.Throws<System.InvalidOperationException>(() => ContentRepository.Validate(pack));
    }

    [Test]
    public void Validate_RejectsEmptyItems()
    {
        var pack = new LearningContentPack { id = "alphabet", items = new LearningContentItem[0] };
        Assert.Throws<System.InvalidOperationException>(() => ContentRepository.Validate(pack));
    }
}
