using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressTracker : MonoBehaviour, IProgressProvider
{
    public Transform[] splinePoints;
    public Transform playerTransform;

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
}

