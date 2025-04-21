using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerInput : MonoBehaviour
{
    [Header("Sensor Settings")]
    public Transform sensorOrigin;
    public float frontSensorLength = 30f;
    public float sideSensorLength = 15f;
    public LayerMask sensorMask;

    [Header("Vehicle Control")]
    public CarController_Old carController;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 40f;

    [Header("Behavior Thresholds")]
    public float obstacleAvoidanceThreshold = 6f;
    public float forwardClearanceThreshold = 10f;

    private float pedalInput = 0f;
    private float steeringInput = 0f;
    
    private void FixedUpdate()
    {
        SenseEnvironment();
        ApplyInputs();
    }

    private void SenseEnvironment()
    {
        float leftDist = CastSensor(-45f, sideSensorLength);
        float rightDist = CastSensor(45f, sideSensorLength);
        float forwardDist = CastSensor(0f, frontSensorLength);

        // Determine throttle
        if (forwardDist < forwardClearanceThreshold)
            pedalInput = -1f; // brake
        else
            pedalInput = Mathf.Lerp(0f, 1f, 0.9f); // accelerate

        // Determine steering
        float steerDelta = (rightDist - leftDist);
        steeringInput = Mathf.Clamp(steerDelta * 0.1f, -1f, 1f);
    }

    private float CastSensor(float angle, float sensorLength)
    {
        Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
        Ray ray = new Ray(sensorOrigin.position, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, sensorLength, sensorMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            return hit.distance;
        }
        else
        {
            Debug.DrawRay(ray.origin, dir * sensorLength, Color.green);
            return sensorLength;
        }
    }

    private void ApplyInputs()
    {
        carController.pedalInput = pedalInput;
        carController.steeringInput = steeringInput;
    }
}
