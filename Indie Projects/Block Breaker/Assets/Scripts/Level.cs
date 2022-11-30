using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    // Parameters
    [SerializeField] int BreakableBlock; // for debugging purposes

    // Start is called before the first frame update
    void Start()
    { 
        BreakableBlock = 0;
    }

    public void IncrCountBlocks()
    {
        BreakableBlock++;
    }

    public void DecrCountBlocks()
    {
        BreakableBlock--;
        if (BreakableBlock <= 0)
        {
            FindObjectOfType<SceneLoader>().LoadNextScene();
        }
    }

    public int getCount() { return BreakableBlock; }
}
