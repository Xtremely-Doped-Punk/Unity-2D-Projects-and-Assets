using System.Collections.Generic;
using UnityEngine;

public class EnemyPathing : MonoBehaviour
{
    // Parameters
    WaveConfig waveConfig;
    List<Transform> PathPoints = new List<Transform>();
    int CurPointIdx; int NxtPointIdx;
    float Speed;
    WaveConfig.SpawnDetails.MoveOptions MoveType;

    public void SetWaveConfig(WaveConfig waveConfig)
    {
        this.waveConfig = waveConfig;
    }

    // Start is called before the first frame update
    void Start()
    {
        // getting the disired path-waypoints for the respective wave
        PathPoints = waveConfig.GetPathPoints();

        // Initializing Point node-indexes from WaveConfig
        CurPointIdx = NxtPointIdx = waveConfig.GetInitialPathPoint().Key;

        // Initializing MoveType from WaveConfig
        MoveType = waveConfig.GetSpawnEnemyDetails().movetype;

        // Initializaing WayPoints
        UpdateNextPoint();

        // Initializing Speed from WaveConfig
        Speed = waveConfig.GetSpawnEnemyDetails().speed;

        // Intializing position of enemy to first point in path
        transform.position = PathPoints[CurPointIdx].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (PathPoints.Count >= 2)
        {
            var targetPos = PathPoints[NxtPointIdx].transform.position;
            var MoveThisFrame = Speed * Time.deltaTime; // move speed frame rate independent
            
            transform.position = Vector2.MoveTowards(transform.position, targetPos, MoveThisFrame);
            // given the current position, target positon and max distance move-able for the resp frame

            if (transform.position == targetPos) // update targets position if the previous target is reached
                UpdateNextPoint();
           
        }
    }

    private void UpdateNextPoint()
    {
        int PointIdx = NxtPointIdx;
        if (MoveType == WaveConfig.SpawnDetails.MoveOptions.Circular)
        {
            // consider the paths are circular
            NxtPointIdx = (NxtPointIdx + 1) % PathPoints.Count;
        }
        else if (MoveType == WaveConfig.SpawnDetails.MoveOptions.Elevator)
        {
            if (NxtPointIdx >= CurPointIdx)
                NxtPointIdx++;
            else
                NxtPointIdx--;

            if (NxtPointIdx == PathPoints.Count)
                NxtPointIdx -= 2;
            else if (NxtPointIdx == -1)
                NxtPointIdx += 2;
        }
        CurPointIdx = PointIdx;
    }
}
