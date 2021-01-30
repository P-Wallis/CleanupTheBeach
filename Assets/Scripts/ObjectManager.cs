using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public GameObject beachObjectPrefab;
    public PlayerController player;
    public int objectSpawnNumber;
    [Range(1,20)]public float radius;

    private List<BeachObject> beachObjects = new List<BeachObject>();
    private BeachObject closestObject = null;

    private void Start()
    {
        for (int i = 0; i < objectSpawnNumber; i++)
        {
            GameObject go = Instantiate(beachObjectPrefab, transform);
            Vector2 pos = Random.insideUnitCircle * radius;
            go.transform.localPosition = new Vector3(pos.x, 0, pos.y);

            BeachObject bo = go.GetComponent<BeachObject>();
            bo.id = i;
            beachObjects.Add(bo);
        }
    }

    private BeachObject GetClosestBeachObject()
    {
        BeachObject closest = null;
        float dist = float.MaxValue;
        for (int i = 0; i < beachObjects.Count; i++)
        {
            float currentDist = Vector3.SqrMagnitude(beachObjects[i].position - player.transform.position);
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
        BeachObject newClosest = GetClosestBeachObject();
        if (closestObject == null || newClosest.id != closestObject.id)
        {
            if (closestObject != null)
            {
                closestObject.DebugSetColor(Color.yellow);
            }
            if (newClosest != null)
            {
                newClosest.DebugSetColor(Color.blue);
                closestObject = newClosest;
            }
        }
    }
}
