using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    TextMeshProUGUI score;
    GameSession session;

    // Start is called before the first frame update
    void Start()
    {
        score = GetComponent<TextMeshProUGUI>(); // selct text component in the obj
        session = FindObjectOfType<GameSession>(); // select game-session class/comp containing obj
    }

    // Update is called once per frame
    void Update()
    {
        if (session != null)
            score.text = session.GetScore().ToString();
    }
}
