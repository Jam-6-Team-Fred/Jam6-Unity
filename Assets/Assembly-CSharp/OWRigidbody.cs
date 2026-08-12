using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OWRigidbody : MonoBehaviour
{
	public delegate void OWRigidbodyEvent(OWRigidbody suspendedBody);

	[SerializeField]
	private bool _kinematicSimulation;

	[SerializeField]
	private bool _autoGenerateCenterOfMass = true;

	[SerializeField]
	private Vector3 _centerOfMass = Vector3.zero;

	[SerializeField]
	private bool _isTargetable = true;

	[SerializeField]
	private bool _maintainOriginalCenterOfMass = true;

	[SerializeField]
	private Sector _simulateInSector;

	protected Rigidbody _rigidbody;

	protected KinematicRigidbody _kinematicRigidbody;

	protected CenterOfTheUniverseOffsetApplier _offsetApplier;

	protected Transform _transform;

	protected Transform _scaleRoot;

	protected Transform _origParent;

	protected OWRigidbody _origParentBody;

	protected OWRigidbody _suspensionBody;

	protected Vector3 _currentAccel = Vector3.zero;

	protected Vector3 _lastAccel = Vector3.zero;

	protected Vector3 _currentVelocity = Vector3.zero;

	protected Vector3 _lastVelocity = Vector3.zero;

	protected Vector3 _currentAngularVelocity = Vector3.zero;

	protected Vector3 _lastAngularVelocity = Vector3.zero;

	protected Vector3 _lastPosition = Vector3.zero;

	private ReferenceFrame _referenceFrame;

	private ReferenceFrameVolume _attachedRFVolume;

	private GravityVolume _attachedGravityVolume;

	private ForceDetector _attachedForceDetector;

	private FluidDetector _attachedFluidDetector;

	private Collider[] _childColliders;

	private bool _suspended;

	private Vector3 _cachedRelativeVelocity = Vector3.zero;

	private Vector3 _cachedAngularVelocity = Vector3.zero;

	private Vector3 _origCenterOfMass = Vector3.zero;

	private bool _unsuspendNextUpdate;

	private bool _restoreCachedVelocityOnUnsuspend;

	protected bool _kinematicStateNewlyChanged;

	public event OWRigidbodyEvent OnSuspendOWRigidbody;

	public event OWRigidbodyEvent OnPreUnsuspendOWRigidbody;

	public event OWRigidbodyEvent OnUnsuspendOWRigidbody;

	public event OWRigidbodyEvent OnDestroyOWRigidbody;

	public event OWRigidbodyEvent OnWarpOWRigidbody;

	private void OnValidate()
	{
		if (_kinematicSimulation && !_autoGenerateCenterOfMass)
		{
			_autoGenerateCenterOfMass = true;
		}
		if (_autoGenerateCenterOfMass && _centerOfMass != Vector3.zero)
		{
			_centerOfMass = Vector3.zero;
		}
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null && component.useGravity)
		{
			component.useGravity = false;
		}
	}

	protected virtual void Awake()
	{
		_transform = base.transform;
		if (_scaleRoot == null)
		{
			_scaleRoot = _transform;
		}
		CenterOfTheUniverse.TrackRigidbody(this);
		_offsetApplier = base.gameObject.GetAddComponent<CenterOfTheUniverseOffsetApplier>();
		_offsetApplier.Init(this);
		if (_simulateInSector != null)
		{
			_simulateInSector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_origParent = _transform.parent;
		_origParentBody = ((_origParent != null) ? _origParent.GetAttachedOWRigidbody() : null);
		if (_transform.parent != null)
		{
			_transform.parent = null;
		}
		_rigidbody = this.GetRequiredComponent<Rigidbody>();
		_rigidbody.interpolation = RigidbodyInterpolation.None;
		if (!_autoGenerateCenterOfMass)
		{
			_rigidbody.centerOfMass = _centerOfMass;
		}
		if (IsSimulatedKinematic())
		{
			EnableKinematicSimulation();
		}
		_origCenterOfMass = (RunningKinematicSimulation() ? _kinematicRigidbody.centerOfMass : _rigidbody.centerOfMass);
		_referenceFrame = new ReferenceFrame(this);
	}

	protected virtual void Start()
	{
		UpdateCenterOfMass();
	}

	private void OnEnable()
	{
		FixedEarlyUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedEarlyUpdateManager.Unregister(this);
	}

	protected virtual void OnDestroy()
	{
		CenterOfTheUniverse.UntrackRigidbody(this);
		if (this.OnDestroyOWRigidbody != null)
		{
			this.OnDestroyOWRigidbody(this);
		}
		if (_simulateInSector != null)
		{
			_simulateInSector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_referenceFrame != null)
		{
			_referenceFrame.Destroy();
		}
		if (_kinematicRigidbody != null)
		{
			Object.Destroy(_kinematicRigidbody);
		}
		Object.Destroy(_rigidbody);
		Object.Destroy(_offsetApplier);
	}

	public void UpdateCenterOfMass()
	{
		if (_maintainOriginalCenterOfMass && _rigidbody.centerOfMass != _origCenterOfMass)
		{
			_rigidbody.centerOfMass = _origCenterOfMass;
		}
	}

	public void MoveToRelativeLocation(RelativeLocationData location, Transform relativeTransform)
	{
		MoveToRelativeLocation(location, relativeTransform.GetComponentInParent<OWRigidbody>(), relativeTransform);
	}

	public void MoveToRelativeLocation(RelativeLocationData location, OWRigidbody relativeBody, Transform relativeTransform = null)
	{
		if (relativeTransform == null)
		{
			relativeTransform = relativeBody.transform;
		}
		Vector3 vector = relativeTransform.TransformPoint(location.localPosition);
		SetRotation(relativeTransform.rotation * location.localRotation);
		SetPosition(vector);
		SetVelocity(relativeBody.GetPointVelocity(vector) + relativeTransform.TransformDirection(location.localRelativeVelocity));
	}

	protected virtual void OnSectorOccupantsUpdated()
	{
		if (_suspended && _simulateInSector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			Unsuspend();
		}
		else if (!_suspended && !_simulateInSector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			Suspend();
		}
	}

	public bool IsSuspended()
	{
		return _suspended;
	}

	public virtual void Suspend()
	{
		if (!_suspended)
		{
			if (_origParentBody != null)
			{
				Suspend(_origParent, _origParentBody);
			}
			else if (_simulateInSector != null)
			{
				Suspend(_simulateInSector.GetOWRigidbody());
			}
			else
			{
				Debug.Log("Unable to suspend : " + base.gameObject.name);
			}
		}
	}

	public void Suspend(OWRigidbody suspensionBody)
	{
		Suspend(suspensionBody.transform, suspensionBody);
	}

	public void Suspend(Transform suspensionParent, OWRigidbody suspensionBody)
	{
		if (_suspended && !_unsuspendNextUpdate)
		{
			return;
		}
		_suspensionBody = suspensionBody;
		Vector3 direction = GetVelocity() - suspensionBody.GetPointVelocity(_transform.position);
		_cachedRelativeVelocity = suspensionBody.transform.InverseTransformDirection(direction);
		_cachedAngularVelocity = (RunningKinematicSimulation() ? _kinematicRigidbody.angularVelocity : _rigidbody.angularVelocity);
		base.enabled = false;
		_offsetApplier.enabled = false;
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.enabled = false;
		}
		else
		{
			MakeKinematic();
		}
		_transform.parent = suspensionParent;
		_suspended = true;
		_unsuspendNextUpdate = false;
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
		if (_childColliders == null)
		{
			_childColliders = GetComponentsInChildren<Collider>();
			for (int i = 0; i < _childColliders.Length; i++)
			{
				_childColliders[i].gameObject.GetAddComponent<OWCollider>().ListenForParentBodySuspension();
			}
		}
		if (this.OnSuspendOWRigidbody != null)
		{
			this.OnSuspendOWRigidbody(this);
		}
	}

	public void ChangeSuspensionBody(OWRigidbody newSuspensionBody)
	{
		if (_suspended)
		{
			_cachedRelativeVelocity = Vector3.zero;
			_suspensionBody = newSuspensionBody;
			_transform.parent = newSuspensionBody.transform;
		}
	}

	public void Unsuspend(bool restoreCachedVelocity = true)
	{
		if (_suspended && !_unsuspendNextUpdate)
		{
			_unsuspendNextUpdate = true;
			_restoreCachedVelocityOnUnsuspend = restoreCachedVelocity;
			base.enabled = true;
			_offsetApplier.enabled = true;
			if (RunningKinematicSimulation())
			{
				_kinematicRigidbody.enabled = true;
			}
			if (this.OnPreUnsuspendOWRigidbody != null)
			{
				this.OnPreUnsuspendOWRigidbody(this);
			}
		}
	}

	private void UnsuspendImmediate(bool restoreCachedVelocity)
	{
		if (_suspended)
		{
			if (RunningKinematicSimulation())
			{
				_kinematicRigidbody.enabled = true;
			}
			else
			{
				MakeNonKinematic();
			}
			base.enabled = true;
			_transform.parent = null;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			Vector3 vector = (restoreCachedVelocity ? _suspensionBody.transform.TransformDirection(_cachedRelativeVelocity) : Vector3.zero);
			SetVelocity(_suspensionBody.GetPointVelocity(_transform.position) + vector);
			SetAngularVelocity(restoreCachedVelocity ? _cachedAngularVelocity : Vector3.zero);
			_suspended = false;
			_suspensionBody = null;
			if (this.OnUnsuspendOWRigidbody != null)
			{
				this.OnUnsuspendOWRigidbody(this);
			}
		}
	}

	public void SetAttachedReferenceFrameVolume(ReferenceFrameVolume rfVolume)
	{
		_attachedRFVolume = rfVolume;
	}

	public void RegisterAttachedGravityVolume(GravityVolume gravityVolume)
	{
		_attachedGravityVolume = gravityVolume;
	}

	public void RegisterAttachedForceDetector(ForceDetector detector)
	{
		_attachedForceDetector = detector;
	}

	public void RegisterAttachedFluidDetector(FluidDetector detector)
	{
		_attachedFluidDetector = detector;
	}

	public GravityVolume GetAttachedGravityVolume()
	{
		return _attachedGravityVolume;
	}

	public ForceDetector GetAttachedForceDetector()
	{
		return _attachedForceDetector;
	}

	public FluidDetector GetAttachedFluidDetector()
	{
		return _attachedFluidDetector;
	}

	public void AddForce(Vector3 force)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForce(force);
		}
		else
		{
			_rigidbody.AddForce(force, ForceMode.Force);
		}
	}

	public void AddForce(Vector3 force, Vector3 worldPosition)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForceAtPosition(force, worldPosition);
		}
		else
		{
			_rigidbody.AddForceAtPosition(force, worldPosition, ForceMode.Force);
		}
	}

	public void AddLocalForce(Vector3 localForce)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeForce(localForce);
		}
		else
		{
			_rigidbody.AddRelativeForce(localForce, ForceMode.Force);
		}
	}

	public void AddLocalForce(Vector3 localForce, Vector3 localPosition)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForceAtPosition(_transform.TransformDirection(localForce), _transform.TransformPoint(localPosition));
		}
		else
		{
			_rigidbody.AddForceAtPosition(_transform.TransformDirection(localForce), _transform.TransformPoint(localPosition), ForceMode.Force);
		}
	}

	public void AddAcceleration(Vector3 acceleration)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForce(acceleration, ForceMode.Acceleration);
		}
		else
		{
			_rigidbody.AddForce(acceleration, ForceMode.Acceleration);
		}
	}

	public void AddAcceleration(Vector3 acceleration, Vector3 worldPosition)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForceAtPosition(acceleration, worldPosition, ForceMode.Acceleration);
		}
		else
		{
			_rigidbody.AddForceAtPosition(acceleration, worldPosition, ForceMode.Acceleration);
		}
	}

	public void AddLocalAcceleration(Vector3 localAcceleration)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeForce(localAcceleration, ForceMode.Acceleration);
		}
		else
		{
			_rigidbody.AddRelativeForce(localAcceleration, ForceMode.Acceleration);
		}
	}

	public void AddLocalAcceleration(Vector3 localAcceleration, Vector3 localPosition)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForceAtPosition(_transform.TransformDirection(localAcceleration), _transform.TransformPoint(localPosition), ForceMode.Acceleration);
		}
		else
		{
			_rigidbody.AddForceAtPosition(_transform.TransformDirection(localAcceleration), _transform.TransformPoint(localPosition), ForceMode.Acceleration);
		}
	}

	public void AddImpulse(Vector3 impulse)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForce(impulse, ForceMode.Impulse);
		}
		else
		{
			_rigidbody.AddForce(impulse, ForceMode.Impulse);
		}
	}

	public void AddImpulse(Vector3 impulse, Vector3 worldPosition)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForceAtPosition(impulse, worldPosition, ForceMode.Impulse);
		}
		else
		{
			_rigidbody.AddForceAtPosition(impulse, worldPosition, ForceMode.Impulse);
		}
	}

	public void AddLocalImpulse(Vector3 localImpulse)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeForce(localImpulse, ForceMode.Impulse);
		}
		else
		{
			_rigidbody.AddRelativeForce(localImpulse, ForceMode.Impulse);
		}
	}

	public void AddVelocityChange(Vector3 velocityChange)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
		}
		else
		{
			_rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
		}
	}

	public void AddLocalVelocityChange(Vector3 localVelocityChange)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeForce(localVelocityChange, ForceMode.VelocityChange);
		}
		else
		{
			_rigidbody.AddRelativeForce(localVelocityChange, ForceMode.VelocityChange);
		}
	}

	public void AddTorque(Vector3 torque)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddTorque(torque);
		}
		else
		{
			_rigidbody.AddTorque(torque, ForceMode.Force);
		}
	}

	public void AddLocalTorque(Vector3 localTorque)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeTorque(localTorque);
		}
		else
		{
			_rigidbody.AddRelativeTorque(localTorque, ForceMode.Force);
		}
	}

	public void AddAngularAcceleration(Vector3 angularAcceleration)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddTorque(angularAcceleration, ForceMode.Acceleration);
		}
		else
		{
			_rigidbody.AddTorque(angularAcceleration, ForceMode.Acceleration);
		}
	}

	public void AddLocalAngularAcceleration(Vector3 localAngularAcceleration)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeTorque(localAngularAcceleration, ForceMode.Acceleration);
		}
		else
		{
			_rigidbody.AddRelativeTorque(localAngularAcceleration, ForceMode.Acceleration);
		}
	}

	public void AddAngularImpulse(Vector3 angularImpulse)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddTorque(angularImpulse, ForceMode.Impulse);
		}
		else
		{
			_rigidbody.AddTorque(angularImpulse, ForceMode.Impulse);
		}
	}

	public void AddLocalAngularImpulse(Vector3 localAngularImpulse)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeTorque(localAngularImpulse, ForceMode.Impulse);
		}
		else
		{
			_rigidbody.AddRelativeTorque(localAngularImpulse, ForceMode.Impulse);
		}
	}

	public void AddAngularVelocityChange(Vector3 angularVelocityChange)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddTorque(angularVelocityChange, ForceMode.VelocityChange);
		}
		else
		{
			_rigidbody.AddTorque(angularVelocityChange, ForceMode.VelocityChange);
		}
	}

	public void AddLocalAngularVelocityChange(Vector3 localAngularVelocityChange)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.AddRelativeTorque(localAngularVelocityChange, ForceMode.VelocityChange);
		}
		else
		{
			_rigidbody.AddRelativeTorque(localAngularVelocityChange, ForceMode.VelocityChange);
		}
	}

	public virtual void WarpToPositionRotation(Vector3 worldPosition, Quaternion worldRotation)
	{
		SetRotation(worldRotation);
		SetPosition(worldPosition);
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
		if (this.OnWarpOWRigidbody != null)
		{
			this.OnWarpOWRigidbody(this);
		}
	}

	public virtual void SetPosition(Vector3 worldPosition)
	{
		_transform.position = worldPosition;
	}

	public virtual void SetRotation(Quaternion rotation)
	{
		_transform.rotation = rotation;
	}

	public void SetLocalScale(Vector3 localScale)
	{
		_scaleRoot.localScale = localScale;
	}

	public void SetScaleRoot(Transform scaleRoot)
	{
		_scaleRoot = scaleRoot;
	}

	public virtual void SetVelocity(Vector3 newVelocity)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}
		else
		{
			_rigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
		}
		_lastVelocity = _currentVelocity;
		_currentVelocity = newVelocity;
	}

	public void SetAngularVelocity(Vector3 newAngularVelocity)
	{
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.angularVelocity = newAngularVelocity;
		}
		else
		{
			_rigidbody.angularVelocity = newAngularVelocity;
		}
	}

	public void AddRotation(Quaternion deltaRotation)
	{
		_rigidbody.MoveRotation(deltaRotation * _rigidbody.rotation);
	}

	public void MoveToPosition(Vector3 worldPosition)
	{
		_rigidbody.MovePosition(worldPosition);
	}

	public void MoveToRotation(Quaternion rotation)
	{
		_rigidbody.MoveRotation(rotation);
	}

	public void TranslateWithPhysics(Vector3 deltaTranslation)
	{
		SetVelocity(Vector3.zero);
		AddVelocityChange(deltaTranslation / Time.fixedDeltaTime);
	}

	public void RotateWithPhysics(Vector3 deltaRadians)
	{
		SetAngularVelocity(Vector3.zero);
		AddAngularVelocityChange(deltaRadians / Time.fixedDeltaTime);
	}

	public Vector3 GetVelocity()
	{
		return _currentVelocity;
	}

	public virtual Vector3 GetVelocity_Internal()
	{
		if (!RunningKinematicSimulation())
		{
			return _rigidbody.velocity;
		}
		return _kinematicRigidbody.velocity;
	}

	public Vector3 GetRelativeVelocity(OWRigidbody relativeBody)
	{
		return relativeBody.GetVelocity() - GetVelocity();
	}

	public Vector3 GetRelativeVelocity(ReferenceFrame referenceFrame)
	{
		return referenceFrame.GetVelocity() - GetVelocity();
	}

	public Vector3 GetAcceleration()
	{
		return (_currentVelocity - _lastVelocity) / Time.fixedDeltaTime;
	}

	public Vector3 GetVelocityChange()
	{
		return _currentVelocity - _lastVelocity;
	}

	public Vector3 GetRelativeAcceleration(OWRigidbody relativeBody)
	{
		return relativeBody.GetAcceleration() - GetAcceleration();
	}

	public Vector3 GetAngularVelocity()
	{
		return _currentAngularVelocity;
	}

	public Vector3 GetAngularAcceleration()
	{
		return (_currentAngularVelocity - _lastAngularVelocity) / Time.fixedDeltaTime;
	}

	public Vector3 GetJerk()
	{
		return (_currentAccel - _lastAccel) / Time.fixedDeltaTime;
	}

	public Vector3 GetPointVelocity(Vector3 worldPoint)
	{
		return GetPointTangentialVelocity(worldPoint) + GetVelocity();
	}

	public Vector3 GetPointTangentialVelocity(Vector3 worldPoint)
	{
		Vector3 angularVelocity = GetAngularVelocity();
		Vector3 rhs = worldPoint - GetWorldCenterOfMass();
		return Vector3.Cross(angularVelocity, rhs);
	}

	public Vector3 GetPointAcceleration(Vector3 worldPoint)
	{
		Vector3 angularVelocity = GetAngularVelocity();
		Vector3 rhs = worldPoint - GetWorldCenterOfMass();
		return GetAcceleration() + Vector3.Cross(angularVelocity, Vector3.Cross(angularVelocity, rhs)) + Vector3.Cross(GetAngularAcceleration(), rhs);
	}

	public Vector3 GetPointTangentialAcceleration(Vector3 worldPoint)
	{
		Vector3 rhs = worldPoint - GetWorldCenterOfMass();
		return Vector3.Cross(GetAngularAcceleration(), rhs);
	}

	public Vector3 GetPointCentripetalAcceleration(Vector3 worldPoint)
	{
		Vector3 angularVelocity = GetAngularVelocity();
		Vector3 rhs = worldPoint - GetWorldCenterOfMass();
		return Vector3.Cross(angularVelocity, Vector3.Cross(angularVelocity, rhs));
	}

	public Transform GetOrigParent()
	{
		return _origParent;
	}

	public OWRigidbody GetOrigParentBody()
	{
		return _origParentBody;
	}

	public Rigidbody GetRigidbody()
	{
		return _rigidbody;
	}

	public Vector3 GetPosition()
	{
		return _rigidbody.position;
	}

	public Vector3 GetLastPosition()
	{
		return _lastPosition;
	}

	public Quaternion GetRotation()
	{
		return _rigidbody.rotation;
	}

	public Vector3 GetLocalScale()
	{
		return _scaleRoot.localScale;
	}

	public Vector3 GetLocalUpDirection()
	{
		return _rigidbody.rotation * Vector3.up;
	}

	public float GetMass()
	{
		return _rigidbody.mass;
	}

	public void SetMass(float mass)
	{
		_rigidbody.mass = mass;
	}

	public Vector3 GetWorldCenterOfMass()
	{
		if (!RunningKinematicSimulation())
		{
			return _rigidbody.worldCenterOfMass;
		}
		return _kinematicRigidbody.worldCenterOfMass;
	}

	public Vector3 GetCenterOfMass()
	{
		if (!RunningKinematicSimulation())
		{
			return _rigidbody.centerOfMass;
		}
		return _kinematicRigidbody.centerOfMass;
	}

	public void SetCenterOfMass(Vector3 centerOfMass)
	{
		_origCenterOfMass = centerOfMass;
		if (RunningKinematicSimulation())
		{
			_kinematicRigidbody.centerOfMass = centerOfMass;
		}
		else
		{
			_rigidbody.centerOfMass = centerOfMass;
		}
	}

	public bool IsKinematic()
	{
		return _rigidbody.isKinematic;
	}

	public bool HasKinematicStateNewlyChanged()
	{
		return _kinematicStateNewlyChanged;
	}

	public bool IsSimulatedKinematic()
	{
		return _kinematicSimulation;
	}

	public bool RunningKinematicSimulation()
	{
		if (IsKinematic())
		{
			return IsSimulatedKinematic();
		}
		return false;
	}

	public void EnableKinematicSimulation()
	{
		if (_kinematicRigidbody == null)
		{
			_kinematicRigidbody = base.gameObject.GetAddComponent<KinematicRigidbody>();
		}
		_kinematicRigidbody.enabled = true;
		_kinematicSimulation = true;
	}

	public void DisableKinematicSimulation()
	{
		if ((bool)_kinematicRigidbody)
		{
			_kinematicRigidbody.enabled = false;
		}
		_kinematicSimulation = false;
	}

	public bool IsTargetable()
	{
		return _isTargetable;
	}

	public void SetIsTargetable(bool targetable)
	{
		_isTargetable = targetable;
	}

	public ReferenceFrame GetReferenceFrame()
	{
		if (!_attachedRFVolume)
		{
			return _referenceFrame;
		}
		return _attachedRFVolume.GetReferenceFrame();
	}

	public Sector GetSimulateInSector()
	{
		return _simulateInSector;
	}

	public void FreezePosition()
	{
		_rigidbody.constraints |= RigidbodyConstraints.FreezePosition;
	}

	public void UnfreezePosition()
	{
		_rigidbody.constraints &= (RigidbodyConstraints)(-15);
	}

	public void FreezeRotation()
	{
		_rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
	}

	public void UnfreezeRotation()
	{
		_rigidbody.constraints &= (RigidbodyConstraints)(-113);
	}

	public void SetMaxAngularVelocity(float maxAngularVelocity)
	{
		_rigidbody.maxAngularVelocity = maxAngularVelocity;
	}

	public void MakeNonKinematic()
	{
		_rigidbody.isKinematic = false;
		_kinematicStateNewlyChanged = true;
	}

	public void MakeKinematic()
	{
		_rigidbody.isKinematic = true;
		_kinematicStateNewlyChanged = true;
	}

	public void EnableCollisionDetection()
	{
		_rigidbody.detectCollisions = true;
	}

	public void DisableCollisionDetection()
	{
		_rigidbody.detectCollisions = false;
	}

	public virtual void ManagedFixedUpdate(float invFixedDeltaTime, Vector3 cotuStaticFrameVel)
	{
		bool flag = _kinematicSimulation && _rigidbody.isKinematic;
		UpdateCenterOfMass();
		_lastPosition = _rigidbody.position;
		_lastVelocity = _currentVelocity;
		_currentVelocity = (flag ? _kinematicRigidbody.velocity : _rigidbody.velocity);
		_currentVelocity -= cotuStaticFrameVel;
		_lastAngularVelocity = _currentAngularVelocity;
		_currentAngularVelocity = (flag ? _kinematicRigidbody.angularVelocity : _rigidbody.angularVelocity);
		_lastAccel = _currentAccel;
		_currentAccel = (_currentVelocity - _lastVelocity) * invFixedDeltaTime;
		if (_unsuspendNextUpdate)
		{
			UnsuspendImmediate(_restoreCachedVelocityOnUnsuspend);
			_unsuspendNextUpdate = false;
		}
		_kinematicStateNewlyChanged = false;
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 center = ((!_autoGenerateCenterOfMass) ? base.transform.TransformPoint(_centerOfMass) : (_rigidbody ? GetWorldCenterOfMass() : GetComponent<Rigidbody>().worldCenterOfMass));
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(center, 0.2f);
	}
}
