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
        SceneManager.LoadScene(sceneName);
    }

    //função para sair do jogo
    public void exit()
    {
        Application.Quit();
    }
}
