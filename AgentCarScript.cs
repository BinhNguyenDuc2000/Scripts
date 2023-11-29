using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCarScript : CarScript 
{
    private bool isCollied;
    // Controlling Agent
    private CarControllerAgent carControllerAgent;

    // Steering Wheel Component
    [SerializeField] private GameObject steeringWheelObject;

    // Setters
    public void SetCarControllerAgent(CarControllerAgent carControllerAgent)
    {
        this.carControllerAgent = carControllerAgent;
        this.rBody = GetComponent<Rigidbody>();
        isCollied = false;
    }

    // Events    
    private void OnCollisionEnter(Collision other) {
        if (!isCollied)
        {
            carControllerAgent.Crash(1f);
            isCollied = true;
        }
    }

    // Update at every physics frame
    private void FixedUpdate()
    {
        UpdateSteeringWheel();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        Vector3 newVelocity = rBody.velocity;
        float newCarSpeed = newVelocity.magnitude;
        acceleration = (float)(((int)((Vector3.Distance(newVelocity, velocity) / Time.fixedDeltaTime) * 100))) / 100f;
        carSpeed = (float)((int) (newCarSpeed * 10 * 3.6f) ) / 10f;
        velocity = newVelocity;
    }

    private void UpdateSteeringWheel()
    {
        Vector3 pos = new Vector3(15, 0, -horizontalInput * 450);
        steeringWheelObject.transform.localEulerAngles = pos;
    }
}