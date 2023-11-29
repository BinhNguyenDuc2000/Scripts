using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerRuleBased : MonoBehaviour
{
    // Agent
    [SerializeField] private CarControllerAgent carControllerAgent;

    [SerializeField] private GameObject originalCar; 
    private GameObject car;
    private CarScript carScript;

    // PID for gas, skipping I value
    [SerializeField] private float verticalPID; // Master value, set to 0 to disable
    [SerializeField] private float verticalP;
    [SerializeField] private float verticalD;
    [SerializeField] private float verticalAggressiveness; // How much speed remain when blocked by an obstacle
    private float carSpeed;
    private float verticalInput;
    private float brakeInput;
    private float idealVertical;
    private float verticalError;
    private float previousVerticalError;

    public void Start()
    {
        verticalError = 0;
        idealVertical = 0;
        carSpeed = 0;
        carControllerAgent.OnAgentEndEpisode += carControllerAgent_OnAgentEndEpisode;
        carControllerAgent.OnAgentGo += carControllerAgent_OnAgentGo;
    }

    // Events handler
    public void carControllerAgent_OnAgentEndEpisode(object sender, System.EventArgs e) 
    {
        if (car != null)
        {
            Destroy(car, 0f);
        }
        car = Instantiate(originalCar);
        car.SetActive(true);
        carScript = car.GetComponent<CarScript>();
        idealVertical = 0f;
    }
    
    public void carControllerAgent_OnAgentGo(object sender, System.EventArgs e) 
    {
        idealVertical = 30f;
    }

    // PID
    protected void CalculateVerticalPID()
    {
        previousVerticalError = verticalError;
        // int layerMask = 1 << 6;
        // layerMask = ~ layerMask; //ignore checkpoint
        // int layerMask2 = 1 << 9;
        // layerMask2 = ~ layerMask; //ignore wall
        // layerMask = layerMask & layerMask2;
        // RaycastHit hit;
        // Vector3 raycastPosition = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + 2f); 
        // if (Physics.Raycast(raycastPosition, transform.TransformDirection(Vector3.forward), out hit, 40f, layerMask))
        // {
        //     Debug.Log(hit.distance);
        //     Debug.DrawRay(raycastPosition, transform.TransformDirection(Vector3.forward) * 40f, Color.yellow);
        //     if (hit.distance > 10f)
        //     {
        //         verticalError = verticalAggressiveness * idealVertical * hit.distance / 40f - carSpeed;
        //     }
        //     else
        //     {
                
        //         verticalError = -carSpeed;
        //     }
        // }
        // else
        // {
        //     Debug.DrawRay(raycastPosition, transform.TransformDirection(Vector3.forward) * 40f, Color.white);
        verticalError = idealVertical - carSpeed;
        // }
        
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
        carScript.SetInputs(0, verticalInput, brakeInput);
    }

    private void FixedUpdate()
    {
        if (carScript == null)
        {
            return;
        }
        carSpeed = carScript.GetCarSpeed();
        // Debug.Log(carSpeed);
        CalculateVerticalPID();

    }

    // Getters
    public Vector3 GetPosition()
    {
        if (car!=null)
            return car.transform.position;
        return Vector3.zero;
    }

}
