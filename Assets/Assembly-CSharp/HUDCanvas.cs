using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HUDCanvas : MonoBehaviour
{
	[SerializeField]
	private ScreenPromptList _boostPromptList;

	[SerializeField]
	private Camera _hudCamera;

	[SerializeField]
	private Transform _gaugeGroupTransform;

	[SerializeField]
	private Color _normalLevelColor = Color.white;

	[SerializeField]
	private Color _warningLevelColor = Color.white;

	[SerializeField]
	private Color _dangerLevelColor = Color.white;

	[SerializeField]
	private Color _boostRechargingColor;

	private Color _origBoostColor;

	[SerializeField]
	private Image _boostSliderFillImage;

	[SerializeField]
	private Image _fuelGaugeImage;

	[SerializeField]
	private Image _oxyGaugeImage;

	[SerializeField]
	private Image _boostSliderBGImage;

	[SerializeField]
	private Image _healthDisplayImage;

	[SerializeField]
	private float _lowBoostPercent;

	[SerializeField]
	private float _lowerBoostPercent;

	private int _propID_Ramp;

	private Material _healthDisplayMaterialInstance;

	private Material _fuelTextMaterialInstance;

	private Material _oxyTextMaterialInstance;

	[SerializeField]
	private GameObject _boostArrowIndicator;

	[SerializeField]
	private GameObject _fuelArrowIndicator;

	[SerializeField]
	private GameObject _oxyArrowIndicator;

	private float _boostArrowOriginRotation;

	private float _fuelArrowOriginRotation;

	private float _oxyArrowOriginRotation;

	[SerializeField]
	private Transform _boostValueDisplayRoot;

	[SerializeField]
	private Text _boostValueDisplay;

	[SerializeField]
	private Text _fuelValueDisplay;

	[SerializeField]
	private Text _oxyValueDisplay;

	[SerializeField]
	private GameObject _gForceRoot;

	[SerializeField]
	private Text _gForceDisplay;

	private const float G_FORCE_REFRESH_RATE = 0.2f;

	private float _timeSinceGForceRefresh = 0.2f;

	private bool _healthWarningPlayed;

	private bool _healthDangerPlayed;

	private Transform _boostValueDisplayAnchor;

	private Transform _fuelValueDisplayAnchor;

	private Transform _oxyValueDisplayAnchor;

	private PlayerResources _playerResources;

	private PlayerCharacterController _playerController;

	private PlayerAudioController _playerAudioController;

	private NomaiTranslator _playerTranslator;

	[FormerlySerializedAs("_signalscopeUI")]
	[SerializeField]
	private SignalscopeUI _hudSignalscopeUI;

	[SerializeField]
	private SignalscopeUI _nonHudSignalscopeUI;

	[SerializeField]
	private Signalscope _signalscopeTool;

	private SignalscopeReticleController _reticuleController;

	private bool _inZoomMode;

	private bool _wearingHelmet;

	[Space(10f)]
	[SerializeField]
	private ThrustAndAttitudeIndicator _thrusterIndicator;

	[Space(10f)]
	[SerializeField]
	private ProbeLauncherUI _hudProbeLauncherUI;

	private NotificationData _lowFuelNotif;

	private NotificationData _critFuelNotif;

	private NotificationData _lowOxygenNotif;

	private NotificationData _critOxygenNotif;

	private NotificationData _lowHealthNotif;

	private NotificationData _critHealthNotif;

	private float _chargeFraction = -1f;

	private float _fuelFraction = -1f;

	private float _oxygenFraction = -1f;

	private void Awake()
	{
		_hudProbeLauncherUI.gameObject.SetActive(value: false);
		_boostPromptList.Init();
	}

	private void Start()
	{
		Transform playerTransform = Locator.GetPlayerTransform();
		_playerResources = playerTransform.GetComponent<PlayerResources>();
		_playerController = playerTransform.GetRequiredComponent<PlayerCharacterController>();
		_origBoostColor = _boostSliderFillImage.color;
		_playerTranslator = Locator.GetToolModeSwapper().GetTranslator();
		_playerAudioController = playerTransform.GetRequiredComponentInChildren<PlayerAudioController>();
		_propID_Ramp = Shader.PropertyToID("_Ramp");
		_healthDisplayMaterialInstance = Object.Instantiate(_healthDisplayImage.material);
		_healthDisplayImage.material = _healthDisplayMaterialInstance;
		InitArrowIndicator(_boostSliderFillImage, _boostArrowIndicator, ref _boostArrowOriginRotation);
		InitArrowIndicator(_fuelGaugeImage, _fuelArrowIndicator, ref _fuelArrowOriginRotation);
		InitArrowIndicator(_oxyGaugeImage, _oxyArrowIndicator, ref _oxyArrowOriginRotation);
		_boostValueDisplayAnchor = _boostArrowIndicator.transform.GetChild(0);
		_fuelValueDisplayAnchor = _fuelArrowIndicator.transform.GetChild(0);
		_oxyValueDisplayAnchor = _oxyArrowIndicator.transform.GetChild(0);
		_critFuelNotif = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationFuelCrit), 3f, playSound: false);
		_lowFuelNotif = new NotificationData(NotificationTarget.Player, _playerResources.GetLowFuel() + UITextLibrary.GetString(UITextType.NotificationFuelLow), 3f);
		_critOxygenNotif = new NotificationData(NotificationTarget.Player, Mathf.RoundToInt(_playerResources.GetCriticalOxygenInSeconds()) + UITextLibrary.GetString(UITextType.NotificationO2Sec), 3f);
		_lowOxygenNotif = new NotificationData(NotificationTarget.Player, Mathf.RoundToInt(_playerResources.GetLowOxygenInSeconds() / 60f) + UITextLibrary.GetString(UITextType.NotificationO2Min), 3f);
		_critHealthNotif = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationHpCrit), 3f);
		_lowHealthNotif = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationHpLow), 3f);
		GlobalMessenger.AddListener("SuitUp", OnPutOnSuit);
		GlobalMessenger.AddListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.AddListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", OnSignalscopeEquipped);
		GlobalMessenger.AddListener("UnequipSignalscope", OnSignalscopeUnequipped);
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
		GlobalMessenger.AddListener("EquipTranslator", OnEquipTranslator);
		GlobalMessenger.AddListener("UnequipTranslator", OnUnequipTranslator);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
	}

	private void OnDestroy()
	{
		if (_healthDisplayMaterialInstance != null)
		{
			Object.Destroy(_healthDisplayMaterialInstance);
		}
		_healthDisplayMaterialInstance = null;
		GlobalMessenger.RemoveListener("SuitUp", OnPutOnSuit);
		GlobalMessenger.RemoveListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.RemoveListener("RemoveHelmet", OnRemoveHelmet);
		GlobalMessenger<Signalscope>.RemoveListener("EquipSignalscope", OnSignalscopeEquipped);
		GlobalMessenger.RemoveListener("UnequipSignalscope", OnSignalscopeUnequipped);
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
		GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
		GlobalMessenger.RemoveListener("EquipTranslator", OnEquipTranslator);
		GlobalMessenger.RemoveListener("UnequipTranslator", OnUnequipTranslator);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
	}

	public void RegisterSignalscopeReticuleController(SignalscopeReticleController scopeReticuleCtrlr)
	{
		_reticuleController = scopeReticuleCtrlr;
	}

	public Vector3 GetBoostGaugeViewportPosition()
	{
		if (!(_hudCamera != null))
		{
			return Vector3.zero;
		}
		return _hudCamera.WorldToViewportPoint(_boostValueDisplay.transform.position);
	}

	public ScreenPromptList GetBoostPromptList()
	{
		return _boostPromptList;
	}

	private void InitArrowIndicator(Image radialFillImage, GameObject arrowIndicator, ref float degreesVar)
	{
		if (radialFillImage.fillMethod != Image.FillMethod.Radial360)
		{
			Debug.LogError("Arrow Indicators only work with Radial360 fills");
			return;
		}
		float num;
		switch ((Image.Origin360)radialFillImage.fillOrigin)
		{
		case Image.Origin360.Top:
			num = 0f;
			break;
		case Image.Origin360.Bottom:
			num = 180f;
			break;
		case Image.Origin360.Left:
			num = 90f;
			break;
		case Image.Origin360.Right:
			num = 270f;
			break;
		default:
			num = 0f;
			break;
		}
		degreesVar = num;
		Vector3 localEulerAngles = new Vector3(0f, 0f, num);
		arrowIndicator.transform.localEulerAngles = localEulerAngles;
	}

	private void Update()
	{
		UpdateBoost();
		UpdateFuel();
		UpdateOxygen();
		UpdateHealth();
		UpdateGForce();
		UpdateSignalscopeCanvas();
	}

	private void UpdateBoost()
	{
		bool flag = _playerResources.IsBoosterAllowed() || _playerResources.IsBoosterFiring();
		_boostSliderBGImage.enabled = flag;
		_boostSliderFillImage.enabled = flag;
		_boostValueDisplay.enabled = flag;
		_boostArrowIndicator.SetActive(flag);
		if (!flag)
		{
			return;
		}
		float boostChargeFraction = _playerResources.GetBoostChargeFraction();
		if (OWMath.ApproxEquals(_chargeFraction, boostChargeFraction))
		{
			return;
		}
		_chargeFraction = boostChargeFraction;
		_boostSliderFillImage.fillAmount = 0.75f + _chargeFraction * 0.25f;
		_boostSliderFillImage.color = (_playerResources.IsBoosterAllowed() ? _origBoostColor : _boostRechargingColor);
		if (_chargeFraction * 100f > _lowBoostPercent)
		{
			if (_boostValueDisplay.color != _normalLevelColor)
			{
				_boostValueDisplay.color = _normalLevelColor;
			}
		}
		else if (_chargeFraction * 100f <= _lowerBoostPercent)
		{
			if (_boostValueDisplay.color != _dangerLevelColor)
			{
				_boostValueDisplay.color = _dangerLevelColor;
			}
		}
		else if (_chargeFraction * 100f <= _lowBoostPercent && _boostValueDisplay.color != _warningLevelColor)
		{
			_boostValueDisplay.color = _warningLevelColor;
		}
		if (_playerResources.GetFuelFraction() <= 0f)
		{
			_boostSliderFillImage.fillAmount = 0f;
			_chargeFraction = 0f;
			_boostValueDisplay.color = _dangerLevelColor;
		}
		float num = (1f - _chargeFraction) * 90f;
		if (!_boostSliderFillImage.fillClockwise)
		{
			num *= -1f;
		}
		Vector3 localEulerAngles = new Vector3(0f, 0f, _boostArrowOriginRotation + num);
		_boostArrowIndicator.transform.localEulerAngles = localEulerAngles;
		_boostValueDisplayRoot.position = _boostValueDisplayAnchor.position;
		_boostValueDisplay.text = _chargeFraction.ToString("P1");
	}

	private void UpdateFuel()
	{
		float fuelFraction = _playerResources.GetFuelFraction();
		if (OWMath.ApproxEquals(_fuelFraction, fuelFraction, 0.01f))
		{
			return;
		}
		_fuelFraction = fuelFraction;
		float fuel = _playerResources.GetFuel();
		_fuelGaugeImage.fillAmount = 0.75f + _fuelFraction * 0.25f;
		if (fuel > _playerResources.GetLowFuel())
		{
			if (_fuelValueDisplay.color != _normalLevelColor)
			{
				_fuelValueDisplay.color = _normalLevelColor;
			}
		}
		else if (fuel <= _playerResources.GetCriticalFuel())
		{
			if (_fuelValueDisplay.color != _dangerLevelColor)
			{
				_fuelValueDisplay.color = _dangerLevelColor;
				if (!_playerResources.IsRefueling())
				{
					_playerAudioController.PlaySuitCriticalWarning();
					NotificationManager.SharedInstance.PostNotification(_critFuelNotif);
				}
			}
		}
		else if (fuel <= _playerResources.GetLowFuel() && _fuelValueDisplay.color != _warningLevelColor)
		{
			_fuelValueDisplay.color = _warningLevelColor;
			if (!_playerResources.IsRefueling())
			{
				NotificationManager.SharedInstance.PostNotification(_lowFuelNotif);
			}
		}
		float num = (1f - _fuelFraction) * 90f;
		if (!_fuelGaugeImage.fillClockwise)
		{
			num *= -1f;
		}
		Vector3 localEulerAngles = new Vector3(0f, 0f, _fuelArrowOriginRotation + num);
		_fuelArrowIndicator.transform.localEulerAngles = localEulerAngles;
		_fuelValueDisplay.transform.position = _fuelValueDisplayAnchor.position;
		_fuelValueDisplay.text = (_fuelFraction * 5f).ToString("F1");
	}

	private void UpdateOxygen()
	{
		float oxygenFraction = _playerResources.GetOxygenFraction();
		if (OWMath.ApproxEquals(_oxygenFraction, oxygenFraction, 0.01f))
		{
			return;
		}
		_oxygenFraction = oxygenFraction;
		_oxyGaugeImage.fillAmount = 0.75f + _oxygenFraction * 0.25f;
		if (_playerResources.GetOxygenInSeconds() <= _playerResources.GetCriticalOxygenInSeconds() && !_playerResources.IsRefillingOxygen())
		{
			if (_oxyValueDisplay.color != _dangerLevelColor)
			{
				_playerAudioController.PlaySuitCriticalWarning();
				_oxyValueDisplay.color = _dangerLevelColor;
				NotificationManager.SharedInstance.PostNotification(_critOxygenNotif);
			}
		}
		else if (_playerResources.GetOxygenInSeconds() <= _playerResources.GetLowOxygenInSeconds() && !_playerResources.IsRefillingOxygen())
		{
			if (_oxyValueDisplay.color != _warningLevelColor)
			{
				_oxyValueDisplay.color = _warningLevelColor;
				NotificationManager.SharedInstance.PostNotification(_lowOxygenNotif);
			}
		}
		else if (_playerResources.GetOxygenInSeconds() > _playerResources.GetLowOxygenInSeconds() && _oxyValueDisplay.color != _normalLevelColor)
		{
			_oxyValueDisplay.color = _normalLevelColor;
		}
		float num = (1f - _oxygenFraction) * 90f;
		if (!_oxyGaugeImage.fillClockwise)
		{
			num *= -1f;
		}
		Vector3 localEulerAngles = new Vector3(0f, 0f, _oxyArrowOriginRotation + num);
		_oxyArrowIndicator.transform.localEulerAngles = localEulerAngles;
		_oxyValueDisplay.transform.position = _oxyValueDisplayAnchor.position;
		_oxyValueDisplay.text = (_oxygenFraction * 50f).ToString("F1");
	}

	private void UpdateHealth()
	{
		float healthFraction = _playerResources.GetHealthFraction();
		float health = _playerResources.GetHealth();
		if (health < _playerResources.GetCriticalHealth())
		{
			if (!_healthDangerPlayed && !_playerResources.IsHealing())
			{
				_playerAudioController.PlaySuitCriticalWarning();
				NotificationManager.SharedInstance.PostNotification(_critHealthNotif);
				_healthWarningPlayed = true;
				_healthDangerPlayed = true;
			}
		}
		else if (health < _playerResources.GetLowHealth())
		{
			if (!_healthWarningPlayed && !_playerResources.IsHealing())
			{
				NotificationManager.SharedInstance.PostNotification(_lowHealthNotif);
				_healthWarningPlayed = true;
				_healthDangerPlayed = false;
			}
		}
		else
		{
			_healthWarningPlayed = false;
			_healthDangerPlayed = false;
		}
		_healthDisplayMaterialInstance.SetFloat(_propID_Ramp, 1f - healthFraction);
	}

	private void UpdateGForce()
	{
		if (_timeSinceGForceRefresh < 0.2f)
		{
			_timeSinceGForceRefresh += Time.deltaTime;
			return;
		}
		string text = (_playerController.GetNormalAccelerationScalar() / 12f).ToString("F1") + "x";
		_gForceDisplay.text = text;
		_timeSinceGForceRefresh = 0f;
	}

	private void UpdateSignalscopeCanvas()
	{
		if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.SignalScope || Locator.GetToolModeSwapper().GetToolGroup() == ToolGroup.Ship)
		{
			if (_hudSignalscopeUI.IsActivated())
			{
				_hudSignalscopeUI.Deactivate();
			}
			if (_nonHudSignalscopeUI.IsActivated())
			{
				_nonHudSignalscopeUI.Deactivate();
			}
			return;
		}
		bool flag = _wearingHelmet && !_inZoomMode;
		bool flag2 = _inZoomMode || !_wearingHelmet;
		if (_hudSignalscopeUI.IsActivated() && !flag)
		{
			_hudSignalscopeUI.Deactivate();
		}
		if (_nonHudSignalscopeUI.IsActivated() && !flag2)
		{
			_nonHudSignalscopeUI.Deactivate();
		}
		if (!_hudSignalscopeUI.IsActivated() && flag)
		{
			if (_reticuleController == null)
			{
				Debug.LogError("ReticuleController cannot be null!");
			}
			_hudSignalscopeUI.Activate(_signalscopeTool, _reticuleController);
		}
		if (!_nonHudSignalscopeUI.IsActivated() && flag2)
		{
			if (_reticuleController == null)
			{
				Debug.LogError("ReticuleController cannot be null!");
			}
			_nonHudSignalscopeUI.Activate(_signalscopeTool, _reticuleController);
		}
	}

	private void OnPutOnSuit()
	{
		bool flag = Locator.GetPlayerSuit().IsTrainingSuit();
		_boostSliderFillImage.enabled = !flag;
		_boostArrowIndicator.SetActive(!flag);
		_boostValueDisplay.enabled = !flag;
	}

	private void OnPutOnHelmet()
	{
		_wearingHelmet = true;
		_hudProbeLauncherUI.gameObject.SetActive(value: true);
	}

	private void OnRemoveHelmet()
	{
		_wearingHelmet = false;
		_hudProbeLauncherUI.gameObject.SetActive(value: false);
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		_gForceDisplay.enabled = false;
	}

	private void OnExitFlightConsole()
	{
		_gForceDisplay.enabled = true;
	}

	private void OnPlayerDeath(DeathType type)
	{
		base.enabled = false;
	}

	private void OnEnterSignalscopeZoom(Signalscope scope)
	{
		_inZoomMode = true;
	}

	private void OnExitSignalscopeZoom()
	{
		_inZoomMode = false;
	}

	private void OnEnterConversation()
	{
		_gForceRoot.SetActive(value: false);
	}

	private void OnExitConversation()
	{
		_gForceRoot.SetActive(value: true);
	}

	private void OnEquipTranslator()
	{
		_thrusterIndicator.gameObject.SetActive(value: false);
		_gForceRoot.SetActive(value: false);
	}

	private void OnUnequipTranslator()
	{
		_thrusterIndicator.gameObject.SetActive(value: true);
		_gForceRoot.SetActive(value: true);
	}

	private void OnSignalscopeEquipped(Signalscope s)
	{
		_thrusterIndicator.gameObject.SetActive(value: false);
		_gForceRoot.SetActive(value: false);
	}

	private void OnSignalscopeUnequipped()
	{
		_thrusterIndicator.gameObject.SetActive(value: true);
		_gForceRoot.SetActive(value: true);
	}
}
