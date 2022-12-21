using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundConfig
{
    public AudioClip Sound;
    [Range(0, 1)] public float Volume; // volume is to be set bet 0-(no sound) ro 1-(max sound)
}

public class Player : MonoBehaviour
{
    [Header("Player Controls")] 
    [SerializeField] float PlayerSpeed = 10f;
    [SerializeField] float health= 100f; public float GetHealth() { return health; }

    [Header("Shoot Controls")]
    [SerializeField] float ProjSpeed = 20f;
    [SerializeField] [Range(0.0f, 1.0f)] float FireDelay = 0.15f;
    [SerializeField] GameObject LaserPrefab;
    [SerializeField] SoundConfig ShootSound;
    
    [Header("Death Effects")]
    [SerializeField] SoundConfig DeathSound;

    Vector2 MinBounds;
    Vector2 MaxBounds;

    Coroutine FireCoroutine; // to save the temp Coroutine that fires when key is at hold

    // Start is called before the first frame update
    void Start()
    {
        SetupBoundaries();
    }

    private void SetupBoundaries()
    {
        Camera GameCam = Camera.main; // Camera viewport has local coodinate axes as:
        // left bottom corner = (0,0) and right top corner = (1,1)

        // converting those points into global coordinates so that
        // it sets that player movement boundaries in global space
        MinBounds = GameCam.ViewportToWorldPoint(Vector2.zero);
        MaxBounds = GameCam.ViewportToWorldPoint(Vector2.one);

        // padding sprite bounds to center pivoted object
        Vector2 SpriteBounds = GetComponent<SpriteRenderer>().bounds.size;
        MinBounds += SpriteBounds / 2;
        MaxBounds -= SpriteBounds / 2;
    }

    private void EnableVulnerabily(float delayDT)
    {
        Invoke("ImmunityDelay",delayDT);
    }

    private void ImmunityDelay()
    {
        // initially set the collider in prefab to false,
        // the respawn animation will trigger this function event
        GetComponent<Collider2D>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();
    }

    private void Move()
    {

        Vector2 dir2D = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // axes names by default 
        //Debug.Log(dir2D);
        // Gives the direction in which the object moving, maping it to coordinate axes:
        // (left to right) -> (-1 to 1) & (down to top) -> (-1 to 1)
        // Input buttons and other properties along horizontal and vertical axis
        // are configured in Input Manager of Project Settings

        Vector2 delta2D = dir2D * PlayerSpeed * Time.deltaTime; // making frame rate independent

        Vector2 newPos = (Vector2)transform.position + delta2D;
        transform.position = ClampBounds(newPos);
    }

    private Vector2 ClampBounds(Vector2 delta2D)
    {
        return new Vector2(Mathf.Clamp(delta2D.x,MinBounds.x,MaxBounds.x), Mathf.Clamp(delta2D.y, MinBounds.y, MaxBounds.y));
    }

    private void Fire()
    {
        /* Advantage of Using "Input.GetButton()" instead of "Input.GetButtonDown()". 
         * Input.GetButtonDown() only returns true for the frame in which the button was pressed. 

        //if (Input.GetButtonDown("Fire1"))
         * using fire input button - 1 with modified inputs (in proj settings)
         * as SpaceBar 'space' instead of 'left ctrl'
         *  which also considers mouse 0, i.e., left mouse button 
         * can also directly implement with Input.GetMouseButton(0) for mouse input
         * Input.GetKey(KeyCode.Space) for keyboard SpaceBar Input */


        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            // bullets start starts firing infinitly when they keys are pressed 'down'
            && FireCoroutine == null)
            // BUG: ensure that always only one Coroutine initialized, so that
            // pressing both inputs doesnt initialize 2 coroutines and deletes only one
        {
            FireCoroutine = StartCoroutine(FireWithDelay()); // calls the coroutine fn()
        }
        
        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
            // non stop firing corountines are stoped when keys are released 'up'
            && FireCoroutine != null) // simply to avoid null exception
        {
            //StopAllCoroutines(); // abruptly stoping all coroutines
            StopCoroutine(FireCoroutine); // stoping specially referencing to Coroutine variable
            FireCoroutine = null;
        }

        /* coroutine is a method that can pause execution and return control to Unity 
         * but then continue where it left off on the following frame.
         * Ex: Like showing destroying animation of a object before its instance is deleted
         * Coroutine time delay: using WaitForSeconds.
         * To stop a coroutine, use StopCoroutine and StopAllCoroutines. 
         * A coroutine also stops if you’ve set SetActive to false to disable the GameObject
         */

    }

    IEnumerator FireWithDelay()
    {
        while (true) // simply infinitly shooting without end condition
        {
            GameObject laser = Instantiate(LaserPrefab, transform.position, Quaternion.identity);
            /* Quaternion.identity as parameter to set the default prefab (as gameObject) initialization with no rotation
             * Quaternions are used to represent rotations. 
             * They are compact, don't suffer from gimbal lock and can easily be interpolated. 
             * Unity internally uses Quaternions to represent all rotations.
             * They are based on complex numbers and are not easy to understand intuitively. 
             * You almost never access or modify individual Quaternion components (x,y,z,w); 
             * most often you would just take existing rotations (e.g. from the Transform) and 
             * use them to construct new rotations (e.g. to smoothly interpolate between two rotations).*/

            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, ProjSpeed);
            // making laser's x-speed = 0; y-speed as a variable, direction of speed

            AudioSource.PlayClipAtPoint(ShootSound.Sound, Camera.main.transform.position, ShootSound.Volume);
            // playing shooting audio

            yield return new WaitForSeconds(FireDelay);
            // returns back to the next line execution only after condition is satisfied for yield return
        }
    }

    // deal damage using collider interaction
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<DamageDealer>(out var damageDealer)
            && damageDealer.DmgTyp() == DamageDealer.DamageType.Player)
            ProcessHit(damageDealer);
    }
    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.DmgAmt();
        damageDealer.Hit(); // delete the bullet projectile as it has hit the player
        // destroy player if it has no health
        if (health <= 0)
        {
            ProcessDeath();
        }
    }

    private void ProcessDeath()
    {
        // slow down Enemy spawner from start
        FindObjectOfType<EnemySpawner>().loopCounter = 0; //reset

        FindObjectOfType<GameSession>().PlayerDeath(transform.position);

        Destroy(gameObject);

        // playing the audio the position of main camera of the scene to avoid 3D audio system
        AudioSource.PlayClipAtPoint(DeathSound.Sound, Camera.main.transform.position, DeathSound.Volume);
    }
}
