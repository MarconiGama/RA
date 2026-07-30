using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

public static class EngagementPerformanceRunner
{
    private const string PendingKey = "RA.Engagement.PerformancePending";
    private const string ResultKey = "RA.Engagement.PerformanceResult";
    private const int TargetFrames = 120;

    [Serializable]
    private sealed class Result
    {
        public string unityVersion;
        public string environment;
        public int sampledFrames;
        public double sceneOpenMs;
        public double sampleDurationSeconds;
        public double averageFps;
        public double slowestFrameMs;
        public long allocatedMemoryStartBytes;
        public long allocatedMemoryEndBytes;
        public long allocatedMemoryDeltaBytes;
    }

    private static Result result;
    private static bool sampling;
    private static double sampleStarted;
    private static double lastFrameTime;
    private static double slowestFrame;

    [InitializeOnLoadMethod]
    private static void RestoreAfterDomainReload()
    {
        if (!SessionState.GetBool(PendingKey, false))
        {
            return;
        }

        result = JsonUtility.FromJson<Result>(SessionState.GetString(ResultKey, string.Empty));
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    public static void RunSandboxProbe()
    {
        var stopwatch = Stopwatch.StartNew();
        EditorSceneManager.OpenScene("Assets/Scenes/EngagementSandbox.unity", OpenSceneMode.Single);
        stopwatch.Stop();
        result = new Result
        {
            unityVersion = Application.unityVersion,
            environment = "Unity Editor batchmode nographics",
            sceneOpenMs = stopwatch.Elapsed.TotalMilliseconds
        };
        SessionState.SetString(ResultKey, JsonUtility.ToJson(result));
        SessionState.SetBool(PendingKey, true);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }
        if (!sampling)
        {
            sampling = true;
            sampleStarted = EditorApplication.timeSinceStartup;
            lastFrameTime = sampleStarted;
            result.allocatedMemoryStartBytes = Profiler.GetTotalAllocatedMemoryLong();
            return;
        }

        var now = EditorApplication.timeSinceStartup;
        var frameDuration = now - lastFrameTime;
        lastFrameTime = now;
        if (frameDuration > slowestFrame) slowestFrame = frameDuration;
        result.sampledFrames++;
        if (result.sampledFrames < TargetFrames)
        {
            return;
        }

        result.sampleDurationSeconds = now - sampleStarted;
        result.averageFps = result.sampleDurationSeconds <= 0.0
            ? 0.0
            : result.sampledFrames / result.sampleDurationSeconds;
        result.slowestFrameMs = slowestFrame * 1000.0;
        result.allocatedMemoryEndBytes = Profiler.GetTotalAllocatedMemoryLong();
        result.allocatedMemoryDeltaBytes = result.allocatedMemoryEndBytes - result.allocatedMemoryStartBytes;

        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var output = Path.Combine(projectRoot, "Builds", "Engagement", "sandbox-performance.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output, JsonUtility.ToJson(result, true));
        UnityEngine.Debug.Log("RA Engagement performance probe: " + JsonUtility.ToJson(result));

        SessionState.SetBool(PendingKey, false);
        SessionState.EraseString(ResultKey);
        EditorApplication.update -= Tick;
        EditorApplication.Exit(0);
    }
}
