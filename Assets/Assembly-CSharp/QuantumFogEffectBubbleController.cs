using UnityEngine;

public class QuantumFogEffectBubbleController : MonoBehaviour
{
	private Renderer _bubbleRenderer;

	private MaterialPropertyBlock _matPropBlock;

	private int _propID_Color;

	private bool _visible;

	[SerializeField]
	private OWCamera _targetCamera;

	private Color _baseColor;

	private float _alpha;

	private void Awake()
	{
		_bubbleRenderer = this.GetRequiredComponent<Renderer>();
		_bubbleRenderer.enabled = false;
		_visible = false;
		_matPropBlock = new MaterialPropertyBlock();
		_propID_Color = Shader.PropertyToID("_Color");
		_baseColor = _bubbleRenderer.sharedMaterial.GetColor(_propID_Color);
		if (_targetCamera != null)
		{
			_targetCamera.onThisPreCull += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_targetCamera.onThisPostRender += new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
		}
	}

	private void OnDestroy()
	{
		if (_targetCamera != null)
		{
			_targetCamera.onThisPreCull -= new OWEvent<OWCamera>.OWCallback(OnTargetCameraPreCull);
			_targetCamera.onThisPostRender -= new OWEvent<OWCamera>.OWCallback(OnTargetCameraPostRender);
		}
	}

	private void OnTargetCameraPreCull(OWCamera owCamera)
	{
		if (_visible)
		{
			_bubbleRenderer.enabled = true;
		}
	}

	private void OnTargetCameraPostRender(OWCamera owCamera)
	{
		if (_bubbleRenderer.enabled)
		{
			_bubbleRenderer.enabled = false;
		}
	}

	public void SetFogAlpha(float alpha)
	{
		_alpha = ((alpha >= _alpha) ? alpha : Mathf.MoveTowards(_alpha, alpha, 0.5f * Time.deltaTime));
		_matPropBlock.SetColor(_propID_Color, new Color(_baseColor.r, _baseColor.g, _baseColor.b, _alpha));
		_bubbleRenderer.SetPropertyBlock(_matPropBlock);
		_visible = _alpha > 0.001f;
		if (_targetCamera == null)
		{
			_bubbleRenderer.enabled = _visible;
		}
	}
}
