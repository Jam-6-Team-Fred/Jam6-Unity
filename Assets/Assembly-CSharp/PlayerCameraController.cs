using UnityEngine;

[RequireComponent(typeof(OWCamera))]
public class PlayerCameraController : MonoBehaviour
{
	public static float LOOK_RATE = 160f;

	public static float GAMEPAD_LOOK_RATE_Y = 120f;

	public static float ZOOM_SCALAR = 0.5f;

	private const float _minDegreesYNormal = -80f;

	private const float _maxDegreesYNormal = 80f;

	private const float _sensitivityXFree = 180f;

	private const float _sensitivityYFree = 180f;

	private const float _minDegreesXFree = -60f;

	private const float _maxDegreesXFree = 60f;

	private const float _minDegreesYFree = -35f;

	private const float _maxDegreesYFree = 80f;

	protected float _degreesX;

	protected float _degreesY;

	private bool _zoomed;

	private float _initFOV;

	private float _targetFOV;

	private float _initZoomTime;

	private float _zoomDuration;

	private float _zoomRate;

	private float _initSnapFOV;

	private bool _isSnapZooming;

	private bool _smoothStepZoom;

	private bool _useZoomEasing;

	private Vector3 _origLocalPosition;

	private Vector3 _targetLocalPosition;

	private Quaternion _rotationX;

	private Quaternion _rotationY;

	private bool _isLockedOn;

	private Transform _targetTransform;

	private Vector3 _localOffset;

	private float _followRate;

	private bool _isSnapping;

	private bool _smoothStepToDegrees;

	private float _initSnapTime;

	private float _snapDuration;

	private float _snapTargetX;

	private float _snapTargetY;

	private float _initSnapDegreesX;

	private float _initSnapDegreesY;

	private float _origFarClipPlane;

	private int _origCullingMask;

	private PlayerCharacterController _characterController;

	private ShipCockpitController _shipController;

	private OWCamera _playerCamera;

	private AudioListener _audioListener;

	private void Awake()
	{
		base.gameObject.tag = "MainCamera";
		_origLocalPosition = (_targetLocalPosition = base.transform.localPosition);
		_playerCamera = GetComponent<OWCamera>();
		_audioListener = GetComponent<AudioListener>();
		_origFarClipPlane = _playerCamera.farClipPlane;
		_origCullingMask = _playerCamera.cullingMask;
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.AddListener("EnterLandingView", OnEnterLandingView);
		GlobalMessenger.AddListener("ExitLandingView", OnExitLandingView);
		GlobalMessenger.AddListener("EnterShipComputer", OnEnterShipComputer);
		GlobalMessenger.AddListener("ExitShipComputer", OnExitShipComputer);
		GlobalMessenger.AddListener("EnterNomaiRemoteCamera", OnEnterNomaiRemoteCamera);
		GlobalMessenger.AddListener("ExitNomaiRemoteCamera", OnExitNomaiRemoteCamera);
	}

	private void Start()
	{
		_characterController = this.GetAttachedOWRigidbody().GetRequiredComponent<PlayerCharacterController>();
		_shipController = null;
		if (Locator.GetShipTransform() != null)
		{
			_shipController = Locator.GetShipTransform().GetComponentInChildren<ShipCockpitController>();
		}
		float targetFOV = (_playerCamera.fieldOfView = PlayerData.GetGraphicSettings().fieldOfView);
		_initFOV = (_targetFOV = targetFOV);
		_playerCamera.postProcessingSettings.screenSpaceReflectionAvailable = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.RemoveListener("EnterLandingView", OnEnterLandingView);
		GlobalMessenger.RemoveListener("ExitLandingView", OnExitLandingView);
		GlobalMessenger.RemoveListener("EnterShipComputer", OnEnterShipComputer);
		GlobalMessenger.RemoveListener("ExitShipComputer", OnExitShipComputer);
		GlobalMessenger.RemoveListener("EnterNomaiRemoteCamera", OnEnterNomaiRemoteCamera);
		GlobalMessenger.RemoveListener("ExitNomaiRemoteCamera", OnExitNomaiRemoteCamera);
	}

	public float GetZoomFOVRatio()
	{
		return _playerCamera.fieldOfView / _initFOV;
	}

	public float GetOrigFieldOfView()
	{
		return _initFOV;
	}

	public OWCamera GetCamera()
	{
		return _playerCamera;
	}

	public bool IsUpdatingFieldOfView()
	{
		return !OWMath.ApproxEquals(_playerCamera.fieldOfView, _targetFOV);
	}

	public void AddDegreesY(float deltaDegreesY)
	{
		_degreesY += deltaDegreesY;
	}

	public void SetDegreesY(float degreesY)
	{
		_degreesY = degreesY;
		UpdateRotation();
	}

	public float GetDegreesY()
	{
		return _degreesY;
	}

	public float GetMinDegreesY()
	{
		return -80f;
	}

	public void LockOn(Transform targetTransform, float followEasing = 5f)
	{
		LockOn(targetTransform, Vector3.zero, followEasing);
	}

	public void LockOn(Transform targetTransform, Vector3 localOffset, float followEasing = 5f)
	{
		_isSnapping = false;
		_targetTransform = targetTransform;
		_localOffset = localOffset;
		_isLockedOn = true;
		_followRate = followEasing;
	}

	public void BreakLock()
	{
		_isLockedOn = false;
	}

	public void SnapToFieldOfView(float targetFOV, float zoomDuration = 2f, bool smoothStep = false)
	{
		_zoomDuration = zoomDuration;
		_initZoomTime = Time.unscaledTime;
		_targetFOV = targetFOV;
		_initSnapFOV = _playerCamera.fieldOfView;
		_isSnapZooming = true;
		_smoothStepZoom = smoothStep;
	}

	public void SnapToInitFieldOfView(float zoomDuration = 2f, bool smoothStep = false)
	{
		_useZoomEasing = false;
		SnapToFieldOfView(_initFOV, zoomDuration, smoothStep);
	}

	public void SetTargetFieldOfView(float targetFOV, float zoomRate, bool overrideSnapZoom = false)
	{
		_targetFOV = targetFOV;
		_zoomRate = zoomRate;
		_useZoomEasing = true;
		if (overrideSnapZoom)
		{
			_isSnapZooming = false;
		}
	}

	public void SetTargetFieldOfView(float targetFOV)
	{
		_targetFOV = targetFOV;
		_useZoomEasing = false;
	}

	public void ResetTargetFieldOfView(float zoomRate = 1f)
	{
		SetTargetFieldOfView(_initFOV, zoomRate);
	}

	public void SnapToDegrees(float targetX, float targetY, float degreesPerSecond = 100f, bool smoothStep = false)
	{
		SnapToDegreesOverSeconds(targetX, targetY, Mathf.Sqrt(Mathf.Pow(_degreesX - targetX, 2f) + Mathf.Pow(_degreesY - targetY, 2f)) / degreesPerSecond, smoothStep);
	}

	public void SnapToDegreesOverSeconds(float targetX, float targetY, float duration, bool smoothStep = false)
	{
		if (duration < Time.deltaTime)
		{
			_degreesX = targetX;
			_degreesY = targetY;
			return;
		}
		_initSnapTime = Time.unscaledTime;
		_snapDuration = duration;
		_isSnapping = true;
		_smoothStepToDegrees = smoothStep;
		_snapTargetX = targetX;
		_snapTargetY = targetY;
		_initSnapDegreesX = _degreesX;
		_initSnapDegreesY = _degreesY;
	}

	public void CenterCamera(float degreesPerSecond = 50f, bool smoothStep = false)
	{
		SnapToDegrees(0f, 0f, degreesPerSecond, smoothStep);
	}

	public void CenterCameraOverSeconds(float seconds, bool smoothStep = false)
	{
		SnapToDegreesOverSeconds(0f, 0f, seconds, smoothStep);
	}

	private void StopSnapping()
	{
		_isSnapping = false;
	}

	private void Update()
	{
		if (_shipController != null && _shipController.AllowFreeLook() && OWInput.IsNewlyReleased(InputLibrary.freeLook))
		{
			CenterCameraOverSeconds(0.33f, smoothStep: true);
		}
		if (OWTime.IsPaused(OWTime.PauseType.Reading))
		{
			UpdateCamera(Time.unscaledDeltaTime);
		}
	}

	private void FixedUpdate()
	{
		if (!OWTime.IsPaused())
		{
			UpdateCamera(Time.fixedDeltaTime);
		}
	}

	private void UpdateCamera(float deltaTime)
	{
		UpdateInput(deltaTime);
		if (_isLockedOn)
		{
			UpdateLockOnTargeting(deltaTime);
		}
		if (_isSnapping && !_isLockedOn)
		{
			float num = Mathf.InverseLerp(_initSnapTime, _initSnapTime + _snapDuration, Time.unscaledTime);
			if (_smoothStepToDegrees)
			{
				num = Mathf.SmoothStep(0f, 1f, num);
			}
			if (num >= 1f)
			{
				_isSnapping = false;
			}
			_degreesX = Mathf.Lerp(_initSnapDegreesX, _snapTargetX, num);
			_degreesY = Mathf.Lerp(_initSnapDegreesY, _snapTargetY, num);
		}
		float t = ((_characterController.GetJumpCrouchFraction() > 0f) ? 1f : 0.1f);
		_targetLocalPosition = _origLocalPosition - Vector3.up * 0.3f * _characterController.GetJumpCrouchFraction();
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, _targetLocalPosition, t);
		UpdateFieldOfView(deltaTime);
		UpdateRotation();
	}

	private void UpdateFieldOfView(float deltaTime)
	{
		if (_isSnapZooming)
		{
			float num = (Time.unscaledTime - _initZoomTime) / _zoomDuration;
			if (_smoothStepZoom)
			{
				num = Mathf.SmoothStep(0f, 1f, num);
			}
			if (num >= 1f)
			{
				num = 1f;
				_isSnapZooming = false;
			}
			_playerCamera.fieldOfView = Mathf.Lerp(_initSnapFOV, _targetFOV, num);
		}
		else
		{
			float t = (_useZoomEasing ? (_zoomRate * deltaTime) : 1f);
			_playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, _targetFOV, t);
		}
	}

	private void UpdateRotation()
	{
		_degreesX %= 360f;
		_degreesY %= 360f;
		if (!_isSnapping)
		{
			if (_shipController != null && _shipController.AllowFreeLook() && OWInput.IsPressed(InputLibrary.freeLook))
			{
				_degreesX = Mathf.Clamp(_degreesX, -60f, 60f);
				_degreesY = Mathf.Clamp(_degreesY, -35f, 80f);
			}
			else
			{
				_degreesX = 0f;
				_degreesY = Mathf.Clamp(_degreesY, -80f, 80f);
			}
		}
		_rotationX = Quaternion.AngleAxis(_degreesX, Vector3.up);
		_rotationY = Quaternion.AngleAxis(_degreesY, -Vector3.right);
		Quaternion localRotation = _rotationX * _rotationY * Quaternion.identity;
		_playerCamera.transform.localRotation = localRotation;
	}

	private void UpdateInput(float deltaTime)
	{
		bool flag = _shipController != null && _shipController.AllowFreeLook() && OWInput.IsPressed(InputLibrary.freeLook);
		bool flag2 = OWInput.IsInputMode(InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam | InputMode.PatchingSuit);
		if (!_isSnapping && !_isLockedOn && (!PlayerState.InZeroG() || !PlayerState.IsWearingSuit()) && (flag2 || flag))
		{
			bool flag3 = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
			Vector2 one = Vector2.one;
			one *= ((_zoomed || flag3) ? ZOOM_SCALAR : 1f);
			one *= _playerCamera.fieldOfView / _initFOV;
			if (Time.timeScale > 1f)
			{
				one /= Time.timeScale;
			}
			if (flag)
			{
				Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.look);
				_degreesX += axisValue.x * 180f * one.x * deltaTime;
				_degreesY += axisValue.y * 180f * one.y * deltaTime;
			}
			else
			{
				float num = (OWInput.UsingGamepad() ? GAMEPAD_LOOK_RATE_Y : LOOK_RATE);
				_degreesY += OWInput.GetAxisValue(InputLibrary.look).y * num * one.y * deltaTime;
			}
		}
	}

	private void UpdateLockOnTargeting(float deltaTime)
	{
		Vector3 vector = _targetTransform.TransformPoint(_localOffset) - base.transform.position;
		Vector3 vector2 = vector - Vector3.Project(vector, base.transform.parent.up);
		vector = Quaternion.AngleAxis(0f - Vector3.Angle(vector2, base.transform.parent.forward) * Mathf.Sign(Vector3.Dot(vector2, base.transform.parent.right)), base.transform.parent.up) * vector;
		float num = Vector3.Angle(base.transform.forward, vector) * Mathf.Sign(Vector3.Dot(vector, base.transform.up));
		_degreesY += num * _followRate * deltaTime;
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicsSettings)
	{
		if (!OWMath.ApproxEquals(graphicsSettings.fieldOfView, _initFOV))
		{
			if (OWMath.ApproxEquals(_targetFOV, _initFOV))
			{
				_targetFOV = graphicsSettings.fieldOfView;
			}
			if (OWMath.ApproxEquals(_playerCamera.fieldOfView, _initFOV))
			{
				_playerCamera.fieldOfView = graphicsSettings.fieldOfView;
			}
			_initFOV = graphicsSettings.fieldOfView;
		}
	}

	private void OnEnterSignalscopeZoom(Signalscope telescope)
	{
		_zoomed = true;
	}

	private void OnExitSignalscopeZoom()
	{
		_zoomed = false;
	}

	private void OnEnterLandingView()
	{
		_playerCamera.cullingMask = 1 << LayerMask.NameToLayer("ShipInterior");
		_playerCamera.farClipPlane = 3f;
		_playerCamera.postProcessingSettings.ambientOcclusionAvailable = false;
		ProxyShadowLight.SetCameraIgnored(_playerCamera.mainCamera, ignored: true);
	}

	private void OnExitLandingView()
	{
		_playerCamera.cullingMask = _origCullingMask;
		_playerCamera.farClipPlane = _origFarClipPlane;
		_playerCamera.postProcessingSettings.ambientOcclusionAvailable = true;
		ProxyShadowLight.SetCameraIgnored(_playerCamera.mainCamera, ignored: false);
	}

	private void OnEnterShipComputer()
	{
		_playerCamera.cullingMask = (1 << LayerMask.NameToLayer("ShipInterior")) | (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("VisibleToPlayer"));
		_playerCamera.farClipPlane = 3f;
		_playerCamera.postProcessingSettings.ambientOcclusionAvailable = false;
	}

	private void OnExitShipComputer()
	{
		_playerCamera.cullingMask = _origCullingMask;
		_playerCamera.farClipPlane = _origFarClipPlane;
		_playerCamera.postProcessingSettings.ambientOcclusionAvailable = true;
	}

	private void OnEnterNomaiRemoteCamera()
	{
		_playerCamera.enabled = false;
		_audioListener.enabled = false;
	}

	private void OnExitNomaiRemoteCamera()
	{
		_playerCamera.enabled = true;
		_audioListener.enabled = true;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _playerCamera);
	}
}
