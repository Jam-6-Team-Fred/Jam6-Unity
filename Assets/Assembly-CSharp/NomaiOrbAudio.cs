using UnityEngine;

public class NomaiOrbAudio : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _draggingOrbSource;

	[SerializeField]
	private OWAudioSource _rollingOrbSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	private void Start()
	{
		if (_oneShotSource != null)
		{
			_oneShotSource.dopplerLevel = 0f;
		}
		if (_draggingOrbSource == null)
		{
			Debug.LogWarning("NomaiOrbAudio: No AudioSource for dragging!", this);
		}
		else
		{
			_draggingOrbSource.SetMaxVolume(0.5f);
			_draggingOrbSource.AssignAudioLibraryClip(AudioType.NomaiOrbDragging_LP);
			_draggingOrbSource.loop = true;
		}
		if (_rollingOrbSource == null)
		{
			Debug.LogWarning("NomaiOrbAudio: No AudioSource for rolling!", this);
			return;
		}
		_rollingOrbSource.SetMaxVolume(0.2f);
		_rollingOrbSource.AssignAudioLibraryClip(AudioType.NomaiOrbRolling_LP);
		_rollingOrbSource.loop = true;
	}

	public void UpdateMovementAudio(bool isBeingDragged, float speedFraction)
	{
		if (!(_draggingOrbSource == null) && !(_rollingOrbSource == null))
		{
			float num = (isBeingDragged ? Mathf.InverseLerp(0.1f, 0.5f, speedFraction) : 0f);
			float num2 = Mathf.InverseLerp(0.1f, 0.5f, speedFraction);
			_draggingOrbSource.SetLocalVolume(Mathf.MoveTowards(_draggingOrbSource.GetLocalVolume(), num, 2f * Time.deltaTime));
			_rollingOrbSource.SetLocalVolume(Mathf.MoveTowards(_rollingOrbSource.GetLocalVolume(), num2, 2f * Time.deltaTime));
			if (!_draggingOrbSource.isPlaying && num > 0f)
			{
				_draggingOrbSource.SetLocalVolume(0f);
				_draggingOrbSource.Play();
			}
			else if (_draggingOrbSource.isPlaying && num <= 0f && _draggingOrbSource.volume <= 0f)
			{
				_draggingOrbSource.Stop();
			}
			if (!_rollingOrbSource.isPlaying && num2 > 0f)
			{
				_rollingOrbSource.SetLocalVolume(0f);
				_rollingOrbSource.Play();
			}
			else if (_rollingOrbSource.isPlaying && num2 <= 0f && _rollingOrbSource.volume <= 0f)
			{
				_rollingOrbSource.Stop();
			}
		}
	}

	public void StopAllAudio()
	{
		if (_draggingOrbSource != null && _rollingOrbSource != null)
		{
			_draggingOrbSource.SetLocalVolume(0f);
			_draggingOrbSource.Stop();
			_rollingOrbSource.SetLocalVolume(0f);
			_rollingOrbSource.Stop();
		}
	}

	public void PlayStartDragClip()
	{
		if (_oneShotSource != null)
		{
			_oneShotSource.PlayOneShot(AudioType.NomaiOrbStartDrag);
		}
	}

	public void PlaySlotActivatedClip()
	{
		if (_oneShotSource != null)
		{
			_oneShotSource.PlayOneShot(AudioType.NomaiOrbSlotActivated);
		}
	}
}
