using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadsManager : MonoBehaviour
{
    // Agent 
    [SerializeField] private CarControllerAgent carControllerAgent;

    private List<Road> roadList;
    private int nextRoad;

    private void Awake()
    {
        nextRoad = 0;
        roadList = new List<Road>(); 
        foreach (Road road in GetComponentsInChildren<Road>())
        {
            road.SetRoadsManager(this);
            roadList.Add(road);
        }
    }

    // Events
    public void AgentWentThrough(Road road, float x)
    {
        if (roadList.IndexOf(road) == roadList.Count - 2)
        {
            carControllerAgent.onAgentCorrectLastRoad();
            ResetRoadsManager();
        }

        if (roadList.IndexOf(road) == nextRoad)
        {
            carControllerAgent.onAgentCorrectRoad();
            nextRoad = (roadList.IndexOf(road) + 1) % roadList.Count;
        }
    }

    public void ResetRoadsManager()
    {
        nextRoad = 0;
    }

    public Road GetNextCheckpoint()
    {
        return roadList[nextRoad];
    }

}


