using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIProgressTracker : MonoBehaviour, IProgressProvider
{
    public AIPlayerInput aiDriver;
    
    public LapCounter lapCounter; 
    
    public float GetProgress()
    {
        return aiDriver.currentSplineIndex + aiDriver.progress;
    }
    
    public int GetLap()
    {
        return lapCounter != null ? lapCounter.currentLap : 0;
    }

    public float GetPreciseProgress()
    {
        if (aiDriver == null)
        {
            Debug.LogWarning($"[AIProgressTracker] aiDriver is null on {gameObject.name}");
            return 0f;
        }

        return aiDriver.GetPreciseProgress();
    }
}
