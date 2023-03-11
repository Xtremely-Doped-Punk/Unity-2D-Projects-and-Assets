using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentController : Agent
{
    // set the terrain walls dimensions here
    private const float TUNNEL_WIDTH = 25f;
    private const float TUNNEL_Zoffset = -10f;
    private const float TUNNEL_HEIGHT = 20f;
    private const float TUNNEL_Yoffset = 0f;
    private static BehaviorParameters Parameters;
    private Rocket rocket;
    [Tooltip("Agent dying should be rewarded negatively")]
    [SerializeField] float DeadReward = -5;
    [Tooltip("Checkpoint must be rewarded in small incremental steps")]
    [SerializeField] float CheckpointReward = +2;
    [Tooltip("Final Win state should be reward larger than than all")]
    [SerializeField] float WinReward = +25;


    private bool[] inputs = new bool[6];
    private void Awake()
    {
        Parameters = GetComponent<BehaviorParameters>();
        rocket = GetComponent<Rocket>();
        if (this.enabled) rocket.AgentInputOveride = true;
    }

    private void Start()
    {
        rocket.DeadEvents += AddDeathReward;
        rocket.WarningEvents += AddWarningReward;
        rocket.CheckpointEvents += AddCheckpointReward;
        rocket.LevelWinEvents += AddWinReward;
    }
    private void Update()
    {
        CheckOveride();
        inputs = rocket.GetInputConditions();
    }

    public void CheckOveride()
    {
        if (Parameters == null)
            Parameters = GetComponent<BehaviorParameters>();
        else
        {
            if (Parameters.BehaviorType == BehaviorType.HeuristicOnly)
            {
                rocket.AgentInputOveride = false;
                //this.enabled = false;
            }
            else
            {
                rocket.AgentInputOveride = true;
                //this.enabled = true;
            }
        }
    }

    private void AddDeathReward(object sender, EventArgs e)
    {
        AddReward(DeadReward);
        EndEpisode();
    }
    private void AddWarningReward(object sender, Rocket.WarningParameters e)
    {
        AddReward(DeadReward*(Time.deltaTime/e.WarningTimer));
        if (e.isdead) EndEpisode();
    }
    private void AddCheckpointReward(object sender, EventArgs e)
    {
        AddReward(CheckpointReward);
    }
    private void AddWinReward(object sender, EventArgs e)
    {
        AddReward(WinReward);
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        //rocket.ResetLvl();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        int count = 0;
        //base.CollectObservations(sensor);
        // convert the transform position of the rocket in range[0,1] (normalized)
        float z = (rocket.transform.position.z - TUNNEL_Zoffset)/TUNNEL_WIDTH;
        float y = (rocket.transform.position.y - TUNNEL_Yoffset) / TUNNEL_HEIGHT;
        sensor.AddObservation(y);count++; sensor.AddObservation(z);count++;

        // if there is a obstacle spawning object
        // we can those parameters also here, as for this,
        // the obstacle course are fixed and not endless spawning, we dont need those
        sensor.AddObservation(rocket.GetVelocity()); count += 3;

        // total no.of observations added here = 5
        // so dont forget to keep "Vector Space Observation"
        Parameters.BrainParameters.VectorObservationSize = count;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        bool Space, Shift, W, A, S, D;
        /*
        -----------------------------------------------------------------------
        Decrete Actions to Input Mapping:
        
        1.  DiscreteBranch[0] = to handle rocket thrusters
            >>>> switch (val)
                0:  Input.Space = Not Pressed,   Input.Shift = Not Pressed,
                1:  Input.Space = Pressed,       Input.Shift = Not Pressed,
                2:  Input.Space = Pressed,       Input.Shift = Pressed,
        
        2.  DiscreteBranch[1] = to handle rotation in global x-axis
        {if invert_crtls = false}
        >>>> switch (val)
                0:  Input.D = Not Pressed,  Input.A = Not Pressed,
                1:  Input.D = Pressed,      Input.A = Not Pressed,
                2:  Input.D = Not Pressed,      Input.A = Pressed,
        {if invert_crtls = true}
        >>>> switch (val)
                0:  Input.W = Not Pressed,  Input.S = Not Pressed,
                1:  Input.W = Pressed,      Input.S = Not Pressed,
                2:  Input.W = Not Pressed,  Input.S = Pressed,

        3.  DiscreteBranch[2] = to handle rotation in global z-axis
        {if invert_crtls = false}
        >>>> switch (val)
                0:  Input.S = Not Pressed,  Input.W = Not Pressed,
                1:  Input.S = Pressed,      Input.W = Not Pressed,
                2:  Input.S = Not Pressed,  Input.W = Pressed,
        {if invert_crtls = true}
        >>>> switch (val)
                0:  Input.D = Not Pressed,  Input.A = Not Pressed,
                1:  Input.D = Pressed,      Input.A = Not Pressed,
                2:  Input.D = Not Pressed,  Input.A = Pressed,
        -----------------------------------------------------------------------
         */
        switch (actions.DiscreteActions[0])
        {
            case 1: 
                Space = true; Shift = false;
                break;
            case 2:
                Space = true; Shift = true;
                break;
            default: 
                Space = false; Shift = false; 
                break;
        }
        if (!rocket.InvertCtrls)
        {
            switch (actions.DiscreteActions[1])
            {
                case 1:
                    D = true; A = false;
                    break;
                case 2:
                    D = false; A = true;
                    break;
                default:
                    D = false; A = false;
                    break;
            }
            switch (actions.DiscreteActions[2])
            {
                case 1:
                    S = true; W = false;
                    break;
                case 2:
                    S = false; W = true;
                    break;
                default:
                    S = false; W = false;
                    break;
            }
        }
        else
        {
            switch (actions.DiscreteActions[1])
            {
                case 1:
                    W = true; S = false;
                    break;
                case 2:
                    W = false; S = true;
                    break;
                default:
                    W = false; S = false;
                    break;
            }
            switch (actions.DiscreteActions[2])
            {
                case 1:
                    D = true; A = false;
                    break;
                case 2:
                    D = false; A = true;
                    break;
                default:
                    D = false; A = false;
                    break;
            }
        }

        // call player script functionalities
        rocket.ThrusterBoost(Space, Shift);
        rocket.RotationControl(W, A, S, D);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //ActionSegment<int> DiscreteActions = actionsOut.DiscreteActions;

        // 1st branch of discrete actions
        /*
        int val = 0;
        if (inputs[0] && inputs[1]) val = 2;
        else if (inputs[0]) val = 1;
        DiscreteActions[0] = val;
        */
        // 2nd branch of discrete actions
        // 3rd branch need to be done in similar fashion
        // to lazy to do this again and again
        // so have added a bool to activate the original movement in rocket script itself


        CheckOveride();
    }
}
