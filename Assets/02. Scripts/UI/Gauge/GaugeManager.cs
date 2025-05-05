using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GaugeManager : MonoBehaviour
{
    public static GaugeManager Instance;
    
    [Header("Player")]
    private CarController_Old player;

    [Header("UI")] 
    public RectTransform needle;
    public Sprite driveImage;
    public Sprite reverseImage;
    public Image gearImage;
    public TMP_Text gearStageText;
    public TMP_Text speedText;
    
    private float niddleStart;
    private float niddleEnd;
    private float needleCurrentAngle;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gearImage.sprite = driveImage;

        niddleStart = 245f;
        niddleEnd = 475f;
    }

    private void Update()
    {
        float rpmRatio = Mathf.Clamp01(player.currentRPM / player.maxRPM);
        float targetAngle = Mathf.Lerp(niddleStart, niddleEnd, rpmRatio);
        needleCurrentAngle = Mathf.Lerp(needleCurrentAngle, targetAngle, Time.deltaTime * 10f);
        needle.rotation = Quaternion.Euler(0, 180, needleCurrentAngle);

        gearImage.sprite = player.currentGearState == GearState_Old.Reverse ? reverseImage : driveImage;
        gearStageText.text = player.currentGear > 0 ? player.currentGear.ToString() : "N";
        speedText.text = Mathf.RoundToInt(player.currentSpeed * 3.6f).ToString();
    }
    
    public void SetTarget(GameObject target)
    {
        player = target.GetComponent<CarController_Old>();
    }
}
