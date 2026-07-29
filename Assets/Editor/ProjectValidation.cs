using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public static class ProjectValidation
{
    public const string MenuScene = "Assets/Scenes/Menu.unity";
    public const string ArScene = "Assets/Scenes/SampleScene.unity";

    private static readonly string[] RequiredScenePaths =
    {
        MenuScene,
        ArScene
    };

    public static string[] GetRequiredScenes()
    {
        return (string[])RequiredScenePaths.Clone();
    }

    [MenuItem("RA/Validate Project")]
    public static void ValidateFromMenu()
    {
        ValidateOrThrow();
        UnityEngine.Debug.Log("Validação do projeto RA concluída sem erros.");
    }

    public static void ValidateOrThrow()
    {
        var errors = new List<string>();

        foreach (var scenePath in RequiredScenePaths)
        {
            if (!File.Exists(scenePath))
            {
                errors.Add("Cena obrigatória ausente: " + scenePath);
            }
        }

        var enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        foreach (var scenePath in RequiredScenePaths)
        {
            if (!enabledScenes.Contains(scenePath))
            {
                errors.Add("Cena obrigatória não está habilitada em Build Settings: " + scenePath);
            }
        }

        if (enabledScenes.Length > 0 && enabledScenes[0] != MenuScene)
        {
            errors.Add("A primeira cena do build deve ser " + MenuScene + ".");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Falha na validação do projeto RA:" + Environment.NewLine +
                "- " + string.Join(Environment.NewLine + "- ", errors.ToArray()));
        }
    }
}
