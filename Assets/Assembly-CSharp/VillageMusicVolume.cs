using UnityEngine;

public class VillageMusicVolume : AudioVolume
{
	private const float _daytimeHalfAngle = 94f;

	private const float _musicDurationDegrees = 183f;

	[SerializeField]
	private Transform _daytimeLocationTransform;

	private OWRigidbody _planetRigidbody;

	private Transform _planetTransform;

	private float _musicTime;

	private bool _isDaytimeWindow;

	protected override void Reset()
	{
		AudioSource component = GetComponent<AudioSource>();
		component.loop = false;
		component.playOnAwake = false;
	}

	protected override void Awake()
	{
		base.Awake();
		_planetRigidbody = base.gameObject.GetAttachedOWRigidbody();
		_planetTransform = _planetRigidbody.transform;
		GlobalMessenger.AddListener("GamePaused", OnGamePaused);
		GlobalMessenger.AddListener("GameUnpaused", OnGameUnpaused);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
		GlobalMessenger.RemoveListener("GameUnpaused", OnGameUnpaused);
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	public override void Activate()
	{
		base.Activate();
		base.enabled = true;
		UpdateDaytimeWindow();
		if (_isDaytimeWindow)
		{
			_owAudioSrc.FadeIn(_fadeSeconds);
			_owAudioSrc.time = _musicTime;
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		base.enabled = false;
	}

	protected override void PlayAudio()
	{
	}

	private void OnGamePaused()
	{
		_owAudioSrc.FadeOut(_fadeSeconds);
	}

	private void OnGameUnpaused()
	{
		UpdateDaytimeWindow();
		if (_isDaytimeWindow && _isActive)
		{
			_owAudioSrc.FadeIn(_fadeSeconds);
			_owAudioSrc.time = _musicTime;
		}
	}

	private void OnStartFastForward()
	{
		OnGamePaused();
	}

	private void OnEndFastForward()
	{
		OnGameUnpaused();
	}

	private void Update()
	{
		if (!OWTime.IsPaused() && !PlayerState.IsFastForwarding())
		{
			bool isDaytimeWindow = _isDaytimeWindow;
			UpdateDaytimeWindow();
			if (!isDaytimeWindow && _isDaytimeWindow)
			{
				_owAudioSrc.Stop();
				_owAudioSrc.SetLocalVolume(1f);
				_owAudioSrc.Play();
			}
		}
	}

	private void UpdateDaytimeWindow()
	{
		Vector3 normalized = _planetRigidbody.GetAngularVelocity().normalized;
		Vector3 from = Vector3.ProjectOnPlane(_daytimeLocationTransform.position - _planetTransform.position, normalized);
		Vector3 to = Vector3.ProjectOnPlane(Locator.GetSunTransform().position - _planetTransform.position, normalized);
		float num = OWMath.Angle(from, to, normalized);
		_isDaytimeWindow = num > -94f && num < 94f;
		if (_isDaytimeWindow)
		{
			float t = Mathf.Clamp01((num + 94f) / 183f);
			_musicTime = Mathf.Lerp(0f, _owAudioSrc.clip.length, t);
		}
		else
		{
			_musicTime = 0f;
		}
	}
}
