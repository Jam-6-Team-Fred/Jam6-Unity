using UnityEngine;

public class SlideReelMusicManager : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource[] _backdropSources;

	[SerializeField]
	private OWAudioSource[] _beatSources;

	private AudioType _currentBackdropType;

	private AudioType _lastBeatType;

	private int _backdropIndex = -1;

	private int _beatIndex = -1;

	public void OnTransitionToFirstSlide()
	{
		if (_currentBackdropType == AudioType.Reel_Backdrop_Burnt)
		{
			StopAllBackdropSources(0.1f);
		}
	}

	public void OnExitSlideProjector(bool mindProjectionComplete = false)
	{
		if (_backdropIndex > -1)
		{
			float fadeTime = 4f;
			if (_currentBackdropType == AudioType.Reel_Backdrop_Burnt)
			{
				fadeTime = 1f;
			}
			StopAllBackdropSources(fadeTime);
			_backdropIndex = -1;
		}
		if (_beatIndex > -1)
		{
			float fadeTime2 = 4f;
			if (_lastBeatType == AudioType.Reel_5_Short || _lastBeatType == AudioType.Reel_5_Long)
			{
				fadeTime2 = (mindProjectionComplete ? 6f : 2f);
			}
			else if (_lastBeatType == AudioType.Reel_4_Beat_D || _lastBeatType == AudioType.Reel_Farewell)
			{
				fadeTime2 = (mindProjectionComplete ? 8f : 2f);
			}
			StopAllBeatSources(fadeTime2);
			_beatIndex = -1;
		}
	}

	public void PlayBackdrop(AudioType audioType, float fadeTime = 1f)
	{
		if (audioType == _currentBackdropType)
		{
			return;
		}
		if (_currentBackdropType == AudioType.Reel_Backdrop_Burnt || audioType == AudioType.Reel_Backdrop_Burnt)
		{
			fadeTime = 0.1f;
		}
		switch (audioType)
		{
		case AudioType.Reel_Backdrop_Burnt:
			StopAllBeatSources(0.1f);
			break;
		case AudioType.Reel_Rule_Backdrop_Glitch:
			StopAllBeatSources(fadeTime);
			break;
		}
		StopAllBackdropSources(fadeTime);
		_currentBackdropType = audioType;
		for (int i = 0; i < _backdropSources.Length; i++)
		{
			if (_backdropSources[i].isPlaying && _backdropSources[i].audioLibraryClip == audioType)
			{
				MonoBehaviour.print("Reusing source that is still fading out this audio type");
				_backdropIndex = i;
				_backdropSources[_backdropIndex].FadeInToLibraryVolume(fadeTime, fadeFromNothing: true);
				return;
			}
		}
		_backdropIndex = SelectNextFreePoolIndex(_backdropSources, _backdropIndex);
		_backdropSources[_backdropIndex].AssignAudioLibraryClip(audioType);
		_backdropSources[_backdropIndex].FadeInToLibraryVolume(fadeTime, fadeFromNothing: true);
		if (audioType == AudioType.Reel_Lab_Backdrop_Success)
		{
			_backdropSources[_backdropIndex].time = 25f;
		}
	}

	private void StopAllBackdropSources(float fadeTime = 1f)
	{
		_currentBackdropType = AudioType.None;
		for (int i = 0; i < _backdropSources.Length; i++)
		{
			if (_backdropSources[i].isPlaying && !_backdropSources[i].IsFadingOut())
			{
				_backdropSources[i].FadeOut(fadeTime);
			}
		}
	}

	public void PlayBeat(AudioType audioType, bool allowOverlap = false)
	{
		if (!allowOverlap)
		{
			StopAllBeatSources(2f);
		}
		_beatIndex = SelectNextFreePoolIndex(_beatSources, _beatIndex);
		_beatSources[_beatIndex].Stop();
		_beatSources[_beatIndex].AssignAudioLibraryClip(audioType);
		_beatSources[_beatIndex].PlayWithLibraryVolume();
		_lastBeatType = audioType;
	}

	private void StopAllBeatSources(float fadeTime = 1f)
	{
		for (int i = 0; i < _beatSources.Length; i++)
		{
			if (_beatSources[i].isPlaying && !_beatSources[i].IsFadingOut())
			{
				_beatSources[i].FadeOut(fadeTime);
			}
		}
	}

	private int SelectNextFreePoolIndex(OWAudioSource[] poolSources, int currentIndex)
	{
		int num = currentIndex - 1;
		if (num < 0)
		{
			num = poolSources.Length - 1;
		}
		int num2 = currentIndex;
		while (num2 != num)
		{
			num2++;
			if (num2 > poolSources.Length - 1)
			{
				num2 = 0;
			}
			if (!poolSources[num2].isPlaying)
			{
				return num2;
			}
		}
		MonoBehaviour.print("Audio pool ran out, returning oldest index");
		return num;
	}
}
