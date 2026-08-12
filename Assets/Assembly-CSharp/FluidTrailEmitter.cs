using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FluidTrailEmitter : MonoBehaviour
{
	private ParticleSystem _particleSystem;

	private ParticleSystem.MainModule _particleSystemMain;

	[SerializeField]
	private FluidDetector _fluidDetector;

	[SerializeField]
	private FluidVolume.Type _fluidType = FluidVolume.Type.AIR;

	[SerializeField]
	private float _speedScale = 1f;

	[SerializeField]
	private float _emissionRate = 10f;

	[SerializeField]
	private AnimationCurve _emissionRateDensityScale = AnimationCurve.Linear(0f, 0f, 1.2f, 1f);

	[SerializeField]
	private AnimationCurve _emissionRateFluidSpeedScale = AnimationCurve.Linear(0f, 0f, 10f, 1f);

	[SerializeField]
	private bool _startLifetimeByDensity;

	[SerializeField]
	private AnimationCurve _startLifetimeByDensityCurve = AnimationCurve.Linear(0f, 0f, 1.2f, 1f);

	[SerializeField]
	private bool _startLifetimeByFluidSpeed;

	[SerializeField]
	private AnimationCurve _startLifetimeByFluidSpeedCurve = AnimationCurve.Linear(0f, 0f, 1.2f, 1f);

	[SerializeField]
	private bool _startColorByDensity;

	[SerializeField]
	private float _startColorByDensityMin;

	[SerializeField]
	private float _startColorByDensityMax = 1.2f;

	[SerializeField]
	private Gradient _startColorByDensityGradient = new Gradient();

	[SerializeField]
	private bool _startColorByFluidSpeed;

	[SerializeField]
	private float _startColorByFluidSpeedMin;

	[SerializeField]
	private float _startColorByFluidSpeedMax = 10f;

	[SerializeField]
	private Gradient _startColorByFluidSpeedGradient = new Gradient();

	private float _emitAccumulator;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		_particleSystemMain = _particleSystem.main;
		_emitAccumulator = 0f;
	}

	private void Start()
	{
		_particleSystemMain.simulationSpace = ParticleSystemSimulationSpace.Custom;
		_particleSystemMain.customSimulationSpace = Locator.GetSunTransform();
	}

	private void FixedUpdate()
	{
		bool flag = PlayerState.InCloakingField() || PlayerState.InDreamWorld();
		float fixedDeltaTime = Time.fixedDeltaTime;
		if (_fluidDetector.InFluidType(_fluidType) && !flag)
		{
			float fluidDensity = _fluidDetector.GetFluidDensity(_fluidType);
			Vector3 relativeFluidVelocity = _fluidDetector.GetRelativeFluidVelocity(_fluidType);
			float magnitude = relativeFluidVelocity.magnitude;
			Vector3 velocity = _fluidDetector.GetAttachedOWRigidbody().GetVelocity();
			float emissionRate = _emissionRate;
			emissionRate *= _emissionRateDensityScale.Evaluate(fluidDensity);
			emissionRate *= _emissionRateFluidSpeedScale.Evaluate(magnitude);
			emissionRate *= fixedDeltaTime;
			_emitAccumulator = Mathf.Clamp(_emitAccumulator + emissionRate, 0f, fixedDeltaTime * 1000f);
			int num = Mathf.FloorToInt(_emitAccumulator);
			if (num > 0)
			{
				Vector3 vector = _particleSystem.transform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
				Vector3 b = vector + relativeFluidVelocity * fixedDeltaTime;
				ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
				emitParams.applyShapeToPosition = true;
				emitParams.velocity = velocity + relativeFluidVelocity * _speedScale;
				float num2 = _particleSystemMain.startLifetime.constant;
				if (_startLifetimeByDensity)
				{
					num2 *= _startLifetimeByDensityCurve.Evaluate(fluidDensity);
				}
				if (_startLifetimeByFluidSpeed)
				{
					num2 *= _startLifetimeByFluidSpeedCurve.Evaluate(magnitude);
				}
				Color color = _particleSystemMain.startColor.color;
				if (_startColorByDensity)
				{
					color *= _startColorByDensityGradient.Evaluate(Mathf.InverseLerp(_startColorByDensityMin, _startColorByDensityMax, fluidDensity));
				}
				if (_startColorByFluidSpeed)
				{
					color *= _startColorByFluidSpeedGradient.Evaluate(Mathf.InverseLerp(_startColorByFluidSpeedMin, _startColorByFluidSpeedMax, fluidDensity));
				}
				for (int i = 0; i < num; i++)
				{
					float t = (float)i / (float)num;
					emitParams.position = Vector3.Lerp(vector, b, t);
					if (_startLifetimeByDensity || _startLifetimeByFluidSpeed)
					{
						emitParams.startLifetime = num2;
					}
					if (_startColorByDensity || _startColorByFluidSpeed)
					{
						emitParams.startColor = color;
					}
					_particleSystem.Emit(emitParams, 1);
				}
				_emitAccumulator -= num;
			}
			_fluidDetector.MakeDirty();
		}
		if (_particleSystem.particleCount > 0)
		{
			_particleSystem.Simulate(fixedDeltaTime, withChildren: true, restart: false, fixedTimeStep: true);
		}
	}

	public ParticleSystem GetParticleSystem()
	{
		return _particleSystem;
	}
}
