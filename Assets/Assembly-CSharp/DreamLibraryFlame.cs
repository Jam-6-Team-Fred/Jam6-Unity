using UnityEngine;

public class DreamLibraryFlame : MonoBehaviour
{
	[SerializeField]
	private OWFlameController _flame;

	[SerializeField]
	private ParticleSystem[] _particles;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private HazardVolume _hazardVolume;

	private bool _lit;

	private void Start()
	{
		if (_flame != null)
		{
			_flame.SetIntensity(0f);
		}
		if (_loopingAudio != null)
		{
			_loopingAudio.SetLocalVolume(0f);
		}
		if (_hazardVolume != null)
		{
			_hazardVolume.SetVolumeActivation(active: false);
		}
	}

	public void SetLit(bool lit)
	{
		if (lit == _lit)
		{
			return;
		}
		_lit = lit;
		if (_hazardVolume != null)
		{
			_hazardVolume.SetVolumeActivation(lit);
		}
		if (_flame != null)
		{
			_flame.FadeTo(lit ? 1f : 0f, 1f);
		}
		if (_oneShotAudio != null)
		{
			_oneShotAudio.PlayOneShot(lit ? AudioType.Artifact_Light : AudioType.Artifact_Extinguish);
		}
		if (_loopingAudio != null)
		{
			if (lit)
			{
				_loopingAudio.FadeIn(1f, fadeFromNothing: false, randomizePlayhead: true);
			}
			else
			{
				_loopingAudio.FadeOut(1f);
			}
		}
		for (int i = 0; i < _particles.Length; i++)
		{
			if (lit)
			{
				_particles[i].Play();
			}
			else
			{
				_particles[i].Stop();
			}
		}
	}
}
