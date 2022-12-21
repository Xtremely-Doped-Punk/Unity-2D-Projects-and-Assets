using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    int score = 0;
    int lives= 3;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] TextMeshProUGUI LifeCounter;
    [SerializeField] float PlayerRespawnDelay = 1.25f;
    Vector3 initPlayerPos = Vector3.zero;

    private void Awake()
    {
        // set up Singleton,
        if (FindObjectsOfType(GetType()).Length > 1)
            Destroy(gameObject);
        else 
            DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LifeCounter.text = lives.ToString() + "X";
    }
    public int GetScore()
    {
        return score;
    }
    public void AddScore(int points)
    {
        score += points;
    }

    public void PlayerDeath(Vector3 initPlayerPos)
    {
        lives--;
        LifeCounter.text = lives.ToString() + "X";
        if (lives == 0)
        {
            // as each scene has only only one gameobj having level script in it,
            // we can directly find that object of the particular type without ambiguity
            FindObjectOfType<Level>().LoadGameOver(
                DamageDealer.DamageType.Player,
                FindObjectOfType<EnemySpawner>().waveDelay);
            // player lost, scene-delay = wave-delay
        }
        else
        {
           this.initPlayerPos = initPlayerPos;
           Invoke("RespawnPlayer", PlayerRespawnDelay);
        }
    }

    private void RespawnPlayer()
    {
        Instantiate(PlayerPrefab, initPlayerPos, Quaternion.identity);
    }

    public void ResetGame()
    {
        Destroy(gameObject); //destroy this game session
    }
}
