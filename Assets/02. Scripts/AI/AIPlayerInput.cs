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

    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float lookAheadDistance = 5f;

    private float targetPedal = 0f;
    private float targetSteer = 0f;

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0 || carController == null) return;

        EvaluateDrivingIntent();      // 기본 목표 입력 계산
        EvaluateSensorOverride();     // 센서 기반 보정
        ApplyInputs();                // 최종 입력 반영
    }

    private void EvaluateDrivingIntent()
    {
        // 현재 목표 웨이포인트 기준 조향 계산
        Transform target = waypoints[currentWaypointIndex];
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f; // Y축 무시한 평면 거리 계산

        Vector3 localTarget = transform.InverseTransformPoint(target.position);
        float steerRaw = (localTarget.x / localTarget.magnitude);
        targetSteer = Mathf.Clamp(steerRaw, -1f, 1f) * maxSteerAngle;

        // 속도 기반으로 페달 계산
        float speedError = maxSpeed - carController.currentSpeed;
        targetPedal = Mathf.Clamp(speedError * 0.05f, 0f, 1f);

        // 웨이포인트 도달 판정
        if (toTarget.magnitude < lookAheadDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void EvaluateSensorOverride()
    {
        float left = CastSensor(-45f, sideSensorLength);
        float right = CastSensor(45f, sideSensorLength);
        float front = CastSensor(0f, frontSensorLength);

        // 좌우 회피 조향 보정
        float steerDelta = right - left;
        float steerAdjust = Mathf.Clamp(steerDelta * 0.05f, -10f, 10f); // 약한 조향 보정
        targetSteer += steerAdjust;

        // 전방 감지 시 감속
        if (front < forwardClearanceThreshold && carController.currentSpeed > 1f)
        {
            float brakeStrength = Mathf.InverseLerp(forwardClearanceThreshold, 0f, front);
            targetPedal = Mathf.Min(targetPedal, -brakeStrength); // 감속만 덮어씀
        }
    }

    private float CastSensor(float angle, float length)
    {
        Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
        Ray ray = new Ray(sensorOrigin.position, dir);
        if (Physics.Raycast(ray, out RaycastHit hit, length, sensorMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            return hit.distance;
        }
        else
        {
            Debug.DrawRay(ray.origin, dir * length, Color.green);
            return length;
        }
    }

    private void ApplyInputs()
    {
        carController.pedalInput = targetPedal;
        carController.steeringInput = targetSteer;
    }
}
