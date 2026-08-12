using UnityEngine;

public class RingWaveAudioController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private Transform _railPointsRoot;

	[SerializeField]
	private float _startFadeOutDegrees;

	[SerializeField]
	private float _fadeOutDuration;

	private Quaternion _startLocalRotation;

	private void Start()
	{
		_audioSource.SetLocalVolume(0f);
		_startLocalRotation = _railPointsRoot.localRotation;
		if (Locator.GetDreamWorldAudioController() != null)
		{
			Locator.GetDreamWorldAudioController().SetWaveAudioProperties(_startFadeOutDegrees);
		}
		GlobalMessenger.AddListener("DamBroken", OnDamBroken);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("DamBroken", OnDamBroken);
	}

	public void SetFloodLerp(float floodLerp)
	{
		if (_audioSource.isPlaying && !_audioSource.IsFadingOut() && floodLerp >= Mathf.InverseLerp(0f, 360f, _startFadeOutDegrees))
		{
			_audioSource.FadeOut(_fadeOutDuration);
		}
		float y = Mathf.Max(0f, Mathf.Lerp(0f, 360f, floodLerp) - 2f);
		_railPointsRoot.localRotation = _startLocalRotation * Quaternion.Euler(0f, y, 0f);
	}

	private void OnDamBroken()
	{
		_audioSource.FadeIn(5f);
	}
}
