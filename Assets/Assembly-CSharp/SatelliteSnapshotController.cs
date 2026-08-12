using UnityEngine;

public class SatelliteSnapshotController : MonoBehaviour
{
	[SerializeField]
	private bool _allowRearview = true;

	[SerializeField]
	private bool _showSplashTexture = true;

	[SerializeField]
	private GameObject _splashObject;

	[SerializeField]
	private GameObject _diagramObject;

	[SerializeField]
	private OWCamera _satelliteCamera;

	[SerializeField]
	private Renderer _projectionScreen;

	[SerializeField]
	private FadeLight _fadeLight;

	[SerializeField]
	private MeshRenderer _probeMesh;

	[Space]
	[SerializeField]
	private OWAudioSource _loopingSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	private RenderTexture _snapshotTexture;

	private Light _satelliteLight;

	private float _initLightIntensity;

	private Vector3 _initCamLocalRot;

	private SingleInteractionVolume _interactVolume;

	private PlayerLockOnTargeting _lockOnTargeting;

	private ScreenPrompt _forwardPrompt;

	private ScreenPrompt _rearviewPrompt;

	private ScreenPrompt _exitPrompt;

	private void Awake()
	{
		string prompt = (_allowRearview ? (UITextLibrary.GetString(UITextType.ProbeForwardSnapshotPrompt) + "   <CMD>") : (UITextLibrary.GetString(UITextType.ProbeSnapshotPrompt) + "   <CMD>"));
		_forwardPrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, prompt);
		if (_allowRearview)
		{
			_rearviewPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, UITextLibrary.GetString(UITextType.ProbeRearSnapshotPrompt) + "   <CMD>");
		}
		_exitPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LeavePrompt) + "   <CMD>");
		_satelliteLight = _satelliteCamera.GetComponent<Light>();
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_interactVolume.OnPressInteract += OnPressInteract;
		_initCamLocalRot = _satelliteCamera.transform.localEulerAngles;
		_satelliteCamera.enabled = false;
		_snapshotTexture = ProbeCamera.GetSharedSnapshotTexture();
		_satelliteCamera.targetTexture = _snapshotTexture;
		if (_fadeLight != null)
		{
			_initLightIntensity = _fadeLight.GetComponent<Light>().intensity;
		}
		SetSatelliteLightEnabled(enabled: false);
	}

	private void Start()
	{
		if (_showSplashTexture)
		{
			_splashObject.SetActive(value: true);
			_diagramObject.SetActive(value: false);
			_projectionScreen.gameObject.SetActive(value: false);
			_projectionScreen.material.SetTexture("_MainTex", _snapshotTexture);
			_projectionScreen.material.SetTexture("_EmissionMap", _snapshotTexture);
		}
		else
		{
			_projectionScreen.gameObject.SetActive(value: false);
		}
		_loopingSource.SetLocalVolume(0f);
		_lockOnTargeting = Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_snapshotTexture = null;
		_interactVolume.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract()
	{
		base.enabled = true;
		Locator.GetToolModeSwapper().UnequipTool();
		Locator.GetPromptManager().AddScreenPrompt(_exitPrompt, PromptPosition.UpperRight, makeVisible: true);
		Locator.GetPromptManager().AddScreenPrompt(_forwardPrompt, PromptPosition.UpperRight, makeVisible: true);
		if (_allowRearview)
		{
			Locator.GetPromptManager().AddScreenPrompt(_rearviewPrompt, PromptPosition.UpperRight, makeVisible: true);
		}
		if (_showSplashTexture)
		{
			_splashObject.SetActive(value: false);
			_diagramObject.SetActive(value: true);
			_projectionScreen.gameObject.SetActive(value: false);
		}
		if (_fadeLight != null)
		{
			_fadeLight.StartFade(0f, 2f);
		}
		AudioClip audioClip = _oneShotSource.PlayOneShot(AudioType.TH_ProjectorActivate);
		_loopingSource.FadeIn(audioClip.length);
		_lockOnTargeting.LockOn(_projectionScreen.transform, Vector3.zero);
		GlobalMessenger.FireEvent("EnterSatelliteCameraMode");
	}

	private void Update()
	{
		if (OWInput.IsInputMode(InputMode.SatelliteCam))
		{
			if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
			{
				_satelliteCamera.transform.localEulerAngles = _initCamLocalRot;
				RenderSnapshot();
			}
			else if (_allowRearview && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary))
			{
				_satelliteCamera.transform.localEulerAngles = _initCamLocalRot + new Vector3(0f, 180f, 0f);
				RenderSnapshot();
			}
			else if (OWInput.IsNewlyPressed(InputLibrary.cancel))
			{
				TurnOffProjector();
			}
		}
	}

	private void TurnOffProjector()
	{
		base.enabled = false;
		_lockOnTargeting.BreakLock();
		_interactVolume.ResetInteraction();
		if (_showSplashTexture)
		{
			_splashObject.SetActive(value: true);
			_diagramObject.SetActive(value: false);
			_projectionScreen.gameObject.SetActive(value: false);
		}
		if (_fadeLight != null)
		{
			_fadeLight.StartFade(_initLightIntensity, 2f);
		}
		AudioClip audioClip = _oneShotSource.PlayOneShot(AudioType.TH_ProjectorStop);
		_loopingSource.FadeOut(audioClip.length);
		Locator.GetPromptManager().RemoveScreenPrompt(_exitPrompt);
		if (_allowRearview)
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_rearviewPrompt);
		}
		Locator.GetPromptManager().RemoveScreenPrompt(_forwardPrompt);
		GlobalMessenger.FireEvent("ExitSatelliteCameraMode");
	}

	private void RenderSnapshot()
	{
		_projectionScreen.gameObject.SetActive(value: true);
		_splashObject.SetActive(value: false);
		_diagramObject.SetActive(value: false);
		_projectionScreen.gameObject.SetActive(value: true);
		_projectionScreen.material.SetTexture("_MainTex", _snapshotTexture);
		_projectionScreen.material.SetTexture("_EmissionMap", _snapshotTexture);
		_probeMesh.enabled = false;
		SetSatelliteLightEnabled(enabled: true);
		_satelliteCamera.Render();
		SetSatelliteLightEnabled(enabled: false);
		_probeMesh.enabled = true;
		_oneShotSource.PlayOneShot(AudioType.TH_SatelliteSnapshot);
	}

	private void SetSatelliteLightEnabled(bool enabled)
	{
		if (_satelliteLight != null)
		{
			_satelliteLight.enabled = enabled;
		}
	}
}
