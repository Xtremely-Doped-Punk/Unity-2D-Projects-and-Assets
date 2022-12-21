using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : MonoBehaviour
{
    // To destroy the bullets once they leave the camera space
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(collision.gameObject); // destroy the object that triggers the collider
    }
}
