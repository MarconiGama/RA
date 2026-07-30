using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class SceneRouteValidationTests
{
    private const string OriginalMenu = "Assets/Scenes/Menu.unity";
    private const string OriginalDestination = "Assets/Scenes/SampleScene.unity";
    private const string PilotMenu = "Assets/Scenes/Menu_EngagementPilot.unity";
    private const string PilotDestination =
        "Assets/Scenes/SampleScene_EngagementPilot.unity";

    [Test]
    public void OriginalMenu_ContinuesPointingToSampleScene()
    {
        AssertMenuRoute(OriginalMenu, "SampleScene");
    }

    [Test]
    public void PilotMenu_PointsToEngagementPilotScene()
    {
        AssertMenuRoute(PilotMenu, "SampleScene_EngagementPilot");
    }

    [Test]
    public void OriginalMenu_HashRemainsValid()
    {
        Assert.AreEqual(
            "10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE",
            Sha256(OriginalMenu));
    }

    [Test]
    public void OriginalSampleScene_HashRemainsValid()
    {
        Assert.AreEqual(
            "042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E",
            Sha256(OriginalDestination));
    }

    [Test]
    public void BaselineBuild_HasResolvableRoute()
    {
        Assert.DoesNotThrow(SceneRouteValidation.ValidateBaselineOrThrow);
    }

    [Test]
    public void EngagementPilotBuild_HasResolvableRoute()
    {
        Assert.DoesNotThrow(SceneRouteValidation.ValidateEngagementPilotOrThrow);
    }

    [Test]
    public void MissingDestination_IsRejected()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            SceneRouteValidation.ValidateRouteDefinitionOrThrow(
                PilotMenu,
                "SceneThatDoesNotExist",
                SceneRouteValidation.GetEngagementPilotScenes()));
    }

    [Test]
    public void EmptyRoute_IsRejected()
    {
        Assert.Throws<System.InvalidOperationException>(() =>
            SceneRouteValidation.ValidateRouteDefinitionOrThrow(
                PilotMenu,
                string.Empty,
                SceneRouteValidation.GetEngagementPilotScenes()));
    }

    [Test]
    public void EngagementPilotBuild_DoesNotContainOriginalSampleScene()
    {
        CollectionAssert.DoesNotContain(
            AndroidBuildPipeline.GetEngagementPilotQaScenes(),
            OriginalDestination);
    }

    [Test]
    public void EngagementPilotBuild_DoesNotContainOriginalMenu()
    {
        CollectionAssert.DoesNotContain(
            AndroidBuildPipeline.GetEngagementPilotQaScenes(),
            OriginalMenu);
    }

    private static void AssertMenuRoute(string path, string expectedDestination)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        SceneChange route = null;
        Button playButton = null;
        for (var index = 0; index < roots.Length; index++)
        {
            var candidateRoutes =
                roots[index].GetComponentsInChildren<SceneChange>(true);
            if (candidateRoutes.Length > 0)
            {
                Assert.IsNull(route, "Mais de um SceneChange foi encontrado.");
                Assert.AreEqual(1, candidateRoutes.Length);
                route = candidateRoutes[0];
            }
            var buttons = roots[index].GetComponentsInChildren<Button>(true);
            for (var buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
            {
                if (buttons[buttonIndex].name == "ButtonPlay")
                {
                    playButton = buttons[buttonIndex];
                }
            }
        }

        Assert.NotNull(route);
        Assert.AreEqual(expectedDestination, route.sceneName);
        Assert.NotNull(playButton);
        Assert.AreEqual(1, playButton.onClick.GetPersistentEventCount());
        Assert.AreSame(route, playButton.onClick.GetPersistentTarget(0));
        Assert.AreEqual("sChange", playButton.onClick.GetPersistentMethodName(0));
    }

    private static string Sha256(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var hash = System.Security.Cryptography.SHA256.Create())
        {
            return System.BitConverter.ToString(hash.ComputeHash(stream))
                .Replace("-", string.Empty);
        }
    }
}
