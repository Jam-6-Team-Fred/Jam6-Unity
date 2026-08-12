using UnityEngine;

public class FlashbackRecorder : MonoBehaviour
{
	[SerializeField]
	private float _snapshotInterval = 5f;

	private int _numCapturedSnapshots;

	private RenderTexture[] _renderTextureArray;

	private bool[] _isSnapshotInDreamWorld;

	private float _lastSnapshotTime;

	private void Awake()
	{
		int num = ((PlayerData.GetGraphicSettings().textureQuality == TextureQuality.FULL) ? 270 : 135);
		int width = Mathf.RoundToInt((float)num * ((float)Screen.width / (float)Screen.height));
		int num2 = Mathf.CeilToInt(1500f / _snapshotInterval);
		_numCapturedSnapshots = 0;
		_renderTextureArray = new RenderTexture[num2];
		_isSnapshotInDreamWorld = new bool[num2];
		for (int i = 0; i < num2; i++)
		{
			_renderTextureArray[i] = new RenderTexture(width, num, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			_renderTextureArray[i].name = "FlashbackRenderTex_" + i;
			_isSnapshotInDreamWorld[i] = false;
		}
		_lastSnapshotTime = 0f;
		GlobalMessenger.AddListener("TakeFirstFlashbackSnapshot", OnTakeFirstFlashbackSnapshot);
	}

	private void OnDestroy()
	{
		if (_renderTextureArray != null)
		{
			for (int i = 0; i < _renderTextureArray.Length; i++)
			{
				Object.Destroy(_renderTextureArray[i]);
			}
		}
		GlobalMessenger.RemoveListener("TakeFirstFlashbackSnapshot", OnTakeFirstFlashbackSnapshot);
	}

	private void Update()
	{
		if (!(Time.timeSinceLevelLoad < 3f) && !PlayerState.IsDead() && Time.timeSinceLevelLoad > _lastSnapshotTime + _snapshotInterval)
		{
			TakeSnapshot();
		}
	}

	public void TakeSnapshot()
	{
		if (_numCapturedSnapshots < _renderTextureArray.Length)
		{
			OWCamera oWCamera = Locator.GetActiveCamera();
			if (oWCamera.CompareTag("LandingCamera") && oWCamera.GetComponent<LandingCamera>().mode == LandingCamera.Mode.Double)
			{
				oWCamera = Locator.GetPlayerCamera();
			}
			FlashbackScreenGrabImageEffect component = oWCamera.GetComponent<FlashbackScreenGrabImageEffect>();
			if (component == null)
			{
				Debug.LogError("Tried to take flashback snapshot on Camera " + oWCamera.name + " but there is no FlashbackScreenGrabImageEffect attached!", oWCamera);
			}
			else if (!oWCamera.mainCamera.isActiveAndEnabled)
			{
				ClearTargetTexture(_renderTextureArray[_numCapturedSnapshots]);
			}
			else
			{
				component.QueueScreenGrab(_renderTextureArray[_numCapturedSnapshots]);
			}
			_isSnapshotInDreamWorld[_numCapturedSnapshots] = PlayerState.InDreamWorld();
			_numCapturedSnapshots++;
			_lastSnapshotTime = Time.timeSinceLevelLoad;
		}
	}

	public int GetNumSnapshots()
	{
		return _numCapturedSnapshots;
	}

	public RenderTexture GetSnapshot(int index)
	{
		if (_renderTextureArray == null || index < 0 || index >= _numCapturedSnapshots)
		{
			return null;
		}
		return _renderTextureArray[index];
	}

	public bool IsSnapshotInDreamWorld(int index)
	{
		if (_isSnapshotInDreamWorld == null || index < 0 || index >= _numCapturedSnapshots)
		{
			return false;
		}
		return _isSnapshotInDreamWorld[index];
	}

	private void OnTakeFirstFlashbackSnapshot()
	{
		TakeSnapshot();
	}

	private void ClearTargetTexture(RenderTexture renderTexture)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		RenderTexture.active = active;
	}
}
