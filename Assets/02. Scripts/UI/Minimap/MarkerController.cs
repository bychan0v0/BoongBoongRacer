using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    private Transform player;
    private Vector3 markerPosition;
    
    private void Update()
    {
        markerPosition = new Vector3(player.position.x, player.position.y - 500f, player.position.z);
        
        transform.position = markerPosition;
    }
    
    public void SetTarget(Transform target)
    {
        player = target;
    }
}
