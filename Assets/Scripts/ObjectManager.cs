using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject beachObjectPrefab;
    public PlayerController player;
    public int objectSpawnNumber = 10;
    [Range(1,300)]public float scatterRadius = 10;
    [Range(0, 5)] public float playerProximityRadius = 1;
    [Range(0, 20)] public float scannerMaxRadius = 5;
    [Range(0, 1)] public float scannerMaxVolume = 0.5f;
    public BeachObjectData[] valuableObjects;
    public BeachObjectData[] nonValuableObjects;
    public MenuManager menuManager;

    private List<BeachObject> beachObjects = new List<BeachObject>();
    private BeachObject closestObject = null;

    private AudioSource scannerBackground, scannerFound;

    List<RaycastHit> hits = new List<RaycastHit>();
    List<Ray> rays = new List<Ray>();

    private void Start()
    {
        BeachObjectData data;

        int terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        int waterMask = 1 << LayerMask.NameToLayer("Water");


        for (int i = 0; i < objectSpawnNumber; i++)
        {
            GameObject go = Instantiate(beachObjectPrefab, transform);
            bool posBad = true;
            Vector2 pos = Random.insideUnitCircle * scatterRadius;
            Vector3 rayPos = Vector3.zero;
            while(posBad)
            {
                rayPos = new Vector3(pos.x, 2, pos.y) + transform.position;
                Ray ray = new Ray(new Vector3(pos.x, 2, pos.y) + transform.position, -transform.up);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, waterMask | terrainMask))
                {
                    if (hit.collider.tag != "Water")
                        posBad = false;
                    rayPos = hit.point;
                }
                pos = Random.insideUnitCircle * scatterRadius;
            }

            go.transform.position = rayPos;

            if (i < valuableObjects.Length)
                data = valuableObjects[i];
            else
                data = nonValuableObjects[Random.Range(0, nonValuableObjects.Length)];

            BeachObject bo = go.GetComponent<BeachObject>();
            bo.Init(i,data);
            beachObjects.Add(bo);
        }

        scannerBackground = AudioManager._.PlayLoopedAudio(SoundID.Scanner, MixerID.SFX);
        scannerBackground.volume = 0;
        scannerFound = AudioManager._.PlayLoopedAudio(SoundID.Scanner_Found, MixerID.SFX);
        scannerFound.volume = 0;
    }

    private void OnDrawGizmos()
    {
        foreach (RaycastHit hit in hits)
        {
            Gizmos.DrawSphere(hit.point, 5);
        }
    }

    private BeachObject GetClosestBuriedBeachObject()
    {
        BeachObject closest = null;
        float dist = float.MaxValue;
        for (int i = 0; i < beachObjects.Count; i++)
        {
            if (beachObjects[i].WasDugUp) // Skip dug up items
                continue;

            float currentDist = Vector2.SqrMagnitude(new Vector2(beachObjects[i].position.x- player.position.x, beachObjects[i].position.z - player.position.z));
            if (currentDist < dist)
            {
                dist = currentDist;
                closest = beachObjects[i];
            }
        }

        return closest;
    }

    private void Update()
    {
        if (player.IsDigging || menuManager.IsShowingMenu)
        {
            scannerFound.volume = 0;
            scannerBackground.volume = 0;
        }
        else
        {

            BeachObject newClosest = GetClosestBuriedBeachObject();

            if (closestObject == null || (newClosest != null && newClosest.ID != closestObject.ID))
            {
                if (closestObject != null && !closestObject.WasDugUp)
                {
                    closestObject.DebugSetColor(Color.yellow);
                }
                if (newClosest != null)
                {
                    closestObject = newClosest;
                }
            }

            if (closestObject != null && newClosest != null)
            {
                float distance = Vector2.Distance(new Vector2(newClosest.position.x, newClosest.position.z), new Vector2(player.position.x, player.position.z));

                if (distance < playerProximityRadius)
                {
                    if (player.foundScanner)
                    {
                        scannerFound.volume = scannerMaxVolume;
                        scannerBackground.volume = 0;
                    }
                    closestObject.DebugSetColor(Color.cyan);
                    if (Input.GetKeyDown(KeyCode.Space))
                        player.Dig(closestObject);
                }
                else
                {
                    float proximity = Mathf.InverseLerp(scannerMaxRadius, playerProximityRadius, distance);
                    if (player.foundScanner)
                    {
                        scannerFound.volume = 0;
                        scannerBackground.volume = Mathf.Lerp(scannerMaxVolume * 0.2f, scannerMaxVolume, proximity);
                        scannerBackground.pitch = 0.8f + (proximity * 0.4f);
                    }
                    closestObject.DebugSetColor(Color.Lerp(Color.yellow, Color.gray, proximity));
                }
            }
            else
            {
                scannerFound.volume = 0;
                scannerBackground.volume = scannerMaxVolume * 0.2f;
                scannerBackground.pitch = 0.8f;
            }
        }
    }
}
