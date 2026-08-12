using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SolarFlareController : MonoBehaviour
{
	private MaterialPropertyBlock s_matPropBlock;

	private int s_propID_MainTex_ST;

	private int s_propID_Color;

	private int s_propID_MaskCutoff;

	private MeshRenderer _meshRenderer;

	[SerializeField]
	private float _lifetimeScale = 1f;

	[SerializeField]
	private Vector3 _scaleFactor = Vector3.one;

	[SerializeField]
	private Vector2 _uvScrollSpeed = Vector2.zero;

	[SerializeField]
	private AnimationCurve _alphaOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	private AnimationCurve _maskCutoffOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 0f);

	private float _lifetime;

	private float _lifeLength;

	private float _startScale;

	private float _endScale;

	private Color _tint;

	private Vector4 _texScaleOffset;

	private Color _color;

	private void Awake()
	{
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
			s_propID_Color = Shader.PropertyToID("_Color");
			s_propID_MaskCutoff = Shader.PropertyToID("_MaskCutoff");
		}
		_meshRenderer = GetComponent<MeshRenderer>();
		_texScaleOffset = _meshRenderer.sharedMaterial.GetVector(s_propID_MainTex_ST);
		_texScaleOffset.z = Random.value;
		_texScaleOffset.w = Random.value;
		_color = _meshRenderer.sharedMaterial.GetColor(s_propID_Color);
		_meshRenderer.enabled = false;
		base.enabled = false;
	}

	public void Spawn(Vector3 localPosition, Quaternion localRotation, float startScale, float endScale, float lifeLength, Color tint)
	{
		_lifetime = 0f;
		_lifeLength = lifeLength * _lifetimeScale;
		_startScale = startScale;
		_endScale = endScale;
		_tint = tint;
		base.transform.localPosition = localPosition;
		base.transform.localRotation = localRotation;
		UpdateAnimation(0f);
		_meshRenderer.enabled = true;
		base.enabled = true;
	}

	public void Despawn()
	{
		if (base.enabled)
		{
			_meshRenderer.enabled = false;
			base.enabled = false;
		}
	}

	private void Update()
	{
		_lifetime += Time.deltaTime;
		float num = Mathf.Clamp01(_lifetime / _lifeLength);
		UpdateAnimation(num);
		if (num >= 1f)
		{
			_meshRenderer.enabled = false;
			base.enabled = false;
		}
	}

	private void UpdateAnimation(float t)
	{
		Vector4 value = new Vector4(_texScaleOffset.x, _texScaleOffset.y, t * _uvScrollSpeed.x, t * _uvScrollSpeed.y);
		Vector3 localScale = _scaleFactor * Mathf.Lerp(_startScale, _endScale, t);
		Color gamma = new Color(_color.r * _tint.r, _color.g * _tint.g, _color.b * _tint.b, _color.a * _tint.a * _alphaOverLifetime.Evaluate(t)).gamma;
		float value2 = _maskCutoffOverLifetime.Evaluate(t);
		base.transform.localScale = localScale;
		s_matPropBlock.SetVector(s_propID_MainTex_ST, value);
		s_matPropBlock.SetColor(s_propID_Color, gamma);
		s_matPropBlock.SetFloat(s_propID_MaskCutoff, value2);
		_meshRenderer.SetPropertyBlock(s_matPropBlock);
	}

	public void SetRenderingEnabled(bool renderingEnabled)
	{
		if (base.enabled)
		{
			_meshRenderer.enabled = renderingEnabled;
		}
	}
}
