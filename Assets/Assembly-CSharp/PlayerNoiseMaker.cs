using UnityEngine;

public class PlayerNoiseMaker : NoiseMaker
{
	private AnimationCurve _thrustNoiseRadius = AnimationCurve.Linear(0f, 0f, 6f, 200f);

	private float _probeLaunchNoiseRadius = 250f;

	private ThrusterModel _thrusterModel;

	private float _lastLaunchTime;

	protected override void Awake()
	{
		base.Awake();
		_thrusterModel = _attachedBody.GetRequiredComponent<ThrusterModel>();
		GlobalMessenger<SurveyorProbe>.AddListener("LaunchProbe", OnLaunchProbe);
	}

	private void OnDestroy()
	{
		GlobalMessenger<SurveyorProbe>.RemoveListener("LaunchProbe", OnLaunchProbe);
	}

	private void OnLaunchProbe(SurveyorProbe probe)
	{
		_lastLaunchTime = Time.time;
	}

	private void Update()
	{
		float b = ((Time.time > _lastLaunchTime + 1f) ? 0f : _probeLaunchNoiseRadius);
		float a = _thrustNoiseRadius.Evaluate(_thrusterModel.GetLocalAcceleration().magnitude);
		_noiseRadius = Mathf.Max(a, b);
	}
}
