using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SplashController : MonoBehaviour
{
	private Renderer _renderer;

	[SerializeField]
	private float _lifetime = 1f;

	[SerializeField]
	private float _delay;

	[SerializeField]
	private AnimationCurve _yOffsetOverLife = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _cutoffOverLife = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);

	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_MainTex_ST;

	private static int s_propID_Cutoff;

	private Vector4 _startScaleOffset;

	private float _timer;

	private void Awake()
	{
		_renderer = GetComponent<Renderer>();
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_MainTex_ST = Shader.PropertyToID("_MainTex_ST");
			s_propID_Cutoff = Shader.PropertyToID("_Cutoff");
		}
		_startScaleOffset = _renderer.sharedMaterial.GetVector(s_propID_MainTex_ST);
		s_matPropBlock.SetVector(s_propID_MainTex_ST, _startScaleOffset + new Vector4(0f, 0f, 0f, _yOffsetOverLife.Evaluate(0f)));
		s_matPropBlock.SetFloat(s_propID_Cutoff, _cutoffOverLife.Evaluate(0f));
		_renderer.SetPropertyBlock(s_matPropBlock);
		_timer = 0f - _delay;
		_renderer.enabled = _timer >= 0f;
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		float time = Mathf.Clamp01(_timer / _lifetime);
		s_matPropBlock.SetVector(s_propID_MainTex_ST, _startScaleOffset + new Vector4(0f, 0f, 0f, _yOffsetOverLife.Evaluate(time)));
		s_matPropBlock.SetFloat(s_propID_Cutoff, _cutoffOverLife.Evaluate(time));
		_renderer.SetPropertyBlock(s_matPropBlock);
		_renderer.enabled = _timer >= 0f;
		if (_timer > _lifetime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
