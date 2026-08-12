using System.Collections;
using UnityEngine;

[RequireComponent(typeof(OWCamera))]
[RequireComponent(typeof(FlashbackRecorder))]
public class Flashback : MonoBehaviour
{
	[SerializeField]
	private float _screenStartDist = 12f;

	[SerializeField]
	private float _screenEndDist = 1f;

	[SerializeField]
	private float _playbackDelay = 6f;

	[SerializeField]
	private float _maskStartDist = 10f;

	[SerializeField]
	private float _maskEndDist = -1f;

	[SerializeField]
	private float _maskAnimDuration = 7f;

	[SerializeField]
	private float _lightFadeDuration = 5f;

	[SerializeField]
	private float _dataStreamDelay;

	[SerializeField]
	private float _dataStreamRevealDuration = 12f;

	[Space]
	[SerializeField]
	private float _reverseScreenStartDist = 2f;

	[SerializeField]
	private float _reverseScreenEndDist = 10f;

	[SerializeField]
	private float _reverseStreamRevealDuration = 3f;

	[SerializeField]
	private float _reversePlaybackDelay = 3f;

	[SerializeField]
	private float _reversePlaybackDuration = 7f;

	[SerializeField]
	private float _reverseScreenShrinkDuration = 3f;

	[Space]
	[SerializeField]
	private FlashbackAudioController _audioController;

	[SerializeField]
	private Transform _screenTransform;

	[SerializeField]
	private Transform _maskTransform;

	[SerializeField]
	private GameObject _forwardStreams;

	[SerializeField]
	private GameObject _reverseStreams;

	[Space]
	[SerializeField]
	private CanvasGroupAnimator _whiteFadeAnimator;

	[SerializeField]
	private AnimationCurve _whiteFadeCurve;

	private float _flashbackStartDelay = 1f;

	private const float INIT_FRAME_LENGTH = 0.6f;

	private const float MIN_FRAME_LENGTH = 0.06f;

	private const float FRAME_LENGTH_MULTIPLIER = 0.9f;

	private const float FADE_OUT_LENGTH = 1f;

	private const float UPLINK_DURATION = 10f;

	private bool _updateFlashback;

	private bool _updateMemoryUplink;

	private bool _waitForLoading;

	private Timer _uplinkTimer;

	private Timer _flashbackTimer;

	private float[] _imageDisplayTimes;

	private int _snapshotIndex;

	private Vector3 _origScreenScale;

	private MaterialPropertyBlock _matPropBlock_Streams;

	private int _propID_Fade;

	private Color _origStaticTint;

	private Renderer _screenRenderer;

	private Renderer[] _forwardStreamsRenderers;

	private Renderer[] _reverseStreamsRenderers;

	private OWLight[] _maskLights;

	private FlashbackRecorder _flashbackRecorder;

	private OWCamera _flashbackCamera;

	private AudioListener _audioListener;

	private void Awake()
	{
		GlobalMessenger.AddListener("TriggerFlashback", OnTriggerFlashback);
		GlobalMessenger.AddListener("TriggerMemoryUplink", OnTriggerMemoryUplink);
	}

	private void Start()
	{
		_flashbackRecorder = GetComponent<FlashbackRecorder>();
		_flashbackCamera = GetComponent<OWCamera>();
		_audioListener = GetComponentInChildren<AudioListener>();
		_flashbackCamera.enabled = false;
		float num = (float)Screen.width / (float)Screen.height;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			_screenTransform.localScale = Vector3.Scale(_screenTransform.localScale, new Vector3(num / 1.7777778f, 1f, 1f));
		}
		_origScreenScale = _screenTransform.localScale;
		_matPropBlock_Streams = new MaterialPropertyBlock();
		_propID_Fade = Shader.PropertyToID("_Fade");
		_screenRenderer = _screenTransform.GetComponent<Renderer>();
		_forwardStreamsRenderers = _forwardStreams.GetComponentsInChildren<Renderer>(includeInactive: true);
		_reverseStreamsRenderers = _reverseStreams.GetComponentsInChildren<Renderer>(includeInactive: true);
		_maskLights = _maskTransform.GetComponentsInChildren<OWLight>();
		_origStaticTint = _screenRenderer.sharedMaterial.GetColor("_StaticTint");
		_screenRenderer.material.SetTextureScale("_StaticMask", new Vector2(num, 1f));
		ResetEffects();
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TriggerFlashback", OnTriggerFlashback);
		GlobalMessenger.RemoveListener("TriggerMemoryUplink", OnTriggerMemoryUplink);
	}

	private void ResetEffects()
	{
		_forwardStreams.SetActive(value: false);
		_reverseStreams.SetActive(value: false);
		_maskTransform.gameObject.SetActive(value: false);
		_screenTransform.gameObject.SetActive(value: false);
		_matPropBlock_Streams.SetFloat(_propID_Fade, 0f);
		for (int i = 0; i < _forwardStreamsRenderers.Length; i++)
		{
			_forwardStreamsRenderers[i].SetPropertyBlock(_matPropBlock_Streams);
		}
		for (int j = 0; j < _reverseStreamsRenderers.Length; j++)
		{
			_reverseStreamsRenderers[j].SetPropertyBlock(_matPropBlock_Streams);
		}
		_screenTransform.localScale = _origScreenScale;
		for (int k = 0; k < _maskLights.Length; k++)
		{
			_maskLights[k].SetIntensity(0f);
		}
	}

	private void SetFlashbackImage(int snapshotIndex)
	{
		_screenRenderer.material.SetTexture("_MainTex", _flashbackRecorder.GetSnapshot(snapshotIndex));
		if (_flashbackRecorder.IsSnapshotInDreamWorld(snapshotIndex))
		{
			_screenRenderer.material.SetTextureOffset("_StaticMask", new Vector2(Random.value, Random.value));
			_screenRenderer.material.SetColor("_StaticTint", _origStaticTint);
		}
		else
		{
			_screenRenderer.material.SetColor("_StaticTint", new Color(1f, 1f, 1f, 0f));
		}
	}

	private void Update()
	{
		if (_updateFlashback)
		{
			UpdateFlashback();
		}
		else if (_updateMemoryUplink)
		{
			UpdateMemoryUplink();
		}
		else if (_waitForLoading)
		{
			WaitForLoading();
		}
	}

	private void OnTriggerFlashback()
	{
		if (PlayerData.GetPersistentCondition("GAME_OVER_LAST_SAVE"))
		{
			PlayerData.SetPersistentCondition("GAME_OVER_LAST_SAVE", state: false);
		}
		CenterOfTheUniverse.DeactivateUniverse();
		Locator.GetActiveCamera().enabled = false;
		base.transform.position = Vector3.zero;
		ResetEffects();
		_flashbackCamera.clearFlags = CameraClearFlags.Color;
		_flashbackCamera.enabled = true;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _flashbackCamera);
		_audioListener.enabled = true;
		_screenTransform.gameObject.SetActive(value: true);
		_maskTransform.gameObject.SetActive(value: true);
		_forwardStreams.SetActive(value: true);
		int numSnapshots = _flashbackRecorder.GetNumSnapshots();
		float num = 0f;
		_imageDisplayTimes = new float[numSnapshots];
		for (int i = 0; i < numSnapshots; i++)
		{
			num += Mathf.Max(0.6f * Mathf.Pow(0.9f, i), 0.06f);
			_imageDisplayTimes[numSnapshots - 1 - i] = num;
		}
		_snapshotIndex = numSnapshots - 1;
		num += 1f;
		DeathType deathType = Locator.GetDeathManager().GetDeathType();
		if (deathType == DeathType.Supernova || deathType == DeathType.Energy || deathType == DeathType.Lava || deathType == DeathType.DreamExplosion)
		{
			_flashbackStartDelay = 3f;
			_whiteFadeAnimator.gameObject.SetActive(value: true);
			_whiteFadeAnimator.SetImmediate(1f);
			_whiteFadeAnimator.AnimateTo(0f, Vector3.one, 3f, _whiteFadeCurve);
		}
		_flashbackTimer = new Timer(_flashbackStartDelay + _playbackDelay + num);
		_updateFlashback = true;
		_screenTransform.SetLocalPositionZ(_screenStartDist);
		_screenRenderer.material.color = new Color(1f, 1f, 1f, 0f);
		SetFlashbackImage(_snapshotIndex);
		_maskTransform.SetLocalPositionZ(_maskStartDist);
		GlobalMessenger.FireEvent("FlashbackStart");
		LoadManager.ReloadSceneAsync(autoTransition: false);
	}

	private void UpdateFlashback()
	{
		_flashbackTimer.Update();
		if (_flashbackTimer.SecondsNewlyElapsed(_flashbackStartDelay))
		{
			for (int i = 0; i < _maskLights.Length; i++)
			{
				_maskLights[i].FadeTo(1f, _lightFadeDuration);
			}
			_audioController.StartFlashback();
		}
		float t = _flashbackTimer.FractionOfSubspan(_flashbackStartDelay, _flashbackStartDelay + _maskAnimDuration);
		_maskTransform.SetLocalPositionZ(Mathf.Lerp(_maskStartDist, _maskEndDist, t));
		float value = _flashbackTimer.FractionOfSubspan(_flashbackStartDelay + _dataStreamDelay, _flashbackStartDelay + _dataStreamDelay + _dataStreamRevealDuration);
		_matPropBlock_Streams.SetFloat(_propID_Fade, value);
		for (int j = 0; j < _forwardStreamsRenderers.Length; j++)
		{
			_forwardStreamsRenderers[j].SetPropertyBlock(_matPropBlock_Streams);
		}
		for (int k = 0; k < _reverseStreamsRenderers.Length; k++)
		{
			_reverseStreamsRenderers[k].SetPropertyBlock(_matPropBlock_Streams);
		}
		float num = _flashbackStartDelay + _playbackDelay;
		_screenRenderer.material.color = new Color(1f, 1f, 1f, _flashbackTimer.FractionOfSubspan(num - 1f, num));
		float t2 = _flashbackTimer.FractionOfSubspan(num - 1f, _flashbackTimer.GetDuration());
		_screenTransform.SetLocalPositionZ(Mathf.Lerp(_screenStartDist, _screenEndDist, t2));
		if (_flashbackTimer.SecondsNewlyElapsed(num))
		{
			_audioController.StartPlayback(_flashbackTimer.startTime + num, _flashbackTimer.endTime);
		}
		if (_flashbackTimer.SecondsElapsed() > num && _flashbackTimer.Fraction() < 1f)
		{
			if (_snapshotIndex > 0 && _flashbackTimer.SecondsElapsed() > num + _imageDisplayTimes[_snapshotIndex - 1])
			{
				_snapshotIndex--;
				SetFlashbackImage(_snapshotIndex);
			}
			float num2 = Timer.FractionOfTimespan(_flashbackTimer.endTime - 1f, _flashbackTimer.endTime);
			_flashbackCamera.postProcessingSettings.eyeMask.openness = 1f - num2;
			if (num2 >= 1f)
			{
				_screenRenderer.material.SetTexture("_MainTex", null);
				_screenRenderer.material.SetColor("_StaticTint", new Color(1f, 1f, 1f, 0f));
			}
		}
		if (_flashbackTimer.SecondsNewlyElapsed(_flashbackTimer.GetDuration()))
		{
			_updateFlashback = false;
			TimeLoop.RestartTimeLoop();
			if (LoadManager.IsAsyncLoadComplete())
			{
				LoadManager.EnableAsyncLoadTransition();
				return;
			}
			_waitForLoading = true;
			SpinnerUI.Show();
		}
	}

	private void OnTriggerMemoryUplink()
	{
		_flashbackCamera.enabled = false;
		_flashbackRecorder.TakeSnapshot();
		ResetEffects();
		Locator.GetActiveCamera().postProcessingSettings.ditheringAvailable = false;
		Locator.GetActiveCamera().postProcessingSettings.customGammaAvailable = false;
		_flashbackCamera.postProcessingSettings.bloom.intensity = 1f;
		_flashbackCamera.postProcessingSettings.bloom.threshold = 1.3f;
		_flashbackCamera.clearFlags = CameraClearFlags.Depth;
		_flashbackCamera.enabled = true;
		_reverseStreams.SetActive(value: true);
		_screenTransform.gameObject.SetActive(value: true);
		_screenTransform.SetLocalPositionZ(_reverseScreenStartDist);
		_screenRenderer.material.color = new Color(1f, 1f, 1f, 0f);
		SetFlashbackImage(0);
		_updateMemoryUplink = true;
		_uplinkTimer = new Timer(_reversePlaybackDelay + _reversePlaybackDuration + _reverseScreenShrinkDuration);
		_audioController.StartMemoryUplink(_uplinkTimer.startTime, _uplinkTimer.endTime);
		LoadManager.ReloadSceneAsync(autoTransition: false, isStatueSequence: true);
	}

	private void UpdateMemoryUplink()
	{
		_uplinkTimer.Update();
		float value = _uplinkTimer.FractionOfSubspan(0f, _reverseStreamRevealDuration) - _uplinkTimer.FractionOfSubspan(_uplinkTimer.GetDuration() - _reverseScreenShrinkDuration, _uplinkTimer.GetDuration());
		_matPropBlock_Streams.SetFloat(_propID_Fade, value);
		for (int i = 0; i < _forwardStreamsRenderers.Length; i++)
		{
			_forwardStreamsRenderers[i].SetPropertyBlock(_matPropBlock_Streams);
		}
		for (int j = 0; j < _reverseStreamsRenderers.Length; j++)
		{
			_reverseStreamsRenderers[j].SetPropertyBlock(_matPropBlock_Streams);
		}
		float num = _uplinkTimer.FractionOfSubspan(_reversePlaybackDelay, _uplinkTimer.GetDuration());
		_screenTransform.SetLocalPositionZ(Mathf.Lerp(_reverseScreenStartDist, _reverseScreenEndDist, num * num));
		_screenRenderer.material.color = new Color(1f, 1f, 1f, _uplinkTimer.FractionOfSubspan(_reversePlaybackDelay, _reversePlaybackDelay + 2f));
		float num2 = _uplinkTimer.FractionOfSubspan(_reversePlaybackDelay, _reversePlaybackDelay + _reversePlaybackDuration, clamped: false);
		if (num2 >= 0f && num2 <= 1f)
		{
			int numSnapshots = _flashbackRecorder.GetNumSnapshots();
			int flashbackImage = Mathf.Min((int)((float)numSnapshots * num2), numSnapshots - 1);
			SetFlashbackImage(flashbackImage);
		}
		if (_uplinkTimer.SecondsNewlyElapsed(_uplinkTimer.GetDuration() - _reverseScreenShrinkDuration))
		{
			StartCoroutine(ShrinkScreen(_reverseScreenShrinkDuration));
		}
		if (_uplinkTimer.IsFinished())
		{
			ResetEffects();
			_updateMemoryUplink = false;
			_flashbackCamera.enabled = false;
			Locator.GetActiveCamera().postProcessingSettings.ditheringAvailable = true;
			Locator.GetActiveCamera().postProcessingSettings.customGammaAvailable = true;
			Locator.GetActiveCamera().postProcessingSettings.bloomEnabled = true;
			GlobalMessenger.FireEvent("MemoryUplinkComplete");
			if (LoadManager.IsAsyncLoadComplete())
			{
				LoadManager.EnableAsyncLoadTransition();
				return;
			}
			_waitForLoading = true;
			SpinnerUI.Show();
		}
	}

	private void WaitForLoading()
	{
		if (LoadManager.IsAsyncLoadComplete())
		{
			LoadManager.EnableAsyncLoadTransition();
			SpinnerUI.Hide();
		}
	}

	private IEnumerator ShrinkScreen(float duration)
	{
		float initTime = Time.time;
		while (true)
		{
			float num = Mathf.Clamp01((Time.time - initTime) / duration);
			_screenTransform.localScale = _origScreenScale * (1f - num);
			if (!(num >= 1f))
			{
				yield return null;
				continue;
			}
			break;
		}
	}
}
