using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    private RoadsManager roadsManager;
    private Checkpoint checkpoint;

    private void Awake()
    {
        checkpoint = GetComponentsInChildren<Checkpoint>()[0];
        checkpoint.SetRoad(this);
    }

    public void AgentWentThrough(float x)
    {
        roadsManager.AgentWentThrough(this, x);
    }

    public void SetRoadsManager(RoadsManager roadsManager)
    {
        this.roadsManager = roadsManager;
    }
}
