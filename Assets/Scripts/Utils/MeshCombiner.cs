using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour {

	class CombineObject
	{
		public bool real = true;
		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;
		public Transform transform;

		public CombineObject(GameObject go)
		{
			if (go == null){
				real = false;
				return;
			}

			transform = go.transform;

			meshFilter = go.GetComponent<MeshFilter>();
			if (meshFilter == null){
				real = false;
				return;
			}

			meshRenderer = go.GetComponent<MeshRenderer>();
			if (meshRenderer == null){
				real = false;
				return;
			}
		}

	}
	class SubMeshBits{
		public GameObject go;
		public List<Vector3> verts;
		public List<Vector3> nrmls;
		public List<Vector2> UVs;
		public List<int> tris;
		public Material mat;

		public SubMeshBits(GameObject g, List<Vector3> v, List<Vector3> n, List<Vector2> u, List<int> t, Material m){
			go = g;
			verts = v;
			nrmls = n;
			UVs = u;
			tris = t;
			mat = m;
		}
	}
	private string m_name = "";
	public string Name { get { return m_name; } set { m_name = value; } }
	private bool m_enableExperimentalFeatures = false;
	public bool EnableExperimentalFeatures { get { return m_enableExperimentalFeatures; } set { m_enableExperimentalFeatures = value; } }
	private bool m_deleteOverlappedFaces = false;
	public bool DeleteOverlapped { get { return m_deleteOverlappedFaces; } set { m_deleteOverlappedFaces = value; } }
	private bool m_generateSubmeshes = false;
	public bool GenerateSubmeshes { get { return m_generateSubmeshes; } set { m_generateSubmeshes = value; } }
	private bool m_deleteOriginal = false;
	public bool DeleteOriginalObjects { get { return m_deleteOriginal; } set { m_deleteOriginal = value; } }
	public GameObject[] m_objectsToCombine;

	public void Combine()
	{
		//Get a list of the relevant objects
		List<CombineObject> objectsList = new List<CombineObject> ();

		for (int i = 0; i < m_objectsToCombine.Length; i++) {
			CombineObject co = new CombineObject (m_objectsToCombine [i]);
			if (co.real)
				objectsList.Add (co);
		}

		//Group objects by material
		List<List<CombineObject>> objectGroups = new List<List<CombineObject>>();
		List<CombineObject> preGroupedObjects = new List<CombineObject> ();
		preGroupedObjects.AddRange (objectsList);

		List<CombineObject> removeObjects = new List<CombineObject> ();
		while (preGroupedObjects.Count > 0) {
			removeObjects.Clear ();
			List<CombineObject> group = new List<CombineObject> ();
			group.Add (preGroupedObjects [0]);
			preGroupedObjects.RemoveAt (0);

			for (int i = 0; i < preGroupedObjects.Count; i++) 
			{
				if (preGroupedObjects [i].meshRenderer.sharedMaterial == group [0].meshRenderer.sharedMaterial)
				{
					removeObjects.Add (preGroupedObjects [i]);
					group.Add (preGroupedObjects [i]);
				}
			}

			objectGroups.Add (group);

			for (int i = 0; i < removeObjects.Count; i++) {
				preGroupedObjects.Remove (removeObjects [i]);
			}
		}

		//Combine Group Meshes into one mesh
		List<SubMeshBits> SMBs = new List<SubMeshBits>();
		for (int i = 0; i < objectGroups.Count; i++)
		{
			List<CombineObject> group = objectGroups [i];

			GameObject go = new GameObject ();
			go.transform.parent = this.transform;
			string nameString = "Combined Mesh " + (i + 1).ToString ();
			go.name = (m_name != "") ? m_name + " " + nameString : nameString;
			MeshFilter mf = go.AddComponent<MeshFilter> ();
			MeshRenderer mr = go.AddComponent<MeshRenderer> ();

			Vector3 groupCenter = GetGroupCenter (group);
			go.transform.position = groupCenter;

			List<Vector3> verts = new List<Vector3>();
			List<Vector3> nrmls = new List<Vector3> ();
			List<Vector2> UVs = new List<Vector2>();
			List<int> tris = new List<int>();

			Mesh msh;
			Transform tf;

			for (int j = 0; j < group.Count; j++) {
				msh = group [j].meshFilter.sharedMesh;
				tf = group [j].transform;
				int vertOffset = verts.Count;

				Vector3[] mshVrts = msh.vertices;
				for (int v = 0; v < mshVrts.Length; v++) { verts.Add (tf.TransformPoint (mshVrts [v])-groupCenter); }
				Vector3[] mshNrms = msh.normals;
				for (int n = 0; n < mshNrms.Length; n++) { nrmls.Add (tf.TransformDirection (mshNrms[n])); }
				Vector2[] mshUVs = msh.uv;
				for (int u = 0; u < mshUVs.Length; u++) { UVs.Add (mshUVs [u]); }
				int[] mshTris = msh.triangles;
				for (int t = 0; t < mshTris.Length; t++) { tris.Add (mshTris[t] + vertOffset); }
			}

			//If required, remove overlapped faces
			if (m_deleteOverlappedFaces) {
				List<int> badTris = new List<int> ();
				int index;
				for (int j = 0; j < (tris.Count / 3); j++) {
					index = j * 3;
					Vector3 v0 = verts [tris [index]];
					Vector3 v1 = verts [tris [index + 1]];
					Vector3 v2 = verts [tris [index + 2]];

					List<int> i0 = GetOverlappingPoints (v0, verts);
					if (i0.Count == 0)
						continue;
					List<int> i1 = GetOverlappingPoints (v1, verts);
					if (i1.Count == 0)
						continue;
					List<int> i2 = GetOverlappingPoints (v2, verts);
					if (i2.Count == 0)
						continue;

					bool b0 = PointHasInverse (tris[index], i0, nrmls);
					if (b0 == false)
						continue;
					bool b1 = PointHasInverse (tris [index + 1], i1, nrmls);
					if (b1 == false)
						continue;
					bool b2 = PointHasInverse (tris [index + 2], i2, nrmls);
					if (b2 == false)
						continue;

					badTris.Add (index);
				}

				badTris.Reverse ();
				for (int j = 0; j < badTris.Count; j++) {
					index = badTris [j];
					tris.RemoveAt (index + 2);
					tris.RemoveAt (index + 1);
					tris.RemoveAt (index);
				}

				if (badTris.Count > 0)
					Debug.Log ("Removed "+badTris.Count+" overlapping triangles from Mesh "+(i+1));
				
				List<int> deadVerts = new List<int> ();
				for (int j = 0; j < verts.Count; j++) {
					if (!tris.Contains (j))
						deadVerts.Add (j);
				}

				deadVerts.Reverse ();
				for (int j = 0; j < deadVerts.Count; j++) {
					verts.RemoveAt (deadVerts [j]);
					nrmls.RemoveAt (deadVerts [j]);
					UVs.RemoveAt (deadVerts [j]);
					for (int t = 0; t < tris.Count; t++) {
						if (tris [t] > deadVerts [j])
							tris [t]--;
					}
				}

				if (deadVerts.Count > 0)
					Debug.Log ("Removed "+deadVerts.Count+" unused verts from Mesh "+(i+1));

			}
				
			Mesh mesh = new Mesh ();
			mesh.name = go.name;
			mesh.SetVertices (verts);
			mesh.SetNormals (nrmls);
			mesh.SetUVs (0,UVs);
			mesh.SetTriangles (tris,0);

			mf.mesh = mesh;
			mr.sharedMaterial = group [0].meshRenderer.sharedMaterial;

			if (m_generateSubmeshes)
				SMBs.Add (new SubMeshBits(go, verts, nrmls,UVs,tris, mr.sharedMaterial));
		}

		//If requred, create one single object with the above versions as submeshes
		if (m_generateSubmeshes) {

			GameObject go = new GameObject ();
			go.transform.parent = this.transform;
			string nameString = "Combined Mesh";
			go.name = (m_name != "") ? m_name + " " + nameString : nameString;
			MeshFilter mf = go.AddComponent<MeshFilter> ();
			MeshRenderer mr = go.AddComponent<MeshRenderer> ();

			Vector3 groupCenter = GetGroupCenter (SMBs);
			go.transform.position = groupCenter;

			List<Vector3> verts = new List<Vector3>();
			List<Vector3> nrmls = new List<Vector3> ();
			List<Vector2> UVs = new List<Vector2>();
			Material[] mats = new Material[SMBs.Count];

			Transform tf;

			for (int i = 0; i < SMBs.Count; i++) {
				tf = SMBs [i].go.transform;
				int vertOffset = verts.Count;

				for (int v = 0; v < SMBs [i].verts.Count; v++) { verts.Add (tf.TransformPoint (SMBs [i].verts [v])-groupCenter); }
				for (int n = 0; n < SMBs [i].nrmls.Count; n++) { nrmls.Add (tf.TransformDirection (SMBs [i].nrmls[n])); }
				for (int u = 0; u < SMBs [i].UVs.Count; u++) { UVs.Add (SMBs [i].UVs[u]); }
				for (int t = 0; t < SMBs [i].tris.Count; t++) { SMBs [i].tris[t] += vertOffset; }
				mats [i] = SMBs [i].mat;
				DestroyImmediate (SMBs [i].go);
			}

			Mesh mesh = new Mesh ();
			mesh.name = go.name;
			mesh.SetVertices (verts);
			mesh.SetNormals (nrmls);
			mesh.SetUVs (0,UVs);
			mesh.subMeshCount = SMBs.Count;
			for (int i = 0; i < SMBs.Count; i++) { mesh.SetTriangles (SMBs [i].tris, i); }

			mf.mesh = mesh;
			mr.sharedMaterials = mats;
		}

		//If required, delete old objects
		if(m_deleteOriginal)
		{
			for (int i = 0; i < objectsList.Count; i++) {
				if(objectsList[i] != null && objectsList[i].real && objectsList[i].transform!=null)
					DestroyImmediate (objectsList [i].transform.gameObject);
			}
		}
	}

	Vector3 GetGroupCenter (List<CombineObject> group){
		Bounds bounds = new Bounds ();
		Vector3 min = group[0].transform.position, max = group[0].transform.position, cur;
		for (int j = 0; j < group.Count; j++) {
			cur = group [j].transform.position;

			//check x
			if (cur.x < min.x)
				min.x = cur.x;
			else if (cur.x > max.x)
				max.x = cur.x;
			//check y
			if (cur.y < min.y)
				min.y = cur.y;
			else if (cur.y > max.y)
				max.y = cur.y;
			//check z
			if (cur.z < min.z)
				min.z = cur.z;
			else if (cur.z > max.z)
				max.z = cur.y;
		}
		bounds.SetMinMax (min, max);
		return bounds.center;
	}
	Vector3 GetGroupCenter (List<SubMeshBits> group){
		Bounds bounds = new Bounds ();
		Vector3 min = group[0].go.transform.position, max = group[0].go.transform.position, cur;
		for (int j = 0; j < group.Count; j++) {
			cur = group [j].go.transform.position;

			//check x
			if (cur.x < min.x)
				min.x = cur.x;
			else if (cur.x > max.x)
				max.x = cur.x;
			//check y
			if (cur.y < min.y)
				min.y = cur.y;
			else if (cur.y > max.y)
				max.y = cur.y;
			//check z
			if (cur.z < min.z)
				min.z = cur.z;
			else if (cur.z > max.z)
				max.z = cur.y;
		}
		bounds.SetMinMax (min, max);
		return bounds.center;
	}

	List<int> GetOverlappingPoints(Vector3 point, List<Vector3> pointsList, float threshhold = 0.0001f){
		List<int> overlappedPoints = new List<int> ();
		float squareThreshhold = threshhold * threshhold;
		Vector3 relativePos;
		for (int i = 0; i < pointsList.Count; i++) {
			relativePos = pointsList [i] - point;
			if (relativePos.sqrMagnitude < squareThreshhold)
				overlappedPoints.Add (i);
		}
		return overlappedPoints;
	}

	bool PointHasInverse(int original, List<int> overlapedPoints, List<Vector3> normals){
		Vector3 nrml = normals [original];
		for (int i = 0; i < overlapedPoints.Count; i++) {
			if (overlapedPoints[i] == original)
				continue;
			if (Vector3.Cross(nrml, normals [overlapedPoints[i]]).magnitude < 0.0001f) 
				return true;
		}
		return false;
	}
}