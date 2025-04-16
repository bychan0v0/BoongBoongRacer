using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralGear : IGearStrategy
{
    public void UpdateState(CarController car)
    {
        float pedalInput = Input.GetAxis("Vertical");
        
        if (pedalInput > 0)
        {
            car.SetGearStrategy(new DriveGear());
        }
        else if (pedalInput < 0)
        {
            car.SetGearStrategy(new ReverseGear());
        }
    }

    public void ApplyMotor(CarController car)
    {
        car.SetMotorTorque(0f);
    }

    public void UpdateRPM(CarController car)
    {
        car.currentRPM = Mathf.Lerp(car.currentRPM, car.idleRPM, Time.deltaTime);
    }
}
