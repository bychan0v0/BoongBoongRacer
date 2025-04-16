using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeGear : IGearStrategy
{
    public void UpdateState(CarController car)
    {
        float pedal = Input.GetAxis("Vertical");
        if (pedal > 0)
            car.SetGearStrategy(new DriveGear());
        else if (pedal < 0 && car.currentSpeed < 0.5f)
            car.SetGearStrategy(new ReverseGear());
        else if (Mathf.Approximately(pedal, 0f))
            car.SetGearStrategy(new NeutralGear());
    }

    public void ApplyMotor(CarController car)
    {
        float brakeInput = Mathf.Abs(Input.GetAxis("Vertical"));
        car.SetMotorTorque(0f);
        car.SetBrakeTorque(brakeInput);
    }

    public void UpdateRPM(CarController car)
    {
        float brakeInput = Mathf.Abs(Input.GetAxis("Vertical"));
        car.currentRPM = Mathf.Lerp(car.currentRPM, 0f, brakeInput * 0.5f);
    }
}
