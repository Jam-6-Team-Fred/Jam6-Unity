using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Audio/OW Audio Source", 250)]
[RequireComponent(typeof(AudioSource))]
public class OWAudioSource : MonoBehaviour
{
	public enum FadeOutCompleteAction
	{
		STOP = 0,
		PAUSE = 1,
		CONTINUE = 2
	}

	public enum ClipSelectionOnPlay
	{
		RANDOM = 0,
		SEQUENTIAL = 1,
		MANUAL = 2
	}

	[SerializeField]
	private AudioType _audioLibraryClip;

	[SerializeField]
	private ClipSelectionOnPlay _clipSelectionOnPlay;

	[SerializeField]
	private OWAudioMixer.TrackName _track;

	[SerializeField]
	private bool _randomizePlayheadOnAwake;

	private int _clipArrayLength;

	private int _clipArrayIndex;

	private AudioSource _audioSource;

	private float _localVolume = 1f;

	private float _maxSourceVolume = 1f;

	private bool _isLocalFading;

	private bool _hasFadedOut;

	private float _fadeOutTime;

	private FadeOutCompleteAction _fadeOutCompleteAction = FadeOutCompleteAction.CONTINUE;

	private bool _makeDynamicOnPause;

	private float _fadeFraction;

	private float _initFadeTime;

	private float _fadeDuration;

	private float _initFadeVolume;

	private float _targetFadeVolume;

	public AudioClip clip
	{
		get
		{
			return _audioSource.clip;
		}
		set
		{
			_audioSource.clip = value;
		}
	}

	public bool playOnAwake
	{
		get
		{
			return _audioSource.playOnAwake;
		}
		set
		{
			_audioSource.playOnAwake = value;
		}
	}

	public bool loop
	{
		get
		{
			return _audioSource.loop;
		}
		set
		{
			_audioSource.loop = value;
		}
	}

	public AudioType audioLibraryClip => _audioLibraryClip;

	public bool isPlaying => _audioSource.isPlaying;

	public float minDistance
	{
		get
		{
			return _audioSource.minDistance;
		}
		set
		{
			_audioSource.minDistance = value;
		}
	}

	public float maxDistance
	{
		get
		{
			return _audioSource.maxDistance;
		}
		set
		{
			_audioSource.maxDistance = value;
		}
	}

	public AudioRolloffMode rolloffMode
	{
		get
		{
			return _audioSource.rolloffMode;
		}
		set
		{
			_audioSource.rolloffMode = value;
		}
	}

	public float time
	{
		get
		{
			return _audioSource.time;
		}
		set
		{
			_audioSource.time = value;
		}
	}

	public int timeSamples
	{
		get
		{
			return _audioSource.timeSamples;
		}
		set
		{
			_audioSource.timeSamples = value;
		}
	}

	public bool mute
	{
		get
		{
			return _audioSource.mute;
		}
		set
		{
			_audioSource.mute = value;
		}
	}

	public float volume => _audioSource.volume;

	public float pitch
	{
		get
		{
			return _audioSource.pitch;
		}
		set
		{
			_audioSource.pitch = value;
		}
	}

	public float panStereo
	{
		get
		{
			return _audioSource.panStereo;
		}
		set
		{
			_audioSource.panStereo = value;
		}
	}

	public float spatialBlend
	{
		get
		{
			return _audioSource.spatialBlend;
		}
		set
		{
			_audioSource.spatialBlend = value;
		}
	}

	public float dopplerLevel
	{
		get
		{
			return _audioSource.dopplerLevel;
		}
		set
		{
			_audioSource.dopplerLevel = value;
		}
	}

	public float spread
	{
		get
		{
			return _audioSource.spread;
		}
		set
		{
			_audioSource.spread = value;
		}
	}

	public void Play()
	{
		SelectClip();
		_audioSource.Play();
	}

	public void Play(ulong delay)
	{
		SelectClip();
		_audioSource.Play(delay);
	}

	public void PlayDelayed(float delay)
	{
		SelectClip();
		_audioSource.PlayDelayed(delay);
	}

	public void PlayWithLibraryVolume()
	{
		SelectClip();
		AudioLibrary.AudioEntry audioEntry = Locator.GetAudioManager().GetAudioEntry(_audioLibraryClip);
		SetLocalVolume((audioEntry.type != 0) ? audioEntry.volume : 1f);
		_audioSource.Play();
	}

	public void Stop()
	{
		_audioSource.Stop();
		_isLocalFading = false;
	}

	public void Pause()
	{
		_audioSource.Pause();
		_isLocalFading = false;
	}

	public void UnPause()
	{
		_audioSource.UnPause();
	}

	public void SetCustomCurve(AudioSourceCurveType type, AnimationCurve curve)
	{
		_audioSource.SetCustomCurve(type, curve);
	}

	public AnimationCurve GetCustomCurve(AudioSourceCurveType type)
	{
		return _audioSource.GetCustomCurve(type);
	}

	public void GetOutputData(float[] samples, int channel)
	{
		_audioSource.GetOutputData(samples, channel);
	}

	public bool GetRandomizePlayheadOnAwake()
	{
		return _randomizePlayheadOnAwake;
	}

	public bool IsFadingOut()
	{
		if (_isLocalFading)
		{
			return _targetFadeVolume <= 0f;
		}
		return false;
	}

	public bool IsFadingIn()
	{
		if (_isLocalFading)
		{
			return _targetFadeVolume > 0f;
		}
		return false;
	}

	public float GetLocalVolume()
	{
		return _localVolume;
	}

	public OWAudioMixer.TrackName GetTrack()
	{
		return _track;
	}

	public void SetTrack(OWAudioMixer.TrackName track)
	{
		_track = track;
	}

	public AudioSource GetAudioSource()
	{
		return _audioSource;
	}

	private void Awake()
	{
		_audioSource = this.GetRequiredComponent<AudioSource>();
		_audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
		if (_track == OWAudioMixer.TrackName.Undefined || _track == OWAudioMixer.TrackName.Undefined)
		{
			Debug.LogError("Audio source track is Undefined, moving to Menu track", this);
			_track = OWAudioMixer.TrackName.Menu;
		}
		if (_audioSource.volume > 0f)
		{
			_maxSourceVolume = _audioSource.volume;
		}
	}

	private void Start()
	{
		if (spatialBlend <= 0f)
		{
			_audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
		}
		AudioMixerGroup audioMixerGroup = Locator.GetAudioMixer().GetAudioMixerGroup(_track);
		if (audioMixerGroup != null)
		{
			_audioSource.outputAudioMixerGroup = audioMixerGroup;
		}
		else
		{
			Debug.LogError("Could not find mixer group for audio track " + _track, this);
		}
		if (_audioLibraryClip != 0)
		{
			AssignAudioLibraryClip(_audioLibraryClip);
		}
		if (_audioSource.playOnAwake)
		{
			FadeIn(0.5f, fadeFromNothing: true, _randomizePlayheadOnAwake);
		}
		UpdateSourceVolume();
	}

	private void OnDestroy()
	{
		if (_makeDynamicOnPause)
		{
			GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
			GlobalMessenger.RemoveListener("GameUnpaused", OnGameUnpaused);
		}
	}

	public void MakeVelocityUpdateDynamicOnPause()
	{
		if (!_makeDynamicOnPause)
		{
			_makeDynamicOnPause = true;
			GlobalMessenger.AddListener("GamePaused", OnGamePaused);
			GlobalMessenger.AddListener("GameUnpaused", OnGameUnpaused);
		}
	}

	public void PlayOneShot()
	{
		PlayOneShot(_audioLibraryClip);
	}

	public void PlayOneShot(AudioClip clip)
	{
		_audioSource.PlayOneShot(clip);
	}

	public void PlayOneShot(AudioClip clip, float volumeScale)
	{
		_audioSource.PlayOneShot(clip, volumeScale);
	}

	public AudioClip PlayOneShot(AudioType type, float volume = 1f)
	{
		AudioLibrary.AudioEntry audioEntry = Locator.GetAudioManager().GetAudioEntry(type);
		if (audioEntry.clips.Length != 0)
		{
			AudioClip result = audioEntry.clips[Random.Range(0, audioEntry.clips.Length)];
			PlayOneShot(result, audioEntry.volume * volume);
			return result;
		}
		Debug.LogError(string.Concat("Audio entry ", audioEntry.type, " has no AudioClips assigned"));
		return null;
	}

	public AudioClip PlayOneShot(AudioType type, int index, float volume = 1f)
	{
		AudioLibrary.AudioEntry audioEntry = Locator.GetAudioManager().GetAudioEntry(type);
		if (audioEntry.clips.Length != 0)
		{
			int num = Mathf.Min(index, audioEntry.clips.Length - 1);
			AudioClip result = audioEntry.clips[num];
			PlayOneShot(result, audioEntry.volume * volume);
			return result;
		}
		Debug.LogError(string.Concat("Audio entry ", audioEntry.type, " has no AudioClips assigned"));
		return null;
	}

	public void AssignAudioLibraryClip(AudioType type)
	{
		_audioLibraryClip = type;
		_clipArrayLength = Locator.GetAudioManager().GetAudioClipArray(_audioLibraryClip).Length;
		_clipArrayIndex = -1;
		SelectClip();
	}

	public void SetClipSelectionType(ClipSelectionOnPlay clipSelectType)
	{
		_clipSelectionOnPlay = clipSelectType;
	}

	public ClipSelectionOnPlay GetClipSelectionType()
	{
		return _clipSelectionOnPlay;
	}

	public void SelectClip(int index)
	{
		if (_audioLibraryClip != 0)
		{
			_clipArrayIndex = index;
			if (_clipArrayIndex > _clipArrayLength)
			{
				_clipArrayIndex = 0;
				Debug.LogWarning("Index out of range! Resetting index to 0");
			}
			clip = Locator.GetAudioManager().GetAudioClipArray(_audioLibraryClip)[_clipArrayIndex];
		}
	}

	public void SelectClip()
	{
		if (_audioLibraryClip == AudioType.None || _clipArrayLength == 0)
		{
			return;
		}
		switch (_clipSelectionOnPlay)
		{
		case ClipSelectionOnPlay.RANDOM:
			clip = Locator.GetAudioManager().GetSingleAudioClip(_audioLibraryClip);
			break;
		case ClipSelectionOnPlay.SEQUENTIAL:
			_clipArrayIndex++;
			if (_clipArrayIndex > _clipArrayLength)
			{
				_clipArrayIndex = 0;
			}
			clip = Locator.GetAudioManager().GetAudioClipArray(_audioLibraryClip)[_clipArrayIndex];
			break;
		case ClipSelectionOnPlay.MANUAL:
			if (clip == null)
			{
				clip = Locator.GetAudioManager().GetSingleAudioClip(_audioLibraryClip, getRandomClip: false);
			}
			break;
		}
		if (clip != null)
		{
			time = Mathf.Clamp(time, 0f, clip.length - 0.01f);
		}
	}

	public void SetLocalVolume(float volume)
	{
		_localVolume = volume;
		UpdateSourceVolume();
	}

	public void SetMaxVolume(float maxVolume)
	{
		_maxSourceVolume = maxVolume;
		UpdateSourceVolume();
	}

	public float GetMaxVolume()
	{
		return _maxSourceVolume;
	}

	public void FadeIn(float fadeDuration, bool fadeFromNothing = false, bool randomizePlayhead = false, float targetVolume = 1f)
	{
		if (fadeFromNothing || !_audioSource.isPlaying)
		{
			_localVolume = 0f;
			UpdateSourceVolume();
		}
		FadeTo(targetVolume, fadeDuration);
		if (randomizePlayhead && _localVolume < 0.001f)
		{
			RandomizePlayhead();
		}
	}

	public void FadeInToLibraryVolume(float fadeDuration, bool fadeFromNothing = false, bool randomizePlayhead = false)
	{
		float targetVolume = Locator.GetAudioManager().GetAudioEntry(_audioLibraryClip).volume;
		FadeIn(fadeDuration, fadeFromNothing, randomizePlayhead, targetVolume);
	}

	public void FadeOut(float fadeDuration, FadeOutCompleteAction fadeCompleteAction = FadeOutCompleteAction.STOP, float targetVolume = 0f)
	{
		_isLocalFading = false;
		if (_localVolume > 0f)
		{
			FadeTo(targetVolume, fadeDuration, fadeCompleteAction);
		}
	}

	public void FadeTo(float targetVolume, float fadeDuration, FadeOutCompleteAction fadeCompleteAction = FadeOutCompleteAction.CONTINUE)
	{
		if (_audioSource.isActiveAndEnabled && !_audioSource.isPlaying && targetVolume > 0f)
		{
			Play();
		}
		if (fadeDuration < 0.001f)
		{
			_localVolume = targetVolume;
			UpdateSourceVolume();
			ExecuteFadeOutCompleteAction(fadeCompleteAction);
			return;
		}
		_initFadeVolume = _localVolume;
		_targetFadeVolume = targetVolume;
		_fadeDuration = fadeDuration;
		_initFadeTime = Time.unscaledTime;
		_fadeFraction = 0f;
		_isLocalFading = true;
		_hasFadedOut = false;
		_fadeOutCompleteAction = fadeCompleteAction;
		base.enabled = true;
	}

	public void RandomizePlayhead()
	{
		if (!_audioSource.isPlaying)
		{
			Debug.LogError("Cannot randomize playhead unless audio source is playing", this);
		}
		else if (_audioSource != null && _audioSource.clip != null)
		{
			_audioSource.time = Random.Range(0f, _audioSource.clip.length);
		}
	}

	private void Update()
	{
		if (_isLocalFading)
		{
			UpdateLocalFade();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void UpdateLocalFade()
	{
		if (_fadeFraction < 1f)
		{
			_fadeFraction = Mathf.InverseLerp(_initFadeTime, _initFadeTime + _fadeDuration, Time.unscaledTime);
			_localVolume = Mathf.Lerp(_initFadeVolume, _targetFadeVolume, Mathf.SmoothStep(0f, 1f, _fadeFraction));
			UpdateSourceVolume();
		}
		else if (_targetFadeVolume <= 0f)
		{
			if (!_hasFadedOut)
			{
				_fadeOutTime = Time.unscaledTime;
				_hasFadedOut = true;
			}
			else if (_hasFadedOut && Time.unscaledTime > _fadeOutTime + 0.1f)
			{
				ExecuteFadeOutCompleteAction(_fadeOutCompleteAction);
			}
		}
		else
		{
			_isLocalFading = false;
		}
	}

	private void ExecuteFadeOutCompleteAction(FadeOutCompleteAction action)
	{
		switch (action)
		{
		case FadeOutCompleteAction.STOP:
			Stop();
			break;
		case FadeOutCompleteAction.PAUSE:
			Pause();
			break;
		}
	}

	private void UpdateSourceVolume()
	{
		if (_audioSource != null)
		{
			_audioSource.volume = _maxSourceVolume * _localVolume;
		}
	}

	private void OnGamePaused()
	{
		_audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
	}

	private void OnGameUnpaused()
	{
		_audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
	}
}
