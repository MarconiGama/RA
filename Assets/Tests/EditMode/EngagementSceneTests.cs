using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class EngagementSceneTests
{
    private const string SandboxScene = "Assets/Scenes/EngagementSandbox.unity";
    private const string PilotScene = "Assets/Scenes/SampleScene_EngagementPilot.unity";

    [Test]
    public void OriginalScenesRemainByteStable()
    {
        Assert.AreEqual(
            "10FF8686EE7981599C7D15E4B721D38D49B0F9B9D95AD3CC10D9A77B2575B4AE",
            Sha256("Assets/Scenes/Menu.unity"));
        Assert.AreEqual(
            "042393D421736CF73E71A23C44175AB7AE0EC19E66628A6B4A7503F0F525A09E",
            Sha256("Assets/Scenes/SampleScene.unity"));
    }

    [Test]
    public void SandboxContainsNoVuforiaComponents()
    {
        var scene = EditorSceneManager.OpenScene(SandboxScene, OpenSceneMode.Single);
        Assert.IsTrue(scene.IsValid());
        Assert.AreEqual(0, Object.FindObjectsOfType<LegacyVuforiaTargetAdapter>().Length);
        StringAssert.DoesNotContain("bab6fa851cf5a1a4bba3cec5f191cb8e", File.ReadAllText(SandboxScene));
        Assert.AreEqual(1, Object.FindObjectsOfType<EngagementSandboxController>().Length);
        Assert.AreEqual(1, Object.FindObjectsOfType<LearningExperienceController>().Length);
    }

    [Test]
    public void PilotIntegratesOnlyTargetAAndPreservesLegacyObject()
    {
        var scene = EditorSceneManager.OpenScene(PilotScene, OpenSceneMode.Single);
        Assert.IsTrue(scene.IsValid());
        var adapters = Object.FindObjectsOfType<LegacyVuforiaTargetAdapter>();
        Assert.AreEqual(1, adapters.Length);
        Assert.AreEqual("A", adapters[0].TargetName);
        var marker = Object.FindObjectOfType<PilotIntegrationMarker>();
        Assert.NotNull(marker);
        Assert.AreEqual("A", marker.integratedTarget);
        Assert.AreEqual("RaA", marker.disabledLegacyObjectName);
        Assert.IsTrue(marker.originalTransformPreserved);
        Assert.IsFalse(marker.originalObjectDeleted);
        var legacy = FindIncludingInactive(adapters[0].transform, marker.disabledLegacyObjectName);
        Assert.NotNull(legacy);
        Assert.IsFalse(legacy.gameObject.activeSelf);
    }

    private static Transform FindIncludingInactive(Transform root, string name)
    {
        if (root.name == name) return root;
        for (var index = 0; index < root.childCount; index++)
        {
            var found = FindIncludingInactive(root.GetChild(index), name);
            if (found != null) return found;
        }
        return null;
    }

    private static string Sha256(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var hash = System.Security.Cryptography.SHA256.Create())
        {
            return System.BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
