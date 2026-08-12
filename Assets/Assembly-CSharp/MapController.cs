using System;
using UnityEngine;

public class MapController : MonoBehaviour
{
	[SerializeField]
	private bool _isTrailerMap;

	[Header("MarkerManager")]
	[SerializeField]
	private MapMarkerManager _mapMarkerManager;

	[Header("Pan")]
	[SerializeField]
	private float _panSpeed = 1f;

	[SerializeField]
	private float _maxPanDistance = 50000f;

	[Header("Yaw")]
	[SerializeField]
	private float _yawSpeed = 1f;

	[SerializeField]
	private float _defaultYawAngle;

	[Header("Pitch")]
	[SerializeField]
	private float _pitchSpeed = 1f;

	[SerializeField]
	private float _minPitchAngle;

	[SerializeField]
	private float _maxPitchAngle = 90f;

	[SerializeField]
	private float _defaultPitchAngle = 70f;

	[Header("Zoom")]
	[SerializeField]
	private float _zoomSpeed = 1f;

	[SerializeField]
	private float _minZoomDistance = 10000f;

	[SerializeField]
	private float _maxZoomDistance = 50000f;

	[SerializeField]
	private float _defaultZoomDist = 40000f;

	[Header("Reveal Movement")]
	[SerializeField]
	private AnimationCurve _revealCurve;

	[SerializeField]
	private float _initialPitchAngle = 90f;

	[SerializeField]
	private float _initialZoomDist = 10000f;

	[SerializeField]
	private float _observatoryRevealDist = 1000f;

	[SerializeField]
	private float _observatoryRevealTwist = 90f;

	[SerializeField]
	private float _defaultRevealLength = 2f;

	[SerializeField]
	private float _observatoryRevealLength = 10f;

	[SerializeField]
	private float _observatoryInteractDelay = 8f;

	[Header("Lock On")]
	[SerializeField]
	private float _lockOnMoveLength = 1f;

	[SerializeField]
	private float _playerFramingScale = 1.25f;

	[SerializeField]
	private float _verticalResetLength = 1f;

	[Header("Grid")]
	[SerializeField]
	private MeshRenderer _gridRenderer;

	[SerializeField]
	private Color _gridColor = Color.white;

	[SerializeField]
	private float _gridSize = 10f;

	[SerializeField]
	private float _gridLockOnLength = 1f;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _audioSource;

	private OWCamera _activeCam;

	private OWCamera _mapCamera;

	private Transform _playerTransform;

	private ReferenceFrame _currentRFrame;

	private Transform _targetTransform;

	private Vector3 _position;

	private float _yaw;

	private float _pitch;

	private float _zoom;

	private float _targetZoom;

	private bool _isMapMode;

	private bool _isObservatoryMap;

	private bool _isLockedOntoMapSatellite;

	private bool _lockedToTargetTransform;

	private bool _interpPosition;

	private bool _interpPitch;

	private bool _interpZoom;

	private bool _framingPlayer;

	private float _lockTimer;

	private float _verticalOffsetT;

	private bool _gridOverride;

	private float _gridOverrideSize;

	private float _gridTimer;

	private float _revealTimer;

	private float _revealLength;

	private const float c_audioFadeTime = 0.1f;

	private bool _mapSatelliteBroken;

	private bool _playerMapRestricted;

	private bool _screenPromptsVisible;

	private bool _isPaused;

	private ScreenPrompt _closePrompt;

	private ScreenPrompt _panPrompt;

	private ScreenPrompt _rotatePrompt;

	private ScreenPrompt _zoomPrompt;

	private void Awake()
	{
		base.gameObject.tag = "MapCamera";
		_mapCamera = this.GetRequiredComponent<OWCamera>();
		OWInput.SharedInputManager.OnUpdateInputDevice += OnSwitchInputDevice;
		GlobalMessenger.AddListener("TriggerObservatoryMap", OnTriggerObservatoryMap);
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger<OWRigidbody>.AddListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<ReferenceFrame>.AddListener("TargetReferenceFrame", OnTargetReferenceFrame);
		GlobalMessenger.AddListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
		GlobalMessenger.AddListener("PlayerEnterBrambleDimension", OnPlayerEnterMapRestriction);
		GlobalMessenger.AddListener("PlayerExitBrambleDimension", OnPlayerExitMapRestriction);
		GlobalMessenger.AddListener("BrokeMapSatellite", OnBrokeMapSatellite);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("GamePaused", OnGamePaused);
		GlobalMessenger.AddListener("GameUnpaused", OnGameUnpaused);
	}

	private void Start()
	{
		_playerTransform = Locator.GetPlayerTransform();
		_activeCam = Locator.GetPlayerCamera();
		BuildScreenPrompts();
		_mapSatelliteBroken = false;
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterMapRestriction);
			Locator.GetCloakFieldController().OnPlayerExit += new OWEvent.OWCallback(OnPlayerExitMapRestriction);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_isMapMode)
		{
			RumbleManager.StopMapMode();
		}
		OWInput.SharedInputManager.OnUpdateInputDevice -= OnSwitchInputDevice;
		if (Locator.GetCloakFieldController() != null)
		{
			Locator.GetCloakFieldController().OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterMapRestriction);
			Locator.GetCloakFieldController().OnPlayerExit -= new OWEvent.OWCallback(OnPlayerExitMapRestriction);
		}
		GlobalMessenger.RemoveListener("TriggerObservatoryMap", OnTriggerObservatoryMap);
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterFlightConsole", OnEnterFlightConsole);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<ReferenceFrame>.RemoveListener("TargetReferenceFrame", OnTargetReferenceFrame);
		GlobalMessenger.RemoveListener("UntargetReferenceFrame", OnUntargetReferenceFrame);
		GlobalMessenger.RemoveListener("PlayerEnterBrambleDimension", OnPlayerEnterMapRestriction);
		GlobalMessenger.RemoveListener("PlayerExitBrambleDimension", OnPlayerExitMapRestriction);
		GlobalMessenger.RemoveListener("BrokeMapSatellite", OnBrokeMapSatellite);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
		GlobalMessenger.RemoveListener("GameUnpaused", OnGameUnpaused);
	}

	private void BuildScreenPrompts()
	{
		if (_closePrompt == null)
		{
			_closePrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MapClosePrompt));
			_panPrompt = new ScreenPrompt(InputLibrary.moveXZ, UITextLibrary.GetString(UITextType.MapPanPrompt));
			_rotatePrompt = new ScreenPrompt(InputLibrary.look, UITextLibrary.GetString(UITextType.MapRotatePrompt));
			_zoomPrompt = new ScreenPrompt(InputLibrary.mapZoomIn, InputLibrary.mapZoomOut, UITextLibrary.GetString(UITextType.MapZoomPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
			Locator.GetPromptManager().AddScreenPrompt(_closePrompt, PromptPosition.UpperRight);
			Locator.GetPromptManager().AddScreenPrompt(_panPrompt, PromptPosition.UpperRight);
			Locator.GetPromptManager().AddScreenPrompt(_rotatePrompt, PromptPosition.UpperRight);
			Locator.GetPromptManager().AddScreenPrompt(_zoomPrompt, PromptPosition.UpperRight);
		}
	}

	private void OnTriggerObservatoryMap()
	{
		if (!_isMapMode)
		{
			if (_mapSatelliteBroken)
			{
				NotificationManager.SharedInstance.PostNotification(new NotificationData(UITextLibrary.GetString(UITextType.MapOfflineMessage)));
				return;
			}
			base.enabled = true;
			_isObservatoryMap = true;
			EnterMapView(null);
		}
	}

	private void EnterMapView(Transform targetTransform)
	{
		if (!_isMapMode)
		{
			_mapMarkerManager.SetVisible(value: true);
			GlobalMessenger.FireEvent("EnterMapView");
			GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _mapCamera);
			if (_audioSource.isPlaying)
			{
				_audioSource.Stop();
				_audioSource.SetLocalVolume(1f);
				_audioSource.Play();
			}
			else
			{
				_audioSource.SetLocalVolume(1f);
				_audioSource.Play();
			}
			Locator.GetAudioMixer().MixMap();
			_activeCam.enabled = false;
			_mapCamera.enabled = true;
			_gridRenderer.enabled = !_isTrailerMap && !_isObservatoryMap;
			_targetTransform = targetTransform;
			_lockedToTargetTransform = _targetTransform != null;
			_position = _playerTransform.position - Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
			_position.y = 0f;
			_yaw = _defaultYawAngle;
			_pitch = _initialPitchAngle;
			_zoom = _initialZoomDist;
			_targetZoom = _defaultZoomDist;
			if (_lockedToTargetTransform)
			{
				float value = Vector3.Distance(_playerTransform.position, _targetTransform.position) / Mathf.Tan((float)Math.PI / 180f * _mapCamera.fieldOfView * 0.5f) * _playerFramingScale;
				_targetZoom = Mathf.Clamp(value, _minZoomDistance, _maxZoomDistance);
			}
			if (_isObservatoryMap)
			{
				base.transform.rotation = (_isTrailerMap ? _activeCam.transform.rotation : Quaternion.LookRotation(-_playerTransform.up, _playerTransform.forward));
				base.transform.position = _activeCam.transform.position;
			}
			else
			{
				base.transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);
				base.transform.position = _position + -base.transform.forward * _zoom + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
			}
			_interpPosition = true;
			_interpPitch = true;
			_interpZoom = true;
			_framingPlayer = _lockedToTargetTransform;
			_lockTimer = _lockOnMoveLength;
			_verticalOffsetT = 0f;
			_gridOverrideSize = ((_currentRFrame != null) ? _currentRFrame.GetAutopilotArrivalDistance() : 0f);
			_gridOverride = _gridOverrideSize > 0f;
			_gridTimer = (_gridOverride ? _gridLockOnLength : 0f);
			_revealLength = (_isObservatoryMap ? _observatoryRevealLength : _defaultRevealLength);
			_revealTimer = 0f;
			if (!_isObservatoryMap)
			{
				_closePrompt.SetVisibility(isVisible: true);
				_panPrompt.SetVisibility(isVisible: true);
				_rotatePrompt.SetVisibility(isVisible: true);
				_zoomPrompt.SetVisibility(isVisible: true);
				_screenPromptsVisible = true;
			}
			RumbleManager.StartMapMode();
			_isMapMode = true;
		}
	}

	private void LateUpdate()
	{
		if (!_isMapMode)
		{
			if (!OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit) || !OWInput.IsNewlyPressed(InputLibrary.map))
			{
				return;
			}
			if (MapInoperable())
			{
				PostNotification();
				return;
			}
			if (_isTrailerMap)
			{
				_isObservatoryMap = true;
				_observatoryRevealTwist = 0f;
				_defaultPitchAngle = 30f;
				_initialPitchAngle = 0f;
				_defaultZoomDist = 35000f;
				_observatoryRevealLength = 20f;
				_mapCamera.fieldOfView = 70f;
			}
			EnterMapView((_currentRFrame != null && _currentRFrame.GetOWRigidBody() != null) ? _currentRFrame.GetOWRigidBody().transform : null);
			return;
		}
		_lockTimer = Mathf.Min(_lockTimer + Time.deltaTime, _lockOnMoveLength);
		float t = Mathf.Clamp01(_lockTimer / _lockOnMoveLength);
		if (!_interpPosition && !_lockedToTargetTransform)
		{
			_verticalOffsetT = Mathf.Min(_verticalOffsetT + Time.deltaTime, _verticalResetLength);
		}
		float t2 = Mathf.Clamp01(_verticalOffsetT / _verticalResetLength);
		_gridTimer = Mathf.Clamp(_gridOverride ? (_gridTimer + Time.deltaTime) : (_gridTimer - Time.deltaTime), 0f, _gridLockOnLength);
		float t3 = Mathf.Clamp01(_gridTimer / _gridLockOnLength);
		_revealTimer = Mathf.Min(_revealTimer + Time.deltaTime, _revealLength);
		float num = Mathf.Clamp01(_revealTimer / _revealLength);
		float t4 = Mathf.SmoothStep(0f, 1f, num);
		bool flag = !_isObservatoryMap || _revealTimer > _observatoryInteractDelay;
		if (_screenPromptsVisible && _isPaused)
		{
			_closePrompt.SetVisibility(isVisible: false);
			_panPrompt.SetVisibility(isVisible: false);
			_rotatePrompt.SetVisibility(isVisible: false);
			_zoomPrompt.SetVisibility(isVisible: false);
			_screenPromptsVisible = false;
		}
		else if (!_screenPromptsVisible && flag && !_isPaused)
		{
			_closePrompt.SetVisibility(isVisible: true);
			_panPrompt.SetVisibility(isVisible: true);
			_rotatePrompt.SetVisibility(isVisible: true);
			_zoomPrompt.SetVisibility(isVisible: true);
			_screenPromptsVisible = true;
			_gridRenderer.enabled = !_isTrailerMap;
		}
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		float num2 = 0f;
		if (flag)
		{
			vector = OWInput.GetAxisValue(InputLibrary.moveXZ);
			vector2 = InputLibrary.look.GetAxisValue(useSensitivity: false);
			num2 = OWInput.GetValue(InputLibrary.mapZoomIn) - OWInput.GetValue(InputLibrary.mapZoomOut);
			vector2.y *= -1f;
			num2 *= -1f;
		}
		_lockedToTargetTransform &= vector.sqrMagnitude < 0.01f;
		_interpPosition &= vector.sqrMagnitude < 0.01f;
		_interpPitch &= Mathf.Abs(vector2.y) < 0.1f;
		_interpZoom &= Mathf.Abs(num2) < 0.1f;
		_framingPlayer &= _lockedToTargetTransform && _interpZoom;
		_gridOverride &= _lockedToTargetTransform;
		if (_interpPosition)
		{
			Vector3 a = _activeCam.transform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
			Vector3 b = Vector3.zero;
			if (_lockedToTargetTransform && _targetTransform != null)
			{
				b = _targetTransform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
				if (!_isLockedOntoMapSatellite)
				{
					b.y = 0f;
				}
			}
			_position = Vector3.Lerp(a, b, t4);
		}
		else if (_lockedToTargetTransform && _targetTransform != null)
		{
			Vector3 position = _targetTransform.position;
			position -= Locator.GetCenterOfTheUniverse().GetOffsetPosition();
			if (!_isLockedOntoMapSatellite)
			{
				position.y = 0f;
			}
			_position = Vector3.Lerp(_position, position, t);
		}
		else
		{
			Vector3 normalized = Vector3.Scale(base.transform.forward + base.transform.up, new Vector3(1f, 0f, 1f)).normalized;
			Vector3 vector3 = base.transform.right * vector.x + normalized * vector.y;
			_position += vector3 * _panSpeed * _zoom * Time.deltaTime;
			_position.y = Mathf.Lerp(_position.y, 0f, t2);
			if (_position.sqrMagnitude > _maxPanDistance * _maxPanDistance)
			{
				_position = _position.normalized * _maxPanDistance;
			}
		}
		_yaw += vector2.x * _yawSpeed * Time.deltaTime;
		_yaw = OWMath.WrapAngle(_yaw);
		if (_interpPitch)
		{
			_pitch = Mathf.Lerp(_initialPitchAngle, _defaultPitchAngle, t4);
		}
		else
		{
			_pitch += vector2.y * _pitchSpeed * Time.deltaTime;
			_pitch = Mathf.Clamp(_pitch, _minPitchAngle, _maxPitchAngle);
		}
		if (_interpZoom)
		{
			if (_framingPlayer)
			{
				float value = Vector3.Distance(_playerTransform.position, _targetTransform.position) / Mathf.Tan((float)Math.PI / 180f * _mapCamera.fieldOfView * 0.5f) * 1.33f;
				_targetZoom = Mathf.Clamp(value, _minZoomDistance, _maxZoomDistance);
			}
			_zoom = Mathf.Lerp(_initialZoomDist, _targetZoom, t4);
		}
		else
		{
			_zoom += num2 * _zoomSpeed * Time.deltaTime;
			_zoom = Mathf.Clamp(_zoom, _minZoomDistance, _maxZoomDistance);
		}
		_mapCamera.nearClipPlane = Mathf.Lerp(0.1f, 1f, t4);
		Quaternion quaternion = Quaternion.Euler(_pitch, _yaw, 0f);
		Vector3 position2 = _position + quaternion * Vector3.back * _zoom + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
		if (_isObservatoryMap)
		{
			float num3 = (_isTrailerMap ? _revealCurve.Evaluate(num) : (num * (2f - num)));
			float num4 = (_isTrailerMap ? num3 : Mathf.SmoothStep(0f, 1f, num3));
			Quaternion a2 = (_isTrailerMap ? _activeCam.transform.rotation : Quaternion.LookRotation(-_playerTransform.up, Vector3.up));
			Vector3 position3 = _activeCam.transform.position;
			position3 += (_isTrailerMap ? (-_activeCam.transform.forward) : _playerTransform.up) * num4 * _observatoryRevealDist;
			base.transform.rotation = Quaternion.Lerp(a2, quaternion, num4);
			base.transform.rotation *= Quaternion.AngleAxis(Mathf.Lerp(_observatoryRevealTwist, 0f, num3), Vector3.forward);
			position2 = _position + -base.transform.forward * _zoom + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
			base.transform.position = Vector3.Lerp(position3, position2, num4);
		}
		else
		{
			base.transform.rotation = quaternion;
			base.transform.position = position2;
		}
		float num5 = Mathf.Lerp(_zoom * (_gridSize / 1000f), _gridOverrideSize, t3);
		Vector3 vector4 = new Vector3(_position.x, 0f, _position.z);
		_gridRenderer.transform.position = vector4 + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
		_gridRenderer.transform.rotation = ((vector4.sqrMagnitude < 0.001f) ? Quaternion.identity : Quaternion.LookRotation(vector4, Vector3.up));
		_gridRenderer.transform.localScale = Vector3.one * num5;
		_gridRenderer.material.color = _gridColor;
		_gridRenderer.material.SetMatrix("_GridCenterMatrix", Matrix4x4.TRS(Locator.GetCenterOfTheUniverse().GetOffsetPosition(), Quaternion.identity, Vector3.one).inverse);
		if (OWInput.IsInputMode(InputMode.Map) && (OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.map)))
		{
			ExitMapView();
		}
	}

	private bool MapInoperable()
	{
		if (!PlayerState.OnQuantumMoon() && !_playerMapRestricted)
		{
			return _mapSatelliteBroken;
		}
		return true;
	}

	private void PostNotification()
	{
		if (PlayerState.OnQuantumMoon() || _playerMapRestricted)
		{
			NotificationManager.SharedInstance.PostNotification(new NotificationData(UITextLibrary.GetString(UITextType.NotificationUnableToOpenMap)));
		}
		else if (_mapSatelliteBroken)
		{
			NotificationManager.SharedInstance.PostNotification(new NotificationData(UITextLibrary.GetString(UITextType.MapOfflineMessage)));
		}
	}

	private void ExitMapView()
	{
		if (_isMapMode)
		{
			GlobalMessenger.FireEvent("ExitMapView");
			GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _activeCam);
			Locator.GetAudioMixer().UnmixMap();
			if (_audioSource.isPlaying)
			{
				_audioSource.FadeOut(0.1f);
			}
			_activeCam.enabled = true;
			_mapCamera.enabled = false;
			_mapMarkerManager.SetVisible(value: false);
			_gridRenderer.enabled = false;
			base.enabled = !_isObservatoryMap || Locator.GetPlayerSuit().IsWearingSuit();
			_isObservatoryMap = false;
			_closePrompt.SetVisibility(isVisible: false);
			_panPrompt.SetVisibility(isVisible: false);
			_rotatePrompt.SetVisibility(isVisible: false);
			_zoomPrompt.SetVisibility(isVisible: false);
			_screenPromptsVisible = false;
			RumbleManager.StopMapMode();
			_isMapMode = false;
		}
	}

	private void OnSwitchInputDevice()
	{
		BuildScreenPrompts();
		_closePrompt.SetVisibility(_screenPromptsVisible);
		_panPrompt.SetVisibility(_screenPromptsVisible);
		_rotatePrompt.SetVisibility(_screenPromptsVisible);
		_zoomPrompt.SetVisibility(_screenPromptsVisible);
	}

	private void OnSwitchActiveCamera(OWCamera activeCam)
	{
		if (activeCam != _mapCamera)
		{
			_activeCam = activeCam;
		}
	}

	private void OnTargetReferenceFrame(ReferenceFrame referenceFrame)
	{
		_currentRFrame = referenceFrame;
		_targetTransform = ((_currentRFrame != null && _currentRFrame.GetOWRigidBody() != null) ? _currentRFrame.GetOWRigidBody().transform : null);
		AstroObject astroObject = ((_currentRFrame != null && _currentRFrame.GetOWRigidBody() != null) ? _currentRFrame.GetOWRigidBody().GetComponent<AstroObject>() : null);
		_isLockedOntoMapSatellite = astroObject != null && astroObject.GetAstroObjectName() == AstroObject.Name.MapSatellite;
		_lockedToTargetTransform = _targetTransform != null;
		_lockTimer = (_lockedToTargetTransform ? 0f : _lockOnMoveLength);
		_verticalOffsetT = 0f;
		_gridOverrideSize = ((_currentRFrame != null) ? _currentRFrame.GetAutopilotArrivalDistance() : 0f);
		_gridOverride = _gridOverrideSize > 0f;
	}

	private void OnUntargetReferenceFrame()
	{
		_currentRFrame = null;
		_isLockedOntoMapSatellite = false;
		_lockedToTargetTransform = false;
		_interpPosition = false;
		_framingPlayer = false;
		_gridOverride = false;
		_verticalOffsetT = 0f;
	}

	private void OnSuitUp()
	{
		base.enabled = !Locator.GetPlayerSuit().IsTrainingSuit();
	}

	private void OnRemoveSuit()
	{
		base.enabled = false;
	}

	private void OnEnterFlightConsole(OWRigidbody shipBody)
	{
		base.enabled = true;
	}

	private void OnExitFlightConsole()
	{
		base.enabled = Locator.GetPlayerSuit().IsWearingSuit();
	}

	private void OnBrokeMapSatellite()
	{
		_mapSatelliteBroken = true;
		ExitMapView();
		PostNotification();
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		ExitMapView();
		base.enabled = false;
	}

	private void OnGamePaused()
	{
		_isPaused = true;
	}

	private void OnGameUnpaused()
	{
		_isPaused = false;
	}

	private void OnPlayerEnterMapRestriction()
	{
		_playerMapRestricted = true;
		if (_isMapMode)
		{
			ExitMapView();
			PostNotification();
		}
	}

	private void OnPlayerExitMapRestriction()
	{
		_playerMapRestricted = false;
	}

	public MapMarkerManager GetMarkerManager()
	{
		return _mapMarkerManager;
	}

	public float GetCurrentZoom()
	{
		return _zoom;
	}

	public bool IsObservatoryMap()
	{
		return _isObservatoryMap;
	}
}
