using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StreamingMeshManager : EditorWindow
{
	private List<StreamingMeshHandle> _meshHandles = new List<StreamingMeshHandle>();
	private Dictionary<string, PlanetInfo> _planetNames = new Dictionary<string, PlanetInfo>();
	private List<(AssetBundle bundle, string path)> _loadedAssetBundles = new List<(AssetBundle bundle, string path)>();

	[MenuItem("Tools/Streaming Mesh Manager")]
	public static void ShowWindow()
	{
		var window = GetWindow(typeof(StreamingMeshManager));
		window.Show();
	}

	void OnGUI()
	{
		//test = (GameObject)EditorGUILayout.ObjectField("test go", test, typeof(GameObject), true);

		if (GUILayout.Button($"Find StreamingMeshHandles (Found {_meshHandles.Count})"))
		{
			_meshHandles = Resources.FindObjectsOfTypeAll<StreamingMeshHandle>().ToList();
			_planetNames = _meshHandles.Select(x => x.assetBundle.Split('/').First()).Distinct().ToDictionary(x => x, x => new PlanetInfo() { selected = false, count = 0 });
			foreach (var kvp in _planetNames)
			{
				kvp.Value.count = _meshHandles.Count(x => x.assetBundle.StartsWith(kvp.Key));
			}
		}

		/*
		if (GUILayout.Button("Copy asset bundles"))
		{
			FileUtil.CopyFileOrDirectory(@"D:\EpicGames\OuterWilds\OuterWilds_Data\StreamingAssets", @"Assets/StreamingAssets");
		}
  		*/

		var temp = new List<KeyValuePair<string, PlanetInfo>>(_planetNames);
		foreach (var kvp in temp)
		{
			_planetNames[kvp.Key].selected = EditorGUILayout.ToggleLeft($"{kvp.Key} ({kvp.Value.count})", _planetNames[kvp.Key].selected);
		}

		if (GUILayout.Button("BUILD MESHES"))
		{
			var table = JsonUtility.FromJson<StreamingAssetBundleTable>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "StreamingAssetsTable.json")));

			_meshHandles = Resources.FindObjectsOfTypeAll<StreamingMeshHandle>().ToList();

			foreach (var handle in _meshHandles)
			{
				if (_planetNames.Any(x => handle.assetBundle.StartsWith(x.Key) && !x.Value.selected))
				{
					continue;
				}

				// find asset bundle
				var path = Path.Combine(Application.streamingAssetsPath, handle.assetBundle);

				AssetBundle bundle;
				if (_loadedAssetBundles.Any(x => x.path == path))
				{
					bundle = _loadedAssetBundles.First(x => x.path == path).bundle;
				}
				else
				{
					bundle = AssetBundle.LoadFromFile(path);
					_loadedAssetBundles.Add((bundle, path));
				}

				var entry = table.assetBundles.First(x => x.assetBundleName == handle.assetBundle);
				var meshName = entry.meshNamesByID[handle.meshIndex];

				var mesh = bundle.LoadAsset<Mesh>(meshName);
				if (handle.TryGetComponent(out MeshFilter filter))
				{
					filter.sharedMesh = mesh;
				}
				else if (handle.TryGetComponent(out SkinnedMeshRenderer renderer))
				{
					renderer.sharedMesh = mesh;
				}
				else if (handle.TryGetComponent(out MeshCollider collider))
				{
					collider.sharedMesh = mesh;
				}
			}

			foreach (var (bundle, path) in _loadedAssetBundles)
			{
				bundle.Unload(false);
			}

			_loadedAssetBundles.Clear();
		}
	}


	[MenuItem("Tools/Load All Streaming Bundles")]
	public static void LoadAll()
	{
		var table = JsonUtility.FromJson<StreamingAssetBundleTable>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "StreamingAssetsTable.json")));

		var bundles = new Dictionary<string, AssetBundle>();

		foreach (var handle in Resources.FindObjectsOfTypeAll<StreamingMeshHandle>())
		{
			// find asset bundle
			var path = Path.Combine(Application.streamingAssetsPath, handle.assetBundle);

			if (!bundles.TryGetValue(path, out var bundle))
			{
				bundle = AssetBundle.LoadFromFile(path);
				bundles.Add(path, bundle);
			}

			var entry = table.assetBundles.First(x => x.assetBundleName == handle.assetBundle);
			var meshName = entry.meshNamesByID[handle.meshIndex];

			var mesh = bundle.LoadAsset<Mesh>(meshName);
			if (handle.TryGetComponent(out MeshFilter filter))
			{
				filter.sharedMesh = mesh;
			}
			else if (handle.TryGetComponent(out SkinnedMeshRenderer renderer))
			{
				renderer.sharedMesh = mesh;
			}
			else if (handle.TryGetComponent(out MeshCollider collider))
			{
				collider.sharedMesh = mesh;
			}
		}

		foreach (var pair in bundles)
		{
			pair.Value.Unload(false);
		}

		GC.Collect();
		EditorUtility.UnloadUnusedAssetsImmediate();
		GC.Collect();
	}
}

class PlanetInfo
{
	public bool selected;
	public int count;
}
