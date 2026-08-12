using UnityEngine;
using UnityEngine.PostProcessing;

public class Peephole : MonoBehaviour
{
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private OWCamera _peepholeCamera;

	[SerializeField]
	private PostProcessingBehaviour _peepholeCameraPostProcessing;

	[SerializeField]
	private Sector _viewingSector;

	[SerializeField]
	private OWRenderer _hideWhileViewing;

	[Space]
	[SerializeField]
	private float _enterTransitionInLength = 0.25f;

	[SerializeField]
	private float _exitTransitionOutLength = 0.15f;

	[SerializeField]
	private float _exitZoomOutLength = 0.5f;

	[Space]
	[SerializeField]
	private float _enterCloseEyesLength = 0.25f;

	[SerializeField]
	private float _enterOpenEyesLength = 0.25f;

	[SerializeField]
	private float _exitCloseEyesLength = 0.25f;

	[SerializeField]
	private float _exitOpenEyesLength = 0.25f;

	[Space]
	[SerializeField]
	private string[] _factIDs = new string[0];

	private bool _peeping;

	private bool _enterTransitioningIn;

	private bool _exitTransitioningOut;

	private bool _enterClosingEyes;

	private bool _enterOpeningEyes;

	private bool _exitClosingEyes;

	private bool _exitOpeningEyes;

	private float _fadeTimer;

	private PlayerCameraEffectController _playerCameraEffectController;

	private PostProcessingProfile _peepCameraProfile;

	private ScreenPrompt _returnPrompt;

	public PlayerCameraEffectController playerCameraEffectController
	{
		get
		{
			if (_playerCameraEffectController == null)
			{
				_playerCameraEffectController = Locator.GetPlayerCameraController().GetComponent<PlayerCameraEffectController>();
			}
			return _playerCameraEffectController;
		}
	}

	private void Awake()
	{
		_interactReceiver.OnPressInteract += OnPressInteract;
		_peepholeCamera.enabled = false;
		_peepCameraProfile = Object.Instantiate(_peepholeCameraPostProcessing.profile);
		_peepCameraProfile.eyeMask.enabled = true;
		_peepholeCameraPostProcessing.profile = _peepCameraProfile;
	}

	private void Start()
	{
		_interactReceiver.SetPromptText(UITextType.PeepPrompt);
		_returnPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.LeavePrompt));
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_interactReceiver.OnPressInteract -= OnPressInteract;
	}

	private void OnPressInteract()
	{
		_peeping = true;
		_enterTransitioningIn = true;
		_enterClosingEyes = false;
		_enterOpeningEyes = false;
		_fadeTimer = 0f;
		if (!Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item))
		{
			Locator.GetToolModeSwapper().UnequipTool();
		}
		StartEnterCameraTransition();
		base.enabled = true;
	}

	private void StartEnterCameraTransition()
	{
		PlayerCameraController playerCameraController = Locator.GetPlayerCameraController();
		OWInput.ChangeInputMode(InputMode.None);
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(base.transform, Vector3.zero, 4f);
		playerCameraController.SnapToFieldOfView(30f, _enterTransitionInLength + _enterCloseEyesLength, smoothStep: true);
	}

	private void Update()
	{
		if (_peeping && !_enterTransitioningIn && (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel)))
		{
			_peeping = false;
			_exitClosingEyes = true;
			_exitTransitioningOut = false;
			_fadeTimer = 0f;
		}
		UpdateTransition();
		if (!_peeping && !_enterTransitioningIn && !_enterClosingEyes && !_enterOpeningEyes && !_exitClosingEyes && !_exitTransitioningOut)
		{
			base.enabled = false;
		}
	}

	private void UpdateTransition()
	{
		if (_peeping)
		{
			UpdateEnterTransition(Locator.GetPlayerCameraController());
		}
		else
		{
			UpdateExitTransition(Locator.GetPlayerCameraController());
		}
	}

	private void UpdateEnterTransition(PlayerCameraController playerCamera)
	{
		if (_enterTransitioningIn)
		{
			_fadeTimer += Time.deltaTime;
			if (_fadeTimer >= _enterTransitionInLength && !_enterClosingEyes)
			{
				playerCameraEffectController.CloseEyes(_enterCloseEyesLength);
				_enterClosingEyes = true;
			}
			if (_fadeTimer >= _enterTransitionInLength + _enterCloseEyesLength)
			{
				SetPeepholeCameraEyeOpenness(0f);
				Peep();
				_enterTransitioningIn = false;
				_enterClosingEyes = false;
				_enterOpeningEyes = true;
				_fadeTimer = 0f;
			}
		}
		else if (_enterOpeningEyes)
		{
			_fadeTimer += Time.deltaTime;
			float peepholeCameraEyeOpenness = Mathf.Clamp01(_fadeTimer / _enterOpenEyesLength);
			SetPeepholeCameraEyeOpenness(peepholeCameraEyeOpenness);
			if (_fadeTimer >= _enterOpenEyesLength)
			{
				_enterTransitioningIn = false;
				_enterClosingEyes = false;
				_enterOpeningEyes = false;
				_fadeTimer = 0f;
			}
		}
	}

	private void UpdateExitTransition(PlayerCameraController playerCamera)
	{
		if (_exitClosingEyes)
		{
			_fadeTimer += Time.deltaTime;
			float num = Mathf.Clamp01(_fadeTimer / _exitCloseEyesLength);
			SetPeepholeCameraEyeOpenness(1f - num);
			if (_fadeTimer >= _exitCloseEyesLength)
			{
				Unpeep();
				_exitClosingEyes = false;
				_exitTransitioningOut = true;
				_exitOpeningEyes = true;
				_playerCameraEffectController.OpenEyes(_exitOpenEyesLength);
				_fadeTimer = 0f;
			}
		}
		else if (_exitTransitioningOut)
		{
			_fadeTimer += Time.deltaTime;
			if (_fadeTimer >= _exitOpenEyesLength && _exitOpeningEyes)
			{
				_exitOpeningEyes = false;
			}
			if (_fadeTimer >= _exitTransitionOutLength + _exitOpenEyesLength)
			{
				_exitClosingEyes = false;
				_exitOpeningEyes = false;
				_exitTransitioningOut = false;
				_fadeTimer = 0f;
			}
		}
	}

	private void SetPeepholeCameraEyeOpenness(float t)
	{
		EyeMaskModel.Settings settings = _peepCameraProfile.eyeMask.settings;
		settings.openness = t;
		_peepCameraProfile.eyeMask.settings = settings;
	}

	private void Peep()
	{
		SwitchToPeepholeCamera();
		GlobalMessenger<Peephole>.FireEvent("StartPeeping", this);
		if (_hideWhileViewing != null)
		{
			_hideWhileViewing.SetActivation(active: false);
		}
		for (int i = 0; i < _factIDs.Length; i++)
		{
			Locator.GetShipLogManager().RevealFact(_factIDs[i]);
		}
	}

	private void Unpeep()
	{
		SwitchToPlayerCamera();
		GlobalMessenger<Peephole>.FireEvent("StopPeeping", this);
		if (_hideWhileViewing != null)
		{
			_hideWhileViewing.SetActivation(active: true);
		}
	}

	private void SwitchToPeepholeCamera()
	{
		OWInput.ChangeInputMode(InputMode.None);
		Locator.GetPlayerCamera().enabled = false;
		_peepholeCamera.enabled = true;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _peepholeCamera);
		Locator.GetPromptManager().AddScreenPrompt(_returnPrompt, PromptPosition.UpperRight, makeVisible: true);
		if (_viewingSector != null)
		{
			_viewingSector.AddOccupant(Locator.GetPlayerSectorDetector());
		}
		_interactReceiver.DisableInteraction();
	}

	private void SwitchToPlayerCamera()
	{
		OWInput.ChangeInputMode(InputMode.Character);
		_peepholeCamera.enabled = false;
		Locator.GetPlayerCamera().enabled = true;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", Locator.GetPlayerCamera());
		Locator.GetPromptManager().RemoveScreenPrompt(_returnPrompt);
		if (_viewingSector != null)
		{
			_viewingSector.RemoveOccupant(Locator.GetPlayerSectorDetector());
		}
		_interactReceiver.EnableInteraction();
		Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();
		Locator.GetPlayerCameraController().SnapToInitFieldOfView(_exitZoomOutLength, smoothStep: true);
	}
}
