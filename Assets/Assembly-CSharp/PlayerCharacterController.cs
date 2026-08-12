using System;
using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class PlayerCharacterController : MonoBehaviour
{
	public delegate void JumpEvent();

	public delegate void GroundedEvent();

	public delegate void UngroundedEvent();

	[SerializeField]
	private Collider _bodyCollider;

	[SerializeField]
	private Collider _antiSinkingCollider;

	[SerializeField]
	private float _runSpeed = 6f;

	[SerializeField]
	private float _walkSpeed = 3f;

	[SerializeField]
	private float _strafeSpeed = 4f;

	[SerializeField]
	private float _acceleration = 0.5f;

	[SerializeField]
	private float _airSpeed = 3f;

	[SerializeField]
	private float _airAcceleration = 0.2f;

	[SerializeField]
	private float _minJumpSpeed = 3f;

	[SerializeField]
	private float _maxJumpSpeed = 6f;

	[Space]
	[SerializeField]
	private bool _useChargeJump = true;

	[SerializeField]
	private bool _useChargeCurve;

	[SerializeField]
	private AnimationCurve _jumpChargeCurve;

	[Space]
	[SerializeField]
	private float _minStaggerDamage = 5f;

	[SerializeField]
	private float _minStaggerLength = 1f;

	[SerializeField]
	private float _maxStaggerLength = 3f;

	[Space]
	[SerializeField]
	private float _tumbleThreshold = float.PositiveInfinity;

	[SerializeField]
	private float _tumbleDuration = 1.5f;

	[Space]
	[SerializeField]
	private int _maxAngleToBeGrounded = 45;

	[SerializeField]
	private int _maxAngleBetweenSlopes = 115;

	[Space]
	[SerializeField]
	private bool _groundSnappingEnabled = true;

	[SerializeField]
	private float _jumpDurationAfterUngrounded = 0.25f;

	private bool _isAlignedToForce;

	private bool _isWearingSuit;

	private bool _isMovementLocked;

	private bool _isTurningLocked;

	private bool _isZeroGMovementEnabled;

	private bool _isGrounded;

	private bool _wasGrounded;

	private bool _jumpNextFixedUpdate;

	private bool _isStaggered;

	private bool _isTumbling;

	private bool _isPushable;

	private bool _pushNextFixedUpdate;

	private bool _inWarpField;

	private bool _jumpPressedInOtherMode;

	private bool _signalscopeZoom;

	private float _lastGroundedTime;

	private float _jumpChargeTime;

	private float _lastJumpTime;

	private float _lastPushTime;

	private float _initStaggerTime;

	private float _staggerLength;

	private float _initTumbleTime;

	private float _baseAngularVelocity;

	private float _lastTurnInput;

	private Transform _transform;

	private OWRigidbody _owRigidbody;

	private OWRigidbody _groundBody;

	private OWRigidbody _lastGroundBody;

	private Collider _groundCollider;

	private OWRigidbody _pushableBody;

	private MovingPlatform _movingPlatform;

	private QuantumObject _collidingQuantumObject;

	private bool _groundedOnRisingSand;

	private ForceDetector _forceDetector;

	private FluidDetector _fluidDetector;

	private Vector3 _groundContactPt = Vector3.zero;

	private Vector3 _groundNormal = Vector3.zero;

	private Vector3 _normalAcceleration = Vector3.zero;

	private Vector3 _lastLocalGroundFramePosition = Vector3.zero;

	private SurfaceType _groundSurface;

	private Vector3 _pushContactPt = Vector3.zero;

	private PhysicMaterial _runningPhysicMaterial;

	private PhysicMaterial _standingPhysicMaterial;

	private PhysicMaterial _airbornePhysicMaterial;

	private ThrusterModel _jetpackModel;

	private OWCamera _playerCam;

	private float _initFOV;

	private PlayerResources _playerResources;

	private HazardDetector _hazardDetector;

	private DreamLanternItem _heldLanternItem;

	private ScreenPrompt _pushPrompt;

	private const int k_maxRaycastHits = 32;

	private RaycastHit[] _raycastHits;

	private Vector3[] _raycastHitNormals;

	public event JumpEvent OnJump;

	public event GroundedEvent OnBecomeGrounded;

	public event UngroundedEvent OnBecomeUngrounded;

	private void Awake()
	{
		_transform = this.GetRequiredComponent<Transform>();
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
		_runningPhysicMaterial = new PhysicMaterial("PlayerPhysicMaterial_Running");
		_runningPhysicMaterial.staticFriction = 0f;
		_runningPhysicMaterial.dynamicFriction = 0f;
		_runningPhysicMaterial.bounciness = 0f;
		_runningPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
		_standingPhysicMaterial = new PhysicMaterial("PlayerPhysicMaterial_Standing");
		_standingPhysicMaterial.staticFriction = 1f;
		_standingPhysicMaterial.dynamicFriction = 1f;
		_standingPhysicMaterial.bounciness = 0f;
		_standingPhysicMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
		_airbornePhysicMaterial = new PhysicMaterial("PlayerPhysicMaterial_Airborne");
		_airbornePhysicMaterial.staticFriction = 0f;
		_airbornePhysicMaterial.dynamicFriction = 0f;
		_airbornePhysicMaterial.bounciness = 0f;
		_airbornePhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
		SetPhysicsMaterial(_airbornePhysicMaterial);
		_antiSinkingCollider.enabled = false;
		_owRigidbody.FreezeRotation();
		_owRigidbody.MakeNonKinematic();
		_owRigidbody.SetMaxAngularVelocity(10f);
		_jetpackModel = this.GetRequiredComponent<ThrusterModel>();
		_raycastHits = new RaycastHit[32];
		_raycastHitNormals = new Vector3[32];
		GlobalMessenger.AddListener("InitPlayerForceAlignment", OnInitPlayerForceAlignment);
		GlobalMessenger.AddListener("BreakPlayerForceAlignment", OnBreakPlayerForceAlignment);
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger<Signalscope>.AddListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.AddListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.AddListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
	}

	private void Start()
	{
		_playerCam = Locator.GetPlayerCamera();
		_initFOV = _playerCam.fieldOfView;
		_playerResources = this.GetRequiredComponent<PlayerResources>();
		_playerResources.OnInstantDamage += OnInstantDamage;
		_hazardDetector = Locator.GetPlayerDetector().GetComponent<HazardDetector>();
		_hazardDetector.OnHazardsUpdated += OnHazardsUpdated;
		_forceDetector = Locator.GetPlayerDetector().GetComponent<ForceDetector>();
		_fluidDetector = Locator.GetPlayerDetector().GetComponent<FluidDetector>();
		_pushPrompt = new ScreenPrompt(InputLibrary.jump, "<CMD> " + UITextLibrary.GetString(UITextType.PushPrompt));
		_pushPrompt.SetVisibility(isVisible: false);
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(_runningPhysicMaterial);
		UnityEngine.Object.Destroy(_standingPhysicMaterial);
		UnityEngine.Object.Destroy(_airbornePhysicMaterial);
		_runningPhysicMaterial = null;
		_standingPhysicMaterial = null;
		_airbornePhysicMaterial = null;
		GlobalMessenger.RemoveListener("InitPlayerForceAlignment", OnInitPlayerForceAlignment);
		GlobalMessenger.RemoveListener("BreakPlayerForceAlignment", OnBreakPlayerForceAlignment);
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger<Signalscope>.RemoveListener("EnterSignalscopeZoom", OnEnterSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitSignalscopeZoom", OnExitSignalscopeZoom);
		GlobalMessenger.RemoveListener("ExitFlightConsole", OnExitFlightConsole);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
		if (_playerResources != null)
		{
			_playerResources.OnInstantDamage -= OnInstantDamage;
		}
		if (_hazardDetector != null)
		{
			_hazardDetector.OnHazardsUpdated -= OnHazardsUpdated;
		}
	}

	public OWRigidbody GetBody()
	{
		return _owRigidbody;
	}

	public bool IsSlidingOnIce()
	{
		if (IsGrounded() && _groundSurface == SurfaceType.Ice)
		{
			return _groundCollider.material.dynamicFriction == 0f;
		}
		return false;
	}

	public bool IsGroundedOnRisingSand()
	{
		return _groundedOnRisingSand;
	}

	public SurfaceType GetGroundSurface()
	{
		return _groundSurface;
	}

	public float GetRunSpeedMagnitude()
	{
		return _runSpeed;
	}

	public float GetWalkSpeedMagnitude()
	{
		return _walkSpeed;
	}

	public bool HasGroundControl()
	{
		if (_isGrounded && _groundCollider.material.dynamicFriction > 0f)
		{
			return !_inWarpField;
		}
		return false;
	}

	public Collider GetAntiSinkingCollider()
	{
		return _antiSinkingCollider;
	}

	public bool IsGrounded()
	{
		return _isGrounded;
	}

	public bool AllowMidairWobble()
	{
		if (!_isGrounded)
		{
			return !_fluidDetector.InFluidType(FluidVolume.Type.TRACTOR_BEAM);
		}
		return false;
	}

	public OWRigidbody GetGroundBody()
	{
		return _groundBody;
	}

	public OWRigidbody GetLastGroundBody()
	{
		return _lastGroundBody;
	}

	public Vector3 GetGroundContactPoint()
	{
		return _groundContactPt;
	}

	public float GetTurning()
	{
		if (!_isTurningLocked && !_isTumbling)
		{
			return _lastTurnInput;
		}
		return 0f;
	}

	public Vector3 GetRelativeGroundVelocity()
	{
		if (!HasGroundControl() || !(_groundBody != null))
		{
			return Vector3.zero;
		}
		return _transform.InverseTransformDirection(_owRigidbody.GetVelocity()) - GetLocalGroundFrameVelocity();
	}

	public Vector3 GetLocalGroundFrameVelocity()
	{
		if (_groundBody == null)
		{
			return Vector3.zero;
		}
		if (_movingPlatform != null)
		{
			return _transform.InverseTransformDirection(_movingPlatform.GetPointVelocity(_transform.position));
		}
		return _transform.InverseTransformDirection(_groundBody.GetPointVelocity(_transform.position));
	}

	public Vector3 GetNormalAcceleration()
	{
		if (!_isGrounded)
		{
			return Vector3.zero;
		}
		return _normalAcceleration;
	}

	public float GetNormalAccelerationScalar()
	{
		if (!_isGrounded)
		{
			return 0f;
		}
		return _normalAcceleration.magnitude * Mathf.Sign(Vector3.Dot(_normalAcceleration, _transform.up));
	}

	public float GetJumpCrouchFraction()
	{
		if (!_useChargeJump)
		{
			return 0f;
		}
		if (_useChargeCurve)
		{
			return Mathf.Clamp01(_jumpChargeCurve.Evaluate(_jumpChargeTime));
		}
		return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.2f, _jumpChargeTime));
	}

	private float CalculateJumpSpeed()
	{
		if (!_useChargeJump)
		{
			return _maxJumpSpeed;
		}
		float num = Mathf.InverseLerp(0.033f, 0.19f, _jumpChargeTime);
		if (_useChargeCurve)
		{
			num = Mathf.Clamp01(_jumpChargeCurve.Evaluate(_jumpChargeTime));
			return Mathf.Lerp(_minJumpSpeed, _maxJumpSpeed, num);
		}
		return Mathf.Lerp(_minJumpSpeed, _maxJumpSpeed, num * num);
	}

	public void SetInWarpField(bool inWarpField)
	{
		_inWarpField = inWarpField;
	}

	public void SetDreamLantern(DreamLanternItem lanternItem)
	{
		_heldLanternItem = lanternItem;
	}

	public void LockMovement(bool lockTurning = true)
	{
		_isMovementLocked = true;
		_jumpNextFixedUpdate = false;
		_isGrounded = false;
		_antiSinkingCollider.enabled = false;
		_lastGroundBody = _groundBody;
		_groundBody = null;
		_groundContactPt = Vector3.zero;
		_normalAcceleration = Vector3.zero;
		if (lockTurning)
		{
			_isTurningLocked = true;
			_baseAngularVelocity = 0f;
		}
	}

	public void UnlockMovement()
	{
		_isMovementLocked = false;
		_isTurningLocked = false;
	}

	public void SetColliderActivation(bool active)
	{
		_bodyCollider.enabled = active;
		if (!active)
		{
			_antiSinkingCollider.enabled = false;
		}
	}

	private void EnableZeroGMovement()
	{
		_isZeroGMovementEnabled = true;
		if (Locator.GetPromptManager() != null)
		{
			Locator.GetPromptManager().AddScreenPrompt(_pushPrompt, PromptPosition.Center);
		}
	}

	private void DisableZeroGMovement()
	{
		_isZeroGMovementEnabled = false;
		_isPushable = false;
		_pushNextFixedUpdate = false;
		_pushableBody = null;
		_pushContactPt = Vector3.zero;
		if (_pushPrompt != null)
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_pushPrompt);
			_pushPrompt.SetVisibility(isVisible: false);
		}
	}

	private void SetPhysicsMaterial(PhysicMaterial physicsMaterial)
	{
		_bodyCollider.material = physicsMaterial;
		_antiSinkingCollider.material = physicsMaterial;
	}

	private void Update()
	{
		if (_isAlignedToForce || _isZeroGMovementEnabled)
		{
			if (OWInput.GetValue(InputLibrary.thrustUp) == 0f)
			{
				UpdateJumpInput();
			}
			else
			{
				_jumpChargeTime = 0f;
				_jumpNextFixedUpdate = false;
				_jumpPressedInOtherMode = false;
			}
			if (_isZeroGMovementEnabled)
			{
				_pushPrompt.SetVisibility(OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam) && _isPushable);
			}
		}
	}

	private void UpdateJumpInput()
	{
		if (!OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam))
		{
			_jumpPressedInOtherMode = InputLibrary.jump.IsPressed();
			_jumpChargeTime = 0f;
			_jumpNextFixedUpdate = false;
		}
		else if (_jumpPressedInOtherMode)
		{
			if (InputLibrary.jump.IsNewlyReleased())
			{
				_jumpPressedInOtherMode = false;
			}
		}
		else if (_useChargeJump)
		{
			if (_isGrounded)
			{
				if (OWInput.IsPressed(InputLibrary.jump))
				{
					_jumpChargeTime += Time.deltaTime;
				}
				else if (_jumpChargeTime > 0f && OWInput.IsNewlyReleased(InputLibrary.jump))
				{
					_jumpNextFixedUpdate = true;
				}
			}
			else if (_isZeroGMovementEnabled && _isPushable)
			{
				if (OWInput.IsNewlyReleased(InputLibrary.jump))
				{
					_pushNextFixedUpdate = true;
				}
			}
			else if (_jumpChargeTime > 0f && Time.time - _lastGroundedTime <= _jumpDurationAfterUngrounded)
			{
				if (OWInput.IsNewlyReleased(InputLibrary.jump))
				{
					_jumpNextFixedUpdate = true;
				}
				else if (OWInput.IsPressed(InputLibrary.jump))
				{
					_jumpChargeTime += Time.deltaTime;
				}
			}
			else
			{
				_jumpChargeTime = 0f;
			}
		}
		else if (_isGrounded && OWInput.IsNewlyPressed(InputLibrary.jump))
		{
			_jumpNextFixedUpdate = true;
		}
	}

	private void FixedUpdate()
	{
		if (OWTime.IsPaused() || (!_isAlignedToForce && !_isZeroGMovementEnabled))
		{
			return;
		}
		if (!_isMovementLocked)
		{
			if (!_isTumbling)
			{
				UpdateGrounded();
				UpdateTurning();
				if (_isZeroGMovementEnabled)
				{
					UpdatePushable();
				}
				if (HasGroundControl())
				{
					UpdateMovement();
				}
				else if (!IsGrounded() && !_isWearingSuit && !_isZeroGMovementEnabled)
				{
					UpdateAirControl();
				}
				else if (IsSlidingOnIce())
				{
					_owRigidbody.AddAcceleration(-base.transform.up * 5f);
				}
				ApplyJump();
			}
			else if (Time.time > _initTumbleTime + _tumbleDuration)
			{
				_isTumbling = false;
				_owRigidbody.FreezeRotation();
				_owRigidbody.GetRigidbody().angularDrag = 0f;
			}
		}
		else if (!_isTurningLocked)
		{
			UpdateTurning();
		}
	}

	private void UpdateMovement()
	{
		Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character | InputMode.NomaiRemoteCam);
		float magnitude = axisValue.magnitude;
		if (_groundBody != null)
		{
			if (magnitude == 0f)
			{
				PreventSliding();
			}
			Vector3 pointAcceleration = _groundBody.GetPointAcceleration(_groundContactPt);
			Vector3 forceAcceleration = _forceDetector.GetForceAcceleration();
			_normalAcceleration = Vector3.Project(pointAcceleration - forceAcceleration, _groundNormal);
		}
		bool flag = !OWInput.IsPressed(InputLibrary.rollMode, InputMode.Character | InputMode.NomaiRemoteCam) || _heldLanternItem != null;
		float maxSpeedZ = ((!flag) ? _walkSpeed : ((axisValue.y < 0f) ? _strafeSpeed : _runSpeed));
		float maxSpeedX = (flag ? _strafeSpeed : _walkSpeed);
		if (_heldLanternItem != null)
		{
			_heldLanternItem.OverrideMaxRunSpeed(ref maxSpeedX, ref maxSpeedZ);
		}
		if (Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer())
		{
			maxSpeedZ = Mathf.Min(maxSpeedZ, _walkSpeed);
			maxSpeedX = Mathf.Min(maxSpeedX, _walkSpeed);
		}
		if (_jumpChargeTime > 0f && !_useChargeCurve)
		{
			float t = Mathf.InverseLerp(1f, 2f, _jumpChargeTime);
			maxSpeedZ = Mathf.Min(maxSpeedZ, Mathf.Lerp(maxSpeedZ, 2f, t));
			maxSpeedX = Mathf.Min(maxSpeedX, Mathf.Lerp(maxSpeedX, 2f, t));
		}
		Vector3 vector = new Vector3(axisValue.x * maxSpeedX, 0f, axisValue.y * maxSpeedZ);
		if (_isStaggered)
		{
			float num = Mathf.Clamp01((Time.time - _initStaggerTime) / _staggerLength);
			vector *= num;
			if (num == 1f)
			{
				_isStaggered = false;
			}
		}
		if (PlayerState.IsCameraUnderwater())
		{
			vector *= 0.5f;
		}
		else if (!flag || vector.magnitude <= _walkSpeed)
		{
			if (Physics.Raycast(_transform.position + _transform.TransformDirection(new Vector3(axisValue.x, 0f, axisValue.y).normalized * 0.1f), -_transform.up, out var hitInfo, 20f, OWLayerMask.groundMask))
			{
				float num2 = hitInfo.distance - 1f;
				if (num2 > 0.2f && (Vector3.Angle(_owRigidbody.GetLocalUpDirection(), hitInfo.normal) > (float)_maxAngleToBeGrounded || num2 > 1.5f))
				{
					vector = Vector3.zero;
					axisValue = Vector2.zero;
				}
			}
			else
			{
				vector = Vector3.zero;
				axisValue = Vector2.zero;
			}
		}
		SetPhysicsMaterial((magnitude > 0.01f || _movingPlatform != null) ? _runningPhysicMaterial : _standingPhysicMaterial);
		Vector3 vector2 = _transform.InverseTransformDirection(_owRigidbody.GetVelocity());
		Vector3 direction = vector + GetLocalGroundFrameVelocity() - vector2;
		direction.y = 0f;
		if (direction.magnitude > _tumbleThreshold)
		{
			InitTumble();
			return;
		}
		float num3 = Time.fixedDeltaTime * 60f;
		float num4 = _acceleration * num3;
		direction.x = Mathf.Clamp(direction.x, 0f - num4, num4);
		direction.z = Mathf.Clamp(direction.z, 0f - num4, num4);
		Vector3 vector3 = _transform.TransformDirection(direction);
		vector3 -= Vector3.Project(vector3, _groundNormal);
		_owRigidbody.AddVelocityChange(vector3);
	}

	private void UpdateAirControl()
	{
		if (_lastGroundBody != null)
		{
			Vector3 vector = _transform.InverseTransformDirection(_lastGroundBody.GetPointVelocity(_transform.position));
			Vector3 vector2 = _transform.InverseTransformDirection(_owRigidbody.GetVelocity()) - vector;
			vector2.y = 0f;
			if (vector2.magnitude < _airSpeed)
			{
				float num = Time.fixedDeltaTime * 60f;
				float num2 = _airAcceleration * num;
				Vector2 axisValue = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character | InputMode.NomaiRemoteCam);
				Vector3 localVelocityChange = new Vector3(num2 * axisValue.x, 0f, num2 * axisValue.y);
				_owRigidbody.AddLocalVelocityChange(localVelocityChange);
			}
		}
	}

	private void PreventSliding()
	{
		float num = Time.fixedDeltaTime * 60f;
		Vector3 vector = _groundBody.transform.InverseTransformPoint(_transform.position);
		if ((vector - _lastLocalGroundFramePosition).magnitude < 0.001f * num)
		{
			_transform.position = _groundBody.transform.TransformPoint(_lastLocalGroundFramePosition);
			vector = _lastLocalGroundFramePosition;
		}
		_lastLocalGroundFramePosition = vector;
	}

	private void ApplyJump()
	{
		if (_jumpNextFixedUpdate)
		{
			if (this.OnJump != null)
			{
				this.OnJump();
			}
			float num = CalculateJumpSpeed();
			OWRigidbody oWRigidbody = (_isGrounded ? _groundBody : _lastGroundBody);
			num -= _transform.InverseTransformDirection(_owRigidbody.GetVelocity() - oWRigidbody.GetPointVelocity(_transform.position)).y;
			_owRigidbody.AddLocalVelocityChange(new Vector3(0f, num, 0f));
			_jumpNextFixedUpdate = false;
			_jumpChargeTime = 0f;
			_lastJumpTime = Time.time;
		}
		if (_pushNextFixedUpdate)
		{
			if (this.OnJump != null)
			{
				this.OnJump();
			}
			if (_pushableBody != null && _pushableBody.GetMass() < 10f)
			{
				_pushableBody.AddImpulse(_playerCam.transform.forward * 3f * _owRigidbody.GetMass(), _pushContactPt);
			}
			_owRigidbody.AddVelocityChange(-_playerCam.transform.forward * 3f);
			_lastPushTime = Time.time;
			_pushNextFixedUpdate = false;
		}
	}

	private void UpdateTurning()
	{
		float num = 0f;
		float num2 = 1f;
		num2 *= _playerCam.fieldOfView / _initFOV;
		num = (_lastTurnInput = OWInput.GetAxisValue(InputLibrary.look, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam).x * num2);
		bool flag = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
		float num3 = ((_signalscopeZoom || flag) ? (PlayerCameraController.LOOK_RATE * PlayerCameraController.ZOOM_SCALAR) : PlayerCameraController.LOOK_RATE);
		Quaternion quaternion = Quaternion.AngleAxis(num * num3 * Time.fixedDeltaTime / Time.timeScale, _transform.up);
		if (_isGrounded && _groundBody != null)
		{
			Vector3 vector = ((_movingPlatform != null) ? _movingPlatform.GetAngularVelocity() : _groundBody.GetAngularVelocity());
			int num4 = (int)Mathf.Sign(Vector3.Dot(vector, _transform.up));
			_baseAngularVelocity = Vector3.Project(vector, _transform.up).magnitude * (float)num4;
		}
		else
		{
			_baseAngularVelocity *= 0.995f;
		}
		Quaternion quaternion2 = Quaternion.AngleAxis(_baseAngularVelocity * 180f / (float)Math.PI * Time.fixedDeltaTime, _transform.up);
		_transform.rotation = quaternion * quaternion2 * _transform.rotation;
	}

	private void UpdateGrounded()
	{
		if (!_isMovementLocked && !_isTumbling && _isAlignedToForce)
		{
			_wasGrounded = _isGrounded;
			_isGrounded = false;
			if (_collidingQuantumObject != null)
			{
				_collidingQuantumObject.SetPlayerStandingOnObject(isPlayerStandingOnObject: false);
				_collidingQuantumObject = null;
			}
			_movingPlatform = null;
			_groundSurface = SurfaceType.None;
			_groundedOnRisingSand = false;
			if (!_fluidDetector.CheckPreventPlayerGrounded() && !PlayerState.InUndertowVolume())
			{
				CastForGrounded();
			}
			if (!_isGrounded && _wasGrounded)
			{
				MakeUngrounded();
			}
		}
	}

	private void CastForGrounded()
	{
		float num = Time.fixedDeltaTime * 60f;
		bool flag = _fluidDetector.InFluidType(FluidVolume.Type.TRACTOR_BEAM) || _fluidDetector.InFluidType(FluidVolume.Type.SAND) || _fluidDetector.InFluidType(FluidVolume.Type.WATER);
		bool flag2 = _groundSnappingEnabled && _wasGrounded && !flag && _jetpackModel.GetLocalAcceleration().y <= 0f && Time.time > _lastJumpTime + 0.5f;
		float num2 = (flag2 ? 0.1f : 0.06f) * num;
		Vector3 localUpDirection = _owRigidbody.GetLocalUpDirection();
		float num3 = 0.46f;
		float maxDistance = num2 + (1f - num3);
		int num4 = Physics.SphereCastNonAlloc(_owRigidbody.GetPosition(), num3, -localUpDirection, _raycastHits, maxDistance, OWLayerMask.groundMask, QueryTriggerInteraction.Ignore);
		RaycastHit raycastHit = default(RaycastHit);
		bool flag3 = false;
		for (int i = 0; i < num4; i++)
		{
			if (IsValidGroundedHit(_raycastHits[i]))
			{
				if (!flag3)
				{
					raycastHit = _raycastHits[i];
					flag3 = true;
				}
				else if (_raycastHits[i].distance < raycastHit.distance)
				{
					raycastHit = _raycastHits[i];
				}
			}
		}
		if (!flag3)
		{
			return;
		}
		float num5 = float.PositiveInfinity;
		bool flag4 = false;
		if (AllowGroundedOnRigidbody(raycastHit.rigidbody) && Vector3.Angle(localUpDirection, raycastHit.normal) <= (float)_maxAngleToBeGrounded)
		{
			num5 = GetGroundHitDistance(raycastHit);
			flag4 = true;
		}
		else
		{
			for (int j = 0; j < num4; j++)
			{
				if (IsValidGroundedHit(_raycastHits[j]) && AllowGroundedOnRigidbody(_raycastHits[j].rigidbody))
				{
					float groundHitDistance = GetGroundHitDistance(_raycastHits[j]);
					num5 = Mathf.Min(num5, groundHitDistance);
					if (Vector3.Angle(localUpDirection, _raycastHits[j].normal) <= (float)_maxAngleToBeGrounded)
					{
						raycastHit = _raycastHits[j];
						flag4 = true;
					}
					else
					{
						_raycastHitNormals[j] = Vector3.ProjectOnPlane(_raycastHits[j].normal, localUpDirection);
					}
				}
			}
			if (!flag4)
			{
				for (int k = 0; k < num4; k++)
				{
					if (_isGrounded)
					{
						break;
					}
					if (!IsValidGroundedHit(_raycastHits[k]))
					{
						continue;
					}
					for (int l = k + 1; l < num4; l++)
					{
						if (_isGrounded)
						{
							break;
						}
						if (IsValidGroundedHit(_raycastHits[k]) && Vector3.Angle(_raycastHitNormals[k], _raycastHitNormals[l]) > (float)_maxAngleBetweenSlopes)
						{
							flag4 = true;
							raycastHit = _raycastHits[k];
							num5 = GetGroundHitDistance(_raycastHits[k]);
							break;
						}
					}
				}
			}
			if (!flag4 && _wasGrounded && raycastHit.collider.material.dynamicFriction > 0f)
			{
				Vector3 onNormal = _transform.InverseTransformPoint(raycastHit.point);
				onNormal.y = 0f;
				Vector3 vector = _transform.InverseTransformDirection(_owRigidbody.GetVelocity() - _groundBody.GetPointVelocity(_transform.position));
				if (vector.y > 0f)
				{
					Vector3 vector2 = -Vector3.Project(vector, onNormal);
					vector2.y = 0f - vector.y;
					_owRigidbody.AddLocalVelocityChange(vector2 * 0.7f * num);
				}
			}
		}
		IgnoreCollision ignoreCollision = (flag4 ? raycastHit.collider.GetComponent<IgnoreCollision>() : null);
		if (flag4 && (ignoreCollision == null || !ignoreCollision.IgnoresPlayer()))
		{
			if (flag2)
			{
				Vector3 vector3 = -localUpDirection * Mathf.Max(0f, num5);
				vector3 += localUpDirection * 0.001f;
				base.transform.position = base.transform.position + vector3;
			}
			if (raycastHit.rigidbody != null)
			{
				_groundBody = raycastHit.rigidbody.GetRequiredComponent<OWRigidbody>();
			}
			else
			{
				Debug.LogError("The collider we're trying to stand on is not attached to a Rigidbody!");
				Debug.Break();
			}
			_groundCollider = raycastHit.collider;
			_groundContactPt = raycastHit.point;
			_groundNormal = raycastHit.normal;
			_groundSurface = Locator.GetSurfaceManager().GetHitSurfaceType(raycastHit);
			_collidingQuantumObject = _groundCollider.GetComponentInParent<QuantumObject>();
			if (_collidingQuantumObject != null)
			{
				_collidingQuantumObject.SetPlayerStandingOnObject(isPlayerStandingOnObject: true);
			}
			_movingPlatform = _groundCollider.GetComponentInParent<MovingPlatform>();
			_groundedOnRisingSand = _groundCollider.CompareTag("RisingSand");
			_antiSinkingCollider.enabled = _wasGrounded && num5 > -0.1f;
			_isGrounded = true;
			if (!_wasGrounded && this.OnBecomeGrounded != null)
			{
				this.OnBecomeGrounded();
			}
		}
	}

	private bool IsValidGroundedHit(RaycastHit hit)
	{
		if (hit.distance > 0f)
		{
			return hit.rigidbody != _owRigidbody.GetRigidbody();
		}
		return false;
	}

	private bool AllowGroundedOnRigidbody(Rigidbody body)
	{
		if (body != null && body != _owRigidbody.GetRigidbody())
		{
			return body.mass > _owRigidbody.GetRigidbody().mass;
		}
		return false;
	}

	private float GetGroundHitDistance(RaycastHit hit)
	{
		Vector3 vector = _transform.InverseTransformPoint(hit.point);
		float magnitude = Vector3.ProjectOnPlane(vector, Vector3.up).magnitude;
		float num = Mathf.Abs(vector.y) - 0.5f;
		if (magnitude > 0.5f)
		{
			Debug.LogError("ERROR: Player grounded spherecast is not underneath the player. Probably need to sync transforms somewhere.");
			return 0f;
		}
		return num - Mathf.Sqrt(0.25f - magnitude * magnitude);
	}

	private void MakeUngrounded()
	{
		_normalAcceleration = Vector3.zero;
		_lastGroundBody = _groundBody;
		_groundBody = null;
		_groundContactPt = Vector3.zero;
		_groundedOnRisingSand = false;
		_isGrounded = false;
		SetPhysicsMaterial(_airbornePhysicMaterial);
		_antiSinkingCollider.enabled = false;
		_lastGroundedTime = Time.time;
		if (this.OnBecomeUngrounded != null)
		{
			this.OnBecomeUngrounded();
		}
	}

	private void UpdatePushable()
	{
		_isPushable = false;
		if (Time.time < _lastPushTime + 0.5f)
		{
			return;
		}
		float t = Vector3.Dot(_transform.up, _playerCam.transform.forward) * 0.5f + 0.5f;
		float maxDistance = Mathf.Lerp(2.5f, 1.5f, t);
		int num = Physics.RaycastNonAlloc(_playerCam.transform.position, _playerCam.transform.forward, _raycastHits, maxDistance, OWLayerMask.physicalMask);
		RaycastHit raycastHit = default(RaycastHit);
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			if (IsValidGroundedHit(_raycastHits[i]))
			{
				if (!flag)
				{
					raycastHit = _raycastHits[i];
					flag = true;
				}
				else if (_raycastHits[i].distance < raycastHit.distance)
				{
					raycastHit = _raycastHits[i];
				}
			}
		}
		if (flag)
		{
			_isPushable = true;
			if (raycastHit.rigidbody != null)
			{
				_pushableBody = raycastHit.rigidbody.GetRequiredComponent<OWRigidbody>();
			}
			else
			{
				_pushableBody = null;
			}
			_pushContactPt = raycastHit.point;
		}
	}

	private void InitTumble()
	{
		MakeUngrounded();
		_owRigidbody.UnfreezeRotation();
		_initTumbleTime = Time.time;
		_isTumbling = true;
	}

	private void OnInitPlayerForceAlignment()
	{
		_isAlignedToForce = true;
		if (_isZeroGMovementEnabled)
		{
			DisableZeroGMovement();
		}
	}

	private void OnBreakPlayerForceAlignment()
	{
		_isAlignedToForce = false;
		if (_isGrounded)
		{
			MakeUngrounded();
		}
		if (!PlayerState.IsDead() && !_isWearingSuit)
		{
			EnableZeroGMovement();
		}
	}

	private void OnSuitUp()
	{
		_isWearingSuit = true;
		if (_isZeroGMovementEnabled)
		{
			DisableZeroGMovement();
		}
	}

	private void OnRemoveSuit()
	{
		_isWearingSuit = false;
		if (!_isAlignedToForce)
		{
			EnableZeroGMovement();
		}
	}

	private void OnEnterSignalscopeZoom(Signalscope telescope)
	{
		_signalscopeZoom = true;
	}

	private void OnExitSignalscopeZoom()
	{
		_signalscopeZoom = false;
	}

	private void OnExitFlightConsole()
	{
		MakeUngrounded();
	}

	public void StaggerPlayer(float staggerLength)
	{
		_initStaggerTime = Time.time;
		_staggerLength = staggerLength;
		_isStaggered = true;
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		switch (deathType)
		{
		case DeathType.Default:
		case DeathType.Impact:
		case DeathType.Asphyxiation:
			MakeUngrounded();
			_owRigidbody.UnfreezeRotation();
			GetComponent<AlignPlayerWithForce>().enabled = false;
			SetPhysicsMaterial(_standingPhysicMaterial);
			_owRigidbody.GetRigidbody().angularDrag = ((deathType == DeathType.Impact) ? 5f : 20f);
			break;
		case DeathType.Crushed:
			LockMovement();
			base.enabled = false;
			break;
		}
	}

	private void OnPlayerResurrection()
	{
		_owRigidbody.FreezeRotation();
		GetComponent<AlignPlayerWithForce>().enabled = true;
		_owRigidbody.GetRigidbody().angularDrag = 0f;
		UnlockMovement();
		base.enabled = true;
	}

	private void OnInstantDamage(float instantDamage, InstantDamageType damageType)
	{
		if (instantDamage >= _minStaggerDamage)
		{
			float t = Mathf.InverseLerp(_minStaggerDamage, 100f, instantDamage);
			float num = Mathf.Lerp(_minStaggerLength, _maxStaggerLength, t);
			if (_isStaggered)
			{
				float b = _staggerLength - (Time.time - _initStaggerTime);
				num = Mathf.Max(num, b);
			}
			StaggerPlayer(num);
		}
	}

	private void OnHazardsUpdated()
	{
		if (_hazardDetector.InHazardType(HazardVolume.HazardType.DARKMATTER))
		{
			StaggerPlayer(1.5f);
		}
	}
}
