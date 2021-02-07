using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshCombiner))]
public class MeshCombinerEditor : Editor {
	MeshCombiner mc;
	int centerSpacing = 25;

	public void OnEnable()
	{
		mc = (MeshCombiner)target;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		EditorGUILayout.Space ();
		mc.Name = EditorGUILayout.TextField ("Name", mc.Name);
		mc.EnableExperimentalFeatures = EditorGUILayout.Foldout (mc.EnableExperimentalFeatures,"Experimental Features");
		if (mc.EnableExperimentalFeatures) {
			GUILayout.BeginHorizontal("box");
			EditorGUILayout.LabelField ("Note: These may give unpredictable results");
			GUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(centerSpacing);
			mc.DeleteOverlapped = EditorGUILayout.Toggle ("Delete Overlapping", mc.DeleteOverlapped);
			GUILayout.Space(centerSpacing);
			EditorGUILayout.EndHorizontal();

			if (mc.DeleteOverlapped) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(centerSpacing);
				EditorGUILayout.LabelField ("Delete overlapping internal faces");
				GUILayout.Space(centerSpacing);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(centerSpacing);
			mc.GenerateSubmeshes = EditorGUILayout.Toggle ("Full Combine", mc.GenerateSubmeshes);
			GUILayout.Space(centerSpacing);
			EditorGUILayout.EndHorizontal();

			if (mc.GenerateSubmeshes) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(centerSpacing);
				EditorGUILayout.LabelField ("Merge everything into a single object");
				GUILayout.Space(centerSpacing);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(centerSpacing);
			mc.DeleteOriginalObjects = EditorGUILayout.Toggle ("Destroy Originals", mc.DeleteOriginalObjects);
			GUILayout.Space(centerSpacing);
			EditorGUILayout.EndHorizontal();

			if (mc.DeleteOriginalObjects) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(centerSpacing);
				EditorGUILayout.LabelField ("Destroy the original mesh objects.");
				GUILayout.Space(centerSpacing);
				EditorGUILayout.EndHorizontal();
			}
		}
		EditorGUILayout.Space ();

		if (GUILayout.Button ("Combine"))
			mc.Combine ();
	}
}
