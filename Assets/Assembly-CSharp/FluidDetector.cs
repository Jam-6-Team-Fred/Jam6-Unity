using UnityEngine;

public abstract class FluidDetector : PriorityDetector
{
	public delegate void SpawnSplashEvent(FluidVolume splashFluid);

	public delegate void FluidTypeEvent(FluidVolume.Type fluidType);

	public delegate void FluidEvent(FluidVolume volume);

	[SerializeField]
	protected BuoyancyData _buoyancy = new BuoyancyData();

	[SerializeField]
	private SplashEffect[] _splashEffects = new SplashEffect[0];

	[SerializeField]
	protected bool _dontApplyForces;

	[SerializeField]
	private Transform _splashSpawnRoot;

	private FluidTypeData[] _fluidDataByType;

	protected const float DRAG_FACTOR_CONVERSION = 0.00392f;

	protected const float ANGULAR_DRAG_FACTOR_CONVERSION = 0.1f;

	protected Vector3 _netLinearAcceleration = Vector3.zero;

	protected Vector3 _netAngularAcceleration = Vector3.zero;

	protected Vector3 _netInheritibleLinearAcceleration = Vector3.zero;

	protected Vector3 _netRelativeFluidVelocity = Vector3.zero;

	protected Vector3 _netFluidAngularVelocity = Vector3.zero;

	protected float _netFluidDensity;

	protected OWRigidbody _owRigidbody;

	private Vector3 _alignmentFluidVelocity;

	private bool _dirty;

	public event SpawnSplashEvent OnSpawnSplashEvent;

	public event FluidTypeEvent OnEnterFluidType;

	public event FluidTypeEvent OnExitFluidType;

	public event FluidEvent OnEnterFluid;

	public event FluidEvent OnExitFluid;

	private void OnValidate()
	{
		Rigidbody componentInParent = GetComponentInParent<Rigidbody>();
		if (componentInParent != null && componentInParent.drag > 0f)
		{
			componentInParent.drag = 0f;
		}
		if (componentInParent != null && componentInParent.angularDrag > 0f)
		{
			componentInParent.angularDrag = 0f;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (!_dontApplyForces)
		{
			base.gameObject.GetAddComponent<ForceApplier>();
		}
		_owRigidbody = base.gameObject.GetAttachedOWRigidbody();
		if (!_dontApplyForces)
		{
			_owRigidbody.RegisterAttachedFluidDetector(this);
		}
		_fluidDataByType = new FluidTypeData[9];
		if (_splashSpawnRoot == null)
		{
			_splashSpawnRoot = _owRigidbody.transform;
		}
	}

	private void OnEnable()
	{
		FixedEarlyUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedEarlyUpdateManager.Unregister(this);
	}

	public bool CheckPreventPlayerGrounded()
	{
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			if (((FluidVolume)_activeVolumes[i]).PreventPlayerGrounded())
			{
				return true;
			}
		}
		return false;
	}

	public bool GetAppliesForces()
	{
		return !_dontApplyForces;
	}

	public virtual bool AffectsRumble()
	{
		if (_name != Name.Player)
		{
			if (_name == Name.Ship)
			{
				return PlayerState.IsInsideShip();
			}
			return false;
		}
		return true;
	}

	public void MakeDirty()
	{
		_dirty = true;
	}

	public Vector3 GetLinearFluidAcceleration()
	{
		if (_dirty)
		{
			AccumulateFluidAcceleration();
			_dirty = false;
		}
		return _netLinearAcceleration;
	}

	public Vector3 GetInheritibleLinearFluidAcceleration()
	{
		if (_dirty)
		{
			AccumulateFluidAcceleration();
			_dirty = false;
		}
		return _netInheritibleLinearAcceleration;
	}

	public Vector3 GetAngularFluidAcceleration()
	{
		if (_dirty)
		{
			AccumulateFluidAcceleration();
			_dirty = false;
		}
		return _netAngularAcceleration;
	}

	public bool InFluidType(FluidVolume.Type fluidType)
	{
		return _fluidDataByType[(int)fluidType].count > 0;
	}

	public BuoyancyData GetBuoyancyData()
	{
		return _buoyancy;
	}

	public OWRigidbody GetAttachedOWRigidbody()
	{
		return _owRigidbody;
	}

	public Vector3 GetAlignmentFluidVelocity()
	{
		return _alignmentFluidVelocity;
	}

	public Vector3 GetRelativeFluidVelocity()
	{
		return _netRelativeFluidVelocity;
	}

	public Vector3 GetRelativeFluidVelocity(FluidVolume.Type fluidType)
	{
		if (_fluidDataByType[(int)fluidType].count > 0)
		{
			return _fluidDataByType[(int)fluidType].relativeVelocity;
		}
		return Vector3.zero;
	}

	public Vector3 GetFluidAngularVelocity()
	{
		return _netFluidAngularVelocity;
	}

	public float GetFluidDensity()
	{
		return _netFluidDensity;
	}

	public float GetFluidDensity(FluidVolume.Type fluidType)
	{
		if (_fluidDataByType[(int)fluidType].count > 0)
		{
			return _fluidDataByType[(int)fluidType].density;
		}
		return 0f;
	}

	public override void AddVolume(EffectVolume eVol)
	{
		FluidVolume fluidVolume = eVol as FluidVolume;
		if (fluidVolume != null && (!fluidVolume.IsInheritible() || !_dontApplyForces))
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		FluidVolume fluidVolume = eVol as FluidVolume;
		if (fluidVolume != null && (!fluidVolume.IsInheritible() || !_dontApplyForces))
		{
			base.RemoveVolume(eVol);
		}
	}

	protected void ResetAccumulators()
	{
		_netLinearAcceleration = (_netAngularAcceleration = (_netInheritibleLinearAcceleration = Vector3.zero));
	}

	protected virtual void AccumulateFluidAcceleration()
	{
		ResetAccumulators();
		for (int i = 0; i < _activeVolumes.Count; i++)
		{
			FluidVolume fluidVolume = _activeVolumes[i] as FluidVolume;
			float fractionSubmerged = fluidVolume.GetFractionSubmerged(this);
			AddDrag(fluidVolume, fractionSubmerged);
			AddBuoyancy(fluidVolume, fractionSubmerged);
		}
	}

	protected virtual void AddDrag(FluidVolume fluidVolume, float fractionSubmerged)
	{
		float pointDensity = fluidVolume.GetPointDensity(base.transform.position, this);
		_netFluidDensity += pointDensity;
		AddLinearDrag(fluidVolume, pointDensity, fractionSubmerged);
		AddAngularDrag(fluidVolume, pointDensity);
		if (fluidVolume.IsInheritible())
		{
			_netLinearAcceleration += fluidVolume.GetAttachedFluidDetector().GetLinearFluidAcceleration();
		}
	}

	protected void AddBuoyancy(FluidVolume fluidVolume, float fractionSubmerged)
	{
		Vector3 buoyancy = fluidVolume.GetBuoyancy(this, fractionSubmerged);
		_netLinearAcceleration += buoyancy;
	}

	protected abstract float CalculateDragFactor(Vector3 relativeFluidVelocity);

	protected abstract float CalculateAngularDragFactor(Vector3 relativeAngularVelocity);

	private void AddLinearDrag(FluidVolume fluidVolume, float fluidDensity, float fractionSubmerged)
	{
		Vector3 vector = fluidVolume.GetPointFluidVelocity(base.transform.position, this) - _owRigidbody.GetVelocity();
		_netRelativeFluidVelocity += vector;
		_fluidDataByType[(int)fluidVolume.GetFluidType()].relativeVelocity += vector;
		_fluidDataByType[(int)fluidVolume.GetFluidType()].density += fluidDensity;
		if (fluidVolume.IsAlignmentFluid())
		{
			_alignmentFluidVelocity += vector;
		}
		Vector3 vector2 = CalculateDragVelocityChange(vector, fluidDensity, fractionSubmerged) / Time.fixedDeltaTime;
		_netLinearAcceleration += vector2;
	}

	private Vector3 CalculateDragVelocityChange(Vector3 relativeFluidVelocity, float fluidDensity, float fractionSubmerged = 1f)
	{
		float num = CalculateDragFactor(relativeFluidVelocity);
		if (fractionSubmerged < 1f)
		{
			num *= _buoyancy.dragCurve.Evaluate(fractionSubmerged);
		}
		Vector3 vector = 0.5f * fluidDensity * relativeFluidVelocity.sqrMagnitude * num * 0.00392f * relativeFluidVelocity.normalized;
		float magnitude = relativeFluidVelocity.magnitude;
		float a = vector.magnitude * Time.fixedDeltaTime;
		a = Mathf.Min(a, magnitude);
		return relativeFluidVelocity.normalized * a;
	}

	private void AddAngularDrag(FluidVolume fluidVolume, float fluidDensity)
	{
		Vector3 vector = _owRigidbody.GetAngularVelocity() - fluidVolume.GetPointFluidAngularVelocity(base.transform.position, this);
		_netFluidAngularVelocity += vector;
		Vector3 vector2 = 0.5f * fluidDensity * vector.sqrMagnitude * CalculateAngularDragFactor(vector) * 0.1f * vector.normalized;
		float magnitude = vector.magnitude;
		float a = vector2.magnitude * Time.fixedDeltaTime;
		a = Mathf.Min(a, magnitude);
		Vector3 vector3 = -vector.normalized * a;
		_netAngularAcceleration += vector3 / Time.fixedDeltaTime;
	}

	public virtual void ManagedFixedUpdate()
	{
		_alignmentFluidVelocity = Vector3.zero;
		_netRelativeFluidVelocity = Vector3.zero;
		_netFluidAngularVelocity = Vector3.zero;
		_netFluidDensity = 0f;
		for (int i = 0; i < _fluidDataByType.Length; i++)
		{
			_fluidDataByType[i].ResetAccumulators();
		}
		if (_dirty)
		{
			AccumulateFluidAcceleration();
			_dirty = false;
		}
	}

	protected override void OnVolumeActivated(PriorityVolume volume)
	{
		FluidVolume fluidVolume = volume as FluidVolume;
		FluidVolume.Type fluidType = fluidVolume.GetFluidType();
		_fluidDataByType[(int)fluidType].count++;
		if (this.OnEnterFluid != null)
		{
			this.OnEnterFluid(fluidVolume);
		}
		if (_fluidDataByType[(int)fluidType].count == 1)
		{
			OnEnterFluidType_Internal(fluidVolume);
			if (this.OnEnterFluidType != null)
			{
				this.OnEnterFluidType(fluidType);
			}
		}
	}

	protected override void OnVolumeDeactivated(PriorityVolume volume)
	{
		FluidVolume fluidVolume = volume as FluidVolume;
		FluidVolume.Type fluidType = fluidVolume.GetFluidType();
		_fluidDataByType[(int)fluidType].count--;
		if (this.OnExitFluid != null)
		{
			this.OnExitFluid(fluidVolume);
		}
		if (_fluidDataByType[(int)fluidType].count == 0)
		{
			OnExitFluidType_Internal(fluidVolume);
			if (this.OnExitFluidType != null)
			{
				this.OnExitFluidType(fluidType);
			}
		}
	}

	protected virtual void OnEnterFluidType_Internal(FluidVolume fluid)
	{
		SpawnSplash(fluid, SplashEffect.TriggerEvent.OnEntry);
		if (fluid is SphereOceanFluidVolume)
		{
			OceanSplasher component = GetComponent<OceanSplasher>();
			float magnitude = fluid.GetImpactVelocity(_owRigidbody).magnitude;
			if (component != null && magnitude >= component.minSplashSpeed)
			{
				component.Splash();
			}
		}
	}

	protected virtual void OnExitFluidType_Internal(FluidVolume fluid)
	{
		SpawnSplash(fluid, SplashEffect.TriggerEvent.OnExit);
	}

	private void SpawnSplash(FluidVolume fluid, SplashEffect.TriggerEvent triggerEvent)
	{
		if (_owRigidbody.HasKinematicStateNewlyChanged() || (CompareName(Name.Player) && PlayerState.IsRidingRaft(raftMustBeInWater: false) && fluid.GetFluidType() == FluidVolume.Type.WATER) || (fluid.GetFluidType() == FluidVolume.Type.CLOUD && InFluidType(FluidVolume.Type.WATER)))
		{
			return;
		}
		Vector3 impactVelocity = fluid.GetImpactVelocity(_owRigidbody);
		float magnitude = impactVelocity.magnitude;
		int num = -1;
		for (int i = 0; i < _splashEffects.Length; i++)
		{
			if (_splashEffects[i].fluidType == fluid.GetFluidType() && magnitude > _splashEffects[i].minImpactSpeed && (triggerEvent & _splashEffects[i].triggerEvent) > (SplashEffect.TriggerEvent)0 && (num == -1 || _splashEffects[i].minImpactSpeed > _splashEffects[num].minImpactSpeed))
			{
				num = i;
			}
		}
		if (num > -1)
		{
			GameObject gameObject = Object.Instantiate(_splashEffects[num].splashPrefab, rotation: Quaternion.FromToRotation(toDirection: _splashEffects[num].ignoreSphereAligment ? (-impactVelocity) : fluid.GetSplashAlignment(_splashSpawnRoot.position, impactVelocity), fromDirection: _splashSpawnRoot.up) * _splashSpawnRoot.rotation, position: _splashSpawnRoot.position);
			if (gameObject.GetComponent<OWRigidbody>() != null)
			{
				Debug.LogError("SPLASHES SHOULD NO LONGER HAVE RIGIDBODIES!!!", gameObject);
				gameObject.GetComponent<OWRigidbody>().MakeKinematic();
			}
			gameObject.transform.parent = fluid.transform;
			fluid.RegisterSplashTransform(gameObject.transform);
			SplashAudioController component = gameObject.GetComponent<SplashAudioController>();
			if (component != null)
			{
				component.PlaySplash();
			}
			if (this.OnSpawnSplashEvent != null)
			{
				this.OnSpawnSplashEvent(fluid);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_buoyancy.boundingRadius > 0f)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(base.transform.position, _buoyancy.boundingRadius);
		}
	}
}
