using UnityEngine;

public class SlidingDoor : AbstractDoor
{
	[SerializeField]
	private Transform _doorTransform;

	[SerializeField]
	private Transform _closedSocket;

	[SerializeField]
	private Transform _openSocket;

	[Space]
	[SerializeField]
	private float _speed = 2f;

	[Space]
	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	public override void SetOpenImmediate(bool open)
	{
		base.SetOpenImmediate(open);
		_doorTransform.position = (_open ? _openSocket.position : _closedSocket.position);
	}

	public override void Open()
	{
		if (!_open)
		{
			if (!base.enabled && _oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Start);
				_loopingAudio.FadeIn(0.2f);
			}
			base.Open();
		}
	}

	public override void Close()
	{
		if (_open)
		{
			if (!base.enabled && _oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Start);
				_loopingAudio.FadeIn(0.2f);
			}
			base.Close();
		}
	}

	private void FixedUpdate()
	{
		Vector3 vector = (_open ? _openSocket.position : _closedSocket.position);
		_doorTransform.position = Vector3.MoveTowards(_doorTransform.position, vector, Time.deltaTime * _speed);
		if (Vector3.Distance(_doorTransform.position, vector) < 0.001f)
		{
			_doorTransform.position = vector;
			base.enabled = false;
			if (_oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Stop);
				_loopingAudio.FadeOut(0.2f);
			}
		}
	}
}
