using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Mesh.CombineMeshes fails in mysterious ways.
/// this implements that function in a way that acts more reliably
///
/// BUG: fails with negative scale. idk how to fix that
/// BUG: doesnt bring over child objects. oh well
/// BUG: uses float32 and unorm8 everything instead of preserving format, so size will be bigger. oh well
/// </summary>
[CustomEditor(typeof(MeshBatch))]
public class MeshBatchEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var meshBatch = (MeshBatch)target;
		if (GUILayout.Button(!meshBatch.isBatched ? "\nBATCH\n(No Undo)\n" : "\nUNBATCH\n(No Undo)\n"))
		{
			if (!meshBatch.isBatched)
			{
				var meshes = Batch(meshBatch);

				if (!Directory.Exists("Assets/BatchedRenderers")) Directory.CreateDirectory("Assets/BatchedRenderers");
				if (!Directory.Exists("Assets/BatchedColliders")) Directory.CreateDirectory("Assets/BatchedColliders");
				AssetDatabase.StartAssetEditing();
				try
				{
					foreach (var mesh in meshes)
					{
						AssetDatabase.CreateAsset(mesh, $"Assets/{(mesh.name.StartsWith("BatchedRenderer") ? "BatchedRenderers" : "BatchedColliders")}/{mesh.name}.mesh");
					}
				}
				finally
				{
					AssetDatabase.StopAssetEditing();
				}

				GC.Collect();
				EditorUtility.UnloadUnusedAssetsImmediate();
				GC.Collect();
			}
			else
			{
				var meshes = Unbatch(meshBatch);

				if (!Directory.Exists("Assets/BatchedRenderers")) Directory.CreateDirectory("Assets/BatchedRenderers");
				if (!Directory.Exists("Assets/BatchedColliders")) Directory.CreateDirectory("Assets/BatchedColliders");
				AssetDatabase.StartAssetEditing();
				try
				{
					foreach (var mesh in meshes)
					{
						AssetDatabase.DeleteAsset($"Assets/{(mesh.name.StartsWith("BatchedRenderer") ? "BatchedRenderers" : "BatchedColliders")}/{mesh.name}.mesh");
					}
				}
				finally
				{
					AssetDatabase.StopAssetEditing();
				}

				GC.Collect();
				EditorUtility.UnloadUnusedAssetsImmediate();
				GC.Collect();
			}
		}
	}

	public static List<Mesh> Batch(MeshBatch meshBatch)
	{
		var meshes = new List<Mesh>();
		if (meshBatch.isBatched) return meshes;

		var batchRenderers = BatchRenderers(meshBatch.transform, meshBatch.GetComponentsInChildren<MeshRenderer>());
		var batchColliders = BatchColliders(meshBatch.transform, meshBatch.GetComponentsInChildren<MeshCollider>());

		meshBatch.originalGroup = meshBatch.gameObject;
		meshBatch.batchedGroup = new GameObject($"{meshBatch.originalGroup.name}_Batched");
		meshBatch.batchedGroup.transform.SetParent(meshBatch.originalGroup.transform.parent, false);
		meshBatch.batchedGroup.transform.SetSiblingIndex(meshBatch.originalGroup.transform.GetSiblingIndex() + 1);
		meshBatch.batchedGroup.transform.localPosition = meshBatch.originalGroup.transform.localPosition;
		meshBatch.batchedGroup.transform.localRotation = meshBatch.originalGroup.transform.localRotation;
		meshBatch.batchedGroup.transform.localScale = meshBatch.originalGroup.transform.localScale;

		meshBatch.originalGroup.SetActive(false);
		meshBatch.originalGroup.tag = "EditorOnly";

		foreach (var kvp in batchRenderers)
		{
			// gameobject id, layer id, material id
			var id = $"BatchedRenderer {GlobalObjectId.GetGlobalObjectIdSlow(meshBatch.originalGroup)} {kvp.Key.layer} {GlobalObjectId.GetGlobalObjectIdSlow(kvp.Key.material)}";

			var go = new GameObject(id);
			go.transform.SetParent(meshBatch.batchedGroup.transform, false);
			go.layer = kvp.Key.layer;

			var mesh = new Mesh();
			mesh.name = id;
			if (kvp.Value.tris.Contains(1 << 16)) mesh.indexFormat = IndexFormat.UInt32;
			mesh.vertices = kvp.Value.verts.ToArray();
			mesh.normals = kvp.Value.norms.ToArray();
			mesh.tangents = kvp.Value.tans.ToArray();
			mesh.uv = kvp.Value.uvs.ToArray();
			mesh.uv2 = kvp.Value.uv2s.ToArray();
			mesh.uv3 = kvp.Value.uv3s.ToArray();
			mesh.uv4 = kvp.Value.uv4s.ToArray();
			mesh.colors32 = kvp.Value.cols.ToArray();

			mesh.triangles = kvp.Value.tris.ToArray();
			mesh.Optimize();
			mesh.UploadMeshData(true);

			meshes.Add(mesh);

			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			go.AddComponent<MeshRenderer>().sharedMaterial = kvp.Key.material;
		}

		foreach (var kvp in batchColliders)
		{
			// gameobject id, layer id
			var id = $"BatchedCollider {GlobalObjectId.GetGlobalObjectIdSlow(meshBatch.originalGroup)} {kvp.Key}";

			var go = new GameObject(id);
			go.transform.SetParent(meshBatch.batchedGroup.transform, false);
			go.layer = kvp.Key;

			var mesh = new Mesh();
			mesh.name = id;
			if (kvp.Value.tris.Contains(1 << 16)) mesh.indexFormat = IndexFormat.UInt32;
			mesh.vertices = kvp.Value.verts.ToArray();
			mesh.normals = kvp.Value.norms.ToArray();
			mesh.tangents = kvp.Value.tans.ToArray();
			mesh.uv = kvp.Value.uvs.ToArray();
			mesh.uv2 = kvp.Value.uv2s.ToArray();
			mesh.uv3 = kvp.Value.uv3s.ToArray();
			mesh.uv4 = kvp.Value.uv4s.ToArray();
			mesh.colors32 = kvp.Value.cols.ToArray();

			mesh.triangles = kvp.Value.tris.ToArray();
			mesh.Optimize();
			mesh.UploadMeshData(true);

			meshes.Add(mesh);

			go.AddComponent<MeshCollider>().sharedMesh = mesh;
		}

		EditorUtility.SetDirty(meshBatch);
		return meshes;
	}

	public static List<Mesh> Unbatch(MeshBatch meshBatch)
	{
		var meshes = new List<Mesh>();
		if (!meshBatch.isBatched) return meshes;

		foreach (var meshFilter in meshBatch.batchedGroup.GetComponentsInChildren<MeshFilter>())
		{
			var mesh = meshFilter.sharedMesh;
			meshes.Add(mesh);
		}

		foreach (var meshCollider in meshBatch.batchedGroup.GetComponentsInChildren<MeshCollider>())
		{
			var mesh = meshCollider.sharedMesh;
			meshes.Add(mesh);
		}

		meshBatch.originalGroup = meshBatch.gameObject;
		DestroyImmediate(meshBatch.batchedGroup);
		meshBatch.batchedGroup = null;

		meshBatch.gameObject.SetActive(true);
		meshBatch.tag = "Untagged";

		EditorUtility.SetDirty(meshBatch);
		return meshes;
	}


	[MenuItem("Tools/Mesh Batch/DEBUG Get Mesh Data")]
	public static void GetMeshData()
	{
		var mesh = (Mesh)Selection.activeObject;

		Debug.Log($"attribs\n{mesh.GetVertexAttributes().Join()}");
		Debug.Log($"submeshes\n{mesh.subMeshCount}");
		for (var i = 0; i < mesh.subMeshCount; i++)
		{
			Debug.Log($"submesh\n{mesh.GetSubMesh(i)}");
			Debug.Log($"tris\n{mesh.GetTriangles(i).Length}\n{mesh.GetTriangles(i).Take(100).Join()}");
		}
		Debug.Log($"verts\n{mesh.vertices.Length}\n{mesh.vertices.Take(100).Join()}");
		Debug.Log($"norms\n{mesh.normals.Length}\n{mesh.normals.Take(100).Join()}");
		Debug.Log($"tans\n{mesh.tangents.Length}\n{mesh.tangents.Take(100).Join()}");
		Debug.Log($"uvs\n{mesh.uv.Length}\n{mesh.uv.Take(100).Join()}");
		Debug.Log($"uv2s\n{mesh.uv2.Length}\n{mesh.uv2.Take(100).Join()}");
		Debug.Log($"uv3s\n{mesh.uv3.Length}\n{mesh.uv3.Take(100).Join()}");
		Debug.Log($"uv4s\n{mesh.uv4.Length}\n{mesh.uv4.Take(100).Join()}");
		Debug.Log($"cols\n{mesh.colors32.Length}\n{mesh.colors32.Take(100).Join()}");
	}


	[MenuItem("Tools/Mesh Batch/Batch All")]
	public static void BatchAll()
	{
		var meshes = Resources.FindObjectsOfTypeAll<MeshBatch>()
			.Where(x => x.gameObject.scene.name != null && x.gameObject.scene.name != "DontDestroyOnLoad")
			.SelectMany(Batch)
			.ToList();

		if (!Directory.Exists("Assets/BatchedRenderers")) Directory.CreateDirectory("Assets/BatchedRenderers");
		if (!Directory.Exists("Assets/BatchedColliders")) Directory.CreateDirectory("Assets/BatchedColliders");
		AssetDatabase.StartAssetEditing();
		try
		{
			foreach (var mesh in meshes)
			{
				AssetDatabase.CreateAsset(mesh, $"Assets/{(mesh.name.StartsWith("BatchedRenderer") ? "BatchedRenderers" : "BatchedColliders")}/{mesh.name}.mesh");
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}

	[MenuItem("Tools/Mesh Batch/Unbatch All")]
	public static void UnbatchAll()
	{
		var meshes = Resources.FindObjectsOfTypeAll<MeshBatch>()
			.Where(x => x.gameObject.scene.name != null && x.gameObject.scene.name != "DontDestroyOnLoad")
			.SelectMany(Unbatch)
			.ToList();

		if (!Directory.Exists("Assets/BatchedRenderers")) Directory.CreateDirectory("Assets/BatchedRenderers");
		if (!Directory.Exists("Assets/BatchedColliders")) Directory.CreateDirectory("Assets/BatchedColliders");
		AssetDatabase.StartAssetEditing();
		try
		{
			foreach (var mesh in meshes)
			{
				AssetDatabase.DeleteAsset($"Assets/{(mesh.name.StartsWith("BatchedRenderer") ? "BatchedRenderers" : "BatchedColliders")}/{mesh.name}.mesh");
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}

	#region actual batching algorithm

	private class MeshData
	{
		public List<int> tris = new List<int>();
		public List<Vector3> verts = new List<Vector3>();
		public List<Vector3> norms = new List<Vector3>();
		public List<Vector4> tans = new List<Vector4>();
		public List<Vector2> uvs = new List<Vector2>();
		public List<Vector2> uv2s = new List<Vector2>();
		public List<Vector2> uv3s = new List<Vector2>();
		public List<Vector2> uv4s = new List<Vector2>();
		// base game only uses up to 4 uvs
		public List<Color32> cols = new List<Color32>(); // base game doesnt use hdr vertex colors
	}

	private static Dictionary<(int layer, Material material), MeshData> BatchRenderers(Transform root, MeshRenderer[] meshRenderers)
	{
		var newMeshes = new Dictionary<(int layer, Material material), MeshData>();
		try
		{
			for (var i = 0; i < meshRenderers.Length; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar("Batching Renderers", null, (float)i / meshRenderers.Length))
				{
					throw new OperationCanceledException("Cancelled Batching Renderers");
				}

				var meshRenderer = meshRenderers[i];
				var materials = meshRenderer.sharedMaterials;
				var mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
				if (mesh == null)
				{
					Debug.LogError("null mesh!", meshRenderer);
					continue;
				}

				var verts = mesh.vertices.Select(x => root.InverseTransformPoint(meshRenderer.transform.TransformPoint(x))).ToList();
				var norms = mesh.normals.Select(x => root.InverseTransformDirection(meshRenderer.transform.TransformDirection(x))).ToList();
				var tans = mesh.tangents;
				var uvs = mesh.uv;
				var uv2s = mesh.uv2;
				var uv3s = mesh.uv3;
				var uv4s = mesh.uv4;
				var cols = mesh.colors32;

				for (var j = 0; j < mesh.subMeshCount; j++)
				{
					// Debug.Log($"{mesh} {j}", meshRenderer);

					var material = materials[j];
					if (material == null)
					{
						Debug.LogError($"null material {j}!", meshRenderer);
						continue;
					}

					var tris = mesh.GetTriangles(j);

					if (!newMeshes.TryGetValue((meshRenderer.gameObject.layer, material), out var newMesh))
					{
						newMesh = new MeshData();
						newMeshes.Add((meshRenderer.gameObject.layer, material), newMesh);
					}

					// add verts to new mesh
					var v2t = new Dictionary<(Vector3 vert, Vector3 norm, Vector4 tan, Vector2 uv, Vector2 uv2, Vector2 uv3, Vector2 uv4, Color col), int>();
					foreach (var tri in tris)
					{
						// use default values if necessary
						var newVert = verts[tri];
						var newNorm = norms.Count == 0 ? Vector3.forward : norms[tri];
						var newTan = tans.Length == 0 ? new Vector4(1, 0, 0, 1) : tans[tri];
						var newUv = uvs.Length == 0 ? Vector2.zero : uvs[tri];
						var newUv2 = uv2s.Length == 0 ? Vector2.zero : uv2s[tri];
						var newUv3 = uv3s.Length == 0 ? Vector2.zero : uv3s[tri];
						var newUv4 = uv4s.Length == 0 ? Vector2.zero : uv4s[tri];
						var newCol = cols.Length == 0 ? Color.white : (Color)cols[tri];

						if (v2t.TryGetValue((newVert, newNorm, newTan, newUv, newUv2, newUv3, newUv4, newCol), out var newTri))
						{
							newMesh.tris.Add(newTri);
						}
						else
						{
							newTri = newMesh.verts.Count;
							v2t.Add((newVert, newNorm, newTan, newUv, newUv2, newUv3, newUv4, newCol), newTri);

							newMesh.tris.Add(newTri);
							newMesh.verts.Add(newVert);
							newMesh.norms.Add(newNorm);
							newMesh.tans.Add(newTan);
							newMesh.uvs.Add(newUv);
							newMesh.uv2s.Add(newUv2);
							newMesh.uv3s.Add(newUv3);
							newMesh.uv4s.Add(newUv4);
							newMesh.cols.Add(newCol);
						}
					}

					// Debugger.Break();
				}
			}

			// clear out attribs with only defaults
			foreach (var meshData in newMeshes.Values)
			{
				if (meshData.norms.All(x => x == Vector3.forward)) meshData.norms.Clear();
				if (meshData.tans.All(x => x == new Vector4(1, 0, 0, 1))) meshData.tans.Clear();
				if (meshData.uvs.All(x => x == Vector2.zero)) meshData.uvs.Clear();
				if (meshData.uv2s.All(x => x == Vector2.zero)) meshData.uv2s.Clear();
				if (meshData.uv3s.All(x => x == Vector2.zero)) meshData.uv3s.Clear();
				if (meshData.uv4s.All(x => x == Vector2.zero)) meshData.uv4s.Clear();
				if (meshData.cols.All(x => x == Color.white)) meshData.cols.Clear();
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
		return newMeshes;
	}

	// copied from above LOLLLLL
	private static Dictionary<int, MeshData> BatchColliders(Transform root, MeshCollider[] meshColliders)
	{
		var newMeshes = new Dictionary<int, MeshData>();
		try
		{
			for (var i = 0; i < meshColliders.Length; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar("Batching Colliders", null, (float)i / meshColliders.Length))
				{
					throw new OperationCanceledException("Cancelled Batching Colliders");
				}

				var meshCollider = meshColliders[i];
				var mesh = meshCollider.sharedMesh;
				if (mesh == null)
				{
					Debug.LogError("null mesh!", meshCollider);
					continue;
				}

				var tris = mesh.triangles;
				var verts = mesh.vertices.Select(x => root.InverseTransformPoint(meshCollider.transform.TransformPoint(x))).ToList();
				var norms = mesh.normals.Select(x => root.InverseTransformDirection(meshCollider.transform.TransformDirection(x))).ToList();
				var tans = mesh.tangents;
				var uvs = mesh.uv;
				var uv2s = mesh.uv2;
				var uv3s = mesh.uv3;
				var uv4s = mesh.uv4;
				var cols = mesh.colors32;

				if (!newMeshes.TryGetValue(meshCollider.gameObject.layer, out var newMesh))
				{
					newMesh = new MeshData();
					newMeshes.Add(meshCollider.gameObject.layer, newMesh);
				}

				// add verts to new mesh
				var v2t = new Dictionary<(Vector3 vert, Vector3 norm, Vector4 tan, Vector2 uv, Vector2 uv2, Vector2 uv3, Vector2 uv4, Color col), int>();
				foreach (var tri in tris)
				{
					// use default values if necessary
					var newVert = verts[tri];
					var newNorm = norms.Count == 0 ? Vector3.forward : norms[tri];
					var newTan = tans.Length == 0 ? new Vector4(1, 0, 0, 1) : tans[tri];
					var newUv = uvs.Length == 0 ? Vector2.zero : uvs[tri];
					var newUv2 = uv2s.Length == 0 ? Vector2.zero : uv2s[tri];
					var newUv3 = uv3s.Length == 0 ? Vector2.zero : uv3s[tri];
					var newUv4 = uv4s.Length == 0 ? Vector2.zero : uv4s[tri];
					var newCol = cols.Length == 0 ? Color.white : (Color)cols[tri];

					if (v2t.TryGetValue((newVert, newNorm, newTan, newUv, newUv2, newUv3, newUv4, newCol), out var newTri))
					{
						newMesh.tris.Add(newTri);
					}
					else
					{
						newTri = newMesh.verts.Count;
						v2t.Add((newVert, newNorm, newTan, newUv, newUv2, newUv3, newUv4, newCol), newTri);

						newMesh.tris.Add(newTri);
						newMesh.verts.Add(newVert);
						newMesh.norms.Add(newNorm);
						newMesh.tans.Add(newTan);
						newMesh.uvs.Add(newUv);
						newMesh.uv2s.Add(newUv2);
						newMesh.uv3s.Add(newUv3);
						newMesh.uv4s.Add(newUv4);
						newMesh.cols.Add(newCol);
					}
				}
			}

			// clear out attribs with only defaults
			foreach (var meshData in newMeshes.Values)
			{
				if (meshData.norms.All(x => x == Vector3.forward)) meshData.norms.Clear();
				if (meshData.tans.All(x => x == new Vector4(1, 0, 0, 1))) meshData.tans.Clear();
				if (meshData.uvs.All(x => x == Vector2.zero)) meshData.uvs.Clear();
				if (meshData.uv2s.All(x => x == Vector2.zero)) meshData.uv2s.Clear();
				if (meshData.uv3s.All(x => x == Vector2.zero)) meshData.uv3s.Clear();
				if (meshData.uv4s.All(x => x == Vector2.zero)) meshData.uv4s.Clear();
				if (meshData.cols.All(x => x == Color.white)) meshData.cols.Clear();
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
		return newMeshes;
	}

	#endregion
}
