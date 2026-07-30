using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneRouteValidation
{
    public const string EngagementPilotMenuScene =
        "Assets/Scenes/Menu_EngagementPilot.unity";
    public const string EngagementPilotDestinationScene =
        "Assets/Scenes/SampleScene_EngagementPilot.unity";

    [MenuItem("RA/Engagement/Create Dedicated Pilot Menu")]
    public static void CreateEngagementPilotMenu()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EngagementPilotMenuScene) != null ||
            File.Exists(EngagementPilotMenuScene))
        {
            throw new InvalidOperationException(
                "Menu piloto já existe; CreateNew preservado: " + EngagementPilotMenuScene);
        }
        if (!AssetDatabase.CopyAsset(ProjectValidation.MenuScene, EngagementPilotMenuScene))
        {
            throw new IOException("Falha ao copiar o menu original para o piloto.");
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ReplaceSingleUtf8Literal(
            EngagementPilotMenuScene,
            "  sceneName: SampleScene",
            "  sceneName: SampleScene_EngagementPilot");
        AssetDatabase.ImportAsset(
            EngagementPilotMenuScene, ImportAssetOptions.ForceSynchronousImport);
        ValidateEngagementPilotOrThrow();
        Debug.Log("RA Scene Route: menu piloto criado e validado.");
    }

    public static string[] GetBaselineScenes()
    {
        return new[] { ProjectValidation.MenuScene, ProjectValidation.ArScene };
    }

    public static string[] GetEngagementPilotScenes()
    {
        return new[] { EngagementPilotMenuScene, EngagementPilotDestinationScene };
    }

    public static void ValidateBaselineOrThrow()
    {
        ValidateProfileOrThrow("BASELINE", GetBaselineScenes());
    }

    public static void ValidateEngagementPilotOrThrow()
    {
        ValidateProfileOrThrow("ENGAGEMENT_PILOT", GetEngagementPilotScenes());
    }

    public static void ValidateRouteDefinitionOrThrow(
        string sourceScenePath,
        string requestedScene,
        string[] buildScenes)
    {
        if (string.IsNullOrWhiteSpace(requestedScene))
        {
            throw new InvalidOperationException("SceneChange.sceneName não pode estar vazio.");
        }
        if (buildScenes == null || buildScenes.Length == 0)
        {
            throw new InvalidOperationException("O perfil de build não possui cenas.");
        }

        var sourceName = Path.GetFileNameWithoutExtension(sourceScenePath);
        if (string.Equals(sourceName, requestedScene, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("A rota não pode apontar para a própria cena.");
        }

        var destinationMatches = 0;
        for (var index = 0; index < buildScenes.Length; index++)
        {
            var candidate = buildScenes[index];
            if (string.Equals(
                Path.GetFileNameWithoutExtension(candidate),
                requestedScene,
                StringComparison.Ordinal))
            {
                if (!File.Exists(candidate))
                {
                    throw new FileNotFoundException(
                        "A cena de destino da rota não existe.", candidate);
                }
                destinationMatches++;
            }
        }
        if (destinationMatches != 1)
        {
            throw new InvalidOperationException(
                "A rota deve resolver exatamente uma cena do perfil; observado: " +
                destinationMatches + ", destino: " + requestedScene);
        }
    }

    private static void ValidateProfileOrThrow(string profile, string[] buildScenes)
    {
        if (buildScenes == null || buildScenes.Length != 2)
        {
            throw new InvalidOperationException(
                profile + " deve declarar exatamente duas cenas.");
        }

        var uniquePaths = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < buildScenes.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(buildScenes[index]) || !File.Exists(buildScenes[index]))
            {
                throw new FileNotFoundException(
                    profile + " contém cena ausente.", buildScenes[index]);
            }
            if (!uniquePaths.Add(buildScenes[index]))
            {
                throw new InvalidOperationException(profile + " contém cena duplicada.");
            }
        }

        for (var index = 0; index < buildScenes.Length; index++)
        {
            var scene = EditorSceneManager.OpenScene(buildScenes[index], OpenSceneMode.Single);
            var routes = FindRoutes(scene);
            for (var routeIndex = 0; routeIndex < routes.Count; routeIndex++)
            {
                var route = routes[routeIndex];
                ValidateRouteDefinitionOrThrow(
                    buildScenes[index], route.sceneName, buildScenes);
                ValidatePersistentButtonRouteOrThrow(scene, route);
            }
        }

        Debug.Log("RA Scene Route: perfil validado: " + profile);
    }

    private static List<SceneChange> FindRoutes(Scene scene)
    {
        var routes = new List<SceneChange>();
        var roots = scene.GetRootGameObjects();
        for (var index = 0; index < roots.Length; index++)
        {
            routes.AddRange(roots[index].GetComponentsInChildren<SceneChange>(true));
        }
        return routes;
    }

    private static void ReplaceSingleUtf8Literal(
        string path,
        string sourceLiteral,
        string destinationLiteral)
    {
        var bytes = File.ReadAllBytes(path);
        var source = Encoding.UTF8.GetBytes(sourceLiteral);
        var destination = Encoding.UTF8.GetBytes(destinationLiteral);
        var matchIndex = -1;
        var matchCount = 0;

        for (var index = 0; index <= bytes.Length - source.Length; index++)
        {
            var matches = true;
            for (var byteIndex = 0; byteIndex < source.Length; byteIndex++)
            {
                if (bytes[index + byteIndex] != source[byteIndex])
                {
                    matches = false;
                    break;
                }
            }
            if (matches)
            {
                matchIndex = index;
                matchCount++;
            }
        }

        if (matchCount != 1)
        {
            throw new InvalidOperationException(
                "A cópia do menu deve conter exatamente uma rota original; observado: " +
                matchCount);
        }

        var updated = new byte[bytes.Length - source.Length + destination.Length];
        Buffer.BlockCopy(bytes, 0, updated, 0, matchIndex);
        Buffer.BlockCopy(destination, 0, updated, matchIndex, destination.Length);
        Buffer.BlockCopy(
            bytes,
            matchIndex + source.Length,
            updated,
            matchIndex + destination.Length,
            bytes.Length - matchIndex - source.Length);
        File.WriteAllBytes(path, updated);
    }

    private static void ValidatePersistentButtonRouteOrThrow(
        Scene scene,
        SceneChange route)
    {
        var buttonFound = false;
        var persistentRouteFound = false;
        var roots = scene.GetRootGameObjects();
        for (var rootIndex = 0; rootIndex < roots.Length; rootIndex++)
        {
            var buttons = roots[rootIndex].GetComponentsInChildren<Button>(true);
            for (var buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
            {
                var button = buttons[buttonIndex];
                if (button.name == "ButtonPlay")
                {
                    buttonFound = true;
                }
                for (var eventIndex = 0;
                    eventIndex < button.onClick.GetPersistentEventCount();
                    eventIndex++)
                {
                    var target = button.onClick.GetPersistentTarget(eventIndex);
                    var method = button.onClick.GetPersistentMethodName(eventIndex);
                    if (target == null && method == "sChange")
                    {
                        throw new InvalidOperationException(
                            "Button possui referência persistente quebrada para sChange.");
                    }
                    if (button.name == "ButtonPlay" &&
                        target == route && method == "sChange")
                    {
                        persistentRouteFound = true;
                    }
                }
            }
        }

        if (!buttonFound)
        {
            throw new InvalidOperationException("ButtonPlay não encontrado no menu.");
        }
        if (!persistentRouteFound)
        {
            throw new InvalidOperationException(
                "ButtonPlay.OnClick deve chamar SceneChange.sChange.");
        }
    }
}
