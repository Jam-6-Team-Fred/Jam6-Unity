using System.IO;
using UnityEngine;

public class StreamingAssetBundle
{
	protected static bool s_loadingLock;

	protected static bool s_unloadLock;

	protected readonly string _assetBundleName;

	protected readonly string _assetBundlePath;

	protected int _priority;

	protected AssetBundleCreateRequest _loadBundleOperation;

	protected AssetBundle _assetBundle;

	protected bool _isLoading;

	protected bool _isLoaded;

	protected bool _isUnloading;

	protected float _scheduledUnloadTime;

	public string assetBundleName => _assetBundleName;

	public int priority
	{
		get
		{
			return _priority;
		}
		set
		{
			_priority = value;
		}
	}

	public virtual float progress
	{
		get
		{
			if (!_isLoaded)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public bool isLoading => _isLoading;

	public bool isLoaded => _isLoaded;

	public bool isUnloading => _isUnloading;

	public bool isUnloaded
	{
		get
		{
			if (!_isLoading && !_isLoaded)
			{
				return !_isUnloading;
			}
			return false;
		}
	}

	public static void ClearLocks()
	{
		s_loadingLock = false;
		s_unloadLock = false;
	}

	public StreamingAssetBundle(string assetBundleName)
	{
		_assetBundleName = assetBundleName;
		_assetBundlePath = Path.Combine(Application.streamingAssetsPath, assetBundleName);
		_priority = 0;
		_loadBundleOperation = null;
		_assetBundle = null;
		_isLoading = false;
		_isLoaded = false;
		_isUnloading = false;
		_scheduledUnloadTime = 0f;
	}

	public virtual void Load()
	{
		if (_loadBundleOperation == null)
		{
			_loadBundleOperation = AssetBundle.LoadFromFileAsync(_assetBundlePath);
			_loadBundleOperation.priority = ((_priority >= 0) ? _priority : 0);
			_isLoading = true;
		}
		else if (_isUnloading)
		{
			if (!_isLoaded)
			{
				_isLoading = true;
			}
			_isUnloading = false;
		}
	}

	public virtual void Unload(float delay = 0f)
	{
		if (!isUnloaded)
		{
			_isLoading = false;
			_isUnloading = true;
			_scheduledUnloadTime = Time.time + delay;
		}
	}

	public virtual void Update()
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
			if (Time.time > _scheduledUnloadTime && !s_unloadLock)
			{
				UnloadImmediate();
				s_unloadLock = true;
			}
		}
		else if (_isLoading)
		{
			_isLoading = false;
			_isLoaded = true;
		}
	}

	protected virtual void UnloadImmediate()
	{
		if (_assetBundle != null)
		{
			_assetBundle.Unload(unloadAllLoadedObjects: true);
		}
		_loadBundleOperation = null;
		_assetBundle = null;
		_isLoading = false;
		_isLoaded = false;
		_isUnloading = false;
	}
}
