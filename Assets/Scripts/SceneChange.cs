using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    //declaração de variável do tipo string
    public string sceneName;

    //função para ir para RA
    public void sChange()
    {
        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            var activeScene = SceneManager.GetActiveScene();
            Debug.LogError(
                "RA_SCENE_ROUTE_INVALID\n" +
                "requestedScene=" + (sceneName ?? string.Empty) + "\n" +
                "activeScene=" + activeScene.name + "\n" +
                "buildSceneCount=" + SceneManager.sceneCountInBuildSettings);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    //função para sair do jogo
    public void exit()
    {
        Application.Quit();
    }
}
