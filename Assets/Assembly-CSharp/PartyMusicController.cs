using UnityEngine;

public class PartyMusicController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource[] _instrumentSources;

	[SerializeField]
	private float[] _stopDelays;

	private float _stopTime;

	private void Start()
	{
		base.enabled = false;
		for (int i = 0; i < _instrumentSources.Length; i++)
		{
			_instrumentSources[i].SetLocalVolume(0f);
		}
	}

	public void FadeIn(float duration)
	{
		for (int i = 0; i < _instrumentSources.Length; i++)
		{
			_instrumentSources[i].Stop();
			_instrumentSources[i].FadeInToLibraryVolume(duration);
		}
	}

	public void FadeOut(float duration)
	{
		for (int i = 0; i < _instrumentSources.Length; i++)
		{
			_instrumentSources[i].FadeOut(duration);
		}
	}

	public void StaggerStop()
	{
		base.enabled = true;
		_stopTime = Time.time;
	}

	private void Update()
	{
		int num = 0;
		for (int i = 0; i < _instrumentSources.Length; i++)
		{
			if (!_instrumentSources[i].isPlaying)
			{
				num++;
			}
			else if (!_instrumentSources[i].IsFadingOut() && Time.time >= _stopTime + _stopDelays[i])
			{
				_instrumentSources[i].FadeOut(0.5f);
			}
		}
		if (num == _instrumentSources.Length)
		{
			base.enabled = false;
		}
	}
}
