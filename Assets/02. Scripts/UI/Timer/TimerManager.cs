using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    
    public TMP_Text timerText;
    
    public TMP_Text finishUI;
    public TMP_Text finishTimerText;

    private float secondTimer;
    private float minuteTimer;
    private string lapTimer;
    
    private List<string> finishTimers = new List<string>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        secondTimer = 0f;
        minuteTimer = 0f;
    }

    private void Update()
    {
        secondTimer += Time.deltaTime;
        
        if (secondTimer >= 60f)
        {
            secondTimer = 0f;
            minuteTimer++;
        }

        timerText.text = GetLapTimer();
    }

    public string GetLapTimer()
    {
        lapTimer = minuteTimer.ToString("0") + "'" + secondTimer.ToString("00.000");
        
        return lapTimer;
    }

    public void UpdateFinishTimerUI(string lapTimer)
    {
        finishTimers.Add(lapTimer);
    }
}
