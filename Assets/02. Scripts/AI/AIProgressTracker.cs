using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIProgressTracker : MonoBehaviour, IProgressProvider
{
    public AIPlayerInput aiDriver;

    public float GetProgress()
    {
        return aiDriver.currentSplineIndex + aiDriver.progress;
    }
}
