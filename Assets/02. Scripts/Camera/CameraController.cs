using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Camera firstCamera;
    
    public Transform player;
    public Vector3 offset;
    public float speed;
    
    private Rigidbody playerRb;

    private void Start()
    {
        mainCamera.enabled = true;
        firstCamera.enabled = false;
        
        playerRb = player.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangeCamera();
        }
    }

    private void LateUpdate()
    {
        Vector3 playerForward = (playerRb.velocity + player.transform.forward).normalized;
        
        transform.position = Vector3.Lerp(
            transform.position, 
            player.position + player.transform.TransformVector(offset) + playerForward * (-4f), 
            speed * Time.deltaTime);
        transform.LookAt(player);
    }

    private void ChangeCamera()
    {
        if (mainCamera.enabled)
        {
            mainCamera.enabled = false;
            firstCamera.enabled = true;
        }
        else
        {
            mainCamera.enabled = true;
            firstCamera.enabled = false;
        }
    }
}
