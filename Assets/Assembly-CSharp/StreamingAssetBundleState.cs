public struct StreamingAssetBundleState
{
	private StreamingAssetBundle _streamingAssetBundle;

	public float progress
	{
		get
		{
			if (_streamingAssetBundle == null)
			{
				return 0f;
			}
			return _streamingAssetBundle.progress;
		}
	}

	public bool isLoading
	{
		get
		{
			if (_streamingAssetBundle == null)
			{
				return false;
			}
			return _streamingAssetBundle.isLoading;
		}
	}

	public bool isLoaded
	{
		get
		{
			if (_streamingAssetBundle == null)
			{
				return true;
			}
			return _streamingAssetBundle.isLoaded;
		}
	}

	public bool isUnloading
	{
		get
		{
			if (_streamingAssetBundle == null)
			{
				return false;
			}
			return _streamingAssetBundle.isUnloading;
		}
	}

	public bool isUnloaded
	{
		get
		{
			if (_streamingAssetBundle == null)
			{
				return true;
			}
			return _streamingAssetBundle.isUnloaded;
		}
	}

	public StreamingAssetBundleState(StreamingAssetBundle streamingAssetBundle)
	{
		_streamingAssetBundle = streamingAssetBundle;
	}
}
