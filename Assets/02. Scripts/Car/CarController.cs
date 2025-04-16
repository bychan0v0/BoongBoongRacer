using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GearState
{
    Neutral,
    Drive,
    Brake,
    DriveGearChanging
}

public class CarController : MonoBehaviour
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
    public GearState currentGearState;
    public float currentRPM;
    public float idleRPM;
    public float maxRPM;
    public int currentGear;
    public float gearUpRPM;
    public float gearDownRPM;
    public float[] gearRatio;

    public float pedalInput;
    public float brakeInput;
    private float steeringInput;
    private float slipAngle;
    private Vector3 velocityDirection;
    
    private Rigidbody playerRb;

    private void Start()
    {
        playerRb = gameObject.GetComponent<Rigidbody>();
        
        currentGearState = GearState.Neutral;
        currentGear = 1;
    }

    private void Update()
    {
        UpdateInput();
        CalculateRPM();
        UpdateRPMAndGear();
    }

    private void FixedUpdate()
    {
        currentSpeed = playerRb.velocity.magnitude;
        
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
            currentGearState = GearState.Drive;
        }
        else if (pedalInput < 0)
        {
            currentGearState = GearState.Brake;
        }
        else
        {
            currentGearState = GearState.Neutral;
        }
        
        velocityDirection = playerRb.velocity.normalized;
        slipAngle = Vector3.Angle(transform.forward, velocityDirection);
        
        if (slipAngle < 120f && pedalInput < 0f)
        {
            brakeInput = Mathf.Abs(pedalInput);
            pedalInput = 0;
        }
        else
        {
            brakeInput = 0;
        }
    }

    private void ApplyMotor()
    {
        if (currentSpeed < maxSpeed)
        {
            rearLeftWheelCollider.motorTorque = motorPower * pedalInput;
            rearRightWheelCollider.motorTorque = motorPower * pedalInput;
        }
        else
        {
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
        }
    }

    private void CalculateRPM()
    {
        if (currentGearState.Equals(GearState.Drive))
        {
            currentRPM = Mathf.Lerp(currentRPM, maxRPM, gearRatio[currentGear] * pedalInput);
        }
        else if (currentGearState.Equals(GearState.Brake))
        {
            currentRPM = Mathf.Lerp(currentRPM, 0f, 0.5f * brakeInput);
        }
        else
        {
            currentRPM = Mathf.Lerp(currentRPM, 0f, Time.deltaTime);
        }
    }

    private void UpdateRPMAndGear()
    {
        if (currentRPM >= gearUpRPM)
        {
            currentRPM = idleRPM;
            currentGear++;
        }
        else if (currentRPM <= gearDownRPM && currentGearState.Equals(GearState.Drive) == false && currentGear.Equals(1) == false)
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
