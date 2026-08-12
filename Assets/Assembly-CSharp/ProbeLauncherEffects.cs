using UnityEngine;

public class ProbeLauncherEffects : MonoBehaviour
{
	[SerializeField]
	protected ParticleSystem _launchParticles;

	[SerializeField]
	protected ParticleSystem _underwaterLaunchParticles;

	[Space]
	[SerializeField]
	protected OWAudioSource _owAudioSource;

	public void PlayLaunchParticles(bool underwater)
	{
		if (underwater && _underwaterLaunchParticles != null)
		{
			_underwaterLaunchParticles.Play();
		}
		else if (_launchParticles != null)
		{
			_launchParticles.Play();
		}
	}

	public void PlayChangeModeClip()
	{
		_owAudioSource.PlayOneShot(AudioType.ToolProbeChangeMode);
	}

	public void PlayLaunchClip(bool underwater)
	{
		if (underwater)
		{
			_owAudioSource.PlayOneShot(AudioType.ToolProbeLaunchUnderwater);
		}
		else
		{
			_owAudioSource.PlayOneShot(AudioType.ToolProbeLaunch);
		}
	}

	public void PlaySnapshotClip(bool reverseSnapshot)
	{
		Locator.GetPlayerAudioController().PlayProbeSnapshot();
	}

	public void PlayRetrievalClip()
	{
		_owAudioSource.PlayOneShot(AudioType.ToolProbeRetrieve);
	}
}
