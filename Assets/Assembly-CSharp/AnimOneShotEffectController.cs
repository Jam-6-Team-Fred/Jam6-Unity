using UnityEngine;

public class AnimOneShotEffectController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private AudioType _audioClip;

	[SerializeField]
	private float _randomPitchRange;

	[SerializeField]
	private ParticleSystem _particles;

	private float _lastTime;

	private void PlayOneShotEffect()
	{
		if (Time.time < _lastTime + Time.deltaTime * 0.5f)
		{
			return;
		}
		_lastTime = Time.time;
		if (_audioSource != null)
		{
			if (_randomPitchRange > 0f)
			{
				_audioSource.pitch = Random.Range(1f - _randomPitchRange * 0.5f, 1f + _randomPitchRange * 0.5f);
			}
			_audioSource.PlayOneShot(_audioClip);
		}
		if (_particles != null)
		{
			_particles.Play();
		}
	}
}
