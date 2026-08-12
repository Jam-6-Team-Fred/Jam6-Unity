using UnityEngine;
using UnityEngine.Audio;

public class AudioParameter
{
	private string[] _names;

	private bool _fading;

	private float _value;

	private float _origValue;

	private float _startValue;

	private float _targetValue;

	private float _startTime;

	private float _duration;

	private AudioMixer _mixer;

	private bool _convertDecibelsToLinear;

	public AudioParameter(string[] names, float initialValue, AudioMixer mixer, bool convertDecibelsToLinear = false)
	{
		_names = names;
		_mixer = mixer;
		_fading = false;
		_convertDecibelsToLinear = convertDecibelsToLinear;
		_value = (_origValue = initialValue);
		UpdateMixerFloats();
	}

	public void FadeToOriginal(float duration)
	{
		FadeTo(_origValue, duration);
	}

	public void FadeTo(float value, float duration)
	{
		_fading = false;
		_targetValue = value;
		if (OWMath.ApproxEquals(_value, _targetValue) || duration < 0.001f)
		{
			_value = _targetValue;
			UpdateMixerFloats();
			return;
		}
		_startValue = _value;
		_startTime = Time.unscaledTime;
		_duration = duration;
		_fading = true;
	}

	public void Update()
	{
		if (_fading)
		{
			float num = Mathf.InverseLerp(_startTime, _startTime + _duration, Time.unscaledTime);
			_value = Mathf.Lerp(_startValue, _targetValue, num);
			UpdateMixerFloats();
			if (num >= 1f)
			{
				_fading = false;
			}
		}
	}

	private void UpdateMixerFloats()
	{
		for (int i = 0; i < _names.Length; i++)
		{
			if (!_mixer.SetFloat(_names[i], _convertDecibelsToLinear ? OWMath.LinearToDecibel(_value) : _value))
			{
				Debug.LogError("Failed to find exposed audio parameter: " + _names[i]);
				Debug.Break();
			}
		}
	}
}
