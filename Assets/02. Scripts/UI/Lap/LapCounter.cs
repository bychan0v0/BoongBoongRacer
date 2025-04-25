using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapCounter : MonoBehaviour
{
    public int CurrentLap { get; private set; } = 0;
    public int TotalLaps = 3;

    private bool hasCrossedStart = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishLine"))
        {
            if (!hasCrossedStart)
            {
                CurrentLap++;
                hasCrossedStart = true;
                Debug.Log($"Lap {CurrentLap} started via trigger for {gameObject.name}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FinishLine"))
        {
            hasCrossedStart = false;
        }
    }
}

