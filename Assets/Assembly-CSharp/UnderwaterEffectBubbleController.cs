using UnityEngine;

public class UnderwaterEffectBubbleController : MonoBehaviour
{
	private Renderer _effectBubbleRenderer;

	private MaterialPropertyBlock _matPropBlock;

	private int _propID_DistortMag;

	private bool _visible;

	[SerializeField]
	private RulesetDetector _rulesetDetector;

	[SerializeField]
	private FluidDetector _fluidDetector;

	[SerializeField]
	private OWCamera _targetCamera;

	[SerializeField]
	private OWCamera _altTargetCamera;

	[SerializeField]
	private DampedSpringQuat _alignmentSpring;

	[SerializeField]
	private DampedSpring _speedDistortSpring;

	private float _distortScale;

	private float _minDistort;

	private float _maxDistort;

	private Quaternion _currentRotation;

	private float _currentDistort;

	private void Awake()
	{
		_effectBubbleRenderer = GetComponentInChildren<Renderer>();
		_effectBubbleRenderer.enabled = false;
		_visible = false;
		_matPropBlock = new MaterialPropertyBlock();
		_propID_DistortMag = Shader.PropertyToID("_DistortMag");
		if (_rulesetDetector != null)
		{
			_rulesetDetector.OnChangeRuleset += OnChangeRuleset;
		}
		if (_targetCamera != null)
		{
			_targetCamera.onThisPreCull += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_targetCamera.onThisPostRender += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
		}
		if (_altTargetCamera != null)
		{
			_altTargetCamera.onThisPreCull += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_altTargetCamera.onThisPostRender += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_rulesetDetector != null)
		{
			_rulesetDetector.OnChangeRuleset -= OnChangeRuleset;
		}
		if (_targetCamera != null)
		{
			_targetCamera.onThisPreCull -= new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_targetCamera.onThisPostRender -= new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
		}
		if (_altTargetCamera != null)
		{
			_altTargetCamera.onThisPreCull -= new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_altTargetCamera.onThisPostRender -= new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
		}
	}

	private void OnChangeRuleset()
	{
		if (_rulesetDetector.GetEffectBubbleType() == EffectRuleset.BubbleType.Underwater)
		{
			base.enabled = true;
			_effectBubbleRenderer.sharedMaterial = _rulesetDetector.GetEffectBubbleMaterial();
			EffectRuleset currentEffectRuleset = _rulesetDetector.GetCurrentEffectRuleset();
			_distortScale = currentEffectRuleset.GetEffectBubbleUnderwaterDistortScale();
			_minDistort = currentEffectRuleset.GetEffectBubbleUnderwaterMinDistort();
			_maxDistort = currentEffectRuleset.GetEffectBubbleUnderwaterMaxDistort();
			_currentRotation = _effectBubbleRenderer.transform.rotation;
			_currentDistort = _minDistort;
		}
		_visible = _rulesetDetector.GetEffectBubbleType() == EffectRuleset.BubbleType.Underwater;
		if (!_targetCamera && !_altTargetCamera)
		{
			_effectBubbleRenderer.enabled = _visible;
		}
	}

	private void OnTargetCameraPreCull(OWCamera owCamera)
	{
		if (_visible)
		{
			_effectBubbleRenderer.enabled = true;
		}
	}

	private void OnTargetCameraPostRender(OWCamera owCamera)
	{
		if (_effectBubbleRenderer.enabled)
		{
			_effectBubbleRenderer.enabled = false;
		}
	}

	private void LateUpdate()
	{
		if (_fluidDetector != null)
		{
			Vector3 relativeFluidVelocity = _fluidDetector.GetRelativeFluidVelocity();
			float magnitude = relativeFluidVelocity.magnitude;
			Quaternion targetValue = ((magnitude > 0.001f) ? (Quaternion.LookRotation(relativeFluidVelocity) * Quaternion.Euler(90f, 0f, 0f)) : _currentRotation);
			float targetValue2 = Mathf.Clamp(magnitude * _distortScale, _minDistort, _maxDistort);
			_currentRotation = _alignmentSpring.Update(_currentRotation, targetValue, Time.deltaTime);
			_currentDistort = _speedDistortSpring.Update(_currentDistort, targetValue2, Time.deltaTime);
			_effectBubbleRenderer.transform.rotation = _currentRotation;
			_matPropBlock.SetFloat(_propID_DistortMag, _currentDistort);
			_effectBubbleRenderer.SetPropertyBlock(_matPropBlock);
		}
	}
}
