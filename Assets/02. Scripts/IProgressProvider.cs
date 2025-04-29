using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgressProvider
{
    float GetProgress();
    int GetLap();
    float GetPreciseProgress();
}
