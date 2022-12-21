using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    [SerializeField] GameObject YouLose;
    [SerializeField] GameObject YouWin;
    [SerializeField] float GameOverExtraDelay = .25f;
    public void LoadStartMenu() //load first scene
    {
        SceneManager.LoadScene(0);
    }

    public void LoadGameScene()
    {
        GameSession session = FindObjectOfType<GameSession>();
        if (session != null)
            session.ResetGame(); // reset game session before next
        SceneManager.LoadScene("Game"); // ensure that sure scene name is exactly as given in string
    }
    public void LoadGameOver(DamageDealer.DamageType result, float delayDT = 0f)
    {
        if (result == DamageDealer.DamageType.Player) // if player lost
            YouLose.SetActive(true);
        else if (result == DamageDealer.DamageType.Enemy) // if all enemies have been destroyed
            YouWin.SetActive(true);
        StartCoroutine(DelayedScene(delayDT));
    }
    private IEnumerator DelayedScene(float delayDT)
    {
        yield return new WaitForSeconds(delayDT + GameOverExtraDelay);
        SceneManager.LoadScene("Game Over"); // ensure that sure scene name is exactly as given in string
    }
    public void QuitGame()
    {
        Application.Quit(); // works on build mode to end the application
    }
}
