using UnityEngine;

public class AutoSlowdownAIDriver : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waypointReachDistance = 5f;
    private int currentWaypointIndex = 0;

    [Header("Controller")]
    public CarController_Old carController;

    [Header("Speed Settings")]
    public float maxSpeed = 40f;
    public float minCornerSpeed = 10f;
    public float brakeAggressiveness = 0.05f;

    [Header("Steering")]
    public float maxSteerAngle = 30f;
    public float steeringSmoothing = 5f;
    private float currentSteer = 0f;

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 3 || carController == null) return;

        EvaluateDrivingIntent();
    }

    void EvaluateDrivingIntent()
    {
        // 1. 코너 곡률 분석
        Vector3 dir1 = (waypoints[(currentWaypointIndex + 1) % waypoints.Length].position - waypoints[currentWaypointIndex].position).normalized;
        Vector3 dir2 = (waypoints[(currentWaypointIndex + 2) % waypoints.Length].position - waypoints[(currentWaypointIndex + 1) % waypoints.Length].position).normalized;

        float cornerAngle = Vector3.Angle(dir1, dir2);
        float severity = Mathf.InverseLerp(0f, 90f, cornerAngle);
        float targetSpeed = Mathf.Lerp(maxSpeed, minCornerSpeed, severity);

        // 2. 현재 속도와 비교해서 throttle/brake 계산
        float currentSpeed = carController.currentSpeed;
        float speedError = targetSpeed - currentSpeed;

        float throttle = 0f;
        float brake = 0f;

        if (speedError > 2f)
        {
            throttle = 1f;
            brake = 0f;
        }
        else if (speedError < -2f)
        {
            throttle = 0f;
            brake = Mathf.Clamp01(-speedError * brakeAggressiveness);
        }
        else
        {
            throttle = 0.2f;
            brake = 0f;
        }

        // 3. 조향 계산 및 부드럽게 보간
        Vector3 toWaypoint = waypoints[(currentWaypointIndex + 1) % waypoints.Length].position - transform.position;
        toWaypoint.y = 0f;
        Vector3 localDir = transform.InverseTransformDirection(toWaypoint.normalized);
        float targetSteer = Mathf.Clamp(localDir.x, -1f, 1f) * maxSteerAngle;

        currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.fixedDeltaTime * steeringSmoothing);

        carController.pedalInput = throttle;
        carController.brakeInput = brake;
        carController.steeringInput = currentSteer;

        // 4. 웨이포인트 갱신
        if (toWaypoint.magnitude < waypointReachDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}
