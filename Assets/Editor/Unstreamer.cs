using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// used to unstream meshes/textures
/// </summary>
public static class Unstreamer
{
	[MenuItem("Tools/Unstream/Meshes For Handles Under Selected", true)]
	[MenuItem("Tools/Unstream/Textures For Handles Under Selected", true)]
	[MenuItem("Tools/Unstream/All For Handles Under Selected", true)]
	public static bool HandleExists() => Selection.activeGameObject && Selection.activeGameObject.GetComponentInChildren<StreamingMeshHandle>(true);

	[MenuItem("Tools/Unstream/Textures For Groups Under Selected", true)]
	public static bool GroupExists() => Selection.activeGameObject && Selection.activeGameObject.GetComponentInChildren<StreamingGroup>(true);


	[MenuItem("Tools/Unstream/Meshes For Handles Under Selected")]
	public static void MeshesForHandlesUnderSelected()
	{
		var handles = Selection.activeGameObject.GetComponentsInChildren<StreamingMeshHandle>(true);
		UnstreamMeshes(handles);
		foreach (var handle in handles)
		{
			Undo.DestroyObjectImmediate(handle);
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}

	[MenuItem("Tools/Unstream/Textures For Handles Under Selected")]
	public static void TexturesForHandlesUnderSelected()
	{
		var handles = Selection.activeGameObject.GetComponentsInChildren<StreamingMeshHandle>(true);
		UnstreamTextures(handles);
		foreach (var handle in handles)
		{
			Undo.DestroyObjectImmediate(handle);
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}

	[MenuItem("Tools/Unstream/All For Handles Under Selected")]
	public static void AllForHandlesUnderSelected()
	{
		var handles = Selection.activeGameObject.GetComponentsInChildren<StreamingMeshHandle>(true);
		UnstreamMeshes(handles);
		UnstreamTextures(handles);
		foreach (var handle in handles)
		{
			Undo.DestroyObjectImmediate(handle);
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}

	[MenuItem("Tools/Unstream/Textures For Groups Under Selected")]
	public static void TexturesForGroupsUnderSelected()
	{
		var groups = Selection.activeGameObject.GetComponentsInChildren<StreamingGroup>(true);
		UnstreamTextures(groups);
		foreach (var group in groups)
		{
			Undo.DestroyObjectImmediate(group.gameObject);
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}


	private static void UnstreamMeshes(StreamingMeshHandle[] handles)
	{
		// slow, but wont be run often so who cares
		try
		{
			var table = JsonUtility.FromJson<StreamingAssetBundleTable>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "StreamingAssetsTable.json")));

			for (var i = 0; i < handles.Length; i++)
			{
				var handle = handles[i];
				if (EditorUtility.DisplayCancelableProgressBar("Unstreaming Meshes", null, (float)i / handles.Length))
				{
					throw new OperationCanceledException("Cancelled Unstreaming Meshes");
				}

				var entry = table.assetBundles.First(x => x.assetBundleName == handle.assetBundle);
				var meshPath = entry.meshNamesByID[handle.meshIndex];

				var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

				if (handle.TryGetComponent(out MeshFilter filter))
				{
					Undo.RecordObject(filter, "Unstream Meshes");
					filter.sharedMesh = mesh;
				}
				else if (handle.TryGetComponent(out SkinnedMeshRenderer renderer))
				{
					Undo.RecordObject(renderer, "Unstream Meshes");
					renderer.sharedMesh = mesh;
				}

				var assetImporter = AssetImporter.GetAtPath(meshPath);
				Undo.RecordObject(assetImporter, "Unstream Meshes");
				assetImporter.SetAssetBundleNameAndVariant(null, null);
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private static void UnstreamTextures(StreamingMeshHandle[] handles)
	{
		// slow, but wont be run often so who cares
		try
		{
			var table = JsonUtility.FromJson<StreamingAssetBundleTable>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "StreamingAssetsTable.json")));

			var materialTables = AssetDatabase.FindAssets($"t:{typeof(StreamingMaterialTable)}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.Select(AssetDatabase.LoadAssetAtPath<StreamingMaterialTable>)
				.ToArray();

			var textureLookupPaths = AssetDatabase.FindAssets($"t:{typeof(StreamingTextureLookup)}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.ToArray();

			var materials = handles
				.SelectMany(x => x.TryGetComponent(out Renderer renderer) ? renderer.sharedMaterials : null)
				.Distinct()
				.ToArray();

			for (var i = 0; i < materials.Length; i++)
			{
				var material = materials[i];
				if (EditorUtility.DisplayCancelableProgressBar("Unstreaming Textures", null, (float)i / materials.Length))
				{
					throw new OperationCanceledException("Cancelled Unstreaming Textures");
				}

				var materialTable = materialTables.FirstOrDefault(x => x._materialPropertyLookups.Any(y => y.material == material));
				if (materialTable == null)
				{
					continue; // not a streamed material
				}

				var entry = table.assetBundles.First(x => x.assetBundleName == materialTable.assetBundle);
				var textureLookupPath = textureLookupPaths.First(x => x == entry.textureLookupName);
				var textureLookup = AssetDatabase.LoadAssetAtPath<StreamingTextureLookup>(textureLookupPath);

				var materialPropertyLookup = materialTable._materialPropertyLookups.First(x => x.material == material);

				Undo.RecordObject(material, "Unstream Textures");
				foreach (var propertyLookup in materialPropertyLookup.propertyLookups)
				{
					material.SetTexture(propertyLookup.propertyName, textureLookup.textures[propertyLookup.textureIndex]);

					var texturePath = AssetDatabase.GetAssetPath(textureLookup.textures[propertyLookup.textureIndex]);
					var assetImporter = AssetImporter.GetAtPath(texturePath);
					Undo.RecordObject(assetImporter, "Unstream Textures");
					assetImporter.SetAssetBundleNameAndVariant(null, null);
				}
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private static void UnstreamTextures(StreamingGroup[] groups)
	{
		// slow, but wont be run often so who cares
		try
		{
			var table = JsonUtility.FromJson<StreamingAssetBundleTable>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "StreamingAssetsTable.json")));

			var materialTables = groups
				.SelectMany(x => x._streamingMaterialTables)
				.ToArray();

			var textureLookupPaths = AssetDatabase.FindAssets($"t:{typeof(StreamingTextureLookup)}")
				.Select(AssetDatabase.GUIDToAssetPath)
				.ToArray();

			for (var i = 0; i < materialTables.Length; i++)
			{
				var materialTable = materialTables[i];
				if (EditorUtility.DisplayCancelableProgressBar("Unstreaming Textures", null, (float)i / materialTables.Length))
				{
					throw new OperationCanceledException("Cancelled Unstreaming Textures");
				}

				var entry = table.assetBundles.First(x => x.assetBundleName == materialTable.assetBundle);
				var textureLookupPath = textureLookupPaths.First(x => x == entry.textureLookupName);
				var textureLookup = AssetDatabase.LoadAssetAtPath<StreamingTextureLookup>(textureLookupPath);

				foreach (var materialPropertyLookup in materialTable._materialPropertyLookups)
				{
					var material = materialPropertyLookup.material;

					Undo.RecordObject(material, "Unstream Textures");
					foreach (var propertyLookup in materialPropertyLookup.propertyLookups)
					{
						material.SetTexture(propertyLookup.propertyName, textureLookup.textures[propertyLookup.textureIndex]);

						var texturePath = AssetDatabase.GetAssetPath(textureLookup.textures[propertyLookup.textureIndex]);
						var assetImporter = AssetImporter.GetAtPath(texturePath);
						Undo.RecordObject(assetImporter, "Unstream Textures");
						assetImporter.SetAssetBundleNameAndVariant(null, null);
					}
				}
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}
}
