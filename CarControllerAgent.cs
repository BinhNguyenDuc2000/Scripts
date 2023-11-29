using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarControllerAgent : Agent
{
    // EventsOnAgentGo
    public event System.EventHandler OnAgentEndEpisode;
    public event System.EventHandler OnAgentGo;

    //
    [SerializeField] private int example; 
    [SerializeField] private bool isRandomExample;
    [SerializeField] private bool isRandomPosition; 
    [SerializeField] private bool isPIDModel; 
    [SerializeField] private PIDModel pIDModel; 
    [SerializeField] private GameObject originalAgentCar; // The original agent car, from which copies will be spawn every episode
    [SerializeField] private RoadsManager roadsManager;
    [SerializeField] private CarControllerRuleBased carControllerRuleBased;

    // Steering Wheel Script
    [SerializeField] private CustomSteeringWheel customSteeringWheel;

    
    private GameObject agentCar = null; // The car that the agent control every episode
    private AgentCarScript agentCarScript; 
    [SerializeField] protected int maxReadyDelay;
    private int readyDelay;

    // Getters
    public int GetExample()
    {
        return example;
    }

    public float GetCarSpeed()
    {
        return agentCarScript.GetCarSpeed();
    }

    public float GetSteerAngle()
    {
        return agentCarScript.GetSteerAngle();
    }

    public float GetAcceleration()
    {
        return agentCarScript.GetAcceleration();
    }

    public float GetBrakeInput()
    {
        return agentCarScript.GetBrakeInput();
    }

    public Vector3 GetVelocity()
    {
        return agentCarScript.GetVelocity();
    }

    public Vector3 GetAgentCarPosition()
    {
        return agentCar.transform.position;
    }

    public int GetReadyDelayTime()
    {
        return readyDelay;
    }

    //  Agent
    private void Start() 
    {
        Ready();
    }

    public override void OnEpisodeBegin()
    {
        OnAgentEndEpisode?.Invoke(this, System.EventArgs.Empty);
        readyDelay = maxReadyDelay;
        pIDModel.ResetPIDModel();
        Ready();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(agentCar.transform.position);

        // Agent velocity
        sensor.AddObservation(agentCarScript.GetVelocity());

        // Ready time
        sensor.AddObservation(readyDelay);

        // Rule Based car
        sensor.AddObservation(carControllerRuleBased.GetPosition());
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (readyDelay <= 0)
        {
            if (isPIDModel)
            {
                float pid = pIDModel.CalculateVerticalPID();
                if (pid > 0)
                {
                    agentCarScript.SetInputs(pIDModel.CalculateHorizontalPID(), pid, 0);
                }
                else
                {
                    agentCarScript.SetInputs(pIDModel.CalculateHorizontalPID(), 0, -pid * 0.0001f);
                }
            }
            else
            {
                float horizontalInput, verticalInput, brakeInput;
                horizontalInput = actionBuffers.ContinuousActions[2];
                if(actionBuffers.ContinuousActions[0]==-1)
                {
                    brakeInput = actionBuffers.ContinuousActions[1];
                }
                else
                {
                    brakeInput = -1f;
                }
                verticalInput = actionBuffers.ContinuousActions[0];
                agentCarScript.SetInputs(horizontalInput, (verticalInput + 1f)/2f, (brakeInput + 1)/2f);
            }
        }
    }

    // Controls
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // continuousActionsOut[0] = Input.GetAxis("Gas");
        // continuousActionsOut[1] = Input.GetAxis("Brake");
        // continuousActionsOut[2] = Input.GetAxis("Horizontal");
        continuousActionsOut[0] = customSteeringWheel.GetGas();
        continuousActionsOut[1] = customSteeringWheel.GetBreak();
        continuousActionsOut[2] = customSteeringWheel.GetSteeringWheel();
    }

    //Events
    public void onAgentCorrectRoad() 
    {
        if (agentCarScript.GetCarSpeed() >= 40f)
            AddReward(1f);
        // else
        //     AddReward(0.5f);
        if (GetAgentCarPosition().x >= 0f)
            AddReward(0.5f);
        if (GetAgentCarPosition().x <= -4f)
            AddReward(-1f);
    }

    public void onAgentCorrectLastRoad() 
    {
        if (agentCarScript.GetCarSpeed() >= 40f)
            AddReward(1f);
        // else
        //     AddReward(0.5f);
        if (GetAgentCarPosition().x >= 0f)
            AddReward(0.5f);
        if (GetAgentCarPosition().x <= -2f)
            AddReward(-1f);
        EndEpisode();
    }

    // Update at every physic frames
    private void FixedUpdate()
    {
        if (readyDelay > 1)
        {
            readyDelay--;
            return;
        }

        if (readyDelay == 1)
        {
            readyDelay--;
            Go();
        }
    }

    // End the episode and punish agent
    public void Crash(float punishment)
    {
        AddReward(-punishment);
        EndEpisode();
    }

    //  Spawn Car at the start
    private void Ready()
    {
        // Stopping driving wheel controller
        customSteeringWheel.StopSteeringWheel();

        // Replacing old car with new one
        if (agentCar != null)
        {
            Destroy(agentCar);
        }

        // agentCar = Instantiate(originalAgentCar);
        if (isRandomExample)
        {
            example = UnityEngine.Random.Range(0, 3);
        }
        // Debug.Log(example);
        if (isRandomPosition)
        {
            agentCar = Instantiate(originalAgentCar, 
                            new Vector3(5 + example + UnityEngine.Random.Range(-0.5f, 0.5f), 0, 0),
                            originalAgentCar.transform.rotation);
        }
        else
        {
            agentCar = Instantiate(originalAgentCar, 
                            new Vector3(5 + example, 0, 0),
                            originalAgentCar.transform.rotation);
        }
        agentCar.SetActive(true);
        agentCarScript = agentCar.GetComponent<AgentCarScript>();
        agentCarScript.SetCarControllerAgent(this);
    }

    private void Go()
    {

        OnAgentGo?.Invoke(this, System.EventArgs.Empty);
        customSteeringWheel.StartSteeringWheel();
    }

}