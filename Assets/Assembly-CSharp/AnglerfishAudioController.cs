using UnityEngine;

public class AnglerfishAudioController : MonoBehaviour
{
	[SerializeField]
	private OWAudioSource _loopSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _longRangeOneShotSource;

	private AnglerfishController _anglerfishController;

	private static float s_lastDetectTime;

	private void Awake()
	{
		_anglerfishController = this.GetAttachedOWRigidbody().GetRequiredComponent<AnglerfishController>();
		_anglerfishController.OnChangeAnglerState += OnChangeAnglerState;
		_anglerfishController.OnAnglerSuspended += OnAnglerSuspended;
		_anglerfishController.OnAnglerUnsuspended += OnAnglerUnsuspended;
	}

	private void OnDestroy()
	{
		_anglerfishController.OnChangeAnglerState -= OnChangeAnglerState;
		_anglerfishController.OnAnglerSuspended -= OnAnglerSuspended;
		_anglerfishController.OnAnglerUnsuspended -= OnAnglerUnsuspended;
	}

	private void OnAnglerSuspended(AnglerfishController.AnglerState anglerState)
	{
		if (_loopSource.GetAudioSource() != null)
		{
			_loopSource.clip = null;
			_loopSource.Stop();
		}
	}

	private void OnAnglerUnsuspended(AnglerfishController.AnglerState anglerState)
	{
		UpdateLoopingAudio(anglerState);
	}

	private void OnChangeAnglerState(AnglerfishController.AnglerState anglerState)
	{
		UpdateLoopingAudio(anglerState);
		switch (anglerState)
		{
		case AnglerfishController.AnglerState.Investigating:
			_longRangeOneShotSource.pitch = Random.Range(0.8f, 1f);
			_longRangeOneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance);
			break;
		case AnglerfishController.AnglerState.Chasing:
			if (Time.time > s_lastDetectTime + 2f)
			{
				s_lastDetectTime = Time.time;
				_oneShotSource.pitch = Random.Range(0.8f, 1f);
				_oneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectTarget);
			}
			else
			{
				MonoBehaviour.print("ANGLER DETECT TARGET SOUND BLOCKED");
			}
			break;
		}
	}

	private void UpdateLoopingAudio(AnglerfishController.AnglerState anglerState)
	{
		switch (anglerState)
		{
		case AnglerfishController.AnglerState.Lurking:
			_loopSource.AssignAudioLibraryClip(AudioType.DBAnglerfishLurking_LP);
			_loopSource.FadeIn(0.5f, fadeFromNothing: true);
			break;
		case AnglerfishController.AnglerState.Chasing:
			_loopSource.AssignAudioLibraryClip(AudioType.DBAnglerfishChasing_LP);
			_loopSource.FadeIn(0.5f, fadeFromNothing: true);
			break;
		default:
			_loopSource.FadeOut(0.5f);
			break;
		}
	}
}
