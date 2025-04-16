using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGearStrategy
{
    void UpdateState(CarController car);
    void ApplyMotor(CarController car);
    void UpdateRPM(CarController car);
}
