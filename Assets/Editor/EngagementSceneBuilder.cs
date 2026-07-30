using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EngagementSceneBuilder
{
    public const string SandboxScene = "Assets/Scenes/EngagementSandbox.unity";
    public const string PilotScene = "Assets/Scenes/SampleScene_EngagementPilot.unity";

    [MenuItem("RA/Engagement/Build Sandbox")]
    public static void BuildSandbox()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "EngagementSandbox";

        CreateCameraAndLight();
        CreateEventSystem();
        var experience = CreateExperienceRoot(false);
        var canvas = CreateCanvas("SandboxCanvas");
        var safeArea = CreateRect("SafeArea", canvas.transform, Vector2.zero, Vector2.one);

        var hud = CreateHud(safeArea);
        var simulatorPanel = CreateSimulatorPanel(safeArea);
        var subtitles = CreateSubtitlePresenter(safeArea);
        var stepIndicator = CreateStepIndicator(safeArea);
        var sensoryPanel = CreateSensoryPanel(safeArea);

        var sandbox = new GameObject("EngagementSandboxController").AddComponent<EngagementSandboxController>();
        var stateText = CreateText("CurrentState", simulatorPanel.transform, "Estado: Idle", 18, TextAnchor.MiddleLeft);
        SetRect(stateText.rectTransform, new Vector2(0.02f, 0.68f), new Vector2(0.98f, 0.78f));
        var exportText = CreateText("ExportStatus", simulatorPanel.transform, "Telemetria somente local", 12, TextAnchor.MiddleLeft);
        SetRect(exportText.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.12f));

        ConfigureExperience(experience, hud, subtitles, stepIndicator);
        ConfigureSandbox(sandbox, experience, sensoryPanel, stateText, exportText);
        WireSimulatorButtons(simulatorPanel.transform, sandbox);

        EditorSceneManager.MarkSceneDirty(scene);
        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, SandboxScene);
        AssetDatabase.SaveAssets();
        Debug.Log("RA Engagement: sandbox sem Vuforia gerada em " + SandboxScene);
    }

    [MenuItem("RA/Engagement/Build Vuforia Pilot")]
    public static void BuildPilot()
    {
        if (!File.Exists("Assets/Scenes/SampleScene.unity"))
        {
            throw new FileNotFoundException("SampleScene original ausente.");
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PilotScene) != null)
        {
            AssetDatabase.DeleteAsset(PilotScene);
        }
        if (!AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", PilotScene))
        {
            throw new IOException("Falha ao duplicar SampleScene para o piloto.");
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var scene = EditorSceneManager.OpenScene(PilotScene, OpenSceneMode.Single);
        var targets = UnityEngine.Object.FindObjectsOfType<Vuforia.TrackableBehaviour>();
        Vuforia.TrackableBehaviour targetA = null;
        for (var index = 0; index < targets.Length; index++)
        {
            if (string.Equals(targets[index].TrackableName, "A", System.StringComparison.Ordinal))
            {
                if (targetA != null) throw new InvalidOperationException("Mais de um target A encontrado na cena piloto.");
                targetA = targets[index];
            }
        }
        if (targetA == null) throw new InvalidOperationException("Target A não encontrado na SampleScene copiada.");

        var disabledLegacyName = DisableLegacyVisual(targetA.transform);
        targetA.gameObject.AddComponent<LegacyVuforiaTargetAdapter>();
        var experience = CreateExperienceRoot(true);
        experience.transform.SetParent(targetA.transform, false);
        experience.transform.localPosition = Vector3.zero;
        experience.transform.localRotation = Quaternion.identity;
        experience.transform.localScale = Vector3.one * 0.22f;

        var canvas = CreateCanvas("EngagementPilotCanvas");
        var safeArea = CreateRect("SafeArea", canvas.transform, Vector2.zero, Vector2.one);
        var hud = CreateHud(safeArea);
        var subtitles = CreateSubtitlePresenter(safeArea);
        var step = CreateStepIndicator(safeArea);
        ConfigureExperience(experience, hud, subtitles, step);

        var marker = experience.gameObject.AddComponent<PilotIntegrationMarker>();
        marker.integratedTarget = "A";
        marker.disabledLegacyObjectName = disabledLegacyName;
        marker.originalTransformPreserved = true;
        marker.originalObjectDeleted = false;

        var adapters = UnityEngine.Object.FindObjectsOfType<LegacyVuforiaTargetAdapter>();
        if (adapters.Length != 1 || adapters[0].TargetName != "A")
        {
            throw new InvalidOperationException("A cena piloto deve conter exatamente um adaptador, para o target A.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, PilotScene);
        AssetDatabase.SaveAssets();
        Debug.Log("RA Engagement: piloto Vuforia A gerado; objeto legado desativado: " + disabledLegacyName);
    }

    private static LearningExperienceController CreateExperienceRoot(bool pilot)
    {
        var root = new GameObject(pilot ? "EngagementPilot_A" : "EngagementExperience_A");
        var audio = root.AddComponent<AudioCueService>();

        var letterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Alphabet/Prefabs/RA_Letter_A.prefab");
        var lumiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Characters/Lumi/Prefabs/PLACEHOLDER_Lumi.prefab");
        var araraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Animals/Arara/Prefabs/PLACEHOLDER_Arara.prefab");
        if (letterPrefab == null || lumiPrefab == null || araraPrefab == null)
        {
            throw new FileNotFoundException("Prefabs obrigatórios da vertical slice não foram encontrados.");
        }

        var letter = PrefabUtility.InstantiatePrefab(letterPrefab) as GameObject;
        var lumi = PrefabUtility.InstantiatePrefab(lumiPrefab) as GameObject;
        var arara = PrefabUtility.InstantiatePrefab(araraPrefab) as GameObject;
        letter.name = "Letter_A_Experience";
        lumi.name = "PLACEHOLDER_LUMI";
        arara.name = "PLACEHOLDER_ARARA";
        letter.transform.SetParent(root.transform, false);
        lumi.transform.SetParent(root.transform, false);
        arara.transform.SetParent(root.transform, false);
        letter.transform.localPosition = new Vector3(-1.25f, 0f, 0f);
        lumi.transform.localPosition = new Vector3(0.15f, -0.75f, 0f);
        lumi.transform.localScale = Vector3.one * 0.75f;
        arara.transform.localPosition = new Vector3(1.35f, -0.4f, 0f);
        arara.transform.localScale = Vector3.one * 0.72f;

        var coordinator = root.AddComponent<InteractionCoordinator>();
        var controller = root.AddComponent<LearningExperienceController>();
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("letterRoot").objectReferenceValue = letter;
        serialized.FindProperty("lumiRoot").objectReferenceValue = lumi;
        serialized.FindProperty("contentRoot").objectReferenceValue = arara;
        serialized.FindProperty("audioCueService").objectReferenceValue = audio;
        serialized.FindProperty("interactionCoordinator").objectReferenceValue = coordinator;
        serialized.FindProperty("lumiMotion").objectReferenceValue = lumi.GetComponent<PlaceholderMotionController>();
        serialized.FindProperty("contentMotion").objectReferenceValue = arara.GetComponent<PlaceholderMotionController>();
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return controller;
    }

    private static void ConfigureExperience(
        LearningExperienceController experience,
        LetterExperienceHud hud,
        SubtitlePresenter subtitles,
        ExperienceStepIndicator stepIndicator)
    {
        var serialized = new SerializedObject(experience);
        serialized.FindProperty("hud").objectReferenceValue = hud;
        serialized.FindProperty("subtitles").objectReferenceValue = subtitles;
        serialized.FindProperty("stepIndicator").objectReferenceValue = stepIndicator;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static LetterExperienceHud CreateHud(RectTransform parent)
    {
        var root = CreateRect("LetterExperienceHud", parent, Vector2.zero, Vector2.one);
        var hud = root.gameObject.AddComponent<LetterExperienceHud>();
        var letter = CreateText("Letter", root, "A", 36, TextAnchor.UpperLeft);
        var word = CreateText("Word", root, "Arara", 26, TextAnchor.UpperLeft);
        SetRect(letter.rectTransform, new Vector2(0.03f, 0.82f), new Vector2(0.18f, 0.96f));
        SetRect(word.rectTransform, new Vector2(0.16f, 0.84f), new Vector2(0.48f, 0.95f));
        var exit = CreateButton("Exit", root, "Sair", new Vector2(0.80f, 0.84f), new Vector2(0.97f, 0.96f));
        var replay = CreateButton("Replay", root, "Repetir narração", new Vector2(0.03f, 0.04f), new Vector2(0.34f, 0.15f));
        var action = CreateButton("Action", root, "Toque na arara", new Vector2(0.66f, 0.04f), new Vector2(0.97f, 0.15f));
        var actionLabel = action.GetComponentInChildren<Text>();
        var serialized = new SerializedObject(hud);
        serialized.FindProperty("safeAreaRoot").objectReferenceValue = parent;
        serialized.FindProperty("letterText").objectReferenceValue = letter;
        serialized.FindProperty("wordText").objectReferenceValue = word;
        serialized.FindProperty("actionText").objectReferenceValue = actionLabel;
        serialized.FindProperty("replayButton").objectReferenceValue = replay;
        serialized.FindProperty("exitButton").objectReferenceValue = exit;
        serialized.FindProperty("actionButton").objectReferenceValue = action;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return hud;
    }

    private static SubtitlePresenter CreateSubtitlePresenter(RectTransform parent)
    {
        var root = CreateRect("SubtitlePresenter", parent, new Vector2(0.15f, 0.16f), new Vector2(0.85f, 0.27f));
        var image = root.gameObject.AddComponent<Image>();
        image.color = new Color(0.03f, 0.06f, 0.09f, 0.82f);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        var text = CreateText("Subtitle", root, string.Empty, 24, TextAnchor.MiddleCenter);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one);
        var presenter = root.gameObject.AddComponent<SubtitlePresenter>();
        var serialized = new SerializedObject(presenter);
        serialized.FindProperty("subtitleText").objectReferenceValue = text;
        serialized.FindProperty("canvasGroup").objectReferenceValue = group;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return presenter;
    }

    private static ExperienceStepIndicator CreateStepIndicator(RectTransform parent)
    {
        var text = CreateText("ExperienceStepIndicator", parent, "Ouça", 18, TextAnchor.MiddleCenter);
        SetRect(text.rectTransform, new Vector2(0.36f, 0.91f), new Vector2(0.64f, 0.98f));
        var indicator = text.gameObject.AddComponent<ExperienceStepIndicator>();
        var serialized = new SerializedObject(indicator);
        serialized.FindProperty("label").objectReferenceValue = text;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return indicator;
    }

    private static SensorySettingsPanel CreateSensoryPanel(RectTransform parent)
    {
        var root = CreateRect("SensorySettings", parent, new Vector2(0.02f, 0.30f), new Vector2(0.31f, 0.65f));
        var image = root.gameObject.AddComponent<Image>();
        image.color = new Color(0.06f, 0.12f, 0.16f, 0.92f);
        var title = CreateText("Title", root, "Perfil sensorial", 18, TextAnchor.UpperCenter);
        SetRect(title.rectTransform, new Vector2(0.04f, 0.82f), new Vector2(0.96f, 0.97f));
        var panel = root.gameObject.AddComponent<SensorySettingsPanel>();
        return panel;
    }

    private static RectTransform CreateSimulatorPanel(RectTransform parent)
    {
        var root = CreateRect("SandboxControls", parent, new Vector2(0.69f, 0.30f), new Vector2(0.98f, 0.79f));
        var image = root.gameObject.AddComponent<Image>();
        image.color = new Color(0.06f, 0.12f, 0.16f, 0.92f);
        return root;
    }

    private static void WireSimulatorButtons(Transform parent, EngagementSandboxController sandbox)
    {
        var labels = new[]
        {
            "TargetFound A", "TargetLost A", "Toque", "Replay", "Repetir",
            "Alternar perfil", "Reduced Motion", "Sair", "Exportar local"
        };
        for (var index = 0; index < labels.Length; index++)
        {
            var yMax = 0.64f - index * 0.062f;
            var button = CreateButton("Sandbox_" + index, parent, labels[index], new Vector2(0.06f, yMax - 0.054f), new Vector2(0.94f, yMax));
            if (index == 0) UnityEventTools.AddPersistentListener(button.onClick, sandbox.SimulateTargetFound);
            else if (index == 1) UnityEventTools.AddPersistentListener(button.onClick, sandbox.SimulateTargetLost);
            else if (index == 2) UnityEventTools.AddPersistentListener(button.onClick, sandbox.TriggerTap);
            else if (index == 3) UnityEventTools.AddPersistentListener(button.onClick, sandbox.ReplayNarration);
            else if (index == 4) UnityEventTools.AddPersistentListener(button.onClick, sandbox.RepeatExperience);
            else if (index == 5) UnityEventTools.AddPersistentListener(button.onClick, sandbox.CycleSensoryProfile);
            else if (index == 6) UnityEventTools.AddPersistentListener(button.onClick, sandbox.ToggleReducedMotion);
            else if (index == 7) UnityEventTools.AddPersistentListener(button.onClick, sandbox.ExitExperience);
            else UnityEventTools.AddPersistentListener(button.onClick, sandbox.ExportTelemetry);
        }
    }

    private static string DisableLegacyVisual(Transform target)
    {
        for (var index = 0; index < target.childCount; index++)
        {
            var child = target.GetChild(index);
            if (child.GetComponentInChildren<Renderer>(true) != null)
            {
                child.gameObject.SetActive(false);
                return child.name;
            }
        }
        throw new InvalidOperationException("Objeto visual legado do target A não foi encontrado; integração interrompida.");
    }

    private static void ConfigureSandbox(
        EngagementSandboxController sandbox,
        LearningExperienceController experience,
        SensorySettingsPanel sensory,
        Text state,
        Text export)
    {
        var serialized = new SerializedObject(sandbox);
        serialized.FindProperty("experience").objectReferenceValue = experience;
        serialized.FindProperty("sensoryPanel").objectReferenceValue = sensory;
        serialized.FindProperty("currentStateText").objectReferenceValue = state;
        serialized.FindProperty("exportStatusText").objectReferenceValue = export;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateCameraAndLight()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.20f, 0.24f);
        cameraObject.transform.position = new Vector3(0f, 0.7f, -6f);
        cameraObject.transform.rotation = Quaternion.identity;
        var lightObject = new GameObject("Calm Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.85f;
        lightObject.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
    }

    private static void CreateEventSystem()
    {
        var value = new GameObject("EventSystem");
        value.AddComponent<EventSystem>();
        value.AddComponent<StandaloneInputModule>();
    }

    private static Canvas CreateCanvas(string name)
    {
        var canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var value = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        value.SetParent(parent, false);
        SetRect(value, min, max);
        return value;
    }

    private static Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment)
    {
        var rect = CreateRect(name, parent, Vector2.zero, Vector2.one);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = size;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
    {
        var rect = CreateRect(name, parent, min, max);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.10f, 0.48f, 0.62f, 0.96f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var text = CreateText("Label", rect, label, 20, TextAnchor.MiddleCenter);
        SetRect(text.rectTransform, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
