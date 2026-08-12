using System;
using UnityEngine;

[Serializable]
public class TurbulenceAudio
{
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private float _maxDensity = 5f;

	[SerializeField]
	private float _lowerSpeedLimit = 20f;

	[SerializeField]
	private float _upperSpeedLimit = 40f;

	[SerializeField]
	private float _easeRate = 3f;

	public void Initialize()
	{
		_audioSource.SetLocalVolume(0f);
	}

	public void Update(float fluidSpeed, float fluidDensity, bool isShip)
	{
		bool flag = fluidDensity < _maxDensity && fluidSpeed >= _lowerSpeedLimit && (!isShip || PlayerState.IsInsideShip());
		if (!_audioSource.isPlaying && flag)
		{
			_audioSource.SetLocalVolume(0f);
			_audioSource.Play();
			return;
		}
		float b = (flag ? Mathf.InverseLerp(_lowerSpeedLimit, _upperSpeedLimit, fluidSpeed) : 0f);
		_audioSource.SetLocalVolume(Mathf.Lerp(_audioSource.GetLocalVolume(), b, _easeRate * Time.deltaTime));
		if (_audioSource.GetLocalVolume() < 0.001f)
		{
			_audioSource.Stop();
		}
	}
}
