using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class PilotMenuNavigationPlayModeTests
{
    private const string PilotMenu = "Assets/Scenes/Menu_EngagementPilot.unity";
    private const string PilotDestination =
        "Assets/Scenes/SampleScene_EngagementPilot.unity";

    private readonly List<string> exceptions = new List<string>();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        exceptions.Clear();
        Application.logMessageReceived += CaptureException;
        LogAssert.ignoreFailingMessages = true;
        SceneManager.LoadScene("Menu_EngagementPilot", LoadSceneMode.Single);
        yield return null;
        Assert.AreEqual(
            "Menu_EngagementPilot",
            SceneManager.GetActiveScene().name,
            "O teste deve começar no menu piloto.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Application.logMessageReceived -= CaptureException;
        yield return null;
        LogAssert.ignoreFailingMessages = false;
    }

    [UnityTest]
    public IEnumerator ButtonPlay_LoadsActualEngagementPilotArScene()
    {
        var buttonObject = GameObject.Find("ButtonPlay");
        Assert.NotNull(buttonObject, "ButtonPlay não foi encontrado no menu piloto.");
        var button = buttonObject.GetComponent<Button>();
        Assert.NotNull(button);

        LogAssert.Expect(
            LogType.Assert,
            new Regex(
                "Cancelling DisplayDialog: Error occurred! Exception occurred when trying to parse web cam profile file:.*webcamprofiles\\.xml",
                RegexOptions.Singleline));
        LogAssert.Expect(
            LogType.Error,
            new Regex(
                "Exception occurred when trying to parse web cam profile file:.*webcamprofiles\\.xml",
                RegexOptions.Singleline));

        button.onClick.Invoke();

        var remainingFrames = 300;
        while (SceneManager.GetActiveScene().name !=
            "SampleScene_EngagementPilot" && remainingFrames-- > 0)
        {
            yield return null;
        }

        Assert.AreEqual(
            "SampleScene_EngagementPilot",
            SceneManager.GetActiveScene().name,
            "O clique não concluiu a troca real para a cena piloto.");
        Assert.AreEqual(0, exceptions.Count, string.Join("\n", exceptions.ToArray()));

        var arCamera = GameObject.Find("ARCamera");
        Assert.NotNull(arCamera, "ARCamera não foi encontrada na cena piloto ativa.");
        var hasVuforiaBehaviour = false;
        var components = arCamera.GetComponents<Component>();
        for (var index = 0; index < components.Length; index++)
        {
            if (components[index] != null &&
                components[index].GetType().FullName == "Vuforia.VuforiaBehaviour")
            {
                hasVuforiaBehaviour = true;
                break;
            }
        }
        Assert.IsTrue(
            hasVuforiaBehaviour,
            "ARCamera não contém o VuforiaBehaviour legado esperado.");
    }

    private void CaptureException(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            exceptions.Add(condition + "\n" + stackTrace);
        }
    }
}
