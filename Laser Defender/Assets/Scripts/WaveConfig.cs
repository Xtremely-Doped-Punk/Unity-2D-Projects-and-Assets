using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Wave Config")] // Object name like: Create > "C# Script"
public class WaveConfig : ScriptableObject // becomes a object in Unity
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] GameObject PathPrefab;
    [SerializeField] int StartWayPoint = 0;

    [System.Serializable] public struct SpawnDetails
    { 
        public float delaytime; 
        public float randomness; 
        public int number; 
        public float speed;
        public enum MoveOptions { Circular, Elevator };
        public MoveOptions movetype;

        // constructor
        public SpawnDetails(float delaytime, float randomness, int number, float speed, MoveOptions movetype)
        {
            this.delaytime = delaytime;
            this.randomness = randomness;
            this.number = number;
            this.speed = speed;
            this.movetype = movetype;
        }
    };
    [SerializeField] SpawnDetails SpawnEnemies = new SpawnDetails(1f, 0.35f, 10, 2.5f, SpawnDetails.MoveOptions.Elevator);

    public GameObject GetEnemyPrefab() => EnemyPrefab;
    public SpawnDetails GetSpawnEnemyDetails() => SpawnEnemies;

    public List<Transform> GetPathPoints()
    {
        List<Transform> WayPoints = new List<Transform>();
        // Initializing path into list of way-points

        // 1st-way using for loop and adding each element using GetChild() into list
        /* 
        for (int i = 0; i < PathPrefab.transform.childCount; i++)
            path.Add(PathPrefab.transform.GetChild(i));
        */

        //2nd-way using for-each loop
        foreach (Transform child in PathPrefab.transform)
            WayPoints.Add(child);

        return WayPoints;
    }

    public KeyValuePair<int,Transform> GetInitialPathPoint()
    {
        return new KeyValuePair<int, Transform>(StartWayPoint, PathPrefab.transform.GetChild(StartWayPoint));
    }
}
