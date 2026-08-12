using System.Collections.Generic;
using UnityEngine;

public class StreamingMeshAssetBundle : StreamingAssetBundle
{
	protected const int kMaxVerticesPerFrame = 65536;

	protected static int s_numVerticesThisFrame;

	protected readonly string[] _meshNamesByID;

	protected readonly int[] _meshVertexCounts;

	protected readonly int _numMeshes;

	protected List<StreamingMeshHandle>[] _streamingMeshHandles;

	protected AssetBundleRequest[] _loadAssetOperations;

	protected bool[] _loadedFlags;

	protected int _currentAssetIndex;

	public StreamingCollisionMeshBakeManager bakeManager;

	public bool doNotUseThreadedBake;

	public override float progress => (float)_currentAssetIndex / (float)_numMeshes;

	public static void ResetVerticesCounter()
	{
		s_numVerticesThisFrame = 0;
	}

	public StreamingMeshAssetBundle(string assetBundleName, string[] meshNamesByID, int[] meshVertexCounts)
		: base(assetBundleName)
	{
		_meshNamesByID = meshNamesByID;
		_meshVertexCounts = meshVertexCounts;
		_numMeshes = meshNamesByID.Length;
		_streamingMeshHandles = new List<StreamingMeshHandle>[_numMeshes];
		for (int i = 0; i < _numMeshes; i++)
		{
			_streamingMeshHandles[i] = new List<StreamingMeshHandle>(1);
		}
		_loadAssetOperations = new AssetBundleRequest[_numMeshes];
		_loadedFlags = new bool[_numMeshes];
		_currentAssetIndex = 0;
	}

	public void RegisterStreamingMeshHandle(StreamingMeshHandle streamingMeshHandle)
	{
		_streamingMeshHandles[streamingMeshHandle.meshIndex].Add(streamingMeshHandle);
		if (_isLoaded)
		{
			streamingMeshHandle.LoadMesh(_loadAssetOperations[streamingMeshHandle.meshIndex].asset as Mesh);
		}
	}

	public void UnregisterStreamingMeshHandle(StreamingMeshHandle streamingMeshHandle)
	{
		_streamingMeshHandles[streamingMeshHandle.meshIndex].QuickRemove(streamingMeshHandle);
	}

	private int CalcMaxVerticesThisFrame()
	{
		if (StreamingManager.loadingPriority == StreamingManager.LoadingPriority.High)
		{
			return 1048576;
		}
		if (StreamingManager.loadingPriority == StreamingManager.LoadingPriority.Normal)
		{
			return 262144;
		}
		return 65536;
	}

	public override void Update()
	{
		if (_loadBundleOperation == null || !_loadBundleOperation.isDone)
		{
			return;
		}
		if (_assetBundle == null)
		{
			_assetBundle = _loadBundleOperation.assetBundle;
		}
		int num = 0;
		if (!_isLoaded)
		{
			for (int i = 0; i < _numMeshes; i++)
			{
				if (_loadedFlags[i] || _loadAssetOperations[i] == null)
				{
					continue;
				}
				if (_loadAssetOperations[i].isDone)
				{
					Mesh mesh = _loadAssetOperations[i].asset as Mesh;
					for (int j = 0; j < _streamingMeshHandles[i].Count; j++)
					{
						_streamingMeshHandles[i][j].LoadMesh(mesh);
					}
					_loadedFlags[i] = true;
				}
				else
				{
					num++;
				}
			}
		}
		if (_isUnloading)
		{
			if (Time.time > _scheduledUnloadTime && num == 0 && !StreamingAssetBundle.s_unloadLock)
			{
				UnloadImmediate();
				StreamingAssetBundle.s_unloadLock = true;
			}
		}
		else
		{
			if (!_isLoading)
			{
				return;
			}
			if (_currentAssetIndex < _numMeshes)
			{
				int num2 = CalcMaxVerticesThisFrame();
				while (_currentAssetIndex < _numMeshes && !StreamingAssetBundle.s_loadingLock)
				{
					_loadAssetOperations[_currentAssetIndex] = _assetBundle.LoadAssetAsync(_meshNamesByID[_currentAssetIndex]);
					_loadAssetOperations[_currentAssetIndex].priority = ((_priority >= 0) ? _priority : 0);
					s_numVerticesThisFrame += _meshVertexCounts[_currentAssetIndex];
					if (s_numVerticesThisFrame > num2)
					{
						StreamingAssetBundle.s_loadingLock = true;
					}
					_currentAssetIndex++;
				}
			}
			else if (num == 0)
			{
				_isLoading = false;
				_isLoaded = true;
			}
		}
	}

	protected override void UnloadImmediate()
	{
		base.UnloadImmediate();
		for (int i = 0; i < _numMeshes; i++)
		{
			if (_loadedFlags[i])
			{
				_loadAssetOperations[i] = null;
				_loadedFlags[i] = false;
				for (int j = 0; j < _streamingMeshHandles[i].Count; j++)
				{
					_streamingMeshHandles[i][j].UnloadMesh();
				}
			}
		}
		_currentAssetIndex = 0;
	}

	public List<int> GetUnloadingMeshIDs()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _numMeshes; i++)
		{
			if (!_loadedFlags[i])
			{
				continue;
			}
			for (int j = 0; j < _streamingMeshHandles[i].Count; j++)
			{
				if (_streamingMeshHandles[i][j] is StreamingCollisionMeshHandle)
				{
					list.Add((_streamingMeshHandles[i][j] as StreamingCollisionMeshHandle).GetMeshID());
					break;
				}
			}
		}
		return list;
	}

	private bool IsColliderMeshBundle()
	{
		return base.assetBundleName.Contains("colliders");
	}
}
