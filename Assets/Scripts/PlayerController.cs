using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string HORIZONTAL = "Horizontal";
    const string VERTICAL = "Vertical";

    public Transform rotationRoot;
    [Range(0,15)]public float movementSpeed = 1;
    [Range(0, 15)] public float rotationSpeed = 1;


    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(
            Input.GetAxis(HORIZONTAL),
            0,
            Input.GetAxis(VERTICAL)
        );

        // Rotation
        if (movement.magnitude > 0.01f)
        {
            Vector3 normalizedMovement = movement.normalized;
            float angle = Mathf.Atan2(normalizedMovement.x, normalizedMovement.z) * Mathf.Rad2Deg;
            rotationRoot.rotation = Quaternion.Lerp(rotationRoot.rotation, Quaternion.Euler(0, angle, 0), rotationSpeed * Time.deltaTime);
        }

        // Translation
        movement *= movementSpeed * Time.deltaTime;
        transform.Translate(movement);
    }
}
