using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject beachObjectPrefab;
    public PlayerController player;
    public int objectSpawnNumber;
    [Range(1,20)]public float scatterRadius;
    [Range(0, 5)] public float playerProximityRadius;
    public BeachObjectData[] valuableObjects;
    public BeachObjectData[] nonValuableObjects;

    private List<BeachObject> beachObjects = new List<BeachObject>();
    private BeachObject closestObject = null;

    private void Start()
    {
        BeachObjectData data;
        for (int i = 0; i < objectSpawnNumber; i++)
        {
            GameObject go = Instantiate(beachObjectPrefab, transform);
            Vector2 pos = Random.insideUnitCircle * scatterRadius;
            go.transform.localPosition = new Vector3(pos.x, 0, pos.y);

            if (i < valuableObjects.Length)
                data = valuableObjects[i];
            else
                data = nonValuableObjects[Random.Range(0, nonValuableObjects.Length)];

            BeachObject bo = go.GetComponent<BeachObject>();
            bo.Init(i,data);
            beachObjects.Add(bo);
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

            float currentDist = Vector3.SqrMagnitude(beachObjects[i].position - player.position);
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
        BeachObject newClosest = GetClosestBuriedBeachObject();
        if (closestObject == null || newClosest.ID != closestObject.ID)
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

        if (closestObject != null)
        {
            float distance = Vector3.Distance(newClosest.position, player.position);
            if (distance < playerProximityRadius)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    closestObject.DigUp();
                closestObject.DebugSetColor(closestObject.WasDugUp ? (closestObject.Data.isValuable ? Color.green : Color.blue) : Color.cyan);
            }
            else
                closestObject.DebugSetColor(Color.gray);
        }
    }
}
