using UnityEngine;

public class RaftController : MonoBehaviour, IItemDropTarget
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private RingRiverFluidVolume _riverFluid;

	[SerializeField]
	private bool _debug;

	[Space]
	[SerializeField]
	private float _acceleration = 5f;

	[SerializeField]
	private LightSensor[] _lightSensors;

	[Space]
	[SerializeField]
	private Shape _detectorShape;

	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private RaftFluidDetector _fluidDetector;

	[SerializeField]
	private OWTriggerVolume _rideVolume;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	[Space]
	[SerializeField]
	private OWCollider[] _railingColliders = new OWCollider[0];

	[SerializeField]
	private Animator _railingAnimator;

	[SerializeField]
	private float _dropDelay;

	private OWRigidbody _raftBody;

	private ForceApplier _forceApplier;

	private RaftEffectsController _effectsController;

	private RaftDock _dock;

	private bool _initialized;

	private bool _skipSuspendOnStart;

	private bool _playerInEffectsRange;

	private bool _playerInRideVolume;

	private bool _allowPush;

	private float _pushTime;

	private Vector3 _localAcceleration = Vector3.zero;

	private bool _movingToTarget;

	private float _targetSpeed;

	private Vector3 _targetLocalPosition;

	private Quaternion _targetLocalRotation;

	private float _startDistance;

	private Quaternion _startLocalRotation;

	private bool _shouldReenableForcesThisTime;

	private int _addToTriggerUpdateCount;

	public OWEvent OnArriveAtTarget = new OWEvent(4);

	public Sector sector => _sector;

	public float currentDistanceLerp { get; private set; }

	public float dropDelay => _dropDelay;

	private void Awake()
	{
		_raftBody = GetComponent<OWRigidbody>();
		_effectsController = GetComponent<RaftEffectsController>();
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
		}
		if (_rideVolume != null)
		{
			_rideVolume.OnEntry += OnEnterRideVolume;
			_rideVolume.OnExit += OnExitRideVolume;
		}
		_fluidDetector.RegisterRaftController(this);
	}

	private void Start()
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (!_skipSuspendOnStart)
		{
			Suspend();
		}
	}

	private void Initialize()
	{
		if (_interactReceiver != null)
		{
			_interactReceiver.SetPromptText(UITextType.PushRaftPrompt);
			_interactReceiver.DisableInteraction();
		}
		_forceApplier = _detectorShape.GetComponent<ForceApplier>();
		_initialized = true;
	}

	private void OnDestroy()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
		if (_rideVolume != null)
		{
			_rideVolume.OnEntry -= OnEnterRideVolume;
			_rideVolume.OnExit -= OnExitRideVolume;
		}
	}

	public void SkipSuspendOnStart()
	{
		_skipSuspendOnStart = true;
	}

	public OWRigidbody GetBody()
	{
		return _raftBody;
	}

	public Vector3 GetLocalAcceleration()
	{
		return _localAcceleration;
	}

	public OWAudioSource GetOneShotAudio()
	{
		return _oneShotAudio;
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return base.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
		item.SetSector(_sector);
	}

	public bool IsPlayerRiding(bool raftMustBeInWater = true)
	{
		if (raftMustBeInWater && !_fluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			return false;
		}
		if (_playerInRideVolume)
		{
			return Vector3.Dot(Locator.GetPlayerTransform().up, base.transform.up) > 0f;
		}
		return false;
	}

	public bool InWater()
	{
		return _fluidDetector.InFluidType(FluidVolume.Type.WATER);
	}

	public bool IsDockingOrDocked()
	{
		if (!(_dock != null))
		{
			return _movingToTarget;
		}
		return true;
	}

	public void Dock(RaftDock dock, bool skipRailAnim = false)
	{
		_dock = dock;
		if (_forceApplier == null)
		{
			_forceApplier = _detectorShape.GetComponent<ForceApplier>();
		}
		_forceApplier.SetApplyForces(applyForces: false);
		_forceApplier.SetApplyFluids(applyFluids: false);
		_raftBody.SetPosition(_dock.GetRaftSocket().position);
		_raftBody.SetRotation(_dock.GetRaftSocket().rotation);
		SetRailingRaised(raised: false, skipRailAnim);
		Suspend();
	}

	public void Undock()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			Unsuspend();
		}
		_dock = null;
		_forceApplier.SetApplyForces(applyForces: true);
		_forceApplier.SetApplyFluids(applyFluids: true);
	}

	public void MoveToTarget(Vector3 position, Quaternion rotation, float speed, bool reenableForcesAfter = true)
	{
		_movingToTarget = true;
		_forceApplier.SetApplyForces(applyForces: false);
		_forceApplier.SetApplyFluids(applyFluids: false);
		OWRigidbody raftBody = _raftBody;
		Transform transform = _raftBody.GetOrigParentBody().transform;
		_targetSpeed = speed;
		_targetLocalPosition = transform.InverseTransformPoint(position);
		_targetLocalRotation = Quaternion.Inverse(transform.rotation) * rotation;
		_startDistance = Vector3.Distance(raftBody.GetPosition(), position);
		_startLocalRotation = Quaternion.Inverse(transform.rotation) * raftBody.GetRotation();
		_shouldReenableForcesThisTime = reenableForcesAfter;
	}

	public void StopMovingToTarget()
	{
		_movingToTarget = false;
		if (_shouldReenableForcesThisTime)
		{
			_forceApplier.SetApplyForces(applyForces: true);
			_forceApplier.SetApplyFluids(applyFluids: true);
		}
	}

	public void SetZeroVelocity()
	{
		_raftBody.SetVelocity(_raftBody.GetOrigParentBody().GetPointVelocity(_raftBody.GetPosition()));
	}

	public void EnableForces()
	{
		_forceApplier.SetApplyForces(applyForces: true);
		_forceApplier.SetApplyFluids(applyFluids: true);
	}

	public void DisableForces()
	{
		_forceApplier.SetApplyForces(applyForces: false);
		_forceApplier.SetApplyFluids(applyFluids: false);
	}

	public void SetRailingRaised(bool raised, bool skipAnimation = false)
	{
		for (int i = 0; i < _railingColliders.Length; i++)
		{
			_railingColliders[i].SetActivation(raised);
		}
		if (_railingAnimator != null && raised != _railingAnimator.GetCurrentAnimatorStateInfo(0).IsName("closed"))
		{
			if (!skipAnimation)
			{
				_railingAnimator.SetTrigger("ToggleRailing");
			}
			else
			{
				_railingAnimator.Play(raised ? "raft_railing_close" : "raft_railing_open", 0, 0.9f);
			}
		}
	}

	private void OnPressInteract()
	{
		Vector3 normalized = Vector3.ProjectOnPlane(Locator.GetPlayerCamera().transform.forward, base.transform.up).normalized;
		_raftBody.AddVelocityChange(normalized * 5f);
		_effectsController.PlayRaftPush();
		_pushTime = Time.time;
		_interactReceiver.SetInteractionEnabled(enable: false);
	}

	private void Update()
	{
		bool flag = _dock == null && Locator.GetPlayerController().IsGrounded() && Locator.GetPlayerController().GetGroundBody() != _raftBody && Time.time > _pushTime + 0.5f;
		if (_interactReceiver != null && _allowPush != flag)
		{
			_allowPush = flag;
			_interactReceiver.SetInteractionEnabled(_allowPush);
			_interactReceiver.ResetInteraction();
		}
	}

	private void FixedUpdate()
	{
		if (_raftBody.IsSuspended())
		{
			return;
		}
		bool playerInEffectsRange = _playerInEffectsRange;
		_playerInEffectsRange = (Locator.GetPlayerBody().GetPosition() - _raftBody.GetPosition()).sqrMagnitude < 2500f;
		if (playerInEffectsRange && !_playerInEffectsRange)
		{
			_effectsController.StopAllEffects();
		}
		if (_dock != null || _movingToTarget)
		{
			_localAcceleration = Vector3.zero;
			if (_playerInEffectsRange)
			{
				_effectsController.UpdateMovementAudio(allowMovement: false, _lightSensors);
			}
			if (_movingToTarget)
			{
				UpdateMoveToTarget();
			}
			return;
		}
		if (_fluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			if (_lightSensors[0].IsIlluminated())
			{
				_localAcceleration += Vector3.forward * _acceleration;
			}
			if (_lightSensors[1].IsIlluminated())
			{
				_localAcceleration += Vector3.right * _acceleration;
			}
			if (_lightSensors[2].IsIlluminated())
			{
				_localAcceleration -= Vector3.forward * _acceleration;
			}
			if (_lightSensors[3].IsIlluminated())
			{
				_localAcceleration -= Vector3.right * _acceleration;
			}
		}
		if (_localAcceleration.sqrMagnitude > 0.001f)
		{
			_raftBody.AddLocalAcceleration(_localAcceleration);
		}
		if (_playerInEffectsRange)
		{
			float num = (_fluidDetector.InFluidType(FluidVolume.Type.WATER) ? _riverFluid.GetFractionSubmerged(_fluidDetector) : 0f);
			bool allowMovement = num > 0.25f && num < 1f;
			_effectsController.UpdateMovementAudio(allowMovement, _lightSensors);
			_effectsController.UpdateGroundedAudio(_fluidDetector);
		}
		_localAcceleration = Vector3.zero;
	}

	private void UpdateMoveToTarget()
	{
		OWRigidbody raftBody = _raftBody;
		OWRigidbody origParentBody = _raftBody.GetOrigParentBody();
		Transform transform = origParentBody.transform;
		Vector3 vector = transform.TransformPoint(_targetLocalPosition);
		Vector3 vector2 = vector - raftBody.GetPosition();
		float num = Mathf.Min(_targetSpeed, vector2.magnitude / Time.deltaTime);
		Vector3 pointVelocity = raftBody.GetOrigParentBody().GetPointVelocity(raftBody.GetPosition());
		Vector3 vector3 = vector2.normalized * num;
		raftBody.SetVelocity(pointVelocity + vector3);
		float t = (currentDistanceLerp = Mathf.InverseLerp(_startDistance, 0.001f, vector2.magnitude));
		t = Mathf.SmoothStep(0f, 1f, t);
		if (t < 1f)
		{
			Quaternion quaternion = Quaternion.Slerp(_startLocalRotation, _targetLocalRotation, t);
			Quaternion toRotation = transform.rotation * quaternion;
			Vector3 vector4 = OWPhysics.FromToAngularVelocity(raftBody.GetRotation(), toRotation);
			raftBody.SetAngularVelocity(origParentBody.GetAngularVelocity() + vector4);
		}
		else
		{
			raftBody.SetPosition(vector);
			raftBody.SetRotation(transform.rotation * _targetLocalRotation);
			StopMovingToTarget();
			OnArriveAtTarget.Invoke();
		}
	}

	private void Suspend()
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (_dock != null)
		{
			_raftBody.Suspend(_dock.GetRaftSocket(), _raftBody.GetOrigParentBody());
		}
		else
		{
			_raftBody.Suspend();
		}
		_localAcceleration = Vector3.zero;
		_detectorShape.SetActivation(newActive: false);
		_playerInEffectsRange = false;
		base.enabled = false;
	}

	private void Unsuspend()
	{
		_raftBody.Unsuspend();
		_detectorShape.SetActivation(newActive: true);
		base.enabled = true;
	}

	private void OnOccupantEnterSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && _dock == null)
		{
			Unsuspend();
		}
	}

	private void OnOccupantExitSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && _dock == null)
		{
			Suspend();
		}
	}

	private void OnEnterRideVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInRideVolume = true;
			Locator.AddOccupiedRaft(this);
		}
	}

	private void OnExitRideVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInRideVolume = false;
			Locator.RemoveOccupiedRaft(this);
		}
	}
}
