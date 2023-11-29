using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Road road;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Car")
        {
            road.AgentWentThrough(other.transform.position.x);   
        }
    }

    public void SetRoad(Road road)
    {
        this.road = road;
    }
}
