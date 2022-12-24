using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Player player;
    Slider health;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        health = GetComponent<Slider>();
        health.maxValue = player.GetHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
            health.value = player.GetHealth();
        else
        {
            health.value = 0;
            player = FindObjectOfType<Player>();
        }
    }
}
