using UnityEngine;

public class FlashbackAudioController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _baseSource;

	[SerializeField]
	private OWAudioSource _overlaySourceOne;

	private float _startUplinkTime;

	private float _endUplinkTime;

	private float _startPlaybackTime;

	private float _endPlaybackTime;

	private float _overlayOneTime;

	private float _overlayTwoTime;

	private bool _updateFlashback;

	private bool _updateMemoryUplink;

	private bool _hasPlayedEndStinger;

	private const float _endStingerDuration = 3.5f;

	private const float _fadeOutDuration = 2f;

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
	}

	public void StartMemoryUplink(float startTime, float endTime)
	{
		_baseSource.AssignAudioLibraryClip(AudioType.MemoryUplink_LP);
		_baseSource.loop = true;
		_baseSource.SetLocalVolume(0f);
		_baseSource.FadeIn(2f);
		_overlaySourceOne.AssignAudioLibraryClip(AudioType.MemoryUplink_Overlay_LP);
		_overlaySourceOne.loop = true;
		_overlaySourceOne.SetLocalVolume(0f);
		_overlaySourceOne.Play();
		_oneShotSource.PlayOneShot(AudioType.MemoryUplink_Start);
		_startUplinkTime = startTime;
		_endUplinkTime = endTime;
		_updateMemoryUplink = true;
		base.enabled = true;
	}

	private void UpdateMemoryUplink()
	{
		float num = Mathf.InverseLerp(_startUplinkTime, _endUplinkTime, Time.time);
		_overlaySourceOne.SetLocalVolume(num * num);
		if (!_hasPlayedEndStinger && Time.time >= _endUplinkTime - 3.5f)
		{
			_oneShotSource.PlayOneShot(AudioType.MemoryUplink_End);
			_hasPlayedEndStinger = true;
		}
		if (Time.time >= _endUplinkTime - 2f)
		{
			_baseSource.FadeOut(2f);
			_overlaySourceOne.FadeOut(2f);
			_updateMemoryUplink = false;
		}
	}

	public void StartFlashback()
	{
		_baseSource.Play();
		_overlaySourceOne.SetLocalVolume(0f);
		_overlaySourceOne.Play();
	}

	public void StartPlayback(float startPlaybackTime, float endPlaybackTime)
	{
		_startPlaybackTime = startPlaybackTime;
		_endPlaybackTime = endPlaybackTime;
		_overlayOneTime = Mathf.Lerp(_startPlaybackTime, _endPlaybackTime - 3.5f, 0.5f);
		_updateFlashback = true;
		base.enabled = true;
	}

	private void UpdateFlashback()
	{
		float localVolume = Mathf.InverseLerp(_overlayOneTime, _endPlaybackTime - 3.5f, Time.time);
		_overlaySourceOne.SetLocalVolume(localVolume);
		if (!_hasPlayedEndStinger && Time.time >= _endPlaybackTime - 3.5f)
		{
			_oneShotSource.PlayOneShot(AudioType.Flashback_End);
			_hasPlayedEndStinger = true;
		}
		if (Time.time >= _endPlaybackTime - 2f)
		{
			_baseSource.FadeOut(2f);
			_overlaySourceOne.FadeOut(2f);
			_updateFlashback = false;
		}
	}

	private void Update()
	{
		if (_updateMemoryUplink)
		{
			UpdateMemoryUplink();
		}
		else if (_updateFlashback)
		{
			UpdateFlashback();
		}
		else
		{
			base.enabled = false;
		}
	}
}
