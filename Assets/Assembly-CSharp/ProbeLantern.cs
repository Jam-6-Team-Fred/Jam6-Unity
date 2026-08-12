using UnityEngine;

[RequireComponent(typeof(OWLight2))]
public class ProbeLantern : MonoBehaviour
{
	[SerializeField]
	private float _fadeInDuration = 2f;

	[SerializeField]
	private AnimationCurve _fadeInCurve;

	[SerializeField]
	private AnimationCurve _fadeOutCurve;

	[Space(10f)]
	[SerializeField]
	private OWEmissiveRenderer _emissiveRenderer;

	private SurveyorProbe _probe;

	private OWLight2 _light;

	private float _originalRange;

	private float _fadeFraction;

	private float _targetFade;

	private float _startFade;

	private float _startFadeTime;

	private float _fadeDuration;

	private void Awake()
	{
		_probe = this.GetAttachedOWRigidbody().GetRequiredComponent<SurveyorProbe>();
		_light = GetComponent<OWLight2>();
		_originalRange = _light.range;
		_probe.OnAnchorProbe += OnProbeAnchorToSurface;
		_probe.OnStartRetrieveProbe += OnStartRetrieveProbe;
		_probe.OnRetrieveProbe += OnFinishRetrieveProbe;
	}

	private void Start()
	{
		if (_emissiveRenderer != null)
		{
			_emissiveRenderer.SetEmissiveScale(0f);
		}
		_light.SetActivation(active: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_probe.OnAnchorProbe -= OnProbeAnchorToSurface;
		_probe.OnStartRetrieveProbe -= OnStartRetrieveProbe;
		_probe.OnRetrieveProbe -= OnFinishRetrieveProbe;
	}

	private void Update()
	{
		AnimationCurve animationCurve = ((_targetFade > 0f) ? _fadeInCurve : _fadeOutCurve);
		float num = Mathf.InverseLerp(_startFadeTime, _startFadeTime + _fadeDuration, Time.time);
		_fadeFraction = Mathf.Lerp(_startFade, _targetFade, animationCurve.Evaluate(num));
		ProbeRuleset probeRuleSet = _probe.GetRulesetDetector().GetProbeRuleSet();
		float num2 = ((probeRuleSet != null && probeRuleSet.GetOverrideLanternRange()) ? probeRuleSet.GetLanternRange() : _originalRange);
		_light.range = num2 * _fadeFraction;
		if (_emissiveRenderer != null)
		{
			_emissiveRenderer.SetEmissiveScale(_fadeFraction);
		}
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}

	private void FadeTo(float fade, float duration)
	{
		_startFadeTime = Time.time;
		_fadeDuration = duration;
		_startFade = _fadeFraction;
		_targetFade = fade;
		base.enabled = true;
	}

	private void OnProbeAnchorToSurface()
	{
		if (!_probe.IsRetrieving())
		{
			_light.SetActivation(active: true);
			_light.range = 0f;
			FadeTo(1f, _fadeInDuration);
		}
	}

	private void OnStartRetrieveProbe(float retrieveLength)
	{
		FadeTo(0f, retrieveLength);
	}

	private void OnFinishRetrieveProbe()
	{
		_light.SetActivation(active: false);
		_light.range = 0f;
		_fadeFraction = 0f;
		base.enabled = false;
	}
}
