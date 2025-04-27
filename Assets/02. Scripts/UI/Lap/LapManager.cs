using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public static LapManager Instance;
    
    public TMP_Text lapText;
    public LapCounter lapCounter;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateLapUI()
    {
        if (lapCounter.currentLap > lapCounter.maxLap) return;
        
        lapText.text = lapCounter.currentLap + " / " + lapCounter.maxLap;
    }
}
