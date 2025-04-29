using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    
    public TMP_Text timerText;
    public TMP_Text[] myLapTimerText;
    public bool canTimer = false;
    
    private float secondTimer;
    private int minuteTimer;
    private string lapTimer;
    private bool isLapUpdated = false;
    private float previousTimer = 0;
    private float currentTimer = 0;

    private int i = 0;
    
    [SerializeField] private List<string> finishTimers = new List<string>();

    private void Awake()
    {
        Instance = this;
        
        secondTimer = 0f;
        minuteTimer = 0;
    }

    private void Update()
    {
        if (canTimer == false) return;
        
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

    public string GetMyLapTimer()
    {
        currentTimer = secondTimer + minuteTimer * 60 - previousTimer;
        lapTimer = ((int)(currentTimer / 60)).ToString("0") + "'" + (currentTimer % 60).ToString("00.000");
        previousTimer = secondTimer + minuteTimer * 60;
        
        return lapTimer;
    }
    
    public void UpdateMyLapTimerUI()
    {
        if (isLapUpdated) return;
        
        myLapTimerText[i].text = GetMyLapTimer();
        isLapUpdated = true;
        
        Invoke(nameof(UpdateUIIndex), 30f);
    }

    private void UpdateUIIndex()
    {
        i++;
        isLapUpdated = false;
    }
    
    [SerializeField] private List<string> finishedCarNames = new List<string>();
    [SerializeField] private TMP_Text countdownText;

    private bool countdownStarted = false;
    private float countdownTime = 10f;
    
    public void UpdateFinishTimer(string carName, string lapTimer)
    {
        finishedCarNames.Add(carName);
        finishTimers.Add(lapTimer);
        
        if (!countdownStarted && finishTimers.Count == 1)
        {
            StartCoroutine(StartCountdown());
            countdownStarted = true;
        }
    }
    
    private IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        
        float timer = countdownTime;

        while (timer > 0)
        {
            countdownText.text = Mathf.Ceil(timer).ToString();
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        countdownText.text = "0";
        yield return new WaitForSeconds(1f);

        timerText.transform.parent.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        UpdateEndInfo();
    }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text[] nameText;
    [SerializeField] private TMP_Text[] finishTimerText;
    
    private void UpdateEndInfo()
    {
        foreach (var car in RankingManager.Instance.allCars)
        {
            if (finishedCarNames.Contains(car.name) == false)
            {
                finishedCarNames.Add(car.name);
            }
        }
        for (int i = 0; i < finishTimerText.Length; i++)
        {
            if (finishedCarNames[i] == null) return;

            nameText[i].text = finishedCarNames[i];

            if (i < finishTimers.Count)
            {
                finishTimerText[i].text = finishTimers[i];
            }
            else
            {
                finishTimerText[i].text = "Retired";
            }
        }
        
        resultPanel.SetActive(true);
    }
}
