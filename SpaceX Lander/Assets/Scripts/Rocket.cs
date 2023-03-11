using System;
using Unity.MLAgents;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class Rocket : MonoBehaviour
{
    private static Rocket Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;
    }
    public static Rocket GetInstance() { return Instance; }

    Rigidbody body;
    AudioSource AudioSystem;
    [Header("Player Controls")]
    [Tooltip("RPS = RPM/60")]
    [SerializeField] float rps = 60f;
    [Tooltip("const 'FORCE' by the thrusters on the rocket")]
    [SerializeField] float MaxSpeed = 25f;
    [Tooltip("accelerated force (v=u+at)")]
    [SerializeField] float Boost = 10f;
    private float speed;
    [SerializeField] float WarningTimer = 5f;
    float timer = 0f;
    public bool InvertCtrls;

    [Header("PC based Config")]
    [Tooltip("Set according to max FPS ur PC can achieve")]
    [SerializeField][Range(30, 420)] private float PhysicsTimeScaler = 120;

    [Header("Audio Config")]
    [SerializeField] AudioClip ThrusterFx;
    [SerializeField] AudioClip WinFx;
    [SerializeField] AudioClip DeathFx;

    [Header("Particle Effects")]
    [SerializeField] ParticleSystem thrusters;
    [SerializeField] ParticleSystem winVFX;
    [SerializeField] GameObject deathVFX;
    [SerializeField] GameObject RocketModel;

    [Header("Score Config")]
    [SerializeField] private int CheckpointScore = 5;
    public int EpisodeScore = 0;

    [Header("Debuger Config")]
    [SerializeField] bool AllowCollision = true;
    [SerializeField] bool LoadNxtLvl = true;
    [Tooltip("bool to turn on/off Input by ML-Agent")]
    public bool AgentInputOveride = false;
    //private AgentController agent;
    private DecisionRequester decision;

    // Event Handlers
    public EventHandler DeadEvents;
    public EventHandler<WarningParameters> WarningEvents; public class WarningParameters : EventArgs { public bool isdead; public float WarningTimer; }
    public EventHandler CheckpointEvents;
    public EventHandler LevelWinEvents;

    private Vector3 InitialPos;
    private Quaternion InitialRot;
    //private RigidbodyConstraints constraints;
    private GameSession session;

    // Start is called before the first frame update
    void Start()
    {
        EpisodeScore = 0;
        speed = MaxSpeed;
        InitialPos = transform.position;
        InitialRot = transform.rotation;

        body = GetComponent<Rigidbody>();
        //constraints = body.constraints;
        AudioSystem = GetComponent<AudioSource>();
        //agent = GetComponent<AgentController>();
        decision = GetComponent<DecisionRequester>();
        session = GameSession.GetInstance();
        deathVFX.SetActive(false);

        timer = WarningTimer;
    }


    public bool[] GetInputConditions()
    {
        return new bool[] {
            Input.GetKey(KeyCode.Space),
            Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift),
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.D) };
    }

    void Update()
    {
        //if (agent == null) agent = GetComponent<AgentController>();
        //else agent.CheckOveride();
        if (!AgentInputOveride)
        {
            var inp = GetInputConditions();
            ThrusterBoost(inp[0], inp[1]);
            RotationControl(inp[2], inp[3], inp[4], inp[5]);
        }
        if (timer <= WarningTimer)
            timer += Time.deltaTime;
    }
    public void ResetRotaion()
    {
        transform.rotation = InitialRot;
    }
    public void ToggleCollision()
    {
        if (Debug.isDebugBuild) // only works in "Development Build" in Build Settings
            AllowCollision = !AllowCollision;
    }

    public void ResetLvl()
    {
        session.SessionUpdate(EpisodeScore);
        transform.SetPositionAndRotation(InitialPos, InitialRot);
        this.enabled = true;
        timer = WarningTimer;
        //AudioSystem.enabled = true;
        RocketModel.SetActive(true); deathVFX.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            other.enabled = false;
            EpisodeScore += CheckpointScore;
            if (CheckpointEvents != null) CheckpointEvents(this, EventArgs.Empty);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!AllowCollision) return;
        if ((this.enabled) && (collision.gameObject.CompareTag("Warning")))
        {
            timer -= 2f * Time.deltaTime;
            // timer limit exceeding lead to death (here no need to call dead event on agent,
            // as the negative reward of death is incrementally added by the agent
            if (timer <= 0)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("DEAD: Illegal Suface Contact (countdown over)");
                AudioSystem.Stop(); AudioSystem.PlayOneShot(DeathFx);
                RocketModel.SetActive(false); deathVFX.SetActive(true);
                this.enabled = false;
                Invoke(nameof(ResetLvl), 2);

                if (WarningEvents != null) WarningEvents(this, new WarningParameters { isdead = true, WarningTimer = WarningTimer });
            }
            else
            {
                // call warning events on agent
                if (WarningEvents != null) WarningEvents(this, new WarningParameters { isdead = false, WarningTimer = WarningTimer });
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!AllowCollision) return;
        if (this.enabled)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
            // we dont rotation over y axis, as it would mess up the camera follower and controlling directions
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!AllowCollision) return;
        if (this.enabled)
        {
            //Debug.Log(this.ToString()+"\t"+collision.gameObject.ToString());
            switch (collision.gameObject.tag)
            {
                case "Friendly":
                    if (Debug.isDebugBuild)
                        Debug.Log("NOT DEAD: Friendly Obstacle (contact allowed)");
                    break;
                case "Warning":
                    if (Debug.isDebugBuild)
                        Debug.Log("WARNING: Illegal Suface Contact (coundown:" + WarningTimer.ToString() + "secs)");
                    break;
                case "Finish":
                    if (Debug.isDebugBuild)
                        Debug.Log("WIN: Level Clear (alive)");
                    AudioSystem.Stop(); AudioSystem.PlayOneShot(WinFx);
                    winVFX.Play();
                    if (!LoadNxtLvl)
                    {
                        ResetLvl();
                        session.SceneReset();
                        break;
                    }
                    this.enabled = false;
                    // now we are simply disabling script even for wining the level
                    // i.e., for simply touching the landing pad itself is considered as a win
                    // later we can add features to see if correctly landed,
                    session.LoadNextSceneDelayed(2.5f);
                    break;
                case "Deadly":
                    if (Debug.isDebugBuild)
                        Debug.Log("DEAD: Deadly Obstacle (contact not allowed)");
                    AudioSystem.Stop(); AudioSystem.PlayOneShot(DeathFx);
                    RocketModel.SetActive(false); deathVFX.SetActive(true);
                    this.enabled = false;
                    Invoke(nameof(ResetLvl), 1.5f);
                    break;
            }
        }
    }

    private int framecounter;
    public void ThrusterBoost(bool IsSpacePressed, bool IsLeftShiftPressed)
    {
        if (IsSpacePressed) // space-bar is assigned to boosting thrusters
        {
            //Debug.Log(PhysicsTimeScaler * Time.deltaTime * speed);
            body.AddRelativeForce(PhysicsTimeScaler * Time.deltaTime * speed * Vector3.up); // vector.up in one unit in y axis
                                                                                            //relative force to the local system of the body

            /* When doing physical calculations in unity it is recomended to do them inside FixedUpdate,(we will use that later) 
             * as it is guaranteed to have constant rate of activation which allows you to avoid certain problems 
             * that are often for physics with even small framerate fluctuation and correct usage of Time.deltaTime */

            if (!AudioSystem.isPlaying) // play rocket-boost sound effect
                AudioSystem.PlayOneShot(ThrusterFx); //  Play One Shot can't loop audio (best used for sound effects)

            if ((thrusters != null) && (!thrusters.isPlaying))
                thrusters.Play();

            framecounter = Time.frameCount;
        }
        else
        {
            // to make particle system and audio disrupted by the timely input of agent
            if (Time.frameCount - framecounter > decision.DecisionPeriod) // adding 1 frame delay
            {
                if ((thrusters != null) && (thrusters.isPlaying))
                    thrusters.Stop();
                AudioSystem.Stop();
            }
        }
        if (IsLeftShiftPressed) // increase spead
        {
            speed += Boost * Time.deltaTime; //acceralation
        }
        else
        {
            speed = MaxSpeed;
        }
    }

    public void RotationControl(bool is_W_Pressed, bool is_A_Pressed, bool is_S_Pressed, bool is_D_Pressed)
    {
        // turn off rigid body phyics properties while updating
        // to get more control over the dynamics control (manual control)
        //body.freezeRotation = true;
        //body.freezeRotation = false; // turn back on after updation
        //body.constraints = constraints; // reset back contraints

        // - - - - - - - - - - [OR] - - - - - - - - - -

        // our ultimate goal is to supress the the existing angular force/velocity cased in
        // the physics system of the rigid body, so that the body can freely rotate (without opposing speeds to inout speed)
        body.angularVelocity = Vector3.zero; //removing existing angular rotational forces by unity physics engine


        if (!InvertCtrls) // invert controls for close cam follows (1st person and 3rd person view night mode)
        {
            if (is_A_Pressed && is_D_Pressed) { }
            // skip rotation if both are pressed
            else if (is_A_Pressed)
            {
                //Debug.Log("Rotate Anticlockwise");
                transform.Rotate(rps * Time.deltaTime * Vector3.forward, Space.World); // forward is one unit in z axis
                                                                                       // (rotate based on local system of the body) 
            }
            else if (is_D_Pressed)
            {
                //Debug.Log("Rotate Clockwise");
                transform.Rotate(rps * Time.deltaTime * Vector3.back, Space.World); // backword is minus one unit in z axis
                                                                                    // (rotate based on local system of the body) 
            }

            if (is_W_Pressed && is_S_Pressed) { }
            // skip rotation if both are pressed
            else if (is_W_Pressed)
            {
                transform.Rotate(rps * Time.deltaTime * Vector3.left, Space.World);    // left is minus one unit in x axis
                                                                                       // (rotate based on local system of the body) 
            }
            else if (is_S_Pressed)
            {
                transform.Rotate(rps * Time.deltaTime * Vector3.right, Space.World); // right is one unit in x axis
                                                                                     // (rotate based on local system of the body) 
            }
        }
        else
        {
            if (is_W_Pressed && is_S_Pressed) { }
            // skip rotation if both are pressed
            else if (is_S_Pressed)
            {
                //Debug.Log("Rotate Anticlockwise");
                transform.Rotate(rps * Time.deltaTime * Vector3.forward, Space.World); // forward is one unit in z axis
                                                                                       // (rotate based on local system of the body) 
            }
            else if (is_W_Pressed)
            {
                //Debug.Log("Rotate Clockwise");
                transform.Rotate(rps * Time.deltaTime * Vector3.back, Space.World); // backword is minus one unit in z axis
                                                                                    // (rotate based on local system of the body) 
            }

            if (is_A_Pressed && is_D_Pressed) { }
            // skip rotation if both are pressed
            else if (is_D_Pressed)
            {
                transform.Rotate(rps * Time.deltaTime * Vector3.left, Space.World);    // left is minus one unit in x axis
                                                                                       // (rotate based on local system of the body) 
            }
            else if (is_A_Pressed)
            {
                transform.Rotate(rps * Time.deltaTime * Vector3.right, Space.World); // right is one unit in x axis
                                                                                     // (rotate based on local system of the body) 
            }
        }
    }

    public Vector3 GetVelocity()
    {
        return body.velocity;
    }
}
