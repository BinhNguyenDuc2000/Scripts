using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
using TMPro;

public class TextScript : MonoBehaviour
{
    [SerializeField] private GameObject accelerationText;
    [SerializeField] private GameObject speedText;
    [SerializeField] private GameObject steerAngleText;
    [SerializeField] private GameObject brakeInputText;
    [SerializeField] private GameObject readyText;

    [SerializeField] private CarControllerAgent car;

    private TextMeshPro accelerationTextMeshPro;
    private TextMeshPro speedTextMeshPro;
    private TextMeshPro steerAngleTextMeshPro;
    private TextMeshPro brakeTextMeshPro;
    private TextMeshPro readyTextMeshPro;

    private void Start() {
        accelerationTextMeshPro = accelerationText.GetComponent<TextMeshPro>();
        speedTextMeshPro = speedText.GetComponent<TextMeshPro>();
        steerAngleTextMeshPro = steerAngleText.GetComponent<TextMeshPro>();
        brakeTextMeshPro = brakeInputText.GetComponent<TextMeshPro>();
        readyTextMeshPro = readyText.GetComponent<TextMeshPro>();
    }

    private void FixedUpdate()
    {
        // accelerationTextMeshPro.text = "Acceleration: " + car.GetAcceleration();
        // speedTextMeshPro.text = "Speed: " + car.GetCarSpeed();
        // steerAngleTextMeshPro.text = "Steer Angle: " + car.GetSteerAngle();
        // brakeTextMeshPro.text = "Brake: " + car.GetBrakeInput();

        if(car.GetReadyDelayTime() > 0)
            readyTextMeshPro.text = "" + (car.GetReadyDelayTime()/60 + 1);
        else   
            readyTextMeshPro.text = "";
    }
}
