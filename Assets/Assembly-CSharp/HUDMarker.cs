using UnityEngine;

public abstract class HUDMarker : MonoBehaviour
{
	protected CanvasMarker _canvasMarker;

	protected string _markerLabel = "";

	protected float _markerRadius;

	protected Transform _markerTarget;

	protected OWRigidbody _attachedBody;

	protected bool _inConversation;

	protected bool _isVisible = true;

	protected QuantumMoon _quantumMoon;

	protected bool _atFlightConsole;

	protected bool _isWearingHelmet;

	protected bool _translatorEquipped;

	protected bool _listeningForCloakFieldEvents;

	protected virtual void Awake()
	{
		_attachedBody = base.gameObject.GetAttachedOWRigidbody();
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		GlobalMessenger.AddListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
		GlobalMessenger.AddListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
		GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
		GlobalMessenger<EyeState>.AddListener("EyeStateChanged", OnEyeStateChanged);
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.AddListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.AddListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.AddListener("EquipTranslator", OnEquipTranslator);
		GlobalMessenger.AddListener("UnequipTranslator", OnUnequipTranslator);
	}

	protected virtual void OnLanguageChanged()
	{
	}

	protected virtual void Start()
	{
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterCloakField);
			Locator.GetCloakFieldController().OnPlayerExit += new OWEvent.OWCallback(OnPlayerExitCloakField);
		}
		_listeningForCloakFieldEvents = true;
		AstroObject astroObject = Locator.GetAstroObject(AstroObject.Name.QuantumMoon);
		if (astroObject != null)
		{
			_quantumMoon = astroObject.GetComponent<QuantumMoon>();
		}
		InitCanvasMarker();
	}

	protected virtual void InitCanvasMarker()
	{
		_canvasMarker = Locator.GetMarkerManager().InstantiateNewMarker();
		Locator.GetMarkerManager().RegisterMarker(_canvasMarker, _markerTarget, _markerLabel, _markerRadius);
		RefreshOwnVisibility();
	}

	protected virtual void OnDestroy()
	{
		GlobalMessenger.RemoveListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
		GlobalMessenger.RemoveListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
		GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
		GlobalMessenger<EyeState>.RemoveListener("EyeStateChanged", OnEyeStateChanged);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.RemoveListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.RemoveListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger.RemoveListener("EquipTranslator", OnEquipTranslator);
		GlobalMessenger.RemoveListener("UnequipTranslator", OnUnequipTranslator);
		if (_listeningForCloakFieldEvents && Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterCloakField);
			Locator.GetCloakFieldController().OnPlayerExit -= new OWEvent.OWCallback(OnPlayerExitCloakField);
		}
		TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
	}

	public abstract FogWarpVolume GetOuterFogWarpVolume();

	protected abstract void RefreshOwnVisibility();

	public virtual bool IsVisible()
	{
		return _isVisible;
	}

	protected virtual Vector3 GetMarkerPosition()
	{
		return _attachedBody.GetPosition();
	}

	protected virtual void OnEyeStateChanged(EyeState eyeState)
	{
		RefreshOwnVisibility();
	}

	protected virtual void OnPlayerEnterQuantumMoon()
	{
		RefreshOwnVisibility();
	}

	protected virtual void OnPlayerExitQuantumMoon()
	{
		RefreshOwnVisibility();
	}

	protected virtual void OnEnterConversation()
	{
		_inConversation = true;
		RefreshOwnVisibility();
	}

	protected virtual void OnExitConversation()
	{
		_inConversation = false;
		RefreshOwnVisibility();
	}

	private void OnPutOnHelmet()
	{
		_isWearingHelmet = true;
		RefreshOwnVisibility();
	}

	private void OnRemoveHelmet()
	{
		_isWearingHelmet = false;
		RefreshOwnVisibility();
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		_atFlightConsole = true;
		RefreshOwnVisibility();
	}

	private void OnExitFlightConsole()
	{
		_atFlightConsole = false;
		RefreshOwnVisibility();
	}

	private void OnEquipTranslator()
	{
		_translatorEquipped = true;
		RefreshOwnVisibility();
	}

	private void OnUnequipTranslator()
	{
		_translatorEquipped = false;
		RefreshOwnVisibility();
	}

	protected void OnPlayerEnterCloakField()
	{
		RefreshOwnVisibility();
	}

	protected void OnPlayerExitCloakField()
	{
		RefreshOwnVisibility();
	}
}
