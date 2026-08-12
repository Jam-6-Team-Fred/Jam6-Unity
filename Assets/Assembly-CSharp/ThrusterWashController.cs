using UnityEngine;

public class ThrusterWashController : MonoBehaviour
{
	private ThrusterModel _thrusterModel;

	[SerializeField]
	private float _raycastDistance = 10f;

	[SerializeField]
	private AnimationCurve _emissionDistanceScale = AnimationCurve.Linear(0f, 1f, 10f, 0f);

	[SerializeField]
	private AnimationCurve _emissionThrusterScale = AnimationCurve.Linear(0f, 0f, 6f, 1f);

	[SerializeField]
	private ParticleSystem _defaultParticleSystem;

	[SerializeField]
	private ParticleSystem[] _particleSystemBySurfaceType = new ParticleSystem[24];

	private ParticleSystem.MainModule _defaultMainModule;

	private ParticleSystem.EmissionModule _defaultEmissionModule;

	private float _baseDefaultEmissionRate;

	private ParticleSystem _activeSurfaceParticleSystem;

	private float[] _baseSurfaceEmissionRate;

	private void Awake()
	{
		_thrusterModel = base.gameObject.GetAttachedOWRigidbody().GetRequiredComponent<ThrusterModel>();
		_defaultMainModule = _defaultParticleSystem.main;
		_defaultEmissionModule = _defaultParticleSystem.emission;
		_baseDefaultEmissionRate = _defaultEmissionModule.rateOverTime.constant;
		_activeSurfaceParticleSystem = null;
		_baseSurfaceEmissionRate = new float[_particleSystemBySurfaceType.Length];
		for (int i = 0; i < _particleSystemBySurfaceType.Length; i++)
		{
			if (_particleSystemBySurfaceType[i] != null)
			{
				_baseSurfaceEmissionRate[i] = _particleSystemBySurfaceType[i].emission.rateOverTimeMultiplier;
			}
		}
		base.enabled = false;
		_thrusterModel.OnStartTranslationalThrust += OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust += OnStopTranslationalThrust;
	}

	private void OnDestroy()
	{
		_thrusterModel.OnStartTranslationalThrust -= OnStartTranslationalThrust;
		_thrusterModel.OnStopTranslationalThrust -= OnStopTranslationalThrust;
	}

	private void Update()
	{
		RaycastHit hitInfo = default(RaycastHit);
		bool flag = false;
		float num = _emissionThrusterScale.Evaluate(_thrusterModel.GetLocalAcceleration().y);
		if (num > 0f)
		{
			flag = Physics.Raycast(base.transform.position, base.transform.forward, out hitInfo, _raycastDistance, OWLayerMask.physicalMask);
		}
		num = (flag ? (num * _emissionDistanceScale.Evaluate(hitInfo.distance)) : 0f);
		if (num > 0f)
		{
			Vector3 position = hitInfo.point + hitInfo.normal * 0.25f;
			Quaternion rotation = Quaternion.LookRotation(hitInfo.normal);
			if (!_defaultParticleSystem.isPlaying)
			{
				_defaultParticleSystem.Play();
			}
			_defaultEmissionModule.rateOverTimeMultiplier = _baseDefaultEmissionRate * num;
			_defaultParticleSystem.transform.SetPositionAndRotation(position, rotation);
			if (_defaultMainModule.customSimulationSpace != hitInfo.transform)
			{
				_defaultMainModule.customSimulationSpace = hitInfo.transform;
				_defaultParticleSystem.Clear();
			}
			SurfaceType hitSurfaceType = Locator.GetSurfaceManager().GetHitSurfaceType(hitInfo);
			ParticleSystem particleSystem = _particleSystemBySurfaceType[(int)hitSurfaceType];
			if (particleSystem != _activeSurfaceParticleSystem)
			{
				if (_activeSurfaceParticleSystem != null)
				{
					_activeSurfaceParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
				}
				if (particleSystem != null)
				{
					particleSystem.Play();
				}
				_activeSurfaceParticleSystem = particleSystem;
			}
			if (_activeSurfaceParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission = _activeSurfaceParticleSystem.emission;
				emission.rateOverTimeMultiplier = _baseSurfaceEmissionRate[(int)hitSurfaceType] * num;
				_activeSurfaceParticleSystem.transform.position = hitInfo.point + hitInfo.normal * 0.25f;
				_activeSurfaceParticleSystem.transform.rotation = Quaternion.LookRotation(hitInfo.normal);
				ParticleSystem.MainModule main = _activeSurfaceParticleSystem.main;
				if (main.customSimulationSpace != hitInfo.transform)
				{
					main.customSimulationSpace = hitInfo.transform;
					_activeSurfaceParticleSystem.Clear();
				}
			}
		}
		else
		{
			if (_defaultParticleSystem.isPlaying)
			{
				_defaultParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
			}
			if (_activeSurfaceParticleSystem != null)
			{
				_activeSurfaceParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
				_activeSurfaceParticleSystem = null;
			}
		}
	}

	private void OnStartTranslationalThrust()
	{
		base.enabled = true;
	}

	private void OnStopTranslationalThrust()
	{
		if (_defaultParticleSystem.isPlaying)
		{
			_defaultParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
		}
		if (_activeSurfaceParticleSystem != null)
		{
			_activeSurfaceParticleSystem.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
			_activeSurfaceParticleSystem = null;
		}
		base.enabled = false;
	}
}
