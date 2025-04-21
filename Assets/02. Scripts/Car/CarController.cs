using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float currentRPM;
    public float idleRPM;
    public float maxRPM;
    public int currentGear;
    public float gearUpRPM;
    public float gearDownRPM;
    public float[] gearRatio;
    public AnimationCurve torqueCurve;
    public float maxEngineTorque;

    private float steeringInput;
    private Rigidbody playerRb;
    private IGearStrategy gearStrategy;
    
    private float torqueRatio = 5000f;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        currentGear = 1;
        
        SetGearStrategy(new DriveGear());
    }

    private void Update()
    {
        currentSpeed = playerRb.velocity.magnitude;
        
        gearStrategy.UpdateState(this);
        gearStrategy.UpdateRPM(this);
    }

    private void FixedUpdate()
    {
        gearStrategy.ApplyMotor(this);
        
        ApplySteering();
        ApplyDownForce();
        ApplyWheelPosition();
    }

    public void SetGearStrategy(IGearStrategy newStrategy)
    {
        gearStrategy = newStrategy;
    }

    public float CalculateTorque()
    {
        return torqueCurve.Evaluate(currentRPM / maxRPM) * maxEngineTorque * gearRatio[currentGear - 1] * torqueRatio;
    }

    public void SetMotorTorque(float torque)
    {
        rearLeftWheelCollider.motorTorque = torque;
        rearRightWheelCollider.motorTorque = torque;
    }

    public void SetBrakeTorque(float input)
    {
        frontLeftWheelCollider.brakeTorque = brakePower * 0.7f * input;
        frontRightWheelCollider.brakeTorque = brakePower * 0.7f * input;
        rearLeftWheelCollider.brakeTorque = brakePower * 0.3f * input;
        rearRightWheelCollider.brakeTorque = brakePower * 0.3f * input;
    }

    private void ApplySteering()
    {
        steeringInput = Input.GetAxis("Horizontal");
        
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