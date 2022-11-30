using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadNextScene()
    {
        int cur_scene_index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(cur_scene_index + 1);
    }
    public void LoadStartScene()
    {
        SceneManager.LoadScene(0);
        FindObjectOfType<GameSession>().GameReset();
    }

    public void QuitGame()
    {
        // Application.Quitis ignored in editor, only work in standalone build
        Application.Quit();
    }
}
