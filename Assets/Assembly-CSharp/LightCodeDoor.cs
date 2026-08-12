using UnityEngine;

public class LightCodeDoor : MonoBehaviour
{
	private enum State
	{
		StartingPos = 0,
		MovingToStart = 1,
		TargetPos = 2,
		MovingToTarget = 3
	}

	[SerializeField]
	private LightCodeInterpreter _lightCodeInterpreter;

	[SerializeField]
	private Transform _target;

	[SerializeField]
	private bool _startsOpen;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[Space]
	[SerializeField]
	private float _speed;

	private Vector3 _startingPos;

	private bool _locked;

	private OWLight2 _light;

	private State _state;

	private void Start()
	{
		_lightCodeInterpreter.OnEnterCode += OnEnterCode;
		_startingPos = base.transform.localPosition;
		if (_startsOpen)
		{
			_state = State.TargetPos;
			base.transform.localPosition = _target.localPosition;
		}
		else
		{
			_state = State.StartingPos;
		}
		_light = GetComponentInChildren<OWLight2>();
		SetActive(toSetTo: false);
	}

	public bool IsOpen()
	{
		return _state != State.StartingPos;
	}

	private void SetActive(bool toSetTo)
	{
		base.enabled = toSetTo;
		_light.SetIntensity(toSetTo ? 1f : 0.05f);
		if (toSetTo)
		{
			_oneShotSource.PlayOneShot(AudioType.NomaiDoorStart);
		}
	}

	public void AddLightToCheckAgainst(OWLight2 toAdd)
	{
		Debug.LogError("FUNCTION NO LONGER EXISTS");
		Debug.Break();
	}

	private void OnDestroy()
	{
		_lightCodeInterpreter.OnEnterCode -= OnEnterCode;
	}

	public void CloseDoor(bool locked)
	{
		if (_state == State.MovingToTarget || _state == State.TargetPos)
		{
			_state = State.MovingToStart;
			SetActive(toSetTo: true);
		}
		if (locked && !_locked)
		{
			_locked = true;
		}
	}

	private void OnEnterCode(LightCodeName codeName)
	{
		if (codeName == LightCodeName.WAKE && !_locked && (_state == State.MovingToStart || _state == State.StartingPos))
		{
			_state = State.MovingToTarget;
			SetActive(toSetTo: true);
		}
		if (codeName == LightCodeName.SLEEP && (_state == State.MovingToTarget || _state == State.TargetPos))
		{
			_state = State.MovingToStart;
			SetActive(toSetTo: true);
		}
	}

	private void FixedUpdate()
	{
		switch (_state)
		{
		case State.MovingToStart:
		{
			Vector3 vector = _startingPos - base.transform.localPosition;
			if (vector.sqrMagnitude < _speed * _speed * Time.fixedDeltaTime)
			{
				base.transform.localPosition = _startingPos;
				SetActive(toSetTo: false);
				_state = State.StartingPos;
			}
			else
			{
				vector.Normalize();
				base.transform.localPosition = base.transform.localPosition + vector * _speed * Time.fixedDeltaTime;
			}
			break;
		}
		case State.MovingToTarget:
		{
			Vector3 vector = _target.localPosition - base.transform.localPosition;
			if (vector.sqrMagnitude < _speed * _speed * Time.fixedDeltaTime)
			{
				base.transform.localPosition = _target.localPosition;
				SetActive(toSetTo: false);
				_state = State.TargetPos;
			}
			else
			{
				vector.Normalize();
				base.transform.localPosition = base.transform.localPosition + vector * _speed * Time.fixedDeltaTime;
			}
			break;
		}
		}
	}
}
