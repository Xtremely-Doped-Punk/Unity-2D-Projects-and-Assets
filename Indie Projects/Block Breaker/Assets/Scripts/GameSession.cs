using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    // Parameters
    [Range(0.0001f,8f)] [SerializeField] float GameSpeed; // ranging between 0.1 to 4 speed multiplier
    [SerializeField] int PointBlock = 5;
    [SerializeField] bool AutoPlay;

    // Debuging purpose parameters
    [SerializeField] int Score = 0; 
    [SerializeField] TextMeshProUGUI score_text; // Only one TextMeshPro Comp should have named score
    
    int scene_index;

    private void Awake()
    {
        int GameStatusCount = FindObjectsOfType<GameSession>().Length;
        if (GameStatusCount > 1)
        {
            gameObject.SetActive(false); // Singleton pattern bug fix
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {   
        TextMeshProUGUI[] TMs = FindObjectsOfType<TextMeshProUGUI>();
        //Debug.Log(TMs.Length);
        for (int i = 0; i < TMs.Length; i++)
        {
            if (TMs[i].name.ToLower().Contains("score"))
            {
                score_text = TMs[i];
                //Debug.Log("Initialized "+score_text);
                score_text.SetText("Score: " + Score.ToString());
                break;
            }
        }
    }

    public void AddBlockPoint()
    {
        Score += PointBlock;
        score_text.text = "Score: " + Score.ToString();
    }

    // Update is called once per frame
    private void Update()
    {
        //Debug.Log(score_text);
        if(score_text == null)
            Start();

        Time.timeScale = GameSpeed;
    }

    public void GameReset()
    {
        Destroy(gameObject);
        
    }

    public bool IsAutoPlayEnabled()
    {
        return AutoPlay;
    }
}
