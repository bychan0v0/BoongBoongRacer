using UnityEngine;
using System.Linq;

public class AutoSlowdownAIDriver : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waypointReachDistance = 5f;
    private int currentWaypointIndex = 0;

    [Header("Controller")]
    public CarController_Old carController;

    [Header("Speed Control")]
    public float averageCornerAngle;
    public float minCornerAngle = 7f;
    public float maxCornerAngle = 25f;
    public float speedLimitForCorner = 10f;
    public int cornerLookaheadCount = 3;

    [Header("Sensors")]
    public Transform sensorOrigin;
    public float frontRayLength = 8f;
    public float angleRayLength = 6f;
    public float sideCastRadius = 0.5f;
    public float sideCastDistance = 2f;
    public LayerMask obstacleMask;

    private bool isAvoiding = false;
    private bool obstacleAhead;
    private bool obstacleLeft;
    private bool obstacleRight;
    private bool sideCollisionLeft;
    private bool sideCollisionRight;

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 2 || carController == null) return;

        SenseEnvironment();

        if (obstacleAhead || obstacleLeft || obstacleRight || sideCollisionLeft || sideCollisionRight)
        {
            isAvoiding = true;
            HandleAvoidance();
        }
        else
        {
            isAvoiding = false;
            FollowWaypoints();
        }
    }

    void SenseEnvironment()
    {
        Vector3 origin = sensorOrigin.position;

        // 전방 및 각도 레이캐스트
        obstacleAhead = Physics.Raycast(origin, transform.forward, frontRayLength, obstacleMask);
        obstacleLeft = Physics.Raycast(origin, Quaternion.Euler(0, -30, 0) * transform.forward, angleRayLength, obstacleMask);
        obstacleRight = Physics.Raycast(origin, Quaternion.Euler(0, 30, 0) * transform.forward, angleRayLength, obstacleMask);

        // 측면 충돌 감지
        sideCollisionLeft = Physics.SphereCast(origin, sideCastRadius, -transform.right, out _, sideCastDistance, obstacleMask);
        sideCollisionRight = Physics.SphereCast(origin, sideCastRadius, transform.right, out _, sideCastDistance, obstacleMask);
    }

    void HandleAvoidance()
    {
        float pedalInput = 0.5f;
        float steerInput = 0f;

        if (obstacleAhead)
            pedalInput = -0.5f;

        if (sideCollisionLeft || obstacleLeft)
            steerInput = 0.5f;
        else if (sideCollisionRight || obstacleRight)
            steerInput = -0.5f;

        carController.pedalInput = pedalInput;
        carController.steeringInput = steerInput;
    }

    void FollowWaypoints()
    {
        Vector3 currentPos = transform.position;

        // 코너 곡률 계산
        float totalCornerAngle = 0f;
        for (int i = 1; i <= cornerLookaheadCount; i++)
        {
            int idxA = (currentWaypointIndex + i) % waypoints.Length;
            int idxB = (currentWaypointIndex + i + 1) % waypoints.Length;

            Vector3 dirA = (waypoints[idxA].position - waypoints[(idxA - 1 + waypoints.Length) % waypoints.Length].position).normalized;
            Vector3 dirB = (waypoints[idxB].position - waypoints[idxA].position).normalized;

            float angle = Vector3.Angle(dirA, dirB);
            totalCornerAngle += angle;
        }

        averageCornerAngle = totalCornerAngle / cornerLookaheadCount;

        // 감속 여부 판단
        float pedalInput = 1f;

        if (averageCornerAngle > minCornerAngle && carController.currentSpeed > speedLimitForCorner)
        {
            float cornerSeverity = Mathf.InverseLerp(minCornerAngle, maxCornerAngle, averageCornerAngle);
            float speedOverLimit = carController.currentSpeed - speedLimitForCorner;
            float speedFactor = Mathf.InverseLerp(0f, carController.maxSpeed - speedLimitForCorner, speedOverLimit);
            float intensity = Mathf.Clamp01(cornerSeverity * speedFactor);
            pedalInput = -Mathf.Lerp(0f, 1f, intensity);
        }

        // 조향 계산
        Vector3 relative = transform.InverseTransformPoint(waypoints[currentWaypointIndex].position);
        float targetAngle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
        float steeringInput = Mathf.Clamp(targetAngle / 30f, -1f, 1f);

        // 입력 적용
        carController.pedalInput = pedalInput;
        carController.steeringInput = steeringInput;

        // 웨이포인트 갱신
        if (relative.magnitude < waypointReachDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(waypoints[currentWaypointIndex].position, 1f);

        if (sensorOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(sensorOrigin.position, transform.forward * frontRayLength);
            Gizmos.DrawRay(sensorOrigin.position, Quaternion.Euler(0, -30, 0) * transform.forward * angleRayLength);
            Gizmos.DrawRay(sensorOrigin.position, Quaternion.Euler(0, 30, 0) * transform.forward * angleRayLength);
            Gizmos.DrawRay(sensorOrigin.position, -transform.right * sideCastDistance);
            Gizmos.DrawRay(sensorOrigin.position, transform.right * sideCastDistance);
        }
    }
}
