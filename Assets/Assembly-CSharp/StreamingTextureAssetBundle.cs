using UnityEngine;

public class StreamingTextureAssetBundle : StreamingAssetBundle
{
	protected readonly string _textureLookupName;

	protected StreamingMaterialTable _streamingMaterialTable;

	protected AssetBundleRequest _loadAllAssetsOperation;

	protected StreamingTextureLookup _textureLookup;

	public override float progress
	{
		get
		{
			if (_loadAllAssetsOperation != null)
			{
				return _loadAllAssetsOperation.progress;
			}
			return 0f;
		}
	}

	public StreamingTextureAssetBundle(string assetBundleName, string textureLookupName)
		: base(assetBundleName)
	{
		_textureLookupName = textureLookupName;
		_loadAllAssetsOperation = null;
		_textureLookup = null;
	}

	public void RegisterStreamingMaterialTable(StreamingMaterialTable streamingMaterialTable)
	{
		_streamingMaterialTable = streamingMaterialTable;
		if (_isLoaded)
		{
			streamingMaterialTable.OnTexturesLoaded(this);
		}
	}

	public void UnregisterStreamingMaterialTable()
	{
		_streamingMaterialTable = null;
	}

	public string GetTextureLookupName()
	{
		return _textureLookupName;
	}

	public Texture GetTextureByID(int id)
	{
		return _textureLookup.textures[id];
	}

	public int GetTextureLookupID(string textureName)
	{
		for (int i = 0; i < _textureLookup.textures.Length; i++)
		{
			if (_textureLookup.textures[i].name.Equals(textureName))
			{
				return i;
			}
		}
		return -1;
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
		if (_isUnloading)
		{
			if (Time.time > _scheduledUnloadTime && (_loadAllAssetsOperation == null || _loadAllAssetsOperation.isDone) && !StreamingAssetBundle.s_unloadLock)
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
			if (_loadAllAssetsOperation == null)
			{
				if (!StreamingAssetBundle.s_loadingLock)
				{
					_loadAllAssetsOperation = _assetBundle.LoadAllAssetsAsync<Texture>();
					_loadAllAssetsOperation.priority = ((_priority >= 0) ? _priority : 0);
				}
			}
			else if (_loadAllAssetsOperation.isDone)
			{
				_isLoading = false;
				_isLoaded = true;
				_textureLookup = _assetBundle.LoadAsset<StreamingTextureLookup>(_textureLookupName);
				if (_streamingMaterialTable != null)
				{
					_streamingMaterialTable.OnTexturesLoaded(this);
				}
			}
		}
	}

	protected override void UnloadImmediate()
	{
		if (_isLoaded && _streamingMaterialTable != null)
		{
			_streamingMaterialTable.OnTexturesUnloaded();
		}
		base.UnloadImmediate();
		_loadAllAssetsOperation = null;
		_textureLookup = null;
	}
}
