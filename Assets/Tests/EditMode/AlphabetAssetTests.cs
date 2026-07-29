using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class AlphabetAssetTests
{
    private const string FbxRoot = "Assets/Models/Alphabet/FBX";
    private const string PrefabRoot = "Assets/Models/Alphabet/Prefabs";
    private const string LegacyRoot = "Assets/Objetos";

    [Test]
    public void Alphabet_HasCompleteNormalizedAssetSet()
    {
        HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            string name = "RA_Letter_" + letter;
            string fbxPath = FbxRoot + "/" + name + ".fbx";
            string prefabPath = PrefabRoot + "/" + name + ".prefab";

            Assert.That(File.Exists(Absolute(fbxPath)), "FBX ausente: " + fbxPath);
            Assert.That(new FileInfo(Absolute(fbxPath)).Length, Is.GreaterThan(0), "FBX vazio: " + fbxPath);
            Assert.That(names.Add(name), "Nome duplicado: " + name);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.NotNull(prefab, "Prefab ausente: " + prefabPath);
            Assert.AreEqual(name, prefab.name);
            Assert.AreEqual(Vector3.zero, prefab.transform.localPosition);
            Assert.AreEqual(Quaternion.identity, prefab.transform.localRotation);
            Assert.AreEqual(Vector3.one, prefab.transform.localScale);

            MeshFilter[] meshes = prefab.GetComponentsInChildren<MeshFilter>(true);
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            Assert.AreEqual(1, meshes.Length, name + " deve possuir uma mesh.");
            Assert.NotNull(meshes[0].sharedMesh);
            Assert.AreEqual(1, renderers.Length, name + " deve possuir um renderer.");
            Assert.NotNull(renderers[0].sharedMaterial);
            Assert.AreEqual(0, prefab.GetComponentsInChildren<Camera>(true).Length);
            Assert.AreEqual(0, prefab.GetComponentsInChildren<Light>(true).Length);

            string legacy = LegacyRoot + "/Ra" + letter + ".blend";
            Assert.That(File.Exists(Absolute(legacy)), "Legado não preservado: " + legacy);
        }

        Assert.AreEqual(26, Directory.GetFiles(Absolute(FbxRoot), "RA_Letter_?.fbx").Length);
        Assert.AreEqual(26, Directory.GetFiles(Absolute(PrefabRoot), "RA_Letter_?.prefab").Length);
        Assert.AreEqual(0, Directory.GetFiles(Absolute("Assets/Models"), "*.blend", SearchOption.AllDirectories).Length);
    }

    [Test]
    public void ContentJson_ReferencesEveryGeneratedPrefab()
    {
        TextAsset content = Resources.Load<TextAsset>("Content/alphabet-pt-br");
        Assert.NotNull(content);
        TestContentPack pack = JsonUtility.FromJson<TestContentPack>(content.text);
        Assert.NotNull(pack);
        Assert.NotNull(pack.items);
        Assert.AreEqual(26, pack.items.Length);

        HashSet<string> targets = new HashSet<string>(StringComparer.Ordinal);
        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (TestContentItem item in pack.items)
        {
            Assert.That(ids.Add(item.id), "ID duplicado: " + item.id);
            Assert.That(targets.Add(item.target), "Target duplicado: " + item.target);
            string expected = "Models/Alphabet/Prefabs/RA_Letter_" + item.target;
            Assert.AreEqual(expected, item.prefabResource);
            Assert.That(File.Exists(Absolute("Assets/" + expected + ".prefab")), "Prefab referenciado ausente: " + expected);
        }
    }

    private static string Absolute(string projectRelativePath)
    {
        return Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    [Serializable]
    private sealed class TestContentPack
    {
        public TestContentItem[] items;
    }

    [Serializable]
    private sealed class TestContentItem
    {
        public string id;
        public string target;
        public string prefabResource;
    }
}
