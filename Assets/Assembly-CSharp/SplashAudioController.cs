using UnityEngine;

[RequireComponent(typeof(OWAudioSource))]
public class SplashAudioController : MonoBehaviour
{
	[SerializeField]
	private AudioType _splashClip;

	private void Reset()
	{
		GetComponent<OWAudioSource>().SetTrack(OWAudioMixer.TrackName.Environment);
		AudioSource component = GetComponent<AudioSource>();
		component.playOnAwake = false;
		component.loop = false;
		component.spatialBlend = 1f;
	}

	public void PlaySplash()
	{
		GetComponent<OWAudioSource>().PlayOneShot(_splashClip);
	}
}
