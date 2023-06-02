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

    // Inputs
    private float horizontalInput = 0;
    private float verticalInput = 0;
    private float steerAngle = 0;
    private float brakeInput = 0;

    // Car object
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;
    [SerializeField] private int maxReadyDelay;

    [SerializeField] private RoadsManager roadsManager;
    [SerializeField] private GameObject carControllerRuleBased;

    // Steering Wheel Controller
    [SerializeField] private CustomSteeringWheel customSteeringWheel;
    // Steering Wheel Component
    [SerializeField] private GameObject steeringWheelObject;
    

    // 
    private Rigidbody rBody;
    private float carSpeed;
    private Vector3 velocity;
    private float acceleration;
    private Quaternion rotation;
    private float delay;
    private int readyDelay;
    private RigidbodyConstraints originalConstraint;
    private Vector3 originalPosition;

    public void Start()
    {
        rBody = GetComponent<Rigidbody>();
        velocity = new Vector3(0,0,0);
        rotation = rBody.rotation;
        originalConstraint = rBody.constraints;
        originalPosition = transform.localPosition;

        // Events handler
        roadsManager.OnAgentCorrectRoad += roadsManager_OnAgentCorrectRoad;
        roadsManager.OnAgentCorrectLastRoad += roadsManager_OnAgentCorrectLastRoad;
    }

    //Agents
    public override void OnEpisodeBegin()
    {
        OnAgentEndEpisode?.Invoke(this, System.EventArgs.Empty);
        roadsManager.ResetRoadsManager();
        readyDelay = maxReadyDelay;
        Ready();
        delay = 0;
        carSpeed = 0;
        acceleration = 0;
        verticalInput = 0;
        transform.rotation = rotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity);

        // Ready time
        sensor.AddObservation(readyDelay);

        // Rule Based car
        sensor.AddObservation(carControllerRuleBased.transform.localPosition);
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (readyDelay > 0)
        {
            return;
        }
        horizontalInput = actionBuffers.ContinuousActions[2];
        horizontalInput = Mathf.Min(horizontalInput, maxSteeringAngle);
        horizontalInput = Mathf.Max(horizontalInput, -maxSteeringAngle);
        if(verticalInput <= 0)
            verticalInput = - Input.GetAxis("Reverse");
        if(actionBuffers.ContinuousActions[0]<=0)
        {
            verticalInput = 0f;
            if (actionBuffers.ContinuousActions[1] > 0)
            {
                brakeInput = actionBuffers.ContinuousActions[1];
                SetReward(-0.001f);
            }
            else
            {
                brakeInput = 0f;
            }
        }
        else
        {
            verticalInput = actionBuffers.ContinuousActions[0];
            brakeInput = 0f;
        }

        // Fell off platform
        if (this.transform.localPosition.y < -1f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        
    }

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
    public void roadsManager_OnAgentCorrectRoad(object sender, System.EventArgs e) 
    {
        if (carSpeed >= 40f)
            AddReward(0.5f);
        else
            AddReward(0.1f);
        if (transform.localPosition.x >= -5 && transform.localPosition.x < 7)
            AddReward(0.1f);
        if (transform.localPosition.x >= 4 && transform.localPosition.x <= 6)
            AddReward(0.4f);
        
    }

    public void roadsManager_OnAgentCorrectLastRoad(object sender, System.EventArgs e) 
    {
        if (carSpeed >= 40f)
            AddReward(0.5f);
        else
            AddReward(0.1f);
        if (transform.localPosition.x >= -5 && transform.localPosition.x < 7)
            AddReward(0.1f);
        if (transform.localPosition.x >= 4 && transform.localPosition.x <= 6)
            AddReward(0.4f);
        EndEpisode();
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-0.5f);
            readyDelay = maxReadyDelay;
            transform.rotation = roadsManager.GetNextCheckpoint().transform.rotation;
            carSpeed = 0;
            acceleration = 0;
            verticalInput = 0;
            EndEpisode();
        }
        if (other.gameObject.TryGetComponent<CarControllerRuleBased>(out CarControllerRuleBased car))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision other) {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-0.5f);
            readyDelay = maxReadyDelay;
            transform.rotation = roadsManager.GetNextCheckpoint().transform.rotation;
            carSpeed = 0;
            acceleration = 0;
            verticalInput = 0;
            EndEpisode();
        }
        if (other.gameObject.TryGetComponent<CarControllerRuleBased>(out CarControllerRuleBased car))
        {
            SetReward(-0.5f);
            EndEpisode();
        }
    }

    //Getters
    public float GetCarSpeed()
    {
        return carSpeed;
    }

    public float GetSteerAngle()
    {
        return steerAngle;
    }

    public float GetAcceleration()
    {
        return acceleration;
    }

    public float GetBrakeInput()
    {
        return brakeInput;
    }

    public int GetReadyDelayTime()
    {
        return readyDelay;
    }


    //Car physics
    private void Ready()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = originalPosition;
        rBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        frontLeftWheelCollider.brakeTorque = Mathf.Infinity;
        frontRightWheelCollider.brakeTorque = Mathf.Infinity;
        rearLeftWheelCollider.brakeTorque = Mathf.Infinity;
        rearRightWheelCollider.brakeTorque = Mathf.Infinity;
        customSteeringWheel.StopSteeringWheel();
    }

    private void Go()
    {
        OnAgentGo?.Invoke(this, System.EventArgs.Empty);
        rBody.constraints = originalConstraint;
        frontLeftWheelCollider.brakeTorque = 0;
        frontRightWheelCollider.brakeTorque = 0;
        rearLeftWheelCollider.brakeTorque = 0;
        rearRightWheelCollider.brakeTorque = 0;
        customSteeringWheel.StartSteeringWheel();
    }

    private void FixedUpdate()
    {
        UpdateSteeringWheel();
        // Break on start up to stop wheel from rolling at the begining
        if (readyDelay > 1)
        {
            readyDelay--;
            Ready();
        }

        if (readyDelay == 1)
        {
            readyDelay--;
            Go();
        }

        delay += Time.fixedDeltaTime;
        if (delay >= 50)
        {
            AddReward(-1f);
            EndEpisode();
        }
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        Vector3 newVelocity = rBody.velocity;
        float newCarSpeed = newVelocity.magnitude;
        acceleration = (float)(((int)((Vector3.Distance(newVelocity, velocity) / Time.fixedDeltaTime) * 100))) / 100f;
        carSpeed = (float)((int) (newCarSpeed * 10 * 3.6f) ) / 10f;
        velocity = newVelocity;
    }

    private void HandleSteering()
    {
        steerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        frontLeftWheelCollider.brakeTorque = brakeInput * brakeForce;
        frontRightWheelCollider.brakeTorque = brakeInput * brakeForce;
        rearLeftWheelCollider.brakeTorque = brakeInput * brakeForce;
        rearRightWheelCollider.brakeTorque = brakeInput * brakeForce;
    }

    private void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }

    private void UpdateSteeringWheel()
    {
        Vector3 pos = new Vector3(15, 0, -horizontalInput * 450);
        steeringWheelObject.transform.eulerAngles = pos;
    }

}