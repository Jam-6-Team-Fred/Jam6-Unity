using UnityEngine;

public class ShipNoiseMaker : NoiseMaker
{
	private const float MAX_IMPACT_SPEED = 50f;

	private const float MAX_IMPACT_NOISE_RADIUS = 800f;

	private const float MAX_THRUST_ACCELERATION = 20f;

	private const float MAX_THRUST_NOISE_RADIUS = 400f;

	private ImpactSensor _impactSensor;

	private ThrusterModel _thrusterModel;

	private float _impactNoiseRadius;

	private float _lastImpactTime;

	public override void OnFogWarp()
	{
		base.OnFogWarp();
		_impactNoiseRadius = 0f;
		_lastImpactTime = 0f;
	}

	protected override void Awake()
	{
		base.Awake();
		_impactSensor = _attachedBody.GetRequiredComponent<ImpactSensor>();
		_thrusterModel = _attachedBody.GetRequiredComponent<ThrusterModel>();
		_impactSensor.OnImpact += OnImpact;
	}

	private void OnDestroy()
	{
		_impactSensor.OnImpact -= OnImpact;
	}

	private void OnImpact(ImpactData impactData)
	{
		float num = Mathf.Lerp(0f, 800f, Mathf.InverseLerp(0f, 50f, impactData.speed));
		if (num > 0f && num > _impactNoiseRadius)
		{
			_impactNoiseRadius = num;
			_lastImpactTime = Time.time;
		}
	}

	private void Update()
	{
		if (Time.time > _lastImpactTime + 1f)
		{
			_impactNoiseRadius = 0f;
		}
		float a = Mathf.Lerp(0f, 400f, Mathf.InverseLerp(0f, 20f, _thrusterModel.GetLocalAcceleration().magnitude));
		_noiseRadius = Mathf.Max(a, _impactNoiseRadius);
	}
}
