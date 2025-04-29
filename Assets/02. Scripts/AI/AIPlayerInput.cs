using System;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class AIPlayerInput : MonoBehaviour
{
    [Header("Spline Points")]
    public Transform[] splinePoints;
    public float waypointReachDistance = 3f;
    public int currentSplineIndex = 0;
    public float progress = 0f;

    [Header("Controller")]
    public CarController_Old carController;

    [Header("Speed Control")]
    public float minCornerAngle = 7f;
    public float maxCornerAngle = 25f;
    public float speedLimitForCorner = 10f;
    public float earlyBrakingDistance = 15f;
    public int cornerLookaheadCount = 3;

    [Header("Lateral Offset")]
    public float lateralOffset = 0f;
    public float maxLateralOffset = 3f;
    public float offsetSmooth = 2f;

    [Header("Obstacle Detection")]
    public Transform sensorOrigin;
    public float detectRange = 6f;
    public float sideRange = 5f;
    public LayerMask carMask;

    private float targetOffset = 0f;
    public float maxSteerAngle = 30f;

    private float stuckTimer = 0f;
    private float stuckThreshold = 0.5f;
    private float stuckDuration = 0.5f;
    private float reverseTimer = 0f;
    private float reverseDuration = 1.5f;

    private bool avoidanceTriggered = false;
    private float avoidancePedalInput = 0f;
    private bool escapeTriggered = false;

    private void FixedUpdate()
    {
        if (splinePoints == null || splinePoints.Length < 4 || carController == null) return;

        HandleUnstuckLogic();
        HandleDynamicOffsetAndCollisionAvoidance();

        if (avoidanceTriggered)
        {
            carController.pedalInput = avoidancePedalInput;
            carController.steeringInput = Mathf.Clamp(targetOffset / maxLateralOffset, -1f, 1f);
            return;
        }

        if (escapeTriggered) return;

        Vector3 currentPos = transform.position;
        Vector3 p0 = splinePoints[(currentSplineIndex - 1 + splinePoints.Length) % splinePoints.Length].position;
        Vector3 p1 = splinePoints[currentSplineIndex % splinePoints.Length].position;
        Vector3 p2 = splinePoints[(currentSplineIndex + 1) % splinePoints.Length].position;
        Vector3 p3 = splinePoints[(currentSplineIndex + 2) % splinePoints.Length].position;

        Vector3 targetPos = CatmullRom(p0, p1, p2, p3, progress);
        Vector3 forwardDir = (CatmullRom(p0, p1, p2, p3, progress + 0.01f) - targetPos).normalized;
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir);

        Vector3 offsetTarget = targetPos + rightDir * lateralOffset;
        Vector3 localTarget = transform.InverseTransformPoint(offsetTarget);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steeringInput = Mathf.Clamp(targetAngle / maxSteerAngle, -1f, 1f);

        float totalCurvature = 0f;
        float totalDistance = 0f;

        for (int i = 0; i < cornerLookaheadCount; i++)
        {
            int idx0 = (currentSplineIndex + i - 1 + splinePoints.Length) % splinePoints.Length;
            int idx1 = (currentSplineIndex + i) % splinePoints.Length;
            int idx2 = (currentSplineIndex + i + 1) % splinePoints.Length;
            int idx3 = (currentSplineIndex + i + 2) % splinePoints.Length;

            Vector3 d1 = (splinePoints[idx2].position - splinePoints[idx1].position).normalized;
            Vector3 d2 = (splinePoints[idx3].position - splinePoints[idx2].position).normalized;
            float angle = Vector3.Angle(d1, d2);
            float dist = Vector3.Distance(splinePoints[idx1].position, splinePoints[idx2].position);

            totalCurvature += angle;
            totalDistance += dist;
        }

        float pedalInput = 1f;

        if (totalCurvature > minCornerAngle && totalDistance < earlyBrakingDistance && carController.currentSpeed > speedLimitForCorner)
        {
            float cornerSeverity = Mathf.InverseLerp(minCornerAngle, maxCornerAngle, totalCurvature);
            float speedOverLimit = carController.currentSpeed - speedLimitForCorner;
            float speedFraction = Mathf.Clamp01(speedOverLimit / (carController.maxSpeed - speedLimitForCorner));
            float intensity = Mathf.Clamp01(cornerSeverity * speedFraction);
            pedalInput = -Mathf.Lerp(0f, 1f, intensity);
        }

        if (carController.currentSpeed < 5f)
        {
            pedalInput = Mathf.Max(pedalInput, 0.8f);
        }

        carController.pedalInput = pedalInput;
        carController.steeringInput = steeringInput;

        float distanceToTarget = Vector3.Distance(currentPos, targetPos);
        // Vector3 toTarget = (targetPos - currentPos).normalized;
        // float forwardDot = Vector3.Dot(transform.forward, toTarget);

        if (!escapeTriggered)
        {
            if (distanceToTarget < waypointReachDistance)
            {
                progress += 0.1f;
                if (progress >= 1f)
                {
                    progress = 0f;
                    currentSplineIndex = (currentSplineIndex + 1) % splinePoints.Length;
                }
            }
        }
    }

    private void HandleUnstuckLogic()
    {
        if (reverseTimer > 0f)
        {
            reverseTimer -= Time.fixedDeltaTime;
            carController.pedalInput = -1f;
            carController.steeringInput = 0f;
            return;
        }

        if (carController.pedalInput > 0.5f && carController.currentSpeed < stuckThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > stuckDuration)
            {
                reverseTimer = reverseDuration;
                stuckTimer = 0f;
                escapeTriggered = true;
            }
        }
        else
        {
            stuckTimer = 0f;
            escapeTriggered = false;
        }
    }

    private void HandleDynamicOffsetAndCollisionAvoidance()
    {
        Vector3 origin = sensorOrigin ? sensorOrigin.position : transform.position + Vector3.up * 0.5f;

        bool leftFrontBlocked = Physics.Raycast(origin - transform.right * 0.75f, transform.forward, detectRange, carMask);
        bool rightFrontBlocked = Physics.Raycast(origin + transform.right * 0.75f, transform.forward, detectRange, carMask);
        bool left30Blocked = Physics.Raycast(origin, Quaternion.Euler(0, -30f, 0) * transform.forward, detectRange, carMask);
        bool right30Blocked = Physics.Raycast(origin, Quaternion.Euler(0, 30f, 0) * transform.forward, detectRange, carMask);
        bool leftSideBlocked = Physics.Raycast(origin, -transform.right, sideRange, carMask);
        bool rightSideBlocked = Physics.Raycast(origin, transform.right, sideRange, carMask);

        avoidanceTriggered = false;
        avoidancePedalInput = 0f;

        if (leftFrontBlocked || left30Blocked)
        {
            if (leftFrontBlocked)
            {
                avoidanceTriggered = true;
                avoidancePedalInput = 0.5f;
            }
            targetOffset = maxLateralOffset;
        }
        else if (rightFrontBlocked || right30Blocked)
        {
            if (rightFrontBlocked)
            {
                avoidanceTriggered = true;
                avoidancePedalInput = 0.5f;
            }
            targetOffset = -maxLateralOffset;
        }
        else if (leftSideBlocked || rightSideBlocked)
        {
            if (leftSideBlocked) targetOffset = maxLateralOffset;
            else if (rightSideBlocked) targetOffset = -maxLateralOffset;
        }
        else
        {
            targetOffset = 0f;
        }

        lateralOffset = Mathf.Lerp(lateralOffset, targetOffset, Time.fixedDeltaTime * offsetSmooth);
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t));
    }
    
    public float GetPreciseProgress()
    {
        Vector3 carPos = transform.position;
    
        Vector3 p0 = splinePoints[(currentSplineIndex - 1 + splinePoints.Length) % splinePoints.Length].position;
        Vector3 p1 = splinePoints[currentSplineIndex % splinePoints.Length].position;
        Vector3 p2 = splinePoints[(currentSplineIndex + 1) % splinePoints.Length].position;
        Vector3 p3 = splinePoints[(currentSplineIndex + 2) % splinePoints.Length].position;

        float closestT = FindClosestTOnSpline(p0, p1, p2, p3, carPos, 10);
    
        return currentSplineIndex + closestT;
    }

    private float FindClosestTOnSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 point, int steps)
    {
        float closestT = 0f;
        float closestDistance = float.MaxValue;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 splinePoint = CatmullRom(p0, p1, p2, p3, t);
            float dist = Vector3.Distance(splinePoint, point);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestT = t;
            }
        }

        return closestT;
    }

    private void OnDrawGizmos()
    {
        if (splinePoints == null || splinePoints.Length < 4) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < splinePoints.Length; i++)
        {
            Vector3 p0 = splinePoints[(i - 1 + splinePoints.Length) % splinePoints.Length].position;
            Vector3 p1 = splinePoints[i % splinePoints.Length].position;
            Vector3 p2 = splinePoints[(i + 1) % splinePoints.Length].position;
            Vector3 p3 = splinePoints[(i + 2) % splinePoints.Length].position;

            Vector3 prevPoint = p1;
            for (float t = 0; t < 1f; t += 0.1f)
            {
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }

        if (sensorOrigin)
        {
            Gizmos.color = Color.red;
            Vector3 origin = sensorOrigin.position;
            Gizmos.DrawRay(origin - transform.right * 0.75f, transform.forward * detectRange);
            Gizmos.DrawRay(origin + transform.right * 0.75f, transform.forward * detectRange);
            Gizmos.DrawRay(origin, Quaternion.Euler(0, -30f, 0) * transform.forward * detectRange);
            Gizmos.DrawRay(origin, Quaternion.Euler(0, 30f, 0) * transform.forward * detectRange);
            Gizmos.DrawRay(origin, -transform.right * sideRange);
            Gizmos.DrawRay(origin, transform.right * sideRange);
        }
    }
}
