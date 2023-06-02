using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerRuleBased : MonoBehaviour
{
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

    // Agent
    [SerializeField] private CarControllerAgent carControllerAgent;

    //
    private Rigidbody rBody;
    private float carSpeed;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private RigidbodyConstraints originalConstraint;

    // PID for gas, skipping I value
    [SerializeField] private float verticalPID; // Master value, set to 0 to disable
    [SerializeField] private float verticalP;
    [SerializeField] private float verticalD;
    [SerializeField] private float verticalAggressiveness; // How much speed remain when blocked by an obstacle
    private float idealVertical;
    private float verticalError;
    private float previousVerticalError;

    // PID for lane swap
    [SerializeField] private float horizontalPID; // Master value, set to 0 to disable
    [SerializeField] private float horizontalP;
    [SerializeField] private float horizontalI;
    [SerializeField] private float horizontalD;
    [SerializeField] private float[] laneList;
    private int lane;
    private float horizontalError;
    private float previousHorizontalError;
    private float cumulativeHorizontalError;

    public void Start()
    {
        rBody = GetComponent<Rigidbody>();
        verticalError = 0;
        horizontalError = 0;
        cumulativeHorizontalError = 0;
        lane = 0;
        idealVertical = 0;
        originalPosition = transform.localPosition;
        originalRotation = transform.rotation;
        carControllerAgent.OnAgentEndEpisode += carControllerAgent_OnAgentEndEpisode;
        carControllerAgent.OnAgentGo += carControllerAgent_OnAgentGo;
        originalConstraint = rBody.constraints;
    }

    public void SwitchLane()
    {
        lane = (lane + 1) % laneList.Length;
        cumulativeHorizontalError = 0;
    }

    // Events handler
    public void carControllerAgent_OnAgentEndEpisode(object sender, System.EventArgs e) 
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        Vector3 newLocalPosition = new Vector3(originalPosition.x + Random.Range(-0.5f, 0.5f), originalPosition.y, originalPosition.z + Random.Range(-5f, 5f));
        transform.localPosition = newLocalPosition;
        rBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        idealVertical = 0f;
        transform.rotation = originalRotation;
    }
    
    public void carControllerAgent_OnAgentGo(object sender, System.EventArgs e) 
    {
        idealVertical = 30f;
        rBody.constraints = originalConstraint;
    }

    // PID
    protected void CalculateVerticalPID()
    {
        previousVerticalError = verticalError;
        int layerMask = 1 << 6;
        layerMask = ~ layerMask; //ignore checkpoint
        int layerMask2 = 1 << 9;
        layerMask2 = ~ layerMask; //ignore wall
        layerMask = layerMask & layerMask2;
        RaycastHit hit;
        Vector3 raycastPosition = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + 2f); 
        if (Physics.Raycast(raycastPosition, transform.TransformDirection(Vector3.forward), out hit, 40f, layerMask))
        {
            Debug.Log(hit.distance);
            Debug.DrawRay(raycastPosition, transform.TransformDirection(Vector3.forward) * 40f, Color.yellow);
            if (hit.distance > 10f)
            {
                verticalError = verticalAggressiveness * idealVertical * hit.distance / 40f - carSpeed;
            }
            else
            {
                
                verticalError = -carSpeed;
            }
        }
        else
        {
            Debug.DrawRay(raycastPosition, transform.TransformDirection(Vector3.forward) * 40f, Color.white);
            verticalError = idealVertical - carSpeed;
        }
        
        float p = verticalError;
        float d = (verticalError - previousVerticalError)/Time.fixedDeltaTime;
        float pid = verticalPID * (p * verticalP + d * verticalD);
        if (pid > 0)
        {
            verticalInput = Mathf.Min(p * verticalP + d * verticalD, 1f); 
            brakeInput = 0;
        }
        else
        {
            verticalInput = 0;
            brakeInput = 0 - Mathf.Max(0.1f * (p * verticalP + d * verticalD), -1f); 
        } 
    }

    protected void CalculateHorizontalPID()
    {
        previousHorizontalError = horizontalError;
        horizontalError = laneList[lane] - rBody.position.x;
        cumulativeHorizontalError += horizontalError;
        float p = horizontalError;
        float d = (horizontalError - previousHorizontalError)/Time.fixedDeltaTime;
        float pid = horizontalPID * (p * horizontalP + cumulativeHorizontalError * horizontalI + d * horizontalD);
        if (pid > 0)
        {
            horizontalInput = Mathf.Min(pid, 1);
        }
        else
        {
            horizontalInput = Mathf.Max(pid, -1);
        }
    }

    private void FixedUpdate()
    {
        Vector3 newVelocity = rBody.velocity;
        float newCarSpeed = newVelocity.magnitude;
        // carSpeed = (float)((int) (newCarSpeed * 10 * 3.6f) ) / 10f;
        carSpeed = newCarSpeed * 3.6f;
        // Debug.Log(carSpeed);
        CalculateHorizontalPID();
        CalculateVerticalPID();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

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
}
