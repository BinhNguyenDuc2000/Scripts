using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DataRecorder : MonoBehaviour
{   
    // Record events
    [SerializeField] private CarControllerAgent carControllerAgent;
    [SerializeField] private float interval;
    private float timer;
    private float nextInterval;
    private List<DrivingData> drivingDataList;

    [Serializable]
    private class DrivingData
    {
        public float xPosition;
        public float zPosition;
        public float speedX;
        public float speedZ;
        public float timer;

        public DrivingData(float xPosition, float zPosition, float speedX, float speedZ, float timer)
        {
            this.xPosition = xPosition;
            this.zPosition = zPosition;
            this.speedX = speedX;
            this.speedZ = speedZ;
            this.timer = timer;
        }
    }

    private void Awake()
    {
        drivingDataList = new List<DrivingData>();
        timer = 0f;
        nextInterval = interval;
    }

    // Record data when application close
    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void SaveData()
    {
        string json = JsonHelper.ToJson<DrivingData>(drivingDataList.ToArray());
        File.WriteAllText(Application.dataPath + "/Saves/Example" + carControllerAgent.GetExample() + ".txt", json);
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= nextInterval)
        {
            Vector3 position = carControllerAgent.GetAgentCarPosition();
            Vector3 velocity = carControllerAgent.GetVelocity();
            DrivingData drivingData = new DrivingData(position.x, position.z, velocity.x, velocity.z, timer);
            drivingDataList.Add(drivingData);
            nextInterval += interval;
        }
    }
}

public static class JsonHelper
{
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, true);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}