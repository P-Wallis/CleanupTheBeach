using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachObject : MonoBehaviour
{
    public Renderer renderer;
    private MaterialPropertyBlock materialProperties = null;

    public Vector3 position { get { return transform.position; } set { transform.position = value; } }
    [HideInInspector]public int id;

    public void DebugSetColor(Color color)
    {
        if (materialProperties == null)
            materialProperties = new MaterialPropertyBlock();

        materialProperties.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(materialProperties);
    }
}
