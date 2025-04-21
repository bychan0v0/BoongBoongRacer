using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public CarController_Old carController;

    private float pedalInput;
    private float steeringInput;
    
    private void Update()
    {
        pedalInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        
        carController.pedalInput = pedalInput;
        carController.steeringInput = steeringInput;
    }
}
