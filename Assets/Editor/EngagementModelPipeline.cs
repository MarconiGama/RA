using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EngagementModelPipeline
{
    private const string LumiFbx = "Assets/Models/Characters/Lumi/PLACEHOLDER_Lumi.fbx";
    private const string AraraFbx = "Assets/Models/Animals/Arara/PLACEHOLDER_Arara.fbx";
    private const string LumiPrefab = "Assets/Models/Characters/Lumi/Prefabs/PLACEHOLDER_Lumi.prefab";
    private const string AraraPrefab = "Assets/Models/Animals/Arara/Prefabs/PLACEHOLDER_Arara.prefab";
    private const string ResourcesRoot = "Assets/Resources/Engagement";

    private static readonly string[] LumiStates =
    {
        "IdleCalm", "IdleLook", "Enter", "Point", "Listen", "Wave",
        "Encourage", "CelebrateCalm", "Exit", "ReducedMotionIdle"
    };

    private static readonly string[] AraraStates =
    {
        "Idle", "Enter", "WingFlap", "ShortFlight", "Land", "ReducedMotionResponse"
    };

    [MenuItem("RA/Engagement/Build Placeholder Assets")]
    public static void RunBatch()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureModel(LumiFbx);
        ConfigureModel(AraraFbx);

        var lumiGold = CreateMaterial(
            "Assets/Models/Characters/Lumi/Materials/Lumi_Gold.mat",
            new Color(0.95f, 0.56f, 0.12f));
        var lumiDark = CreateMaterial(
            "Assets/Models/Characters/Lumi/Materials/Lumi_Dark.mat",
            new Color(0.12f, 0.08f, 0.05f));
        var araraBlue = CreateMaterial(
            "Assets/Models/Animals/Arara/Materials/Arara_Blue.mat",
            new Color(0.05f, 0.35f, 0.82f));
        var araraWarm = CreateMaterial(
            "Assets/Models/Animals/Arara/Materials/Arara_Warm.mat",
            new Color(0.92f, 0.18f, 0.12f));

        BuildPrefab(LumiFbx, LumiPrefab, "PLACEHOLDER_LUMI", LumiStates, lumiGold, lumiDark, false);
        BuildPrefab(AraraFbx, AraraPrefab, "PLACEHOLDER_ARARA", AraraStates, araraBlue, araraWarm, true);
        BuildPrefab(LumiFbx, ResourcesRoot + "/PLACEHOLDER_Lumi.prefab", "PLACEHOLDER_LUMI", LumiStates, lumiGold, lumiDark, false);
        BuildPrefab(AraraFbx, ResourcesRoot + "/PLACEHOLDER_Arara.prefab", "PLACEHOLDER_ARARA", AraraStates, araraBlue, araraWarm, true);

        GenerateWingFlapWave();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("RA Engagement: PLACEHOLDER Lumi, PLACEHOLDER arara e efeito de asas gerados.");
    }

    private static void ConfigureModel(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("FBX de engagement não importado.", path);
        }
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.isReadable = false;
        importer.meshCompression = ModelImporterMeshCompression.Medium;
        importer.optimizeMeshPolygons = true;
        importer.optimizeMeshVertices = true;
        importer.SaveAndReimport();
    }

    private static Material CreateMaterial(string path, Color color)
    {
        EnsureAssetFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
        var value = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (value == null)
        {
            value = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(value, path);
        }
        value.name = Path.GetFileNameWithoutExtension(path);
        value.color = color;
        if (value.HasProperty("_Metallic")) value.SetFloat("_Metallic", 0f);
        if (value.HasProperty("_Glossiness")) value.SetFloat("_Glossiness", 0.25f);
        EditorUtility.SetDirty(value);
        return value;
    }

    private static void BuildPrefab(
        string fbxPath,
        string prefabPath,
        string assetId,
        string[] states,
        Material primary,
        Material secondary,
        bool interactive)
    {
        EnsureAssetFolder(Path.GetDirectoryName(prefabPath).Replace('\\', '/'));
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (model == null) throw new InvalidOperationException("Modelo ausente: " + fbxPath);

        var root = new GameObject(assetId);
        var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (instance == null) throw new InvalidOperationException("Falha ao instanciar " + fbxPath);
        instance.transform.SetParent(root.transform, false);
        instance.name = assetId + "_MODEL";

        var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
        for (var index = 0; index < renderers.Length; index++)
        {
            var lower = renderers[index].name.ToLowerInvariant();
            var useSecondary = lower.Contains("spot") || lower.Contains("eye") ||
                               lower.Contains("nose") || lower.Contains("warm") ||
                               lower.Contains("head") || lower.Contains("beak") ||
                               lower.Contains("tail_r") || lower.Contains("perch");
            renderers[index].sharedMaterial = useSecondary ? secondary : primary;
        }

        root.AddComponent<PlaceholderMotionController>();
        var descriptor = root.AddComponent<PlaceholderAssetDescriptor>();
        descriptor.assetId = assetId;
        descriptor.animationStates = states;
        descriptor.materialCount = 2;
        descriptor.maxTextureSize = 0;
        descriptor.vertexCount = CountVertices(root);

        if (interactive)
        {
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.5f, 0f);
            collider.size = new Vector3(1.4f, 1.8f, 1.1f);
            root.AddComponent<TapInteraction>();
        }

        try
        {
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static int CountVertices(GameObject root)
    {
        var total = 0;
        var filters = root.GetComponentsInChildren<MeshFilter>(true);
        for (var index = 0; index < filters.Length; index++)
        {
            if (filters[index].sharedMesh != null) total += filters[index].sharedMesh.vertexCount;
        }
        return total;
    }

    private static void GenerateWingFlapWave()
    {
        const int sampleRate = 22050;
        const float seconds = 0.65f;
        var sampleCount = Mathf.RoundToInt(sampleRate * seconds);
        var samples = new short[sampleCount];
        var random = new System.Random(1947);
        for (var index = 0; index < sampleCount; index++)
        {
            var time = index / (float)sampleRate;
            var pulse = Mathf.Sin(time * Mathf.PI * 9f) * Mathf.Exp(-time * 5.5f);
            var noise = ((float)random.NextDouble() * 2f - 1f) * Mathf.Exp(-time * 8f);
            samples[index] = (short)(Mathf.Clamp(pulse * 0.16f + noise * 0.055f, -1f, 1f) * short.MaxValue);
        }

        var assetPath = "Assets/Resources/Audio/Engagement/wing-flap-soft.wav";
        EnsureAssetFolder("Assets/Resources/Audio/Engagement");
        var absolutePath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            assetPath.Replace('/', Path.DirectorySeparatorChar));
        using (var stream = new FileStream(absolutePath, FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(stream))
        {
            var dataBytes = samples.Length * 2;
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataBytes);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataBytes);
            for (var index = 0; index < samples.Length; index++) writer.Write(samples[index]);
        }
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureAssetFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
