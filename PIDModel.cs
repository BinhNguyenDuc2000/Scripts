using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDModel : MonoBehaviour
{
    [SerializeField] private CarControllerAgent carControllerAgent;
    [SerializeField] private CarControllerRuleBased carControllerRuleBased;
    

    // PID for gas, skipping I value
    [SerializeField] private float verticalPID; // Master value, set to 0 to disable
    [SerializeField] private float verticalP;
    [SerializeField] private float verticalI;
    [SerializeField] private float verticalD;
    private float idealVertical = 60;
    private float verticalError;
    private float previousVerticalError;
    private float cumilativeVerticalError;

    // PID for lane swap
    [SerializeField] private float horizontalPID; // Master value, set to 0 to disable
    [SerializeField] private float horizontalP;
    [SerializeField] private float horizontalI;
    [SerializeField] private float horizontalD;
    private int idealHorizontal;
    private float horizontalError;
    private float previousHorizontalError;
    private float cumilativeHorizontalError;

    public float CalculateVerticalPID()
    {
        previousVerticalError = verticalError;
        verticalError = idealVertical - carControllerAgent.GetCarSpeed(); 
        cumilativeVerticalError += verticalError;
        float p = verticalError;
        float i = cumilativeVerticalError;
        float d = (verticalError - previousVerticalError)/Time.fixedDeltaTime;
        float pid = verticalPID * (p * verticalP + i * verticalI + d * verticalD);
        return Mathf.Clamp(pid, -1, 1);
    }

    public float CalculateHorizontalPID()
    {
        if (carControllerRuleBased.GetPosition().z >= carControllerAgent.GetAgentCarPosition().z)
        {
            idealHorizontal = -5;
        }
        else
        {
            idealHorizontal = 5;
        }
        previousHorizontalError = horizontalError;
        horizontalError = idealHorizontal - carControllerAgent.GetAgentCarPosition().x; 
        float p = horizontalError;
        float i = cumilativeHorizontalError;
        float d = (horizontalError - previousHorizontalError)/Time.fixedDeltaTime;
        float pid = horizontalPID * (p * horizontalP + i * horizontalI + d * horizontalD);
        return Mathf.Clamp(pid, -1, 1);
    }

    public void ResetPIDModel()
    {
        cumilativeHorizontalError = 0;
        cumilativeVerticalError = 0;
    }

}