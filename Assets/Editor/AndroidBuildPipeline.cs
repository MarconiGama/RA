using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuildPipeline
{
    private const string DevelopmentOutput = "Builds/Android/RealidadeA-development.apk";
    private const string EngagementPilotOutput = "Builds/Android/RealidadeA-engagement-pilot.apk";
    private const string EngagementPilotQaOutput = "Builds/Android/RealidadeA-engagement-pilot-e1.1.apk";
    private const string EngagementPilotMenuScene = "Assets/Scenes/Menu_EngagementPilot.unity";
    private const string EngagementPilotScene = "Assets/Scenes/SampleScene_EngagementPilot.unity";
    private const string ProductionOutput = "Builds/Android/RealidadeA-production.aab";
    private const string DefaultBundleVersion = "0.2.0";
    private const int DefaultVersionCode = 200;

    [MenuItem("RA/Build/Development APK")]
    public static void BuildDevelopmentApk()
    {
        BuildAndroid(DevelopmentOutput, false, true);
    }

    [MenuItem("RA/Build/Engagement Pilot APK")]
    public static void BuildEngagementPilotApk()
    {
        RequireEngagementPilotScenes();
        BuildAndroid(
            EngagementPilotOutput,
            false,
            true,
            new[] { EngagementPilotMenuScene, EngagementPilotScene },
            null,
            null,
            SceneRouteValidation.ValidateEngagementPilotOrThrow);
    }

    [MenuItem("RA/Build/Engagement Pilot E1.1 Device QA APK")]
    public static void BuildEngagementPilotQaApk()
    {
        RequireEngagementPilotScenes();
        BuildAndroid(
            EngagementPilotQaOutput,
            false,
            true,
            new[] { EngagementPilotMenuScene, EngagementPilotScene },
            "0.3.1-e1",
            211,
            SceneRouteValidation.ValidateEngagementPilotOrThrow);
    }

    public static string[] GetEngagementPilotQaScenes()
    {
        return new[] { EngagementPilotMenuScene, EngagementPilotScene };
    }

    [MenuItem("RA/Build/Production AAB")]
    public static void BuildProductionAab()
    {
        BuildAndroid(ProductionOutput, true, false);
    }

    private static void BuildAndroid(string defaultOutput, bool appBundle, bool developmentBuild)
    {
        BuildAndroid(
            defaultOutput,
            appBundle,
            developmentBuild,
            ProjectValidation.GetRequiredScenes(),
            null,
            null,
            SceneRouteValidation.ValidateBaselineOrThrow);
    }

    private static void BuildAndroid(
        string defaultOutput,
        bool appBundle,
        bool developmentBuild,
        string[] scenes,
        string exactBundleVersion,
        int? exactVersionCode,
        Action routeValidation)
    {
        ProjectValidation.ValidateOrThrow();
        if (routeValidation == null)
        {
            throw new InvalidOperationException("A validação de rotas é obrigatória antes do build Android.");
        }
        routeValidation();

        if (scenes == null || scenes.Length == 0)
        {
            throw new InvalidOperationException("O build Android precisa de ao menos uma cena.");
        }

        var outputPath = GetCommandLineArgument("-customBuildPath");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = defaultOutput;
        }

        outputPath = Path.GetFullPath(outputPath);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(outputDirectory))
        {
            throw new InvalidOperationException("Não foi possível determinar o diretório de saída do build.");
        }

        Directory.CreateDirectory(outputDirectory);
        var previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
        var previousScriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
        var previousArchitectures = PlayerSettings.Android.targetArchitectures;
        var previousBundleVersion = PlayerSettings.bundleVersion;
        var previousVersionCode = PlayerSettings.Android.bundleVersionCode;
        var previousApplicationId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);

        string previousKeystoreName = PlayerSettings.Android.keystoreName;
        string previousKeystorePassword = PlayerSettings.Android.keystorePass;
        string previousAliasName = PlayerSettings.Android.keyaliasName;
        string previousAliasPassword = PlayerSettings.Android.keyaliasPass;
        bool previousUseCustomKeystore = PlayerSettings.Android.useCustomKeystore;

        try
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new InvalidOperationException("Não foi possível ativar o target Android.");
            }

            EditorUserBuildSettings.buildAppBundle = appBundle;
            ConfigureVersionAndIdentity(appBundle, exactBundleVersion, exactVersionCode);

            if (!appBundle && exactBundleVersion != null)
            {
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            }

            if (appBundle)
            {
                ConfigureProductionSigning();
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            }

            var buildOptions = BuildOptions.None;
            if (developmentBuild)
            {
                buildOptions |= BuildOptions.Development | BuildOptions.AllowDebugging;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = buildOptions
            };

            Debug.Log("Iniciando build Android em: " + outputPath);
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Build Android falhou. Resultado: " + summary.result +
                    ", erros: " + summary.totalErrors +
                    ", avisos: " + summary.totalWarnings);
            }

            Debug.Log(
                "Build Android concluído. Arquivo: " + outputPath +
                ", tamanho: " + summary.totalSize +
                ", duração: " + summary.totalTime);
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, previousScriptingBackend);
            PlayerSettings.Android.targetArchitectures = previousArchitectures;
            PlayerSettings.bundleVersion = previousBundleVersion;
            PlayerSettings.Android.bundleVersionCode = previousVersionCode;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, previousApplicationId);

            PlayerSettings.Android.keystoreName = previousKeystoreName;
            PlayerSettings.Android.keystorePass = previousKeystorePassword;
            PlayerSettings.Android.keyaliasName = previousAliasName;
            PlayerSettings.Android.keyaliasPass = previousAliasPassword;
            PlayerSettings.Android.useCustomKeystore = previousUseCustomKeystore;
        }
    }

    private static void ConfigureVersionAndIdentity(
        bool productionBuild,
        string exactBundleVersion,
        int? exactVersionCode)
    {
        var bundleVersion = exactBundleVersion ??
            GetEnvironmentVariable("RA_BUNDLE_VERSION", DefaultBundleVersion);
        var versionCodeText = exactVersionCode.HasValue
            ? exactVersionCode.Value.ToString()
            : GetEnvironmentVariable("RA_VERSION_CODE", DefaultVersionCode.ToString());

        int versionCode;
        if (!int.TryParse(versionCodeText, out versionCode) || versionCode <= 0)
        {
            throw new InvalidOperationException("RA_VERSION_CODE deve ser um número inteiro positivo.");
        }

        PlayerSettings.bundleVersion = bundleVersion;
        PlayerSettings.Android.bundleVersionCode = versionCode;

        var applicationId = Environment.GetEnvironmentVariable("RA_APPLICATION_ID");
        if (!string.IsNullOrWhiteSpace(applicationId))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, applicationId.Trim());
        }
        else if (productionBuild)
        {
            throw new InvalidOperationException(
                "Defina RA_APPLICATION_ID para gerar o AAB de produção, por exemplo: br.com.suaempresa.realidadea.");
        }
    }

    private static void RequireEngagementPilotScenes()
    {
        if (!File.Exists(EngagementPilotMenuScene))
        {
            throw new FileNotFoundException(
                "Menu piloto de engagement ausente.", EngagementPilotMenuScene);
        }
        if (!File.Exists(EngagementPilotScene))
        {
            throw new FileNotFoundException(
                "Cena piloto de engagement ausente.", EngagementPilotScene);
        }
    }

    private static void ConfigureProductionSigning()
    {
        var keystorePath = RequireEnvironmentVariable("RA_KEYSTORE_PATH");
        var keystorePassword = RequireEnvironmentVariable("RA_KEYSTORE_PASSWORD");
        var aliasName = RequireEnvironmentVariable("RA_KEY_ALIAS");
        var aliasPassword = RequireEnvironmentVariable("RA_KEY_ALIAS_PASSWORD");

        keystorePath = Path.GetFullPath(keystorePath);
        if (!File.Exists(keystorePath))
        {
            throw new FileNotFoundException("Keystore de produção não encontrado.", keystorePath);
        }

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystorePath;
        PlayerSettings.Android.keystorePass = keystorePassword;
        PlayerSettings.Android.keyaliasName = aliasName;
        PlayerSettings.Android.keyaliasPass = aliasPassword;
    }

    private static string GetCommandLineArgument(string argumentName)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, argumentName);
        if (index < 0 || index + 1 >= args.Length)
        {
            return null;
        }

        return args[index + 1];
    }

    private static string GetEnvironmentVariable(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static string RequireEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Variável de ambiente obrigatória ausente: " + name);
        }

        return value.Trim();
    }
}
