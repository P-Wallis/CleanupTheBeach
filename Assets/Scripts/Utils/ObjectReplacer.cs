using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectReplacer : MonoBehaviour
{
    private const string REPLACE_PREFIX = "[Replaced] ";
    public List<GameObject> gameObjectsToReplace;
    public List<ReplacePair> pairs;

    [System.Serializable]
    public class ReplacePair
    {
        public string name;
        public GameObject prefab;
    }

    public void Replace()
    {
#if UNITY_EDITOR

        Dictionary<string, GameObject> pairDict = new Dictionary<string, GameObject>();
        for (int i = 0; i < pairs.Count; i++)
        {
            if (pairDict.ContainsKey(pairs[i].name))
            {
                Debug.LogError("Some Replacement Prefabs Have Duplicate Names!");
            }
            else
            {
                pairDict.Add(pairs[i].name, pairs[i].prefab);
            }
        }

        Undo.SetCurrentGroupName("Replace Scene Objects with Prefabs");
        int group = Undo.GetCurrentGroup();

        Undo.RecordObject(this, "Running Replace");
        GameObject original;
        GameObject replacement;
        for (int i = 0; i < gameObjectsToReplace.Count; i++)
        {
            original = gameObjectsToReplace[i];
            if (original == null)
                continue;

            ReplacePair pair = null;
            for (int j = 0; j < pairs.Count; j++)
            {
                if (original.name.StartsWith(pairs[j].name))
                {
                    pair = pairs[j];
                    break;
                }
            }

            if (pair == null)
                continue;

            replacement = (GameObject)PrefabUtility.InstantiatePrefab(pair.prefab, original.transform.parent);
            replacement.name = REPLACE_PREFIX + original.name;
            replacement.transform.position = original.transform.position;
            replacement.transform.rotation = original.transform.rotation;
            replacement.transform.localScale = original.transform.localScale;
            Undo.RegisterCreatedObjectUndo(replacement, "Added Replacement Object");

            Undo.DestroyObjectImmediate(original);
            gameObjectsToReplace[i] = replacement;
        }

        Undo.CollapseUndoOperations(group);
#endif
    }

    public void RemovePrefix()
    {
#if UNITY_EDITOR 
        Undo.SetCurrentGroupName("Rename Scene Objects");
        int group = Undo.GetCurrentGroup();

        for (int i = 0; i < gameObjectsToReplace.Count; i++)
        {
            if (gameObjectsToReplace[i] == null)
                continue;

            Undo.RecordObject(gameObjectsToReplace[i], "Removed Object Prefix");
            if (gameObjectsToReplace[i].name.StartsWith(REPLACE_PREFIX))
                gameObjectsToReplace[i].name = gameObjectsToReplace[i].name.Substring(REPLACE_PREFIX.Length);
        }

        Undo.CollapseUndoOperations(group);
#endif
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(ObjectReplacer))]
public class ObjectReplacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Replace"))
        {
            ((ObjectReplacer)target).Replace();
        }

        if (GUILayout.Button("Remove Prefix"))
        {
            ((ObjectReplacer)target).RemovePrefix();
        }
    }
}
#endif
