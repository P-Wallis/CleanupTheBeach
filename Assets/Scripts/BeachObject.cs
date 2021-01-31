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

    public GameObject mound;

    public void DebugSetColor(Color color)
    {
#if DEBUG_COLOR
        if (materialProperties == null)
            materialProperties = new MaterialPropertyBlock();

        materialProperties.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(materialProperties);
#endif
    }


    public void Init(int id, BeachObjectData data)
    {
        this.id = id;
        this.data = data;
        wasDugUp = false;
        mound.transform.localPosition = new Vector3(0, Random.Range(-0.3f, 0.1f), 0);
    }

    public void DigUp()
    {
        if (wasDugUp)
            return;

        mound.SetActive(false);

        wasDugUp = true;
        if (data != null && data.isValuable)
        {
            gameObject.SetActive(false);
        }
    }
}
