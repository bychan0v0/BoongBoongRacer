using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public static LapManager Instance;
    
    public TMP_Text lapText;
    public MyLapCounter myLapCounter;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateLapUI()
    {
        lapText.text = myLapCounter.currentLap + " / " + myLapCounter.maxLap;
    }
}
