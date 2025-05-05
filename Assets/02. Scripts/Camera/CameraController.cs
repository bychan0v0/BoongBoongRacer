using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    
    public Camera mainCamera;
    private Camera frontCamera;
    private Camera backMirrorCamera;
    
    public RenderTexture backMirrorTexture;
    
    public Vector3 offset;
    public float speed;
    
    private Transform player;
    private Rigidbody playerRb;

    private void Awake()
    {
        Instance = this;
    }

    // private void Start()
    // {
    //     mainCamera.enabled = true;
    //     frontCamera.enabled = false;
    // }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ChangeCamera();
        }
    }

    private void LateUpdate()
    {
        if (frontCamera.Equals(null) || backMirrorCamera.Equals(null)) return;
        
        Vector3 playerForward = (playerRb.velocity + player.transform.forward).normalized;
        
        transform.position = Vector3.Lerp(
            transform.position, 
            player.position + player.transform.TransformVector(offset) + playerForward * (-4f), 
            speed * Time.deltaTime);
        transform.LookAt(player);
    }

    public void SetTarget(Transform target)
    {
        player = target;
        playerRb = player.GetComponent<Rigidbody>();
        
        var cameras = player.GetComponentsInChildren<Camera>(true);
        
        foreach (var cam in cameras)
        {
            cam.gameObject.SetActive(true);
            
            if (cam.name == "Front Camera")
            {
                frontCamera = cam;
            }
            else if (cam.name == "BackMirror Camera")
            {
                backMirrorCamera = cam;
                backMirrorCamera.targetTexture = backMirrorTexture;
            }
        }
        
        mainCamera.enabled = true;
        frontCamera.enabled = false;
        backMirrorCamera.enabled = true;
    }
    
    private void ChangeCamera()
    {
        if (mainCamera.enabled)
        {
            mainCamera.enabled = false;
            frontCamera.enabled = true;
        }
        else
        {
            mainCamera.enabled = true;
            frontCamera.enabled = false;
        }
    }
}
