using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0,0,-10);
    public float smoothTime = 0.25f;
    private Vector3 velocity;

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, camPos, ref velocity, smoothTime);
    }
}
