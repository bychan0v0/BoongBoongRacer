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
        return aiDriver.GetPreciseProgress();
    }
}
