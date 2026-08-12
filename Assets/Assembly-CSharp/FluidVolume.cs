using UnityEngine;

public abstract class FluidVolume : PriorityVolume
{
	public enum Type
	{
		NONE = 0,
		AIR = 1,
		WATER = 2,
		TRACTOR_BEAM = 3,
		CLOUD = 4,
		SAND = 5,
		PLASMA = 6,
		FOG = 7,
		GEYSER = 8
	}

	public const int NUM_FLUID_TYPES = 9;

	[SerializeField]
	protected float _density = 1.2f;

	[SerializeField]
	protected Type _fluidType;

	[SerializeField]
	protected bool _alignmentFluid = true;

	[SerializeField]
	protected bool _allowShipAutoroll;

	[SerializeField]
	private bool _disableOnStart;

	protected virtual void Start()
	{
		if (_disableOnStart)
		{
			SetVolumeActivation(active: false);
		}
	}

	public virtual Vector3 GetImpactVelocity(OWRigidbody impactingBody)
	{
		return _attachedBody.GetRelativeVelocity(impactingBody);
	}

	public virtual Vector3 GetSplashAlignment(Vector3 impactPos, Vector3 impactDir)
	{
		if (!IsSpherical())
		{
			return -impactDir;
		}
		return impactPos - base.transform.position;
	}

	public virtual bool IsSpherical()
	{
		return _fluidType == Type.PLASMA;
	}

	public virtual bool PreventPlayerGrounded()
	{
		return false;
	}

	public FluidDetector GetAttachedFluidDetector()
	{
		return _attachedBody.GetAttachedFluidDetector();
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		FluidDetector component = hitObj.GetComponent<FluidDetector>();
		if (component != null)
		{
			component.AddVolume(this);
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		FluidDetector component = hitObj.GetComponent<FluidDetector>();
		if (component != null)
		{
			component.RemoveVolume(this);
		}
	}

	public virtual float GetPointDensity(Vector3 worldPosition, FluidDetector detector)
	{
		return _density;
	}

	public Type GetFluidType()
	{
		return _fluidType;
	}

	public bool IsAtmosphereFluid()
	{
		if (_fluidType != Type.AIR && _fluidType != Type.CLOUD && _fluidType != Type.FOG)
		{
			return _fluidType == Type.SAND;
		}
		return true;
	}

	public bool IsAlignmentFluid()
	{
		return _alignmentFluid;
	}

	public bool AllowShipAutoroll()
	{
		return _allowShipAutoroll;
	}

	public virtual bool IsInheritible()
	{
		return false;
	}

	public virtual bool CheckTriggerProbeDragIncrease(FluidDetector probeDetector)
	{
		Vector3 vector = GetPointFluidVelocity(probeDetector.transform.position, probeDetector) - _attachedBody.GetPointVelocity(probeDetector.transform.position);
		return GetPointDensity(probeDetector.transform.position, probeDetector) * vector.magnitude > 50f;
	}

	public virtual Vector3 GetPointFluidVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _attachedBody.GetPointVelocity(worldPosition);
	}

	public virtual void RegisterSplashTransform(Transform splashTransform)
	{
	}

	public virtual Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
	{
		return Vector3.zero;
	}

	public virtual float GetFractionSubmerged(FluidDetector detector)
	{
		return 1f;
	}

	public virtual Vector3 GetPointFluidAngularVelocity(Vector3 worldPosition, FluidDetector detector)
	{
		return _attachedBody.GetAngularVelocity();
	}

	public virtual float GetDepthAtPosition(Vector3 worldPosition)
	{
		Debug.LogError("Depth check not supported by this fluid!");
		Debug.Break();
		return 0f;
	}
}
