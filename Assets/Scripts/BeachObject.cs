using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachObject : MonoBehaviour
{
    public Renderer renderer;
    private MaterialPropertyBlock materialProperties = null;

    public Vector3 position { get { return transform.position; } set { transform.position = value; } }

    private int id;
    public int ID { get { return id; } }

    private bool wasDugUp;
    public bool WasDugUp { get { return wasDugUp; } }

    private BeachObjectData data;
    public BeachObjectData Data { get { return data; } }


    public void DebugSetColor(Color color)
    {
        if (materialProperties == null)
            materialProperties = new MaterialPropertyBlock();

        materialProperties.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(materialProperties);
    }


    public void Init(int id, BeachObjectData data)
    {
        this.id = id;
        this.data = data;
        wasDugUp = false;
    }

    public void DigUp()
    {
        if (wasDugUp)
            return;

        wasDugUp = true;
        if (data != null && data.isValuable)
        {
            gameObject.SetActive(false);
        }
        else
            DebugSetColor(Color.blue);
    }
}
