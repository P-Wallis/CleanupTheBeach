using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string HORIZONTAL = "Horizontal";
    const string VERTICAL = "Vertical";

    public CameraController cameraController;
    public MemoriesController memoriesController;

    public Vector3 position { get { return transform.position; } }

    public Transform rotationRoot;
    [Range(0, 15)] public float movementSpeed = 1;
    [Range(0, 15)] public float rotationSpeed = 1;
    [Range(0, 1)] public float walkingSoundMaxVolume = 1;
    [Range(0, 25)] public float walkingSoundFadeSpeed = 1;

    public bool foundScanner = true;

    private AudioSource walkingSound;
    private bool isWalking = false;
    private bool isDigging;
    public bool IsDigging { get { return isDigging; } }

    private int terrainMask = 0;

    private void Start()
    {
        walkingSound = AudioManager._.PlayLoopedAudio(SoundID.Walking_On_Sand, MixerID.SFX);
        walkingSound.volume = 0;
        terrainMask = (1 << LayerMask.NameToLayer("Terrain"));
        memoriesController.HideMemories();
    }

    private void Update()
    {
        walkingSound.volume = Mathf.Lerp(walkingSound.volume, (isWalking ? walkingSoundMaxVolume : 0), walkingSoundFadeSpeed * Time.deltaTime);

        Ray ray = new Ray(transform.position + (transform.up * 2), -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask))
        {
            rotationRoot.position = hit.point;
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = isDigging ? Vector3.zero : new Vector3(
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

    public void Dig(BeachObject beachObject)
    {
        {
            if (isDigging)
                return;

            StartCoroutine(Digging(beachObject));
        }

        IEnumerator Digging(BeachObject beachObject)
        {
            isDigging = true;
            cameraController.TweenTo(CameraID.Digging);
            AudioManager._.PlayOneShotSFX(SoundID.Digging, position);
            yield return new WaitForSeconds(AudioManager._.clipDict[SoundID.Digging].clip.length);

            beachObject.DigUp();
            cameraController.TweenTo(CameraID.Examine);
            yield return new WaitForSeconds(2f);
            if (beachObject.Data != null)
            {
                Debug.Log("You Found " + beachObject.Data.objectName + "!");
                if (beachObject.Data.isValuable)
                {
                    AudioManager._.PlayOneShotSFX(SoundID.Special_Object, position);
                    cameraController.TweenTo(CameraID.Trigger_Memory);
                    yield return new WaitForSeconds(cameraController.tweenTime);
                    AudioManager._.CrossfadeMusic(false);
                    yield return new WaitForSeconds(1f);
                    cameraController.TweenTo(CameraID.Remembering);
                    yield return new WaitForSeconds(cameraController.tweenTime);
                    memoriesController.ShowNewMemory(beachObject.Data.memory);
                    while (!Input.GetKey(KeyCode.Space))
                    {
                        yield return null;
                    }
                    memoriesController.HideMemories();
                    AudioManager._.CrossfadeMusic(true);
                    cameraController.TweenSequence(CameraID.Digging, CameraID.Notice, CameraID.Main);
                }
                else
                {
                    cameraController.TweenTo(CameraID.Main);
                }
            }

            isDigging = false;
        }
    }
}