using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressTracker : MonoBehaviour, IProgressProvider
{
    public Transform[] splinePoints;
    public Transform playerTransform;

    public MyLapCounter myLapCounter;

    public int currentSplineIndex { get; private set; } = 0;  // 추가!

    private void Update()
    {
        GetProgress();
    }
    
    public float GetProgress()
    {
        int closestIdx = 0;
        float closestT = 0f;
        float minDistance = float.MaxValue;

        for (int i = 0; i < splinePoints.Length - 3; i++)
        {
            for (float t = 0f; t <= 1f; t += 0.1f)
            {
                Vector3 p = CatmullRom(
                    splinePoints[i].position,
                    splinePoints[i + 1].position,
                    splinePoints[i + 2].position,
                    splinePoints[i + 3].position,
                    t
                );

                float dist = Vector3.Distance(p, playerTransform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestIdx = i + 1;
                    closestT = t;
                }
            }
        }

        currentSplineIndex = closestIdx;  // 이걸 매 프레임 갱신!

        return closestIdx + closestT;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
        );
    }

    public int GetLap()
    {
        return myLapCounter != null ? myLapCounter.currentLap : 0;
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
}


