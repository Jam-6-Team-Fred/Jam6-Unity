using UnityEngine;

public class AudioLoopCrossfader : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotAudioStart;

	[SerializeField]
	private OWAudioSource _oneShotAudioEnd;

	[SerializeField]
	private OWAudioSource _loopingAudio;

	[Header("Start and end audio")]
	[SerializeField]
	private AudioType _startClip;

	[SerializeField]
	private AudioType _endClip;

	[Header("Audio fade controls")]
	[SerializeField]
	private float _loopFadeInDuration;

	[SerializeField]
	private float _loopFadeOutDuration;

	[SerializeField]
	private float _cancelFadeDuration = 0.2f;

	private bool _shouldPlay;

	public bool isLooping
	{
		get
		{
			if (_loopingAudio.isPlaying)
			{
				return !_loopingAudio.IsFadingOut();
			}
			return false;
		}
	}

	public bool shouldBePlaying => _shouldPlay;

	private void Start()
	{
		if (_loopingAudio != null)
		{
			_loopingAudio.SetLocalVolume(0f);
		}
		if (_oneShotAudioStart != null)
		{
			_oneShotAudioStart.AssignAudioLibraryClip(_startClip);
		}
		if (_oneShotAudioEnd != null)
		{
			_oneShotAudioEnd.AssignAudioLibraryClip(_endClip);
		}
		base.enabled = false;
	}

	public void Play()
	{
		_shouldPlay = true;
		if (_oneShotAudioStart != null && _loopingAudio != null)
		{
			base.enabled = true;
		}
		else if (_loopingAudio != null)
		{
			_loopingAudio.FadeInToLibraryVolume(_loopFadeInDuration, fadeFromNothing: false, randomizePlayhead: true);
		}
		if (_oneShotAudioStart != null && !_oneShotAudioStart.isPlaying)
		{
			_oneShotAudioStart.PlayWithLibraryVolume();
		}
		if (_oneShotAudioEnd != null && _oneShotAudioEnd.isPlaying)
		{
			_oneShotAudioEnd.FadeOut(_cancelFadeDuration);
		}
	}

	public void Stop()
	{
		_shouldPlay = false;
		base.enabled = false;
		if (_oneShotAudioStart != null && _oneShotAudioStart.isPlaying)
		{
			_oneShotAudioStart.FadeOut(_cancelFadeDuration);
		}
		if (_loopingAudio != null)
		{
			_loopingAudio.FadeOut(_loopFadeOutDuration);
		}
		if (_oneShotAudioEnd != null)
		{
			_oneShotAudioEnd.PlayWithLibraryVolume();
		}
	}

	private void Update()
	{
		if (_oneShotAudioStart.clip.length - _oneShotAudioStart.time <= _loopFadeInDuration)
		{
			_oneShotAudioStart.FadeOut(_loopFadeInDuration);
			_loopingAudio.FadeInToLibraryVolume(_loopFadeInDuration, fadeFromNothing: false, randomizePlayhead: true);
			base.enabled = false;
		}
	}
}
