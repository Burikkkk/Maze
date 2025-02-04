using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Метод для начала игры
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Scene_MazeGeneratiom");
    }

    // Метод для выхода из игры
    public void ExitGame()
    {

        // Если игра запущена в редакторе, то остановить игру
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Если игра запущена как сборка, то закрыть приложение
        Application.Quit();
        #endif
    }

    public void ChangeScreen(string name)
    {
        SceneManager.LoadSceneAsync(name);
    }

}
