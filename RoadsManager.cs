using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadsManager : MonoBehaviour
{
    //Events
    public event System.EventHandler OnAgentCorrectRoad;
    public event System.EventHandler OnAgentCorrectLastRoad;

    private List<Road> roadList;
    private List<Vector3> roadPositionList;
    private int nextRoad;

    private void Awake()
    {
        nextRoad = 0;
        roadList = new List<Road>();
        roadPositionList = new List<Vector3>();
        foreach (Road road in GetComponentsInChildren<Road>())
        {
            road.SetRoadsManager(this);
            roadList.Add(road);
            roadPositionList.Add(road.transform.localPosition);
        }
    }

    public void AgentWentThrough(Road road)
    {
        if (roadList.IndexOf(road) == roadList.Count - 2)
        {
            OnAgentCorrectLastRoad?.Invoke(this, System.EventArgs.Empty);
            ResetRoadsManager();
        }

        if (roadList.IndexOf(road) == nextRoad)
        {
            OnAgentCorrectRoad?.Invoke(this, System.EventArgs.Empty);
            nextRoad = (roadList.IndexOf(road) + 1) % roadList.Count;
        }
    }

    // Not needed
    public void RuleBasedCarWentThrough(Road road, Transform car)
    {
        // Switch lane randomly
        if (Random.Range(0f, 10f) >= 0.1f)
        {
            if (car.gameObject.TryGetComponent<CarControllerRuleBased>(out CarControllerRuleBased ruleBasedCar))
            {
                // Debug.Log("Switch");
                ruleBasedCar.SwitchLane();
            }
        }
    }

    public void ResetRoadsManager()
    {
        nextRoad = 0;
        for(int i=0; i < roadList.Count; i++)
        {
            roadList[i].transform.localPosition = roadPositionList[i]; 
        }
    }

    public Road GetNextCheckpoint()
    {
        return roadList[nextRoad];
    }
}
