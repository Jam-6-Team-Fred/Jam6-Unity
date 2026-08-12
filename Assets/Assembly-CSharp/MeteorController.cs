using UnityEngine;

public class MeteorController : MonoBehaviour
{
	[SerializeField]
	private float _minDamage = 20f;

	[SerializeField]
	private float _maxDamage = 80f;

	[Space]
	[SerializeField]
	private ConstantForceDetector _constantForceDetector;

	[SerializeField]
	private ConstantFluidDetector _constantFluidDetector;

	[SerializeField]
	private DynamicFluidDetector _dynamicFluidDetector;

	[Space]
	[SerializeField]
	private Renderer _intactRenderer;

	[SerializeField]
	private Light _glowLight;

	[SerializeField]
	private FluidTrailEmitter _smokeTrail;

	[SerializeField]
	private float _nonCollisionDuration = 1.5f;

	[Space]
	[SerializeField]
	private float _ambientCoolTime = 10f;

	[SerializeField]
	private float _impactedCoolTime = 5f;

	[SerializeField]
	private float _waterCoolTime = 3f;

	[SerializeField]
	private float _atmoEntryHeatScale = 10f;

	[SerializeField]
	private float _lightFadeTime = 1f;

	[Space]
	[SerializeField]
	private float _impactSuspendDelay = 5f;

	[SerializeField]
	private ParticleSystem[] _impactParticles = new ParticleSystem[0];

	[SerializeField]
	private Light _impactLight;

	[SerializeField]
	private AnimationCurve _impactLightCurve;

	[Space]
	[SerializeField]
	private OWAudioSource _impactSource;

	private OWRigidbody _owRigidbody;

	private OWCollider[] _owColliders;

	private FluidDetector _primaryFluidDetector;

	private Transform _suspendRoot;

	private bool _suspended;

	private bool _hasLaunched;

	private float _launchTime;

	private bool _ignoringCollisions;

	private bool _hasImpacted;

	private float _impactTime;

	private float _heat;

	private float _lightStartIntensity;

	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_Glow;

	private static int s_propID_GlowWidth;

	public OWRigidbody owRigidbody => _owRigidbody;

	public bool isSuspended => _suspended;

	public bool hasLaunched => _hasLaunched;

	public bool isIgnoringCollisions => _ignoringCollisions;

	public bool hasImpacted => _hasImpacted;

	public float heat => _heat;

	private void Awake()
	{
		_owRigidbody = this.GetAttachedOWRigidbody();
		_owColliders = GetComponentsInChildren<OWCollider>();
		_primaryFluidDetector = ((_dynamicFluidDetector != null) ? _dynamicFluidDetector : _constantFluidDetector);
		_suspended = false;
		_hasLaunched = false;
		_ignoringCollisions = false;
		_hasImpacted = false;
		_lightStartIntensity = _glowLight.intensity;
		_impactLight.enabled = false;
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_Glow = Shader.PropertyToID("_Glow");
			s_propID_GlowWidth = Shader.PropertyToID("_GlowWidth");
		}
	}

	public void Initialize(Transform suspendRoot, ForceVolume constantForceVolume, FluidVolume constantFluidVolume)
	{
		_suspendRoot = suspendRoot;
		if (constantForceVolume != null)
		{
			_constantForceDetector.AddConstantVolume(constantForceVolume, inheritForceAcceleration: true, clearOtherFields: true);
		}
		if (constantFluidVolume != null)
		{
			_constantFluidDetector.SetDetectableFluid(constantFluidVolume);
		}
	}

	private void OnDestroy()
	{
		FragmentSurfaceProxy.UntrackMeteor(this);
		FragmentCollisionProxy.UntrackMeteor(this);
	}

	private void Update()
	{
		if (_ignoringCollisions && Time.time - _launchTime > _nonCollisionDuration)
		{
			for (int i = 0; i < _owColliders.Length; i++)
			{
				_owColliders[i].SetActivation(active: true);
			}
			_ignoringCollisions = false;
		}
		if (_primaryFluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			_heat = Mathf.Max(_heat - Time.deltaTime / _waterCoolTime, 0f);
		}
		else if (_hasImpacted)
		{
			_heat = Mathf.Max(_heat - Time.deltaTime / _impactedCoolTime, 0f);
		}
		else if (_heat > 0.5f)
		{
			_heat = Mathf.Max(_heat - Time.deltaTime / _ambientCoolTime, 0.5f);
		}
		if (!_hasImpacted && !_primaryFluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			Vector3 vector = _primaryFluidDetector.GetRelativeFluidVelocity(FluidVolume.Type.AIR) * _primaryFluidDetector.GetFluidDensity(FluidVolume.Type.AIR);
			_heat = Mathf.Min(_heat + vector.magnitude / _atmoEntryHeatScale * Time.deltaTime, 1f);
		}
		if (_heat >= 0.5f)
		{
			float t = 1f - (_heat * 2f - 1f);
			s_matPropBlock.SetFloat(s_propID_Glow, Mathf.Lerp(1f, 0.5f, t));
			s_matPropBlock.SetFloat(s_propID_GlowWidth, Mathf.Lerp(1f, 0.5f, t));
		}
		else
		{
			float t2 = 1f - _heat * 2f;
			s_matPropBlock.SetFloat(s_propID_Glow, Mathf.Lerp(0.5f, 0f, t2));
			s_matPropBlock.SetFloat(s_propID_GlowWidth, 0.5f);
		}
		if (_hasImpacted)
		{
			float time = Time.time - _impactTime;
			_glowLight.intensity = Mathf.Max(_glowLight.intensity - Time.deltaTime / _lightFadeTime, 0f);
			_impactLight.intensity = _impactLightCurve.Evaluate(time);
		}
		else
		{
			_intactRenderer.SetPropertyBlock(s_matPropBlock);
			if (_heat < 0.5f)
			{
				_glowLight.intensity = _lightStartIntensity * (_heat * 2f);
			}
		}
		if (_hasImpacted && Time.time - _impactTime > _impactSuspendDelay)
		{
			Suspend();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Impact(collision.collider.gameObject, collision.contacts[0].point, collision.relativeVelocity);
	}

	public void Launch(Transform parent, Vector3 worldPosition, Quaternion worldRotation, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		base.gameObject.SetActive(value: true);
		base.transform.SetParent(parent);
		base.transform.SetPositionAndRotation(worldPosition, worldRotation);
		_owRigidbody.MakeNonKinematic();
		_owRigidbody.SetVelocity(linearVelocity);
		_owRigidbody.SetAngularVelocity(angularVelocity);
		for (int i = 0; i < _owColliders.Length; i++)
		{
			if (OWLayerMask.IsLayerInMask(_owColliders[i].gameObject.layer, OWLayerMask.physicalMask))
			{
				_owColliders[i].SetActivation(active: false);
			}
			else
			{
				_owColliders[i].SetActivation(active: true);
			}
		}
		_intactRenderer.enabled = true;
		_glowLight.intensity = _lightStartIntensity;
		_smokeTrail.enabled = true;
		_smokeTrail.GetParticleSystem().Play();
		_suspended = false;
		_hasLaunched = true;
		_launchTime = Time.time;
		_ignoringCollisions = true;
		_hasImpacted = false;
		_impactTime = 0f;
		_heat = 1f;
		FragmentSurfaceProxy.TrackMeteor(this);
		FragmentCollisionProxy.TrackMeteor(this);
	}

	public void Impact(GameObject hitObject, Vector3 impactPoint, Vector3 impactVel)
	{
		FragmentIntegrity componentInParent = hitObject.GetComponentInParent<FragmentIntegrity>();
		if (componentInParent != null)
		{
			float damage = Random.Range(_minDamage, _maxDamage);
			if (!componentInParent.GetIgnoreMeteorDamage())
			{
				componentInParent.AddDamage(damage);
			}
			else if (componentInParent.GetParentFragment() != null && !componentInParent.GetParentFragment().GetIgnoreMeteorDamage())
			{
				componentInParent.GetParentFragment().AddDamage(damage);
			}
		}
		MeteorImpactMapper.RecordImpact(impactPoint, componentInParent);
		_intactRenderer.enabled = false;
		_impactLight.enabled = true;
		_impactLight.intensity = _impactLightCurve.Evaluate(0f);
		Quaternion rotation = Quaternion.LookRotation(impactVel);
		for (int i = 0; i < _impactParticles.Length; i++)
		{
			_impactParticles[i].transform.rotation = rotation;
			_impactParticles[i].Play();
		}
		_impactSource.PlayOneShot(AudioType.BH_MeteorImpact);
		for (int j = 0; j < _owColliders.Length; j++)
		{
			_owColliders[j].SetActivation(active: false);
		}
		_owRigidbody.MakeKinematic();
		base.transform.SetParent(hitObject.GetAttachedOWRigidbody().transform);
		FragmentSurfaceProxy.UntrackMeteor(this);
		FragmentCollisionProxy.UntrackMeteor(this);
		_ignoringCollisions = false;
		_hasImpacted = true;
		_impactTime = Time.time;
	}

	public void Suspend(Transform newSuspendRoot)
	{
		_suspendRoot = newSuspendRoot;
		Suspend();
	}

	public void Suspend()
	{
		for (int i = 0; i < _owColliders.Length; i++)
		{
			_owColliders[i].SetActivation(active: false);
		}
		_owRigidbody.MakeKinematic();
		base.gameObject.SetActive(value: false);
		base.transform.SetParent(_suspendRoot);
		_smokeTrail.enabled = false;
		_impactLight.enabled = false;
		for (int j = 0; j < _impactParticles.Length; j++)
		{
			_impactParticles[j].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		DebrisLeash component = GetComponent<DebrisLeash>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		FragmentSurfaceProxy.UntrackMeteor(this);
		FragmentCollisionProxy.UntrackMeteor(this);
		_suspended = true;
	}
}
