using UnityEngine;

public class RiverPathAudioSource : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource[] _audioPool;

	[Space]
	[SerializeField]
	private float _muffledVolume = 0.2f;

	[SerializeField]
	private float _crossfadeInDuration = 2f;

	[SerializeField]
	private float _crossfadeOutDuration = 3f;

	[SerializeField]
	private float _crossfadeThreshold = 10f;

	[SerializeField]
	private float _minShoreFollowSpeed = 10f;

	private bool _playing;

	private bool _muffled;

	private int _activeIndex;

	private void Start()
	{
		for (int i = 0; i < _audioPool.Length; i++)
		{
			_audioPool[i].SetLocalVolume(0f);
		}
	}

	public void SetPlaying(bool play, float fadeDuration = 2f)
	{
		if (_playing == play)
		{
			return;
		}
		_playing = play;
		if (play)
		{
			_audioPool[_activeIndex].FadeTo(_muffled ? _muffledVolume : 1f, fadeDuration);
			return;
		}
		for (int i = 0; i < _audioPool.Length; i++)
		{
			_audioPool[i].FadeOut(fadeDuration);
		}
	}

	public void SetMuffled(bool muffle, float fadeDuration = 1f)
	{
		if (_muffled != muffle)
		{
			_muffled = muffle;
			if (_playing)
			{
				_audioPool[_activeIndex].FadeTo(_muffled ? _muffledVolume : 1f, fadeDuration);
			}
		}
	}

	public void UpdatePosition(Vector3 targetPosition, bool overWater, Vector3 playerPosition, OWRigidbody parentBody)
	{
		if (!_playing)
		{
			return;
		}
		if (overWater || _audioPool[_activeIndex].GetLocalVolume() <= 0f)
		{
			_audioPool[_activeIndex].transform.position = targetPosition;
			return;
		}
		Vector3 position = _audioPool[_activeIndex].transform.position;
		if (Vector3.Distance(position, targetPosition) < _crossfadeThreshold)
		{
			float magnitude = (Locator.GetPlayerBody().GetVelocity() - parentBody.GetPointVelocity(playerPosition)).magnitude;
			float num = Mathf.Max(_minShoreFollowSpeed, magnitude * 2f);
			_audioPool[_activeIndex].transform.position = Vector3.MoveTowards(position, targetPosition, num * Time.deltaTime);
			return;
		}
		_audioPool[_activeIndex].FadeOut(_crossfadeOutDuration);
		_activeIndex++;
		if (_activeIndex > _audioPool.Length - 1)
		{
			_activeIndex = 0;
		}
		_audioPool[_activeIndex].transform.position = targetPosition;
		_audioPool[_activeIndex].SetLocalVolume(0f);
		_audioPool[_activeIndex].FadeTo(_muffled ? _muffledVolume : 1f, _crossfadeInDuration);
		_audioPool[_activeIndex].RandomizePlayhead();
	}
}
