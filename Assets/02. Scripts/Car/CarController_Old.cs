using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState_Old
{
    Drive,
    Reverse,
}

[System.Serializable]
public class Gear
{
    public int level;
    public float limitUp;
    public float limitDown;
    public float accelRatio;
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
    public float motorPower;
    public float brakePower;
    public AnimationCurve steeringCurve;
    public float downForce;
    public GearState_Old currentGearState;
    public float idleRPM;
    public float maxRPM;
    public float gearUpRPM;
    public float gearDownRPM;
    public float[] gearRatio;
    public AnimationCurve torqueCurve;
    public float torqueRatio = 50000f;
    
    [Header("Input")]
    public float pedalInput;
    public float steeringInput;
    public float brakeInput;
    public float currentSpeed;
    public float currentRPM;
    public int currentGear;
    
    private bool isUpShifting = false;
    private bool isDownShifting = false;
    private float idlePedalInput = 0.1f;
    private float slipAngle;
    private Vector3 velocityDirection;
    
    private Rigidbody playerRb;
    private IGearStrategy gearStrategy;

    public Gear[] gears;
    
    private void Start()
    {
        playerRb = gameObject.GetComponent<Rigidbody>();
        
        currentGearState = GearState_Old.Drive;
        currentGear = 1;
    }

    private void Update()
    {
        motorPower = CalculateRPM();
        currentSpeed = playerRb.velocity.magnitude;
        
        UpdateInput();
        UpdateRPMAndGear();
    }

    private void FixedUpdate()
    {
        ApplyMotor();
        ApplyBrake();
        ApplySteering();
        ApplyDownForce();
        ApplyWheelPosition();
    }

    private void UpdateInput()
    {
        if (pedalInput > 0)
        {
            currentGearState = GearState_Old.Drive;
        }
        else if (pedalInput < 0)
        {
            if (slipAngle < 120f && currentSpeed > 0.1f)
            {
                // 브레이크였던것
            }
            else
            {
                currentGearState = GearState_Old.Reverse;
            }
        }
        
        velocityDirection = playerRb.velocity.normalized;
        slipAngle = Vector3.Angle(transform.forward, velocityDirection);
        
        if (slipAngle < 120f && pedalInput < 0f)
        {
            brakeInput = Mathf.Abs(pedalInput);
            pedalInput = 0f;
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
            float torqueToApply = 0f;
            
            torqueToApply = (pedalInput > 0f) ? motorPower * pedalInput : motorPower * idlePedalInput;
            
            rearLeftWheelCollider.motorTorque = torqueToApply;
            rearRightWheelCollider.motorTorque = torqueToApply;    
        }
        else
        {
            rearLeftWheelCollider.motorTorque = 0f; 
            rearRightWheelCollider.motorTorque = 0f;
        }
    }
    
    public float differentialRatio = 3.42f;
    public float wheelRadius = 0.37f;
    
    private float CalculateRPM()
    {
        float wheelBasedRPM = (currentSpeed / (2 * Mathf.PI * wheelRadius)) * 60f * gearRatio[currentGear - 1] * differentialRatio;
        float targetRPM;
        float t;

        if (pedalInput > 0f)
        {
            float rpmRange = maxRPM - wheelBasedRPM;
            targetRPM = wheelBasedRPM + rpmRange * pedalInput;
            t = gearRatio[currentGear - 1] * pedalInput * 0.01f;
        }
        else if (brakeInput > 0f)
        {
            targetRPM = wheelBasedRPM * 0.5f;
            t = brakeInput * 3f;
        }
        else
        {
            targetRPM = wheelBasedRPM * 0.5f;   
            t = Time.deltaTime * 3f;
        }

        targetRPM = Mathf.Clamp(targetRPM, idleRPM, maxRPM);
        currentRPM = Mathf.Lerp(currentRPM, targetRPM, t);
        
        return torqueCurve.Evaluate(currentRPM / maxRPM) * gearRatio[currentGear - 1] * torqueRatio;
    }

    private void UpdateRPMAndGear()
    {
        if (isUpShifting || isDownShifting) return;
        
        if (currentRPM >= gearUpRPM && currentGear.Equals(gearRatio.Length) == false)
        {
            StartCoroutine(GearUpCoroutine());
        }
        else if (currentRPM <= gearDownRPM && currentGear > 1)
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

        float duration = 0.75f;
        float elapsed = 0f;

        float startRPM = currentRPM;
        float targetRPM = gearDownRPM + 200f;

        currentGear++;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            currentRPM = Mathf.Lerp(startRPM, targetRPM, t * 2f);

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

        float duration = 0.25f;
        float elapsed = 0f;

        float startRPM = currentRPM;
        float targetRPM = gearUpRPM - 200f;
        
        currentGear--;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            currentRPM = Mathf.Lerp(startRPM, targetRPM, t * 2f);

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
