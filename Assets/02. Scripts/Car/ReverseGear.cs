using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseGear : IGearStrategy
{
    public void UpdateState(CarController car)
    {
        float pedalInput = Input.GetAxis("Vertical");

        if (pedalInput > 0)
        {
            car.SetGearStrategy(new DriveGear());
        }
        else if (Mathf.Approximately(pedalInput, 0f))
        {
            car.SetGearStrategy(new NeutralGear());
        }
    }

    public void ApplyMotor(CarController car)
    {
        float pedalInput = Mathf.Abs(Input.GetAxis("Vertical"));
        
        if (car.currentSpeed < car.maxSpeed)
        {
            float torque = car.CalculateTorque();
            car.SetMotorTorque(torque * pedalInput);
        }
        else
        {
            car.SetMotorTorque(0f);
        }
    }

    public void UpdateRPM(CarController car)
    {
        float pedalInput = Mathf.Abs(Input.GetAxis("Vertical"));
        
        car.currentRPM = Mathf.Lerp(car.currentRPM, car.idleRPM, car.gearRatio[car.currentGear - 1] * pedalInput);
    }
}
