using UnityEngine;

public class CloudEffectBubbleController : MonoBehaviour
{
	private Renderer _effectBubbleRenderer;

	private bool _visible;

	[SerializeField]
	private RulesetDetector _rulesetDetector;

	[SerializeField]
	private OWCamera _targetCamera;

	[SerializeField]
	private OWCamera _altTargetCamera;

	private void Awake()
	{
		_effectBubbleRenderer = GetComponentInChildren<Renderer>();
		_effectBubbleRenderer.enabled = false;
		_visible = false;
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
		_effectBubbleRenderer.sharedMaterial = _rulesetDetector.GetCloudEffectBubbleMaterial();
		_visible = _effectBubbleRenderer.sharedMaterial != null;
		if (_targetCamera == null && _altTargetCamera == null)
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
}
