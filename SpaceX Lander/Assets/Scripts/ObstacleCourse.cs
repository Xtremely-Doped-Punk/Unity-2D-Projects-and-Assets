using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCourse : MonoBehaviour
{
    public bool AllowMovingObstacles = true;
    [SerializeField] List<MovingObstacle> MoveableObstacles;
    // Start is called before the first frame update
    void Start()
    {
        MoveableObstacles = new();
        foreach (MovingObstacle m in transform.GetComponentsInChildren<MovingObstacle>())
        {
            Debug.Log(m.name);
            MoveableObstacles.Add(m);
        }
        if (!AllowMovingObstacles)
        {
            foreach (MovingObstacle m in MoveableObstacles)
                m.enabled = false;
        }

    }

}
