using UnityEngine;

public class Equalizer : MonoBehaviour
{
	private int _propID_BarPosition;

	[SerializeField]
	private Renderer[] _barRenderers;

	[SerializeField]
	private AnimationCurve[] _waveforms;

	[SerializeField]
	private float _waveformSpeed = 1f;

	[SerializeField]
	private float _waveformMagnitude = 1f;

	[SerializeField]
	private float _noiseSpeed = 1f;

	[SerializeField]
	private float _noiseMagnitude = 1f;

	private float _volume;

	private float _signal;

	private void Awake()
	{
		_propID_BarPosition = Shader.PropertyToID("_BarPosition");
		_volume = 0f;
		_signal = 0f;
	}

	private void OnEnable()
	{
		ResetBank();
	}

	private void OnDisable()
	{
		ResetBank();
	}

	private void Update()
	{
		float num = Mathf.PerlinNoise(-1f, Time.time * _waveformSpeed) * (float)(_waveforms.Length - 1);
		AnimationCurve animationCurve = _waveforms[Mathf.FloorToInt(num)];
		AnimationCurve animationCurve2 = _waveforms[Mathf.CeilToInt(num)];
		for (int i = 0; i < _barRenderers.Length; i++)
		{
			float a = animationCurve.Evaluate((float)i / (float)(_barRenderers.Length - 1));
			float b = animationCurve2.Evaluate((float)i / (float)(_barRenderers.Length - 1));
			float b2 = Mathf.Lerp(a, b, num - Mathf.Floor(num)) * _waveformMagnitude;
			float value = Mathf.Max(Mathf.Lerp(Mathf.PerlinNoise(i, Time.time * _noiseSpeed) * _noiseMagnitude, b2, _signal), 0f) * _volume;
			_barRenderers[i].material.SetFloat(_propID_BarPosition, value);
		}
	}

	private void ResetBank()
	{
		for (int i = 0; i < _barRenderers.Length; i++)
		{
			_barRenderers[i].material.SetFloat(_propID_BarPosition, 0f);
		}
	}

	public void SetVolume(float v)
	{
		_volume = v;
	}

	public void SetSignal(float s)
	{
		_signal = s;
	}
}
