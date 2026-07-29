using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AlphabetModelImporter
{
    private const string FbxRoot = "Assets/Models/Alphabet/FBX";
    private const string MaterialRoot = "Assets/Models/Alphabet/Materials";
    private const string PrefabRoot = "Assets/Models/Alphabet/Prefabs";
    private const string ReportPath = "docs/art-pipeline/UNITY_MODEL_REPORT.md";

    private static readonly Color[] Palette =
    {
        new Color(0.12f, 0.48f, 0.92f),
        new Color(0.16f, 0.72f, 0.38f),
        new Color(1.00f, 0.78f, 0.10f),
        new Color(1.00f, 0.42f, 0.10f),
        new Color(0.92f, 0.18f, 0.20f),
        new Color(0.55f, 0.25f, 0.82f),
        new Color(0.95f, 0.28f, 0.58f),
        new Color(0.08f, 0.72f, 0.78f)
    };

    [MenuItem("RA/Art Pipeline/Validate Alphabet Models")]
    public static void ValidateAlphabetModels()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        List<string> errors = CollectValidationErrors();
        GenerateModelReportInternal(errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException("Validação do alfabeto falhou:\n" + string.Join("\n", errors.ToArray()));
        }

        Debug.Log("RA Alphabet: 26 modelos e prefabs aprovados.");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("RA Art Pipeline", "26 modelos e prefabs aprovados.", "OK");
        }
    }

    [MenuItem("RA/Art Pipeline/Configure Alphabet Import")]
    public static void ConfigureAlphabetImport()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (char letter in Letters())
        {
            string path = FbxPath(letter);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException("ModelImporter ausente", path);
            }

            importer.globalScale = 1.0f;
            importer.useFileScale = true;
            importer.importAnimation = false;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMesh = true;
            importer.addCollider = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        Debug.Log("RA Alphabet: importação dos 26 FBX configurada.");
    }

    [MenuItem("RA/Art Pipeline/Generate Alphabet Prefabs")]
    public static void GenerateAlphabetPrefabs()
    {
        EnsureAssetFolder("Assets/Models");
        EnsureAssetFolder("Assets/Models/Alphabet");
        EnsureAssetFolder(MaterialRoot);
        EnsureAssetFolder(PrefabRoot);

        foreach (char letter in Letters())
        {
            string expectedName = Name(letter);
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath(letter));
            if (model == null)
            {
                throw new FileNotFoundException("FBX não importado pelo Unity", FbxPath(letter));
            }

            Material material = CreateOrUpdateMaterial(letter);
            GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException("Não foi possível instanciar " + expectedName);
            }

            try
            {
                instance.name = expectedName;
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                if (renderers.Length != 1)
                {
                    throw new InvalidOperationException(expectedName + " deve possuir exatamente um MeshRenderer.");
                }

                renderers[0].sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath(letter));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("RA Alphabet: 26 prefabs gerados.");
    }

    [MenuItem("RA/Art Pipeline/Generate Model Report")]
    public static void GenerateModelReport()
    {
        GenerateModelReportInternal(CollectValidationErrors());
        Debug.Log("RA Alphabet: relatório salvo em " + ReportPath);
    }

    public static void RunBatch()
    {
        ConfigureAlphabetImport();
        GenerateAlphabetPrefabs();
        ValidateAlphabetModels();
        GenerateModelReport();
    }

    private static Material CreateOrUpdateMaterial(char letter)
    {
        string path = MaterialPath(letter);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new InvalidOperationException("Shader Standard não encontrado.");
            }

            material = new Material(shader);
            material.name = MaterialName(letter);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = Palette[(letter - 'A') % Palette.Length];
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.0f);
        }

        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.35f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static List<string> CollectValidationErrors()
    {
        List<string> errors = new List<string>();
        HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);

        foreach (char letter in Letters())
        {
            string expectedName = Name(letter);
            string fbxPath = FbxPath(letter);
            string prefabPath = PrefabPath(letter);
            string materialPath = MaterialPath(letter);

            FileInfo fbx = new FileInfo(ToAbsolutePath(fbxPath));
            if (!fbx.Exists || fbx.Length <= 0)
            {
                errors.Add(letter + ": FBX ausente ou vazio.");
            }

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (model == null)
            {
                errors.Add(letter + ": FBX não importado.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null || material.name != MaterialName(letter))
            {
                errors.Add(letter + ": material ausente ou incorreto.");
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                errors.Add(letter + ": prefab ausente.");
                continue;
            }

            if (!names.Add(prefab.name) || prefab.name != expectedName)
            {
                errors.Add(letter + ": nome duplicado ou incorreto.");
            }

            if (prefab.transform.localPosition != Vector3.zero ||
                prefab.transform.localRotation != Quaternion.identity ||
                prefab.transform.localScale != Vector3.one)
            {
                errors.Add(letter + ": transformação do prefab não normalizada.");
            }

            MeshFilter[] meshes = prefab.GetComponentsInChildren<MeshFilter>(true);
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            if (meshes.Length != 1 || meshes[0].sharedMesh == null)
            {
                errors.Add(letter + ": prefab deve conter uma mesh.");
            }

            if (renderers.Length != 1 || renderers[0].sharedMaterial == null)
            {
                errors.Add(letter + ": prefab deve conter um material.");
            }

            if (prefab.GetComponentsInChildren<Camera>(true).Length > 0)
            {
                errors.Add(letter + ": câmera encontrada no prefab.");
            }

            if (prefab.GetComponentsInChildren<Light>(true).Length > 0)
            {
                errors.Add(letter + ": luz encontrada no prefab.");
            }

            if (prefab.GetComponentsInChildren<MonoBehaviour>(true).Length > 0)
            {
                errors.Add(letter + ": script desnecessário no prefab.");
            }
        }

        return errors;
    }

    private static void GenerateModelReportInternal(List<string> errors)
    {
        HashSet<string> failures = new HashSet<string>(
            errors.Select(error => error.Substring(0, 1)),
            StringComparer.Ordinal);

        StringBuilder report = new StringBuilder();
        report.AppendLine("# Relatório de modelos do alfabeto no Unity");
        report.AppendLine();
        report.AppendLine("- Gerado em: " + DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture));
        report.AppendLine("- Unity: " + Application.unityVersion);
        report.AppendLine("- Resultado: **" + (errors.Count == 0 ? "APROVADO" : "REPROVADO") + "**");
        report.AppendLine();
        report.AppendLine("| Letra | FBX | Material | Prefab | Mesh | Status |");
        report.AppendLine("|---|---|---|---|---:|---|");

        foreach (char letter in Letters())
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(letter));
            int meshes = prefab == null ? 0 : prefab.GetComponentsInChildren<MeshFilter>(true).Length;
            report.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0} | {1} | {2} | {3} | {4} | {5} |",
                letter,
                AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath(letter)) != null ? "OK" : "FALHA",
                AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(letter)) != null ? "OK" : "FALHA",
                prefab != null ? "OK" : "FALHA",
                meshes,
                failures.Contains(letter.ToString()) ? "REPROVADO" : "APROVADO"));
        }

        if (errors.Count > 0)
        {
            report.AppendLine();
            report.AppendLine("## Falhas");
            report.AppendLine();
            foreach (string error in errors)
            {
                report.AppendLine("- " + error);
            }
        }

        File.WriteAllText(ToAbsolutePath(ReportPath), report.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
    }

    private static IEnumerable<char> Letters()
    {
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            yield return letter;
        }
    }

    private static string Name(char letter)
    {
        return "RA_Letter_" + letter;
    }

    private static string MaterialName(char letter)
    {
        return "RA_Mat_" + letter;
    }

    private static string FbxPath(char letter)
    {
        return FbxRoot + "/" + Name(letter) + ".fbx";
    }

    private static string MaterialPath(char letter)
    {
        return MaterialRoot + "/" + MaterialName(letter) + ".mat";
    }

    private static string PrefabPath(char letter)
    {
        return PrefabRoot + "/" + Name(letter) + ".prefab";
    }

    private static string ToAbsolutePath(string projectRelativePath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        EnsureAssetFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}

