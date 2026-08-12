using UnityEngine;

public class RotatingDoubleDoor : AbstractDoor
{
	[SerializeField]
	private float _openDegrees;

	[SerializeField]
	private float _closedDegrees = 20f;

	[Space]
	[SerializeField]
	private float _speed = 5f;

	[Space]
	[SerializeField]
	private Transform _leftDoor;

	[SerializeField]
	private Transform _rightDoor;

	[Space]
	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	public override void SetOpenImmediate(bool open)
	{
		base.SetOpenImmediate(open);
		_leftDoor.localEulerAngles = Vector3.up * _openDegrees;
		_rightDoor.localEulerAngles = Vector3.up * (0f - _openDegrees);
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
		float num = (_open ? _openDegrees : _closedDegrees);
		_leftDoor.localEulerAngles = Vector3.up * Mathf.MoveTowards(OWMath.WrapAngle(_leftDoor.localEulerAngles.y), num, _speed * Time.deltaTime);
		_rightDoor.localEulerAngles = Vector3.up * Mathf.MoveTowards(OWMath.WrapAngle(_rightDoor.localEulerAngles.y), 0f - num, _speed * Time.deltaTime);
		if (Mathf.Abs(_leftDoor.localEulerAngles.y - num) < 0.001f)
		{
			base.enabled = false;
			_leftDoor.localEulerAngles = Vector3.up * num;
			_rightDoor.localEulerAngles = Vector3.up * (0f - num);
			if (_oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Stop);
				_loopingAudio.FadeOut(0.2f);
			}
		}
	}
}
