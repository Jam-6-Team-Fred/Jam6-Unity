using UnityEngine;

public class RaftMusicController : MonoBehaviour
{
	[SerializeField]
	private RingWorldController _ringWorldController;

	[SerializeField]
	private OWRingRiverCollider _riverCollider;

	[SerializeField]
	private OWAudioSource _riverSource;

	[SerializeField]
	private OWAudioSource _calmSource;

	[Space]
	[SerializeField]
	private OWTriggerVolume[] _muteVolumes;

	[SerializeField]
	private float _startReservoirDegrees = 300f;

	private float _lastRaftInWaterTime = float.NegativeInfinity;

	private bool _isPlaying;

	private bool _isCalm;

	private bool _stoppedByPause;

	private float _stopTime = float.NegativeInfinity;

	private float _trackStopTime;

	private void Awake()
	{
		_ringWorldController.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterRingWorld);
		_ringWorldController.OnPlayerExit += new OWEvent.OWCallback(OnPlayerExitRingWorld);
	}

	private void Start()
	{
		_riverSource.SetLocalVolume(0f);
		_calmSource.SetLocalVolume(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_ringWorldController.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterRingWorld);
		_ringWorldController.OnPlayerExit -= new OWEvent.OWCallback(OnPlayerExitRingWorld);
	}

	private void Update()
	{
		bool shouldPlayCalm;
		bool flag = CheckShouldBePlaying(_isPlaying, out shouldPlayCalm);
		if (flag && !_isPlaying)
		{
			float fadeDuration = (_stoppedByPause ? 1f : 5f);
			_isCalm = shouldPlayCalm;
			OWAudioSource oWAudioSource = (_isCalm ? _calmSource : _riverSource);
			oWAudioSource.FadeInToLibraryVolume(fadeDuration);
			if (oWAudioSource.GetLocalVolume() <= 0.001f)
			{
				oWAudioSource.time = GetTrackResumeTime(_isCalm);
			}
			_isPlaying = true;
			_stoppedByPause = false;
			Locator.GetAudioMixer().MixRaftMusic();
		}
		else if (!flag && _isPlaying)
		{
			float fadeDuration2 = 5f;
			if (OWTime.IsPaused())
			{
				_stoppedByPause = true;
				fadeDuration2 = 1f;
			}
			_trackStopTime = (_isCalm ? _calmSource.time : _riverSource.time);
			_stopTime = Time.time;
			_isPlaying = false;
			_isCalm = false;
			_riverSource.FadeOut(fadeDuration2);
			_calmSource.FadeOut(fadeDuration2);
			Locator.GetAudioMixer().UnmixRaftMusic();
		}
		if (_isPlaying && shouldPlayCalm != _isCalm)
		{
			_isCalm = shouldPlayCalm;
			OWAudioSource obj = (shouldPlayCalm ? _calmSource : _riverSource);
			OWAudioSource oWAudioSource2 = (shouldPlayCalm ? _riverSource : _calmSource);
			obj.FadeInToLibraryVolume(5f);
			obj.time = oWAudioSource2.time;
			oWAudioSource2.FadeOut(5f);
		}
	}

	private bool CheckShouldBePlaying(bool isPlaying, out bool shouldPlayCalm)
	{
		shouldPlayCalm = false;
		if (OWTime.IsPaused())
		{
			return false;
		}
		RaftController occupiedRaft = Locator.GetOccupiedRaft();
		if (occupiedRaft == null || occupiedRaft.IsDockingOrDocked() || !occupiedRaft.IsPlayerRiding(raftMustBeInWater: false))
		{
			return false;
		}
		if (occupiedRaft.InWater())
		{
			_lastRaftInWaterTime = Time.time;
		}
		if (Time.time > _lastRaftInWaterTime + 1f)
		{
			return false;
		}
		Vector3 position = occupiedRaft.GetBody().GetPosition();
		float num = _riverCollider.WorldPositionToDegrees(position);
		bool flag = _riverCollider.GetFloodLerp() <= 0.001f && num > _startReservoirDegrees && num <= 360f;
		Vector3 vector = occupiedRaft.GetBody().GetVelocity() - _ringWorldController.GetRingWorldBody().GetPointVelocity(position);
		Vector3 upVectorAtPosition = _riverCollider.GetUpVectorAtPosition(position);
		vector = Vector3.ProjectOnPlane(vector, upVectorAtPosition);
		if (flag)
		{
			if (!isPlaying && vector.magnitude < 5f)
			{
				return false;
			}
		}
		else
		{
			Vector3 normalized = Vector3.Cross(upVectorAtPosition, _riverCollider.transform.up).normalized;
			Vector3 lhs = Vector3.Project(vector, normalized);
			float num2 = lhs.magnitude * Mathf.Sign(Vector3.Dot(lhs, normalized));
			float num3 = (isPlaying ? 0.5f : 5f);
			if (num2 < num3)
			{
				return false;
			}
		}
		float floodWaveDegree = _riverCollider.GetFloodWaveDegree();
		if (floodWaveDegree > 0f && floodWaveDegree < 270f && Mathf.Abs(floodWaveDegree - num) < 60f)
		{
			return false;
		}
		for (int i = 0; i < _muteVolumes.Length; i++)
		{
			if (_muteVolumes[i].IsTrackingObject(Locator.GetPlayerCameraDetector()))
			{
				return false;
			}
		}
		shouldPlayCalm = flag;
		return true;
	}

	private float GetTrackResumeTime(bool isCalm)
	{
		if (Time.time < _stopTime + 60f)
		{
			return _trackStopTime;
		}
		if (isCalm)
		{
			if (_trackStopTime < 9f)
			{
				return 0f;
			}
			if (_trackStopTime < 25f)
			{
				return 18f;
			}
			if (_trackStopTime < 38f)
			{
				return 33f;
			}
			if (_trackStopTime < 52f)
			{
				return 43f;
			}
			if (_trackStopTime < 66f)
			{
				return 61f;
			}
			if (_trackStopTime < 82f)
			{
				return 72f;
			}
			if (_trackStopTime < 106f)
			{
				return 92f;
			}
		}
		else
		{
			if (_trackStopTime < 25f)
			{
				return 0f;
			}
			if (_trackStopTime < 45f)
			{
				return 25f;
			}
			if (_trackStopTime < 66f)
			{
				return 45f;
			}
			if (_trackStopTime < 106f)
			{
				return 93f;
			}
		}
		return 0f;
	}

	private void OnPlayerEnterRingWorld()
	{
		base.enabled = true;
	}

	private void OnPlayerExitRingWorld()
	{
		base.enabled = false;
		_stoppedByPause = false;
		_isCalm = false;
		_isPlaying = false;
		_trackStopTime = _riverSource.time;
		_stopTime = Time.time;
		_riverSource.FadeOut(5f, OWAudioSource.FadeOutCompleteAction.PAUSE);
		Locator.GetAudioMixer().UnmixRaftMusic();
	}
}
