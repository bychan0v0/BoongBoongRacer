using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState_Old
{
    Drive,
    Reverse,
}

public class CarController_Old : MonoBehaviour
{
    [Header("Wheel Collider")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Wheel Mesh")]
    public Transform frontLeftWheelMesh;
    public Transform frontRightWheelMesh;
    public Transform rearLeftWheelMesh;
    public Transform rearRightWheelMesh;

    [Header("Car Setting")]
    public float maxSpeed;
    public float maxRPM;
    public float idleRPM = 1000f;
    public float gearUpRPM = 5500f;
    public float gearDownRPM = 2500f;
    public float[] gearRatio;
    public float differentialRatio = 3.42f;
    public float wheelRadius = 0.37f;
    public float maxEngineTorque = 400f;
    public AnimationCurve torqueCurve;
    public AnimationCurve steeringCurve;
    public float brakePower;
    public float downForce;

    [Header("Input")]
    public float pedalInput;
    public float steeringInput;
    public float brakeInput;
    public float currentSpeed;
    public float currentRPM;
    public int currentGear;

    public GearState_Old currentGearState;

    private bool isUpShifting = false;
    private bool isDownShifting = false;

    private Rigidbody playerRb;
    private float slipAngle;
    private Vector3 velocityDirection;

    private float[] gearSpeedLimits;
    private float reductionFactor = 0.8f;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        currentGear = 1;
        currentGearState = GearState_Old.Drive;

        gearSpeedLimits = new float[gearRatio.Length];
        
        for (int i = 0; i < gearRatio.Length; i++)
        {
            float maxWheelRPM = maxRPM / (gearRatio[i] * differentialRatio);
            float wheelAngularVelocity = maxWheelRPM * 2 * Mathf.PI / 60f;
            float maxSpeedPerGear = wheelAngularVelocity * wheelRadius;
            gearSpeedLimits[i] = maxSpeedPerGear * reductionFactor;
        }
    }

    private void Update()
    {
        currentSpeed = playerRb.velocity.magnitude;
    }

    private void FixedUpdate()
    {
        UpdateInput();
        UpdateRPM();
        UpdateGear();

        ApplyMotor();
        ApplyBrake();
        ApplySteering();
        ApplyDownForce();
        ApplyWheelPosition();
    }

    private void UpdateInput()
    {
        velocityDirection = playerRb.velocity.normalized;
        slipAngle = Vector3.Angle(transform.forward, velocityDirection);

        if (pedalInput > 0f)
        {
            currentGearState = GearState_Old.Drive;
            brakeInput = 0f;
        }
        else if (pedalInput < 0f)
        {
            if (slipAngle < 120f && currentSpeed > 0.1f)
            {
                brakeInput = Mathf.Abs(pedalInput);
                pedalInput = 0f;
            }
            else
            {
                currentGearState = GearState_Old.Reverse;
                brakeInput = 0f;
            }
        }
        else
        {
            brakeInput = 0f;
        }
    }

    private void ApplyMotor()
    {
        if (isUpShifting || isDownShifting) return;

        if (currentSpeed < maxSpeed)
        {
            float normalizedRPM = Mathf.Clamp01(currentRPM / maxRPM);
            float engineTorque = torqueCurve.Evaluate(normalizedRPM) * maxEngineTorque;
            float wheelTorque = engineTorque * gearRatio[currentGear - 1] * differentialRatio;

            float finalTorque = wheelTorque * Mathf.Abs(pedalInput);
            if (currentGearState == GearState_Old.Drive)
            {
                rearLeftWheelCollider.motorTorque = finalTorque;
                rearRightWheelCollider.motorTorque = finalTorque;
            }
            else if (currentGearState == GearState_Old.Reverse)
            {
                rearLeftWheelCollider.motorTorque = -finalTorque;
                rearRightWheelCollider.motorTorque = -finalTorque;
            }
        }
        else
        {
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
        }
    }

    private void UpdateRPM()
    {
        float wheelAngularVelocity = currentSpeed / wheelRadius;
        float wheelRPM = wheelAngularVelocity * Mathf.Rad2Deg / 6f;
        float wheelBasedRPM = wheelRPM * gearRatio[currentGear - 1] * differentialRatio;

        float targetRPM;
        float t;

        if (pedalInput > 0f || (currentGearState == GearState_Old.Reverse && pedalInput < 0f))
        {
            float rpmRange = maxRPM - wheelBasedRPM;
            targetRPM = wheelBasedRPM + rpmRange * Mathf.Abs(pedalInput);
            t = gearRatio[currentGear - 1] * Mathf.Abs(pedalInput) * 0.01f;
        }
        else if (brakeInput > 0f)
        {
            targetRPM = wheelBasedRPM * 0.3f;
            t = brakeInput * 5f;
        }
        else
        {
            targetRPM = wheelBasedRPM * 0.5f;
            t = Time.fixedDeltaTime * 6f;
        }

        float gearLerpMultiplier = Mathf.Lerp(1.5f, 0.3f, (float)(currentGear - 1) / (gearRatio.Length - 1));
        t *= gearLerpMultiplier;

        targetRPM = Mathf.Clamp(targetRPM, idleRPM, maxRPM);
        currentRPM = Mathf.Lerp(currentRPM, targetRPM, t);

        // 저속 시 강제 다운시프트 유도
        float speedThreshold = gearSpeedLimits[currentGear - 1] * 0.5f;
        if (pedalInput > 0f && currentGear > 1 && currentSpeed < speedThreshold)
        {
            currentGear--;
            currentRPM = Mathf.Max(currentRPM, gearUpRPM - 300f);
        }
    }

    private void UpdateGear()
    {
        if (isUpShifting || isDownShifting || currentGearState == GearState_Old.Reverse) return;

        float speedLimitForCurrentGear = gearSpeedLimits[currentGear - 1];

        if (currentRPM >= gearUpRPM && currentGear < gearRatio.Length && currentSpeed > speedLimitForCurrentGear * 0.9f)
        {
            StartCoroutine(GearUpCoroutine());
        }
        else if (currentRPM <= gearDownRPM && currentGear > 1 && currentSpeed < speedLimitForCurrentGear * 0.7f)
        {
            StartCoroutine(GearDownCoroutine());
        }
    }

    private IEnumerator GearUpCoroutine()
    {
        isUpShifting = true;
        rearLeftWheelCollider.motorTorque = 0f;
        rearRightWheelCollider.motorTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 1000f;
        rearRightWheelCollider.brakeTorque = 1000f;

        float duration = 0.5f;
        float elapsed = 0f;
        float startRPM = currentRPM;
        float targetRPM = gearDownRPM + 200f;

        currentGear++;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / duration;
            currentRPM = Mathf.Lerp(startRPM, targetRPM, t);
            yield return null;
        }

        isUpShifting = false;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;
    }

    private IEnumerator GearDownCoroutine()
    {
        isDownShifting = true;
        rearLeftWheelCollider.motorTorque = 0f;
        rearRightWheelCollider.motorTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 1000f;
        rearRightWheelCollider.brakeTorque = 1000f;

        float duration = 0.3f;
        float elapsed = 0f;
        float startRPM = currentRPM;
        float targetRPM = gearUpRPM - 200f;

        currentGear--;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / duration;
            currentRPM = Mathf.Lerp(startRPM, targetRPM, t);
            yield return null;
        }

        isDownShifting = false;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;
    }

    private void ApplyBrake()
    {
        frontLeftWheelCollider.brakeTorque = brakePower * 0.7f * brakeInput;
        frontRightWheelCollider.brakeTorque = brakePower * 0.7f * brakeInput;
        rearLeftWheelCollider.brakeTorque = brakePower * 0.3f * brakeInput;
        rearRightWheelCollider.brakeTorque = brakePower * 0.3f * brakeInput;
    }

    private void ApplySteering()
    {
        float steeringAngle = steeringInput * steeringCurve.Evaluate(currentSpeed);
        frontLeftWheelCollider.steerAngle = steeringAngle;
        frontRightWheelCollider.steerAngle = steeringAngle;
    }

    private void ApplyDownForce()
    {
        float force = downForce * Mathf.Pow(playerRb.velocity.magnitude, 1.2f);
        playerRb.AddForce(-transform.up * force);
    }

    private void ApplyWheelPosition()
    {
        UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
        UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
        UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
        UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
    }

    private void UpdateWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}
