using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveGear : IGearStrategy
{
    public void UpdateState(CarController car)
    {
        float pedalInput = Input.GetAxis("Vertical");

        if (pedalInput < 0)
        {
            car.SetGearStrategy(new ReverseGear());
        }
        else if (Mathf.Approximately(pedalInput, 0f))
        {
            
        }
    }

    public void ApplyMotor(CarController car)
    {
        float pedalInput = Mathf.Clamp01(Input.GetAxis("Vertical"));
        
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
        float pedalInput = Mathf.Clamp01(Input.GetAxis("Vertical"));
        
        car.currentRPM = Mathf.Lerp(car.currentRPM, car.maxRPM, car.gearRatio[car.currentGear - 1] * pedalInput);

        if (car.currentRPM >= car.gearUpRPM && car.currentGear < car.gearRatio.Length)
        {
            car.currentRPM = car.gearDownRPM + 200f;
            car.currentGear++;
        }
        else if (car.currentRPM <= car.gearDownRPM && car.currentGear > 1)
        {
            car.currentRPM = car.gearUpRPM - 200f;
            car.currentGear--;
        }
    }
}
