using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] List<WaveConfig>  waveConfigs;
    [SerializeField] int startingWave = 0;
    [SerializeField] public float waveDelay = 5f;
    [SerializeField] int minEnemies = 5;
    [SerializeField] bool looping = false;
    bool WaveStatus = false;
    public int loopCounter = 0;
    List<float> TimeForEachWave;

    [SerializeField] int EnemyCount = 0; // to check enemy count
    public void DecrementEnemyCount() { EnemyCount--; }

    // Start is called before the first frame update
    void Start()
    {
        TimeForEachWave = new List<float>(new float[waveConfigs.Count]);
        EnemyCount = 0; loopCounter = 0;
        WaveStatus = true;
        StartCoroutine(SpawnAllWaves());
    }
    void Update()
    {
        if (!WaveStatus)
        {
            // waves wont loop again if 'looping' -> false
            if (looping && (EnemyCount < minEnemies))
            {
                WaveStatus = true;
                StartCoroutine(SpawnAllWaves());
            }
            else if (EnemyCount == 0)
            {
                // show win 
                FindObjectOfType<Level>().LoadGameOver(
                    DamageDealer.DamageType.Enemy, waveDelay);
            }
        }
    }


    private IEnumerator SpawnAllWaves()
    {
        for (int WaveCount = 0; WaveCount < waveConfigs.Count; WaveCount++)
        {
            int WaveIndex = (startingWave + WaveCount) % waveConfigs.Count;
            var CurrWave = waveConfigs[WaveIndex];

            // only first loop of waves are done one spawner at a time
            if (loopCounter == 0)
            {
                TimeForEachWave[WaveIndex] = Time.fixedUnscaledTime;
                yield return StartCoroutine(SpawnAllEnemiesInWave(CurrWave)); // nested coroutine
                TimeForEachWave[WaveIndex] = Time.fixedUnscaledTime - TimeForEachWave[WaveIndex];
            }
            else // from the next loop of waves, the time of spawning each wave slowly decreases
            {
                StartCoroutine(SpawnAllEnemiesInWave(CurrWave));
                yield return new WaitForSeconds(TimeForEachWave[WaveIndex]/loopCounter);
            }
        }
        loopCounter++;
        WaveStatus = false;
    }
    private IEnumerator SpawnAllEnemiesInWave(WaveConfig WaveConfig)
    {
        for(int enemyCount = 0; enemyCount < WaveConfig.GetSpawnEnemyDetails().number; enemyCount++)
        { 
            // Initialy WaveConfig is not initialized into Enemy Object
            GameObject Enemy = Instantiate(WaveConfig.GetEnemyPrefab(), // initialing current wave's enemy prefab
                WaveConfig.GetInitialPathPoint().Value.position, // giving the transform-position coordinates of initial waypoint
                Quaternion.identity); // keep the initial rotation as it is

            Enemy.SetActive(false); // so, we disable the Enemy Object
            Enemy.GetComponent<EnemyPathing>().SetWaveConfig(WaveConfig); // and set the waveConfig to current wave
            Enemy.SetActive(true); // then enable back the Enemy Object

            EnemyCount++;
            // keeping delay time in between spawning every enemy
            yield return new WaitForSeconds(WaveConfig.GetSpawnEnemyDetails().delaytime);
        }
        if ( ((EnemyCount < minEnemies) && looping) || ((EnemyCount == 0) && !looping) )
            yield return null; 
        // skip waiting time if less no of enemies or finish game when no more enemies left in non-looping mode
        else
            yield return new WaitForSeconds(waveDelay); // adding delay after all enemies are spawned of the wave
    }
}
