using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string HORIZONTAL = "Horizontal";
    const string VERTICAL = "Vertical";

    public CameraController cameraController;
    public MemoriesController memoriesController;
    public InstructionManager instructionManager;

    public Vector3 position { get { return transform.position; } }

    public Transform rotationRoot;
    [Range(0, 15)] public float movementSpeed = 1;
    [Range(0, 15)] public float rotationSpeed = 1;
    [Range(0, 1)] public float walkingSoundMaxVolume = 1;
    [Range(0, 25)] public float walkingSoundFadeSpeed = 1;

    public bool foundScanner = true;
    public Transform handPosition;

    private AudioSource walkingSound;
    private bool isWalking = false;
    private bool isDigging;
    public bool IsDigging { get { return isDigging; } }

    private int terrainMask = 0;
    private int waterMask = 0;

    private void Start()
    {
        walkingSound = AudioManager._.PlayLoopedAudio(SoundID.Walking_On_Sand, MixerID.SFX);
        walkingSound.volume = 0;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        waterMask = 1 << LayerMask.NameToLayer("Water");
        memoriesController.HideMemories();
        instructionManager.ShowInstructions(InstructionID.Movement);
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
        float move = isDigging ? 0 : Input.GetAxis(VERTICAL);
        float rotate = isDigging ? 0 : Input.GetAxis(HORIZONTAL);
        isWalking = Mathf.Abs(move) > 0.01f;

        // Rotation
        rotationRoot.Rotate(0, rotate *Mathf.Rad2Deg * rotationSpeed * Time.deltaTime, 0);

        // Translation
        Vector3 movement = rotationRoot.forward * (move * movementSpeed * Time.deltaTime);
        Ray ray = new Ray(transform.position + (transform.up * 2) + movement, -transform.up);
        RaycastHit hit;
        bool hitwater = false;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, waterMask | terrainMask))
        {
            hitwater = hit.collider.tag == "Water";
        }
        if (!hitwater)
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
            instructionManager.ShowInstructions(InstructionID.NONE);
            cameraController.TweenTo(CameraID.Digging);
            AudioManager._.PlayOneShotSFX(SoundID.Digging, position);
            yield return new WaitForSeconds(AudioManager._.clipDict[SoundID.Digging].clip.length);

            beachObject.DigUp();
            if (beachObject.Data != null)
            {
                cameraController.TweenTo(CameraID.Examine);
                GameObject go = Instantiate(beachObject.Data.objectPrefab, handPosition.position, beachObject.Data.objectPrefab.transform.rotation);
                Rigidbody rb = go.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                Debug.Log("You Found " + beachObject.Data.objectName + "!");
                yield return new WaitForSeconds(2f);

                if (beachObject.Data.isValuable)
                {
                    go.SetActive(false);

                    AudioManager._.PlayOneShotSFX(SoundID.Special_Object, position);
                    cameraController.TweenTo(CameraID.Trigger_Memory);
                    yield return new WaitForSeconds(cameraController.tweenTime);
                    AudioManager._.CrossfadeMusic(false);
                    yield return new WaitForSeconds(1f);
                    cameraController.TweenTo(CameraID.Remembering);
                    instructionManager.ShowInstructions(InstructionID.Exit_Memory);
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
                    StartCoroutine(CoDropObject(rb));
                    cameraController.TweenTo(CameraID.Main);
                }
            }

            instructionManager.ShowInstructions(InstructionID.Movement);
            isDigging = false;
        }
    }

    IEnumerator CoDropObject(Rigidbody rb)
    {
        rb.isKinematic = false;
        yield return new WaitForSeconds(1);
        rb.isKinematic = true;
    }
}