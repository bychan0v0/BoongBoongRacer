using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameStartManager : MonoBehaviour
{
    public TMP_Text countdownText;
    public GameObject[] allCars;

    public float countdownTime = 3f;

    private void Start()
    {
        StartCoroutine(StartCountdown());
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

        countdownText.text = "START!";
        EnableAllCars();

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private void EnableAllCars()
    {
        foreach (var car in allCars)
        {
            CarController_Old carController = car.GetComponent<CarController_Old>();
            if (carController != null)
            {
                carController.canDrive = true;
                TimerManager.Instance.canTimer = true;
            }
        }
    }
}
