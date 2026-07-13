using NUnit.Framework;

public sealed class ProjectValidationTests
{
    [Test]
    public void RequiredScenes_AreStableAndOrdered()
    {
        var scenes = ProjectValidation.GetRequiredScenes();
        Assert.AreEqual(2, scenes.Length);
        Assert.AreEqual(ProjectValidation.MenuScene, scenes[0]);
        Assert.AreEqual(ProjectValidation.ArScene, scenes[1]);
    }
}
