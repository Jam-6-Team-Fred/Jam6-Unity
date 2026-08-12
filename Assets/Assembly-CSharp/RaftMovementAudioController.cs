using UnityEngine;

public class RaftMovementAudioController : MonoBehaviour
{
	private const float c_fadeDuration = 1f;

	private const float c_oneShotDelay = 0.7f;

	[SerializeField]
	private OWAudioSource[] _oneShotPool;

	[SerializeField]
	private OWAudioSource[] _loopingPool;

	private bool _playing;

	private int _poolIndex;

	private Vector3 _localAudioPosition = Vector3.zero;

	private float _fadeOutTime;

	public void UpdateMovementAudio(bool shouldPlay, Vector3 localAcceleration, bool isMaxAccel)
	{
		if (_playing != shouldPlay)
		{
			_playing = shouldPlay;
			if (shouldPlay)
			{
				Vector3 vector = AccelerationToAudioPosition(localAcceleration);
				IncrementLoopIndex();
				_loopingPool[_poolIndex].transform.localPosition = vector;
				_loopingPool[_poolIndex].FadeIn(1f, fadeFromNothing: false, randomizePlayhead: true, isMaxAccel ? 1f : 0.5f);
				if (Time.time > _fadeOutTime + 0.5f)
				{
					_oneShotPool[_poolIndex].transform.localPosition = vector;
					_oneShotPool[_poolIndex].PlayOneShot(AudioType.Raft_Move_Start, isMaxAccel ? 1f : 0.5f);
				}
				_localAudioPosition = vector;
			}
			else
			{
				_loopingPool[_poolIndex].FadeOut(1f);
				_fadeOutTime = Time.time;
			}
		}
		else
		{
			if (!shouldPlay)
			{
				return;
			}
			Vector3 vector2 = AccelerationToAudioPosition(localAcceleration);
			if (Vector3.Distance(vector2, _localAudioPosition) > 0.01f)
			{
				_loopingPool[_poolIndex].FadeOut(1f);
				IncrementLoopIndex();
				_loopingPool[_poolIndex].transform.localPosition = vector2;
				_loopingPool[_poolIndex].FadeIn(1f, fadeFromNothing: false, randomizePlayhead: true, isMaxAccel ? 1f : 0.5f);
				if (Mathf.Abs(vector2.x) > 0f && Mathf.Abs(vector2.z) > 0f && Vector3.Angle(vector2, _localAudioPosition) > 135f)
				{
					_oneShotPool[_poolIndex].transform.localPosition = vector2;
					_oneShotPool[_poolIndex].PlayOneShot(AudioType.Raft_Move_Start, isMaxAccel ? 1f : 0.5f);
				}
				_localAudioPosition = vector2;
			}
		}
	}

	private Vector3 AccelerationToAudioPosition(Vector3 localAcceleration)
	{
		if (Mathf.Abs(localAcceleration.x) < 0.01f && Mathf.Abs(localAcceleration.z) < 0.01f)
		{
			return Vector3.zero;
		}
		return localAcceleration.normalized * 4f;
	}

	private void IncrementLoopIndex()
	{
		_poolIndex++;
		if (_poolIndex > _loopingPool.Length - 1)
		{
			_poolIndex = 0;
		}
	}
}
