using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MyLapCounter : MonoBehaviour
{
    public int currentLap = 0;
    public int maxLap;

    private Rigidbody rigidBody;
    private bool canCountLap = true;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishTrigger") && canCountLap)
        {
            Vector3 carForward = rigidBody.velocity.normalized;
            Vector3 finishForward = other.transform.right;
            float dot = Vector3.Dot(carForward, finishForward);

            if (dot > 0.5f) // 정방향 통과
            {
                if (currentLap.Equals(maxLap))
                {
                    string finishTimer = TimerManager.Instance.GetLapTimer();
                    
                    TimerManager.Instance.UpdateFinishTimer(gameObject.name, finishTimer);
                    
                    CarController_Old carController = gameObject.GetComponent<CarController_Old>();
                    if (carController != null)
                    {
                        carController.canDrive = false;
                    }
                    
                }

                if (currentLap > 0 && currentLap <= maxLap)
                {
                    TimerManager.Instance.UpdateMyLapTimerUI();
                }
                
                currentLap++;
                StartCoroutine(LapCooldown());
                
                if (currentLap <= maxLap)
                {
                    LapManager.Instance.UpdateLapUI();
                }
            }
        }
    }
    
    private IEnumerator LapCooldown()
    {
        canCountLap = false;
        yield return new WaitForSeconds(30f);
        canCountLap = true;
    }
}

