using UnityEngine;

public class Ball : MonoBehaviour
{
    // Config parameters
    [SerializeField] Paddle paddle1;
    [SerializeField] Vector2 Push; //Offsets to launching ball initially
    [SerializeField] AudioClip[] BlockSounds;
    [SerializeField] AudioClip[] PaddleSounds;
    [SerializeField] AudioClip[] ColliderSounds;

    [Range(0f, 2f)] [SerializeField] float RandFactor = 0f; // at collisions so that ball doesnt infinity move in same path
    [Range(0f, 5f)] [SerializeField] float velocityThreshold = 1f; // min theshold to consider it as low speed
    [Range(2, 10)] [SerializeField] int maxNullCollisions;

    // Cached reference Components
    AudioSource BallAudio;
    Rigidbody2D BallRigidBody2D;

    // State
    private Vector2 paddle_to_ball_vec;
    private bool hasLaunched;
    private Vector3 lastMousePos;
    private float maxVelocity; // Initialiazed Speed doesnt change by random-factor throughout the level
    private int NullCollisionsLeft;
    

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector2(8f,0.87f);
        paddle_to_ball_vec = transform.position - paddle1.transform.position;
        hasLaunched = false;
        lastMousePos = Input.mousePosition;
        maxVelocity = Push.magnitude;
        NullCollisionsLeft = maxNullCollisions;

        BallAudio = GetComponent<AudioSource>();
        BallRigidBody2D = GetComponent<Rigidbody2D>();
    }

    public bool hasBallLaunched()   { return hasLaunched; }

    // Update is called once per frame
    void Update()
    {
        if (!hasLaunched)
        {
            LockBall2Paddle();
            Launch();
        }
    }

    private void Launch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hasLaunched = true;
            Push.x = Mathf.Clamp( (Input.mousePosition - lastMousePos).x , -Push.x, Push.x);
            BallRigidBody2D.velocity = Push;
        }
    }

    private void LockBall2Paddle()
    {
        transform.position = (Vector2)paddle1.transform.position + paddle_to_ball_vec;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLaunched)
        {
            // Audio
            PlayAudioSFX(collision); // also updates null collision state

            // Applying variations in Rigid Body Velocity
            VelocityVariations();
        }
    }

    private void VelocityVariations()
    {
        Vector2 velocityTweak = BallRigidBody2D.velocity;

        // Limiting Null Collisions
        if (NullCollisionsLeft <= 0)
        {
            Block[] blocks = FindObjectsOfType<Block>();
            NullCollisionsLeft = maxNullCollisions;
            velocityTweak = blocks[Random.Range(0, blocks.Length)].transform.position - transform.position;
        }

        // Randomness in Trajectory
        else if ((velocityTweak.x < velocityThreshold) || (velocityTweak.x < velocityThreshold))
        {
            velocityTweak += (maxNullCollisions-NullCollisionsLeft)* new Vector2(Random.Range(-RandFactor, RandFactor), Random.Range(-RandFactor, RandFactor));
        }
        BallRigidBody2D.velocity = maxVelocity * velocityTweak.normalized;
        //Debug.Log(BallRigidBody2D.velocity.magnitude + ";" + BallRigidBody2D.velocity); // to check random patern
    }

    private void PlayAudioSFX(Collision2D collision)
    {
        AudioClip clip;
        if (collision.gameObject.name.ToLower().Contains("paddle"))
        {
            NullCollisionsLeft--;
            clip = PaddleSounds[Random.Range(0, PaddleSounds.Length)];
        }
        else if (collision.gameObject.name.ToLower().Contains("collider") || collision.gameObject.tag == "Unbreakable")
        {
            NullCollisionsLeft--;
            clip = ColliderSounds[Random.Range(0, ColliderSounds.Length)];
        }
        else
        {
            NullCollisionsLeft = maxNullCollisions;
            clip = BlockSounds[Random.Range(0, BlockSounds.Length)];
        }
        BallAudio.PlayOneShot(clip);
    }
}
