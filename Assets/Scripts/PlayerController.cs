using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string HORIZONTAL = "Horizontal";
    const string VERTICAL = "Vertical";
    public Vector3 position { get { return transform.position; } }

    public Transform rotationRoot;
    [Range(0,15)]public float movementSpeed = 1;
    [Range(0, 15)] public float rotationSpeed = 1;
    [Range(0,1)]public float walkingSoundMaxVolume = 1;
    [Range(0, 25)] public float walkingSoundFadeSpeed = 1;

    public bool foundScanner = true;

    private AudioSource walkingSound;
    private bool isWalking = false;

    private int terrainMask = 0;

    private void Start()
    {
        walkingSound = AudioManager._.PlayLoopedAudio(SoundID.Walking_On_Sand, MixerID.SFX);
        walkingSound.volume = 0;
        terrainMask = (1 << LayerMask.NameToLayer("Terrain"));
    }

    private void Update()
    {
        walkingSound.volume = Mathf.Lerp(walkingSound.volume, (isWalking ? walkingSoundMaxVolume : 0), walkingSoundFadeSpeed * Time.deltaTime);

        Ray ray = new Ray(transform.position + (transform.up*2), -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask))
        {
            rotationRoot.position = hit.point;
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(
            Input.GetAxis(HORIZONTAL),
            0,
            Input.GetAxis(VERTICAL)
        );
        isWalking = movement.magnitude > 0.01f;

        // Rotation
        if (isWalking)
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