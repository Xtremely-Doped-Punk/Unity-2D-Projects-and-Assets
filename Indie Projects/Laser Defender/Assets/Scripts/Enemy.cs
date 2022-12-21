using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Health")]
    [SerializeField] float health = 20f; public float GetHealth() { return health; }
    [SerializeField] int ScorePoints = 10;

    [Header("Enemy Projectile")]
    [SerializeField] bool CanShoot;
    // shows no.of shots the resp enemy has fired till the point in simulation
    float ShotCooldown;
    [SerializeField] Vector2 intShotDelay = new Vector2 (2f, 0.5f); // mean, std-diviation pair
    [SerializeField] float ProjSpeed = 10f;
    [SerializeField] GameObject LaserPrefab;
    [SerializeField] SoundConfig ShootSound;

    [Header("Enemy Explosion")]
    [SerializeField] ParticleSystem ExplosionPrefab;
    [SerializeField] float expnDelay = .5f;
    [SerializeField] SoundConfig DeathSound;

    static float RandNormDist(Vector2 MeanStdPair)
    {
        return NextGaussian(MeanStdPair.x, MeanStdPair.y);
    }
    static float NextGaussian(float mean, float std)
    {
        float v1, v2, s, y;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
        y = v1 * s;
        return mean + y * std;
    }

    // Start is called before the first frame update
    void Start()
    {
        ShotCooldown =  RandNormDist(intShotDelay);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanShoot)
            CountDownShooter();
    }

    private void CountDownShooter()
    {
        ShotCooldown -= Time.deltaTime;
        if (ShotCooldown < 0)
        {
            ShotCooldown = RandNormDist(intShotDelay);
            Fire();
        }
    }
    private void Fire()
    {
        GameObject laser = Instantiate(LaserPrefab, transform.position, Quaternion.identity) as GameObject;
        // as the direction of enemy shooting here is top to bottom towards player, -ve y axis direction
        // we are taking the negative of projectile-speed for our convention
        laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -ProjSpeed);

        AudioSource.PlayClipAtPoint(ShootSound.Sound, Camera.main.transform.position, ShootSound.Volume);
        // playing shooting audio
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<DamageDealer>(out var damageDealer) 
            && damageDealer.DmgTyp()==DamageDealer.DamageType.Enemy)
            ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.DmgAmt();
        damageDealer.Hit(); // delete the bullet projectile as it has hit the enemy
        // destroy enemy if it has no health
        if (health <= 0)
        {
            ProcessDeath();
        }
    }

    private void ProcessDeath()
    {
        ParticleSystem ExplosionVFX = Instantiate(ExplosionPrefab, transform.position, Quaternion.identity) as ParticleSystem;
        ExplosionVFX.Play();
        Destroy(ExplosionVFX.gameObject, expnDelay);
        Destroy(gameObject);
        
        FindObjectOfType<EnemySpawner>().DecrementEnemyCount();
        // giving refernce to the spawner (as only one object as this class)
        // so that when the enemies die, they can reduce the enemy count

        FindObjectOfType<GameSession>().AddScore(ScorePoints); // adding points for defeating this resp enemy

        // playing the audio the position of main camera of the scene to avoid 3D audio system
        AudioSource.PlayClipAtPoint(DeathSound.Sound, Camera.main.transform.position, DeathSound.Volume);
    }
}
