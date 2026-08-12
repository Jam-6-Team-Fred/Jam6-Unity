using UnityEngine;

public class SignalscopeAudioController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _oneShotExternalSource;

	[SerializeField]
	private OWAudioSource _zoomAdjustSource;

	[SerializeField]
	private OWAudioSource _staticSource;

	private PlayerCameraController _cameraController;

	private bool _zoomMode;

	private bool _adjustingZoom;

	private void Start()
	{
		base.enabled = false;
		GlobalMessenger.AddListener("IdentifySignal", OnIdentifySignal);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("IdentifySignal", OnIdentifySignal);
	}

	public void PlayChangeSignalscopeFrequency()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolScopeSwitchFreq);
	}

	public void OnZoomIn(PlayerCameraController cameraController)
	{
		_cameraController = cameraController;
		_zoomMode = true;
		base.enabled = true;
	}

	public void OnZoomOut()
	{
		_zoomMode = false;
	}

	public void PlaySignalscopeStatic()
	{
		_staticSource.FadeIn(0.5f, fadeFromNothing: false, randomizePlayhead: true);
	}

	public void StopSignalscopeStatic()
	{
		_staticSource.FadeOut(0.5f);
	}

	private void OnIdentifySignal()
	{
		_oneShotSource.PlayOneShot(AudioType.ToolScopeIdentifySignal);
	}

	private void Update()
	{
		bool flag = _cameraController.IsUpdatingFieldOfView();
		if (flag && !_adjustingZoom)
		{
			_zoomAdjustSource.clip = Locator.GetAudioManager().GetSingleAudioClip(AudioType.ToolScopeZoomAdjust);
			_zoomAdjustSource.Play();
		}
		else if (!flag && _adjustingZoom)
		{
			_zoomAdjustSource.Stop();
		}
		_adjustingZoom = flag;
		if (!_adjustingZoom && !_zoomMode)
		{
			base.enabled = false;
		}
	}
}
