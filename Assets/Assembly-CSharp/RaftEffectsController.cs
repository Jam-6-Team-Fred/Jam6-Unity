using UnityEngine;

public class RaftEffectsController : MonoBehaviour
{
	public const float MAX_PLAYER_SQR_DISTANCE = 2500f;

	private const float AGROUND_SPEED_THRESHOLD = 1f;

	[SerializeField]
	private RaftController _raftController;

	[SerializeField]
	private RaftMovementAudioController _movementAudioController;

	[SerializeField]
	private ImpactSensor _impactSensor;

	[SerializeField]
	private float _lightImpactSpeedThreshold;

	[SerializeField]
	private float _mediumImpactSpeedThreshold;

	[SerializeField]
	private float _heavyImpactSpeedThreshold;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource[] _impactSources;

	[SerializeField]
	private ParticleSystem[] _churnParticles;

	private OWRigidbody _raftBody;

	private float _lastImpactTime;

	private float _lastAgroundUpdateTime;

	private float _lastAgroundTime;

	private float _lastSpeed;

	private void Start()
	{
		_impactSensor.OnImpact += OnImpact;
		_raftBody = _raftController.GetBody();
	}

	private void OnDestroy()
	{
		_impactSensor.OnImpact -= OnImpact;
	}

	public void StopAllEffects()
	{
		for (int i = 0; i < _churnParticles.Length; i++)
		{
			_churnParticles[i].Stop();
		}
		_movementAudioController.UpdateMovementAudio(shouldPlay: false, Vector3.zero, isMaxAccel: false);
	}

	public void UpdateGroundedAudio(RaftFluidDetector fluidDetector)
	{
		float magnitude = (_raftBody.GetVelocity() - _raftBody.GetOrigParentBody().GetPointVelocity(_raftBody.GetPosition())).magnitude;
		float num = (magnitude - _lastSpeed) / Time.fixedDeltaTime;
		_lastSpeed = magnitude;
		float num2 = Time.time - _lastAgroundUpdateTime;
		_lastAgroundUpdateTime = Time.time;
		if (!(num2 > 0.5f) && num < 0f - magnitude && magnitude > 1f && Time.time > _lastAgroundTime + 3f && fluidDetector.IsAnyPointGrounded())
		{
			_oneShotSource.PlayOneShot(AudioType.Raft_RunAground);
			_lastAgroundTime = Time.time;
		}
	}

	public void UpdateMovementAudio(bool allowMovement, LightSensor[] lightSensors)
	{
		bool flag = false;
		for (int i = 0; i < lightSensors.Length; i++)
		{
			if (lightSensors[i].IsIlluminated())
			{
				flag = true;
			}
			bool flag2 = allowMovement && lightSensors[i].IsIlluminated();
			if (_churnParticles[i].isPlaying && !flag2)
			{
				_churnParticles[i].Stop();
			}
			else if (!_churnParticles[i].isPlaying && flag2)
			{
				_churnParticles[i].Play();
			}
		}
		Vector3 localAcceleration = _raftController.GetLocalAcceleration();
		bool isMaxAccel = allowMovement && Mathf.Abs(localAcceleration.x) > 0.01f && Mathf.Abs(localAcceleration.z) > 0.01f;
		_movementAudioController.UpdateMovementAudio(allowMovement && flag, localAcceleration, isMaxAccel);
	}

	public void PlayRaftPush()
	{
		_oneShotSource.PlayOneShot(AudioType.Raft_Push);
	}

	private void OnImpact(ImpactData impactData)
	{
		if (!(impactData.speed < _lightImpactSpeedThreshold) && !impactData.otherBody.CompareTag("Player"))
		{
			AudioType audioType = AudioType.Raft_Impact_Light;
			if (impactData.speed >= _heavyImpactSpeedThreshold)
			{
				audioType = AudioType.Raft_Impact_Heavy;
			}
			else if (impactData.speed >= _mediumImpactSpeedThreshold)
			{
				audioType = AudioType.Raft_Impact_Medium;
			}
			PlayImpactAtPosition(audioType, 1f, Random.Range(0.9f, 1.1f), impactData.point);
			if (Locator.GetPlayerController().GetGroundBody() == _raftBody)
			{
				MonoBehaviour.print("raft impact speed: " + impactData.speed);
				RumbleManager.PulseRaftImpact(audioType);
			}
		}
	}

	public void PlayImpactAtPosition(AudioType audioType, float volume, float pitch, Vector3 worldPos)
	{
		if (Time.time - _lastImpactTime < 0.5f)
		{
			return;
		}
		for (int i = 0; i < _impactSources.Length; i++)
		{
			if (!_impactSources[i].isPlaying)
			{
				_lastImpactTime = Time.time;
				_impactSources[i].transform.position = worldPos;
				_impactSources[i].SetLocalVolume(volume);
				_impactSources[i].pitch = pitch;
				_impactSources[i].clip = Locator.GetAudioManager().GetSingleAudioClip(audioType);
				_impactSources[i].Play();
				break;
			}
		}
	}
}
