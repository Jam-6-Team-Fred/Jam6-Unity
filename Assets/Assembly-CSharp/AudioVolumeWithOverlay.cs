using System.Collections.Generic;
using UnityEngine;

public class AudioVolumeWithOverlay : AudioVolume
{
	private static List<int> s_nomaiRuinsIndexPool;

	private static List<int> s_dreamRuinsIndexPool;

	[SerializeField]
	private AudioType _mainTrackAudio;

	[SerializeField]
	private AudioType _overlayTrackAudio;

	[SerializeField]
	private OWAudioSource _overlayAudioSource1;

	[SerializeField]
	private OWAudioSource _overlayAudioSource2;

	[SerializeField]
	private bool _debug;

	private OWAudioSource _currentOverlayAudioSource;

	private OWAudioSource _pendingOverlayAudioSource;

	private int _pendingIndex = -1;

	private int _lastIndex = -1;

	private int _overlayAudioArrayLength;

	private List<int> _overlayIndexPool;

	protected override void Start()
	{
		if (_owAudioSrc == null)
		{
			Debug.LogError("No OWAudioSource found!", this);
		}
		if (!_initialized)
		{
			Init();
		}
		if (_overlayAudioSource1 == null || _overlayAudioSource2 == null)
		{
			Debug.LogError("Two overlay sources required for smooth transitions");
		}
		else if (_isActive)
		{
			StartPlayback();
		}
	}

	protected override void Init()
	{
		_initialized = true;
		_randomizePlayhead = false;
		_pauseOnFadeOut = false;
		_owAudioSrc.Stop();
		_owAudioSrc.SetLocalVolume(0f);
		if (_owAudioSrc.GetTrack() != OWAudioMixer.TrackName.Music)
		{
			Debug.LogError("OVERLAY AUDIO VOLUMES SHOULD ONLY BE USED FOR NOMAI RUINS MUSIC");
			Debug.Break();
		}
		_owAudioSrc.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.MANUAL);
		_owAudioSrc.AssignAudioLibraryClip(_mainTrackAudio);
		_overlayAudioSource1.Stop();
		_overlayAudioSource1.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.MANUAL);
		_overlayAudioSource1.AssignAudioLibraryClip(_overlayTrackAudio);
		_overlayAudioSource2.Stop();
		_overlayAudioSource2.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.MANUAL);
		_overlayAudioSource2.AssignAudioLibraryClip(_overlayTrackAudio);
		AudioClip[] audioClipArray = Locator.GetAudioManager().GetAudioClipArray(_overlayTrackAudio);
		_overlayAudioArrayLength = audioClipArray.Length;
		_currentOverlayAudioSource = _overlayAudioSource1;
		_pendingOverlayAudioSource = _overlayAudioSource2;
		if (_overlayTrackAudio == AudioType.NomaiRuinsOverlayTracks)
		{
			if (s_nomaiRuinsIndexPool == null)
			{
				s_nomaiRuinsIndexPool = new List<int>(_overlayAudioArrayLength);
				for (int i = 0; i < _overlayAudioArrayLength; i++)
				{
					s_nomaiRuinsIndexPool.Add(i);
				}
			}
			_overlayIndexPool = s_nomaiRuinsIndexPool;
		}
		else
		{
			if (_overlayTrackAudio != AudioType.DreamRuinsOverlayTracks)
			{
				return;
			}
			if (s_dreamRuinsIndexPool == null)
			{
				s_dreamRuinsIndexPool = new List<int>(_overlayAudioArrayLength);
				for (int j = 0; j < _overlayAudioArrayLength; j++)
				{
					s_dreamRuinsIndexPool.Add(j);
				}
			}
			_overlayIndexPool = s_dreamRuinsIndexPool;
		}
	}

	public override void Activate()
	{
		if (!_initialized)
		{
			Init();
		}
		_isActive = true;
		base.enabled = true;
		StartPlayback();
	}

	public override void Deactivate()
	{
		_isActive = false;
		base.enabled = false;
		_owAudioSrc.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_currentOverlayAudioSource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_pendingOverlayAudioSource.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_overlayIndexPool.Add(_pendingIndex);
		_pendingIndex = -1;
	}

	public override void Deactivate(float fadeSeconds)
	{
		_isActive = false;
		base.enabled = false;
		_owAudioSrc.FadeOut(fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_currentOverlayAudioSource.FadeOut(fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_pendingOverlayAudioSource.FadeOut(fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP);
		_overlayIndexPool.Add(_pendingIndex);
		_pendingIndex = -1;
	}

	private void PrepNextOverlayClip()
	{
		_pendingIndex = GetNextAudioClipIndex();
		_pendingOverlayAudioSource.SelectClip(_pendingIndex);
		float time = _owAudioSrc.time;
		float delay = _owAudioSrc.clip.length - time;
		_pendingOverlayAudioSource.PlayDelayed(delay);
	}

	private void Update()
	{
		if (_pendingOverlayAudioSource.time > 0f)
		{
			OWAudioSource currentOverlayAudioSource = _currentOverlayAudioSource;
			_currentOverlayAudioSource = _pendingOverlayAudioSource;
			_pendingOverlayAudioSource = currentOverlayAudioSource;
			PrepNextOverlayClip();
		}
		if (_debug)
		{
			if (_owAudioSrc.clip != null)
			{
				DebugText.SetText("Main Track: " + _owAudioSrc.clip.name);
				DebugText.AppendText(" Time: " + _owAudioSrc.time.ToString("F3"), newLine: false);
			}
			if (_overlayAudioSource1.clip != null)
			{
				DebugText.AppendText("Track 1: " + _overlayAudioSource1.clip.name, newLine: true);
				DebugText.AppendText(" Time: " + _overlayAudioSource1.time.ToString("F3"), newLine: false);
			}
			if (_overlayAudioSource2.clip != null)
			{
				DebugText.AppendText("Track 2: " + _overlayAudioSource2.clip.name, newLine: true);
				DebugText.AppendText(" Time: " + _overlayAudioSource2.time.ToString("F3"), newLine: false);
			}
		}
	}

	private void StartPlayback()
	{
		_overlayAudioSource1.SetLocalVolume(1f);
		_overlayAudioSource2.SetLocalVolume(1f);
		_currentOverlayAudioSource.SelectClip(GetNextAudioClipIndex());
		PrepNextOverlayClip();
		_currentOverlayAudioSource.FadeIn(_fadeSeconds, _randomizePlayhead);
		_owAudioSrc.FadeIn(_fadeSeconds, _randomizePlayhead);
	}

	private int GetNextAudioClipIndex()
	{
		if (_overlayIndexPool.Count == 0)
		{
			for (int i = 0; i < _overlayAudioArrayLength; i++)
			{
				_overlayIndexPool.Add(i);
			}
		}
		int num = Random.Range(0, _overlayIndexPool.Count);
		if (_overlayIndexPool[num] == _lastIndex && _overlayIndexPool.Count > 1)
		{
			num++;
			if (num > _overlayIndexPool.Count - 1)
			{
				num = 0;
			}
			MonoBehaviour.print("tried to play the same thing again, shifting random index by one");
		}
		int num2 = _overlayIndexPool[num];
		_overlayIndexPool.RemoveAt(num);
		_lastIndex = num2;
		return num2;
	}
}
