using System;
using System.Linq;
using UnityEditor;

public static class BuildScript
{
    public static void BuildAndroidAPK()
    {
        var outputPath = Environment.GetCommandLineArgs()
            .SkipWhile(arg => arg != "-customBuildPath")
            .Skip(1)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = "Builds/android/RA-debug.apk";
        }

        var enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (enabledScenes.Length == 0)
        {
            throw new InvalidOperationException("Nenhuma cena habilitada em Build Settings.");
        }

        var options = new BuildPlayerOptions
        {
            scenes = enabledScenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao gerar APK: {report.summary.result}");
        }

        UnityEngine.Debug.Log($"APK gerado com sucesso: {outputPath}");
    }
}
