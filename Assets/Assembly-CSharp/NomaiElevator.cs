using UnityEngine;

public class NomaiElevator : MonoBehaviour
{
	[SerializeField]
	protected NomaiInterfaceSlot[] _upSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	protected NomaiInterfaceSlot[] _downSwitches = new NomaiInterfaceSlot[0];

	[SerializeField]
	private float _maxSpeed;

	[SerializeField]
	private float _acceleration;

	[SerializeField]
	private float _elevationGain;

	[SerializeField]
	private Transform _elevatorRoot;

	[SerializeField]
	private OWAudioSource _audioSource;

	private float _startingLowerPosition;

	private float _speed;

	private bool _goingUp;

	private bool _goingDown;

	protected virtual void Start()
	{
		base.enabled = false;
		if (_audioSource != null)
		{
			_audioSource.SetLocalVolume(0f);
		}
		_goingUp = false;
		_goingDown = false;
		for (int i = 0; i < _upSwitches.Length; i++)
		{
			_upSwitches[i].OnSlotActivated += OnUpSwitchTriggered;
		}
		for (int j = 0; j < _downSwitches.Length; j++)
		{
			_downSwitches[j].OnSlotActivated += OnDownSwitchTriggered;
		}
		_startingLowerPosition = _elevatorRoot.localPosition.y;
	}

	protected void OnDestroy()
	{
		for (int i = 0; i < _upSwitches.Length; i++)
		{
			_upSwitches[i].OnSlotActivated -= OnUpSwitchTriggered;
		}
		for (int j = 0; j < _downSwitches.Length; j++)
		{
			_downSwitches[j].OnSlotActivated -= OnDownSwitchTriggered;
		}
	}

	private void FixedUpdate()
	{
		float target = 0f;
		if (_goingUp)
		{
			target = _maxSpeed;
		}
		else if (_goingDown)
		{
			target = 0f - _maxSpeed;
		}
		_speed = Mathf.MoveTowards(_speed, target, Time.deltaTime * _acceleration);
		float num = _elevatorRoot.localPosition.y + _speed * Time.deltaTime;
		if (num > _startingLowerPosition + _elevationGain)
		{
			_goingUp = false;
			_speed = 0f;
			num = _startingLowerPosition + _elevationGain;
		}
		else if (num < _startingLowerPosition)
		{
			_goingDown = false;
			_speed = 0f;
			num = _startingLowerPosition;
		}
		_elevatorRoot.SetLocalPositionY(num);
		if (!_goingDown && !_goingUp)
		{
			if (_audioSource != null)
			{
				_audioSource.FadeOut(1f);
			}
			base.enabled = false;
		}
	}

	protected void OnUpSwitchTriggered(NomaiInterfaceSlot slot)
	{
		base.enabled = true;
		_goingUp = true;
		_goingDown = false;
		if (_audioSource != null)
		{
			_audioSource.FadeIn(5f);
		}
	}

	protected void OnDownSwitchTriggered(NomaiInterfaceSlot slot)
	{
		base.enabled = true;
		_goingUp = false;
		_goingDown = true;
		if (_audioSource != null)
		{
			_audioSource.FadeIn(5f);
		}
	}
}
