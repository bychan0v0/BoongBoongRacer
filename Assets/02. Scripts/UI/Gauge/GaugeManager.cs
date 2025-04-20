using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    [Header("Player")]
    public CarController_Old player;

    [Header("UI")] 
    public RectTransform needle;
    public Sprite driveImage;
    public Sprite reverseImage;
    public Image gearImage;
    public TMP_Text gearStageText;
    public TMP_Text speedText;
    
    private float niddleStart;
    private float niddleEnd;
    
    private void Start()
    {
        gearImage.sprite = driveImage;

        niddleStart = 245f;
        niddleEnd = 475f;
    }

    private void Update()
    {
        float playerRPMRatio = player.currentRPM / player.maxRPM;
        // float needleAngle = Mathf.LerpAngle(niddleStart, niddleEnd, playerRPMRatio);
        // float needleAngle = playerRPMRatio * 230f + niddleStart;
        float needleAngle = Mathf.Lerp(niddleStart, niddleEnd, playerRPMRatio);
        
        needle.rotation = Quaternion.Euler(0, 180, needleAngle);
        
        switch (player.currentGearState)
        {
           case GearState_Old.Drive:
               gearImage.sprite = driveImage;
               break;
           case GearState_Old.Reverse:
               gearImage.sprite = reverseImage;
               break;
        }
        
        gearStageText.text = player.currentGear.ToString();
        speedText.text = ((int)(player.currentSpeed * 3.6f)).ToString();
    }
}
