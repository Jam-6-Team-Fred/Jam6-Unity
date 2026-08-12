using UnityEngine;

public class GhostSequenceAudioVolume : AudioVolume
{
	[SerializeField]
	private OWAudioSource _reducedFrightsAudio;

	[SerializeField]
	private OWAudioSource _suspenseAudio;

	[SerializeField]
	private OWAudioSource _dreadAudio;

	[SerializeField]
	private OWAudioSource _fearAudio;

	[SerializeField]
	private OWAudioSource _slamAudio;

	[SerializeField]
	private GhostBrain[] _ghosts;

	private bool _playingReducedFrights;

	private float _lastDreadConditionsMetTime = float.NegativeInfinity;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Init()
	{
		_initialized = true;
		_reducedFrightsAudio.SetLocalVolume(0f);
		_suspenseAudio.SetLocalVolume(0f);
		_dreadAudio.SetLocalVolume(0f);
		_fearAudio.SetLocalVolume(0f);
	}

	protected override void PlayAudio()
	{
	}

	public override void Activate()
	{
		if (!_initialized)
		{
			Init();
		}
		_isActive = true;
		base.enabled = true;
		if (PlayerData.GetReducedFrights())
		{
			_reducedFrightsAudio.FadeInToLibraryVolume(_fadeSeconds);
			_playingReducedFrights = true;
		}
		else
		{
			_suspenseAudio.FadeInToLibraryVolume(_fadeSeconds);
			_playingReducedFrights = false;
		}
		OnAudioPlay.Invoke();
	}

	public override void Deactivate()
	{
		_playingReducedFrights = false;
		_isActive = false;
		base.enabled = false;
		_reducedFrightsAudio.FadeOut(_fadeSeconds);
		_suspenseAudio.FadeOut(_fadeSeconds);
		_dreadAudio.FadeOut(_fadeSeconds);
		_fearAudio.FadeOut(_fadeSeconds);
		OnAudioStop.Invoke();
	}

	public override void Deactivate(float fadeSeconds)
	{
		_playingReducedFrights = false;
		_isActive = false;
		base.enabled = false;
		_reducedFrightsAudio.FadeOut(fadeSeconds);
		_suspenseAudio.FadeOut(fadeSeconds);
		_dreadAudio.FadeOut(fadeSeconds);
		_fearAudio.FadeOut(fadeSeconds);
		OnAudioStop.Invoke();
	}

	private void Update()
	{
		if (base.enabled)
		{
			if (!_playingReducedFrights && PlayerData.GetReducedFrights())
			{
				_playingReducedFrights = true;
				_reducedFrightsAudio.FadeInToLibraryVolume(1f);
				_suspenseAudio.FadeOut(1f);
				_dreadAudio.FadeOut(1f);
				_fearAudio.FadeOut(1f);
			}
			else if (_playingReducedFrights && !PlayerData.GetReducedFrights())
			{
				_playingReducedFrights = false;
				_reducedFrightsAudio.FadeOut(1f);
				_suspenseAudio.FadeInToLibraryVolume(1f);
			}
			if (!_playingReducedFrights)
			{
				CheckDreadFearConditions();
			}
		}
	}

	private void CheckDreadFearConditions()
	{
		bool flag = _dreadAudio.isPlaying && !_dreadAudio.IsFadingOut();
		bool flag2 = _fearAudio.isPlaying && !_fearAudio.IsFadingOut();
		bool flag3 = Time.time - _lastDreadConditionsMetTime < 5f;
		bool flag4 = false;
		for (int i = 0; i < _ghosts.Length; i++)
		{
			if (_ghosts[i].CheckDreadAudioConditions())
			{
				_lastDreadConditionsMetTime = Time.time;
				flag3 = true;
			}
			if (_ghosts[i].CheckFearAudioConditions(flag2))
			{
				flag4 = true;
			}
		}
		if (flag4)
		{
			flag3 = false;
		}
		if (flag4 != flag2)
		{
			if (flag4)
			{
				_slamAudio.PlayOneShot(AudioType.GhostSequence_Fear_Slam);
				_fearAudio.FadeInToLibraryVolume(1f);
				_suspenseAudio.FadeOut(1f);
			}
			else
			{
				_fearAudio.FadeOut(2f);
				if (!flag3)
				{
					_suspenseAudio.FadeInToLibraryVolume(2f);
				}
			}
		}
		if (flag3 == flag)
		{
			return;
		}
		if (flag3)
		{
			if (!_dreadAudio.isPlaying)
			{
				float volume = Locator.GetAudioManager().GetAudioEntry(_dreadAudio.audioLibraryClip).volume;
				_dreadAudio.SetLocalVolume(volume);
				_dreadAudio.Play();
				_suspenseAudio.FadeOut(0.2f);
			}
			else
			{
				_dreadAudio.FadeInToLibraryVolume(1f);
				_suspenseAudio.FadeOut(1f);
			}
		}
		else
		{
			_dreadAudio.FadeOut(flag4 ? 0.5f : 2f);
			if (!flag4)
			{
				_suspenseAudio.FadeInToLibraryVolume(2f);
			}
		}
	}
}
