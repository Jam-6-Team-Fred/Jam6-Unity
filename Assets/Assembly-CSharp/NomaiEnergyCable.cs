using UnityEngine;

public class NomaiEnergyCable : MonoBehaviour
{
	[SerializeField]
	private Renderer _cableRenderer;

	[SerializeField]
	private float _scale = 1f;

	[SerializeField]
	private float _offset = 0.6f;

	[SerializeField]
	private bool _startPowered;

	private bool _powered;

	private float _targetScale;

	private float _targetOffset;

	private float _targetGlow;

	private Color _glowColor;

	private void OnValidate()
	{
		if (_cableRenderer == null)
		{
			_cableRenderer = GetComponent<Renderer>();
		}
	}

	private void Start()
	{
		_glowColor = _cableRenderer.material.GetColor("_Glow");
		SetPowered(_startPowered, immediate: true);
		base.enabled = false;
	}

	public void SetPowered(bool powered, bool immediate = false)
	{
		if (powered != _powered || immediate)
		{
			_powered = powered;
			_targetScale = (_powered ? _scale : 0f);
			_targetOffset = (_powered ? _offset : 0f);
			SetCableValues(_targetScale, _targetOffset, _glowColor);
		}
	}

	public void SetTargetGlow(float targetGlow)
	{
		_targetGlow = targetGlow;
		base.enabled = true;
	}

	private void Update()
	{
		Vector2 textureScale = _cableRenderer.material.GetTextureScale("_EmissionMap");
		Vector2 textureOffset = _cableRenderer.material.GetTextureOffset("_EmissionMap");
		textureScale.y = Mathf.MoveTowards(textureScale.y, _targetScale, Time.deltaTime / _scale);
		textureOffset.y = _targetOffset;
		_glowColor.a = Mathf.MoveTowards(_glowColor.a, _targetGlow, Time.deltaTime / 5f);
		SetCableValues(textureScale.y, textureOffset.y, _glowColor);
		if (OWMath.ApproxEquals(textureScale.y, _targetScale) && OWMath.ApproxEquals(textureOffset.y, _targetOffset) && OWMath.ApproxEquals(_glowColor.a, _targetGlow))
		{
			textureScale.y = _targetScale;
			textureOffset.y = _targetOffset;
			_glowColor.a = _targetGlow;
			base.enabled = false;
		}
		SetCableValues(textureScale.y, textureOffset.y, _glowColor);
	}

	private void SetCableValues(float scale, float offset, Color glowColor)
	{
		_cableRenderer.material.SetTextureScale("_EmissionMap", new Vector2(1f, scale));
		_cableRenderer.material.SetTextureOffset("_EmissionMap", new Vector2(0f, offset));
		_cableRenderer.material.SetColor("_Glow", glowColor);
	}
}
