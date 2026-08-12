using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Streaming/Streaming Manager", 0)]
public class StreamingManager : MonoBehaviour
{
	public enum LoadingPriority
	{
		Low = 0,
		Normal = 1,
		High = 2
	}

	private static bool s_tableLoaded;

	private static StreamingAssetBundleTable s_streamingAssetBundleTable;

	private static Dictionary<string, StreamingAssetBundle> s_streamingAssetBundleMap;

	private static List<StreamingAssetBundle> s_activeBundles;

	private static LoadingPriority s_loadingPriority;

	private static bool s_isBusy;

	private static StreamingCollisionMeshBakeManager _bakeManager;

	public static bool isStreamingEnabled => s_tableLoaded;

	public static bool isBusy => s_isBusy;

	public static LoadingPriority loadingPriority
	{
		get
		{
			return s_loadingPriority;
		}
		set
		{
			s_loadingPriority = value;
			if (s_loadingPriority == LoadingPriority.Low)
			{
				Application.backgroundLoadingPriority = ThreadPriority.Low;
			}
			else if (s_loadingPriority == LoadingPriority.Normal)
			{
				Application.backgroundLoadingPriority = ThreadPriority.Normal;
			}
			else
			{
				Application.backgroundLoadingPriority = ThreadPriority.High;
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		_bakeManager = UnityEngine.Object.FindObjectOfType<StreamingCollisionMeshBakeManager>();
		try
		{
			s_streamingAssetBundleTable = JsonUtility.FromJson<StreamingAssetBundleTable>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "StreamingAssetsTable.json")));
			s_tableLoaded = true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to load StreamingAssetsTable!  Streaming will be disabled.\n" + ex.ToString());
			s_streamingAssetBundleMap = new Dictionary<string, StreamingAssetBundle>(0);
			s_activeBundles = new List<StreamingAssetBundle>(0);
			s_tableLoaded = false;
		}
		if (s_tableLoaded)
		{
			int num = s_streamingAssetBundleTable.assetBundles.Length;
			s_streamingAssetBundleMap = new Dictionary<string, StreamingAssetBundle>(num);
			s_activeBundles = new List<StreamingAssetBundle>(num);
			for (int i = 0; i < num; i++)
			{
				StreamingAssetBundleTable.Entry entry = s_streamingAssetBundleTable.assetBundles[i];
				if (entry.assetBundleType == StreamingAssetBundleTable.BundleType.Mesh)
				{
					bool doNotUseThreadedBake = entry.assetBundleName.Contains("timberhearth");
					s_streamingAssetBundleMap.Add(entry.assetBundleName, new StreamingMeshAssetBundle(entry.assetBundleName, entry.meshNamesByID, entry.meshVertexCounts)
					{
						bakeManager = _bakeManager,
						doNotUseThreadedBake = doNotUseThreadedBake
					});
				}
				else if (entry.assetBundleType == StreamingAssetBundleTable.BundleType.Texture)
				{
					s_streamingAssetBundleMap.Add(entry.assetBundleName, new StreamingTextureAssetBundle(entry.assetBundleName, entry.textureLookupName));
				}
				else
				{
					s_streamingAssetBundleMap.Add(entry.assetBundleName, new StreamingAssetBundle(entry.assetBundleName));
				}
			}
		}
		loadingPriority = LoadingPriority.Normal;
	}

	public static void ForcePhysicsBakeJobsComplete()
	{
		_bakeManager.ForceAllJobsComplete();
	}

	public static void RegisterStreamingMeshHandle(StreamingMeshHandle streamingMeshHandle)
	{
		if (s_streamingAssetBundleMap.TryGetValue(streamingMeshHandle.assetBundle, out var value))
		{
			(value as StreamingMeshAssetBundle).RegisterStreamingMeshHandle(streamingMeshHandle);
		}
	}

	public static void UnregisterStreamingMeshHandle(StreamingMeshHandle streamingMeshHandle)
	{
		if (s_streamingAssetBundleMap.TryGetValue(streamingMeshHandle.assetBundle, out var value))
		{
			(value as StreamingMeshAssetBundle).UnregisterStreamingMeshHandle(streamingMeshHandle);
		}
	}

	public static void RegisterStreamingTextureSubscriber(string assetBundleID, IStreamingTexturesSubscriber subscriber)
	{
		if (s_streamingAssetBundleMap.TryGetValue(assetBundleID, out var value))
		{
			(value as StreamingIteratedTextureAssetBundle).RegisterSubscriber(subscriber);
		}
	}

	public static void UnregisterStreamingTextureSubscriber(string assetBundleID, IStreamingTexturesSubscriber subscriber)
	{
		if (s_streamingAssetBundleMap.TryGetValue(assetBundleID, out var value))
		{
			(value as StreamingIteratedTextureAssetBundle).UnregisterSubscriber(subscriber);
		}
	}

	public static void RegisterStreamingMaterialTable(StreamingMaterialTable streamingMaterialTable)
	{
		if (s_streamingAssetBundleMap.TryGetValue(streamingMaterialTable.assetBundle, out var value))
		{
			(value as StreamingTextureAssetBundle).RegisterStreamingMaterialTable(streamingMaterialTable);
		}
	}

	public static void UnregisterStreamingMaterialTable(StreamingMaterialTable streamingMaterialTable)
	{
		if (s_streamingAssetBundleMap.TryGetValue(streamingMaterialTable.assetBundle, out var value))
		{
			(value as StreamingTextureAssetBundle).UnregisterStreamingMaterialTable();
		}
	}

	public static bool StreamingAssetAvailable(string assetBundleName)
	{
		return s_streamingAssetBundleMap.ContainsKey(assetBundleName);
	}

	public static void SetUseMeshBundleThreadedBake(string assetBundleName, bool useThreadedBake)
	{
		if (s_streamingAssetBundleMap.TryGetValue(assetBundleName, out var value) && value is StreamingMeshAssetBundle streamingMeshAssetBundle)
		{
			streamingMeshAssetBundle.doNotUseThreadedBake = !useThreadedBake;
		}
	}

	public static void LoadStreamingAssets(string assetBundleName, int priority = 0)
	{
		if (s_streamingAssetBundleMap.TryGetValue(assetBundleName, out var value))
		{
			int priority2 = value.priority;
			value.priority = priority;
			if (value.isUnloaded)
			{
				InsertSorted(s_activeBundles, value);
			}
			else if (priority != priority2)
			{
				s_activeBundles.Remove(value);
				InsertSorted(s_activeBundles, value);
			}
			value.Load();
		}
	}

	public static void UnloadStreamingAssets(string assetBundleName, float delay = 0f)
	{
		if (s_streamingAssetBundleMap.TryGetValue(assetBundleName, out var value))
		{
			value.Unload(delay);
			if (value.isUnloaded)
			{
				s_activeBundles.Remove(value);
			}
		}
	}

	public static void ConvertTextureAssetBundleToIterable(string assetBundleName)
	{
		if (!s_streamingAssetBundleMap.TryGetValue(assetBundleName, out var value) || value.GetType() != typeof(StreamingTextureAssetBundle))
		{
			return;
		}
		StreamingAssetBundleTable.Entry entry = default(StreamingAssetBundleTable.Entry);
		for (int i = 0; i < s_streamingAssetBundleTable.assetBundles.Length; i++)
		{
			if (s_streamingAssetBundleTable.assetBundles[i].assetBundleName == value.assetBundleName)
			{
				entry = s_streamingAssetBundleTable.assetBundles[i];
				break;
			}
		}
		string[] meshNamesByID = entry.meshNamesByID;
		StreamingIteratedTextureAssetBundle value2 = new StreamingIteratedTextureAssetBundle(value.assetBundleName, (value as StreamingTextureAssetBundle).GetTextureLookupName(), meshNamesByID);
		s_streamingAssetBundleMap[assetBundleName] = value2;
	}

	public static void SetStreamingAssetsPriority(string assetBundleName, int priority)
	{
		if (!s_streamingAssetBundleMap.TryGetValue(assetBundleName, out var value))
		{
			return;
		}
		if (value.isUnloaded)
		{
			value.priority = priority;
			return;
		}
		int priority2 = value.priority;
		value.priority = priority;
		if (priority != priority2)
		{
			s_activeBundles.Remove(value);
			InsertSorted(s_activeBundles, value);
		}
	}

	public static StreamingAssetBundleState GetStreamingAssetBundleState(string assetBundleName)
	{
		if (s_streamingAssetBundleMap.TryGetValue(assetBundleName, out var value))
		{
			return new StreamingAssetBundleState(value);
		}
		return default(StreamingAssetBundleState);
	}

	private static void InsertSorted(List<StreamingAssetBundle> list, StreamingAssetBundle item)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (item.priority > list[i].priority)
			{
				list.Insert(i, item);
				return;
			}
		}
		list.Add(item);
	}

	private void Update()
	{
		StreamingAssetBundle.ClearLocks();
		StreamingMeshAssetBundle.ResetVerticesCounter();
		bool flag = false;
		for (int i = 0; i < s_activeBundles.Count; i++)
		{
			s_activeBundles[i].Update();
			if (s_activeBundles[i].isLoading || s_activeBundles[i].isUnloading)
			{
				flag = true;
			}
			if (s_activeBundles[i].isUnloaded)
			{
				s_activeBundles.RemoveAt(i--);
			}
		}
		if (s_isBusy != flag)
		{
			s_isBusy = flag;
		}
	}
}
