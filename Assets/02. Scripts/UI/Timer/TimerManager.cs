using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public TMP_Text timerText;

    private float secondTimer;
    private float minuteTimer;

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
        
        timerText.text = minuteTimer.ToString("0") + "'" + secondTimer.ToString("00.000");
    }
}
