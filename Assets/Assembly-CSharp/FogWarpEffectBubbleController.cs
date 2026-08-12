using UnityEngine;

public class FogWarpEffectBubbleController : MonoBehaviour
{
	private Renderer _effectBubbleRenderer;

	private MaterialPropertyBlock _matPropBlock;

	private int _propID_Color;

	private bool _visible;

	[SerializeField]
	private RulesetDetector _rulesetDetector;

	[SerializeField]
	private OWCamera _targetCamera;

	private void Awake()
	{
		_effectBubbleRenderer = GetComponentInChildren<Renderer>();
		_effectBubbleRenderer.enabled = false;
		_visible = false;
		_matPropBlock = new MaterialPropertyBlock();
		_propID_Color = Shader.PropertyToID("_Color");
		if (_rulesetDetector != null)
		{
			_rulesetDetector.OnChangeRuleset += OnChangeRuleset;
		}
		if (_targetCamera != null)
		{
			_targetCamera.onThisPreCull += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_targetCamera.onThisPostRender += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
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
	}

	private void OnChangeRuleset()
	{
		if (_rulesetDetector.GetEffectBubbleType() == EffectRuleset.BubbleType.FogWarp)
		{
			_effectBubbleRenderer.sharedMaterial = _rulesetDetector.GetEffectBubbleMaterial();
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

	public void SetFogFade(float fogAlpha, Color fogColor)
	{
		if (_effectBubbleRenderer.sharedMaterial != null)
		{
			Color value = fogColor;
			value.a = fogAlpha;
			_matPropBlock.SetColor(_propID_Color, value);
			_effectBubbleRenderer.SetPropertyBlock(_matPropBlock);
		}
		_visible = _effectBubbleRenderer.sharedMaterial != null && fogAlpha > 0f;
		if (_targetCamera == null)
		{
			_effectBubbleRenderer.enabled = _visible;
		}
	}
}
