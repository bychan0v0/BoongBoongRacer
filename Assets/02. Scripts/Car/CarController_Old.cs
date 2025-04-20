using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState_Old
{
    Drive,
    Brake,
    Reverse
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
    public float currentSpeed;
    public float motorPower;
    public float brakePower;
    public AnimationCurve steeringCurve;
    public float downForce;
    public GearState_Old currentGearState;
    public float currentRPM;
    public float idleRPM;
    public float maxRPM;
    public int currentGear;
    public float gearUpRPM;
    public float gearDownRPM;
    public float[] gearRatio;
    public AnimationCurve torqueCurve;
    public float torqueRatio = 5000f;

    private float pedalInput;
    private float idlePedalInput = 0.1f;
    private float brakeInput;
    private float steeringInput;
    private float slipAngle;
    private Vector3 velocityDirection;
    
    private Rigidbody playerRb;
    private IGearStrategy gearStrategy;
    
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
        pedalInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");

        if (pedalInput > 0)
        {
            currentGearState = GearState_Old.Drive;
        }
        else if (pedalInput < 0)
        {
            if (slipAngle < 120f && currentSpeed > 0.1f)
            {
                currentGearState = GearState_Old.Brake;
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
            t = gearRatio[currentGear - 1] * pedalInput * 0.005f;
        }
        else if (brakeInput > 0f)
        {
            targetRPM = wheelBasedRPM;
            t = brakeInput;
        }
        else
        {
            targetRPM = wheelBasedRPM;
            t = Time.deltaTime * 2f;
        }

        targetRPM = Mathf.Clamp(targetRPM, idleRPM, maxRPM);
        currentRPM = Mathf.Lerp(currentRPM, targetRPM, t);
        
        return torqueCurve.Evaluate(currentRPM / maxRPM) * gearRatio[currentGear - 1] * torqueRatio;
    }

    private void UpdateRPMAndGear()
    {
        if (currentRPM >= gearUpRPM && currentGear.Equals(gearRatio.Length) == false)
        {
            currentRPM = gearDownRPM + 200f;
            currentGear++;
        }
        else if (currentRPM <= gearDownRPM && currentGear.Equals(1) == false)
        {
            currentRPM = gearUpRPM - 200f;
            currentGear--;
        }
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
