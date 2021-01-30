using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerPos;

    private Vector3 offset;

    private void Start()
    {
        offset = transform.position - playerPos.position;
    }

    private void Update()
    {
        transform.position = playerPos.position + offset;
    }
}
