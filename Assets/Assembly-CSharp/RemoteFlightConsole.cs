using UnityEngine;

public class RemoteFlightConsole : MonoBehaviour, ILateInitializer
{
	[SerializeField]
	private OWRigidbody _modelShipBody;

	[SerializeField]
	private OWRigidbody _suspensionBody;

	[SerializeField]
	private OWCollider _playerProxyCollider;

	[SerializeField]
	private Transform _respawnPoint;

	[SerializeField]
	private float _respawnLength = 0.5f;

	[SerializeField]
	private SingularityWarpEffect _respawnEffect;

	[SerializeField]
	private GameObject _respawnProxyGeometry;

	[SerializeField]
	private OWAudioSource _consoleAudio;

	private SingleInteractionVolume _interactVolume;

	private PlayerAttachPoint _attachPoint;

	private PlayerLockOnTargeting _lockOnTargeting;

	private OWRigidbody _attachedBody;

	private SingularityWarpEffect _modelShipRespawnEffect;

	private bool _screenPromptsInitialized;

	private ScreenPrompt _resetPrompt;

	private ScreenPrompt _exitPrompt;

	private ScreenPrompt _verticalThrustPrompt;

	private ScreenPrompt _downThrustPrompt;

	private ScreenPrompt _horizontalThrustPrompt;

	private bool _isRespawning;

	private void Awake()
	{
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_resetPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.ResetPrompt));
		_exitPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LeavePrompt));
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			_verticalThrustPrompt = new ScreenPrompt(InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.UpPrompt));
			_downThrustPrompt = new ScreenPrompt(InputLibrary.thrustDown, UITextLibrary.GetString(UITextType.DownPrompt));
		}
		else
		{
			_verticalThrustPrompt = new ScreenPrompt(InputLibrary.thrustDown, InputLibrary.thrustUp, UITextLibrary.GetString(UITextType.DownUpPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
		}
		_horizontalThrustPrompt = new ScreenPrompt(InputLibrary.moveXZ, UITextLibrary.GetString(UITextType.HorizontalPrompt));
		_attachPoint = this.GetRequiredComponent<PlayerAttachPoint>();
		_interactVolume = this.GetRequiredComponent<SingleInteractionVolume>();
		_lockOnTargeting = base.gameObject.FindWithRequiredTag("Player").GetRequiredComponent<PlayerLockOnTargeting>();
		_attachedBody = base.gameObject.GetAttachedOWRigidbody();
		_modelShipRespawnEffect = _modelShipBody.GetComponentInChildren<SingularityWarpEffect>();
		_respawnProxyGeometry.SetActive(value: false);
		_isRespawning = false;
		_interactVolume.OnPressInteract += OnPressInteract;
		_modelShipRespawnEffect.OnWarpComplete += OnRecallComplete;
	}

	private void Start()
	{
		_modelShipBody.Suspend(_suspensionBody);
		_playerProxyCollider.SetActivation(active: false);
		GlobalMessenger.AddListener("OnModelRocketShipDestroyed", OnModelRocketShipDestroyed);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		_interactVolume.OnPressInteract -= OnPressInteract;
		GlobalMessenger.RemoveListener("OnModelRocketShipDestroyed", OnModelRocketShipDestroyed);
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_exitPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_resetPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_horizontalThrustPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_verticalThrustPrompt, PromptPosition.UpperRight);
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			Locator.GetPromptManager().AddScreenPrompt(_downThrustPrompt, PromptPosition.UpperRight);
		}
	}

	private void OnModelRocketShipDestroyed()
	{
		_modelShipBody = null;
		_lockOnTargeting.BreakLock();
		_attachPoint.DetachPlayer();
		_interactVolume.ResetInteraction();
		_interactVolume.DisableInteraction();
		_playerProxyCollider.SetActivation(active: false);
		base.enabled = false;
		GlobalMessenger.FireEvent("ExitRemoteFlightConsole");
		HideAllPrompts();
	}

	private void OnPressInteract()
	{
		if (!base.enabled)
		{
			Locator.GetToolModeSwapper().UnequipTool();
			_modelShipBody.Unsuspend();
			_lockOnTargeting.LockOn(_modelShipBody.transform, 5f, useZoom: true);
			_attachPoint.AttachPlayer();
			_playerProxyCollider.SetActivation(active: true);
			base.enabled = true;
			GlobalMessenger<OWRigidbody>.FireEvent("EnterRemoteFlightConsole", _modelShipBody);
		}
	}

	private void Update()
	{
		HideAllPrompts();
		if (OWInput.GetInputMode() != InputMode.ModelShip)
		{
			return;
		}
		bool flag = Vector3.Distance(_modelShipBody.transform.position, _respawnPoint.position) > 0.5f;
		if (flag)
		{
			_resetPrompt.SetVisibility(isVisible: true);
		}
		else
		{
			_exitPrompt.SetVisibility(isVisible: true);
			_verticalThrustPrompt.SetVisibility(isVisible: true);
			_horizontalThrustPrompt.SetVisibility(isVisible: true);
			if (JetpackPromptController.SPLIT_VERTICAL)
			{
				_downThrustPrompt.SetVisibility(isVisible: true);
			}
		}
		if (OWInput.IsNewlyPressed(InputLibrary.cancel))
		{
			if (flag)
			{
				RespawnModelShip(playEffects: true);
				return;
			}
			RespawnModelShip(playEffects: false);
			_modelShipBody.Suspend(_suspensionBody);
			_lockOnTargeting.BreakLock();
			_attachPoint.DetachPlayer();
			_interactVolume.ResetInteraction();
			_playerProxyCollider.SetActivation(active: false);
			base.enabled = false;
			GlobalMessenger.FireEvent("ExitRemoteFlightConsole");
			HideAllPrompts();
		}
	}

	private void HideAllPrompts()
	{
		_exitPrompt.SetVisibility(isVisible: false);
		_resetPrompt.SetVisibility(isVisible: false);
		_verticalThrustPrompt.SetVisibility(isVisible: false);
		_horizontalThrustPrompt.SetVisibility(isVisible: false);
		if (JetpackPromptController.SPLIT_VERTICAL)
		{
			_downThrustPrompt.SetVisibility(isVisible: false);
		}
	}

	private void RespawnModelShip(bool playEffects)
	{
		if (!playEffects)
		{
			OnRecallComplete();
		}
		else if (!_isRespawning)
		{
			_isRespawning = true;
			_respawnProxyGeometry.SetActive(value: true);
			_respawnEffect.WarpObjectIn(_respawnLength);
			_consoleAudio.PlayOneShot(AudioType.TH_RetrieveModelShip);
			_modelShipRespawnEffect.WarpObjectOut(_respawnLength);
		}
	}

	private void OnRecallComplete()
	{
		_isRespawning = false;
		_modelShipBody.SetPosition(_respawnPoint.position);
		_modelShipBody.transform.rotation = _respawnPoint.rotation;
		_modelShipBody.SetVelocity(_attachedBody.GetPointVelocity(_respawnPoint.position));
		_modelShipBody.SetAngularVelocity(Vector3.zero);
		_respawnProxyGeometry.SetActive(value: false);
	}
}
