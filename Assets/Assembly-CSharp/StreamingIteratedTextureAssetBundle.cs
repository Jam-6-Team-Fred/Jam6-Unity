using System.Collections.Generic;
using UnityEngine;

public class StreamingIteratedTextureAssetBundle : StreamingTextureAssetBundle
{
	public const int MAX_TEXTURES_PER_FRAME = 5;

	protected string[] _textureNamesByID;

	protected List<IStreamingTexturesSubscriber> _subscribers = new List<IStreamingTexturesSubscriber>(0);

	protected AssetBundleRequest[] _loadAssetOperations;

	protected bool[] _loadedFlags;

	protected int _currentAssetIndex;

	private bool _isLoadingTable;

	private bool _autoLoad = true;

	private Queue<int> _loadIndicesQueue = new Queue<int>(0);

	private int _numTextures
	{
		get
		{
			if (_textureNamesByID == null)
			{
				return 0;
			}
			return _textureNamesByID.Length;
		}
	}

	public int subscriberCount => _subscribers.Count;

	public override float progress => (float)_currentAssetIndex / (float)_numTextures;

	public StreamingIteratedTextureAssetBundle(string assetBundleName, string textureLookupName, string[] textureNames)
		: base(assetBundleName, textureLookupName)
	{
		_textureNamesByID = textureNames;
		_loadAssetOperations = new AssetBundleRequest[_numTextures];
		_loadedFlags = new bool[_numTextures];
	}

	public void LoadTextureLookup()
	{
		if (_numTextures <= 0)
		{
			StreamingManager.LoadStreamingAssets(_assetBundleName);
			_isLoadingTable = true;
		}
	}

	public override void Load()
	{
		base.Load();
		_currentAssetIndex = 0;
		foreach (IStreamingTexturesSubscriber subscriber in _subscribers)
		{
			subscriber.OnAssetBundleBeginLoad(this);
		}
	}

	public void SetAutoLoad(bool auto)
	{
		_autoLoad = auto;
	}

	public void LoadTexturesManual(params int[] indices)
	{
		foreach (int item in indices)
		{
			_loadIndicesQueue.Enqueue(item);
		}
	}

	public void ReleaseStreamsManual(params int[] indices)
	{
		foreach (int streamIndex in indices)
		{
			ReleaseStream(streamIndex);
		}
	}

	public void ReleaseStream(int streamIndex)
	{
		if (_loadAssetOperations[streamIndex] == null)
		{
			return;
		}
		_loadedFlags[streamIndex] = false;
		Resources.UnloadAsset(_loadAssetOperations[streamIndex].asset);
		_loadAssetOperations[streamIndex] = null;
		_currentAssetIndex--;
		foreach (IStreamingTexturesSubscriber subscriber in _subscribers)
		{
			subscriber.OnTextureUnloaded(streamIndex);
		}
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
		if (_isLoadingTable)
		{
			_textureLookup = _assetBundle.LoadAsset<StreamingTextureLookup>(_textureLookupName);
			int num = _textureLookup.textures.Length;
			_textureNamesByID = new string[num];
			_loadAssetOperations = new AssetBundleRequest[num];
			_loadedFlags = new bool[num];
			for (int i = 0; i < _numTextures; i++)
			{
				_textureNamesByID[i] = _textureLookup.textures[i].name;
			}
			_isLoadingTable = false;
			StreamingManager.UnloadStreamingAssets(_assetBundleName);
			return;
		}
		int num2 = 0;
		int num3 = 0;
		if (!_isLoaded)
		{
			for (int j = 0; j < _numTextures; j++)
			{
				if (_loadedFlags[j] || _loadAssetOperations[j] == null)
				{
					continue;
				}
				if (_loadAssetOperations[j].isDone)
				{
					Texture texture = _loadAssetOperations[j].asset as Texture;
					foreach (IStreamingTexturesSubscriber subscriber in _subscribers)
					{
						subscriber.OnTextureLoaded(j, texture);
					}
					_loadedFlags[j] = true;
					num3++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (_isUnloading)
		{
			if (Time.time > _scheduledUnloadTime && num2 == 0 && !StreamingAssetBundle.s_unloadLock)
			{
				UnloadImmediate();
				StreamingAssetBundle.s_unloadLock = true;
			}
		}
		else if (_isLoading)
		{
			if (_autoLoad)
			{
				UpdateAutoStream(num2);
			}
			else
			{
				UpdateManualStream(num3);
			}
		}
	}

	private void UpdateManualStream(int numDone)
	{
		int num = 0;
		while (_loadIndicesQueue.Count > 0 && !StreamingAssetBundle.s_unloadLock)
		{
			int num2 = _loadIndicesQueue.Dequeue();
			if (_loadAssetOperations[num2] == null && !_loadedFlags[num2])
			{
				_loadAssetOperations[num2] = _assetBundle.LoadAssetAsync(_textureNamesByID[num2]);
				_loadAssetOperations[num2].priority = ((_priority >= 0) ? _priority : 0);
				num++;
				_currentAssetIndex++;
			}
			if (num >= 5)
			{
				break;
			}
		}
		if (numDone >= _numTextures)
		{
			_isLoading = false;
			_isLoaded = true;
		}
	}

	private void UpdateAutoStream(int numLoadingAssets)
	{
		if (_currentAssetIndex < _numTextures)
		{
			int num = 0;
			while (_currentAssetIndex < _numTextures && !StreamingAssetBundle.s_loadingLock)
			{
				_loadAssetOperations[_currentAssetIndex] = _assetBundle.LoadAssetAsync(_textureNamesByID[_currentAssetIndex]);
				_loadAssetOperations[_currentAssetIndex].priority = ((_priority >= 0) ? _priority : 0);
				num++;
				_currentAssetIndex++;
				if (num >= 5)
				{
					break;
				}
			}
		}
		else if (numLoadingAssets == 0)
		{
			_isLoading = false;
			_isLoaded = true;
		}
	}

	public Texture GetTexture(int index)
	{
		if (_loadAssetOperations[index] == null || !_loadAssetOperations[index].isDone)
		{
			return null;
		}
		return _loadAssetOperations[index].asset as Texture;
	}

	public bool IsTextureAvailable(int index)
	{
		if (_loadAssetOperations[index] == null || !_loadAssetOperations[index].isDone)
		{
			return false;
		}
		return true;
	}

	public virtual void RegisterSubscriber(IStreamingTexturesSubscriber subscriber)
	{
		if (!_subscribers.Contains(subscriber))
		{
			_subscribers.Add(subscriber);
			subscriber.OnBeginSubscription(this);
			if (_isLoaded)
			{
				subscriber.OnTexturesLoaded(this);
			}
		}
	}

	public virtual void UnregisterSubscriber(IStreamingTexturesSubscriber subscriber)
	{
		if (_subscribers.Contains(subscriber))
		{
			_subscribers.Remove(subscriber);
		}
	}

	protected override void UnloadImmediate()
	{
		foreach (IStreamingTexturesSubscriber subscriber in _subscribers)
		{
			subscriber.OnTexturesUnloaded();
		}
		base.UnloadImmediate();
		for (int i = 0; i < _numTextures; i++)
		{
			if (!_loadedFlags[i])
			{
				continue;
			}
			_loadAssetOperations[i] = null;
			_loadedFlags[i] = false;
			foreach (IStreamingTexturesSubscriber subscriber2 in _subscribers)
			{
				subscriber2.OnTextureUnloaded(i);
			}
		}
		_currentAssetIndex = 0;
	}
}
