using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    // Paramters
    [SerializeField] float ScreenWidth_inUnits = 16f;

    // States
    Vector2 paddleWidth;

    // Cached reference Components
    GameSession gameSession;
    Ball ball;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(8f,0.35f,0f);
        paddleWidth = GetComponent<SpriteRenderer>().bounds.size;
        //Debug.Log("Bounds: "+GetComponent<SpriteRenderer>().bounds.size);
        //Debug.Log("Rect: "+GetComponent<SpriteRenderer>().sprite.rect);

        gameSession = FindObjectOfType<GameSession>();
        ball = FindObjectOfType<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 paddlePos = transform.position;
        
        if (gameSession.IsAutoPlayEnabled() && ball.hasBallLaunched())
        {
            // AutoPlay will make the paddle track the paddle to ball's x-axis movement
            paddlePos.x = ball.transform.position.x;
        }
        else
        {
            // else player's mouse input is taken
            float mousePos_inUnits = (Input.mousePosition.x / Screen.width) * ScreenWidth_inUnits;
            paddlePos.x = mousePos_inUnits;
        }
        

        paddlePos.x = Mathf.Clamp(paddlePos.x, paddleWidth.x/2, ScreenWidth_inUnits - paddleWidth.x/2);
        transform.position = paddlePos;
    }
}
