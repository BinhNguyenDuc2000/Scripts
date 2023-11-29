using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour 
{
    // Inputs
    protected float horizontalInput = 0;
    protected float verticalInput = 0;
    protected float steerAngle = 0;
    protected float brakeInput = 0;

    // Car game objects
    [SerializeField] protected WheelCollider frontLeftWheelCollider;
    [SerializeField] protected WheelCollider frontRightWheelCollider;
    [SerializeField] protected WheelCollider rearLeftWheelCollider;
    [SerializeField] protected WheelCollider rearRightWheelCollider;
    [SerializeField] protected Transform frontLeftWheelTransform;
    [SerializeField] protected Transform frontRightWheelTransform;
    [SerializeField] protected Transform rearLeftWheelTransform;
    [SerializeField] protected Transform rearRightWheelTransform;

    [SerializeField] protected float maxSteeringAngle;
    [SerializeField] protected float motorForce;
    [SerializeField] protected float brakeForce;
    
    protected float carSpeed;
    protected Vector3 velocity;
    protected float acceleration;
    protected Rigidbody rBody;

    // Getters
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

    public Vector3 GetVelocity()
    {
        return GetComponent<Rigidbody>().velocity;
    }

    // Setters
    public void SetInputs(float horizontalInput, float verticalInput, float brakeInput)
    {
        this.horizontalInput = horizontalInput;
        this.verticalInput = verticalInput;
        this.brakeInput = brakeInput;
    }

    // Update at every physics frame
    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        Vector3 newVelocity = GetComponent<Rigidbody>().velocity;
        float newCarSpeed = newVelocity.magnitude;
        acceleration = (float)(((int)((Vector3.Distance(newVelocity, velocity) / Time.fixedDeltaTime) * 100))) / 100f;
        carSpeed = (float)((int) (newCarSpeed * 10 * 3.6f) ) / 10f;
        velocity = newVelocity;
    }

    protected void HandleSteering()
    {
        steerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    protected void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        frontLeftWheelCollider.brakeTorque = brakeInput * brakeForce;
        frontRightWheelCollider.brakeTorque = brakeInput * brakeForce;
        rearLeftWheelCollider.brakeTorque = brakeInput * brakeForce;
        rearRightWheelCollider.brakeTorque = brakeInput * brakeForce;
    }

    protected void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform);
    }

    protected void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }
}