using UnityEngine;

public class NomaiMultiPartDoor : NomaiRotator
{
	protected enum StartingRotationState
	{
		OPEN = 0,
		CLOSED = 1
	}

	protected enum RotationState
	{
		OPEN = 0,
		CLOSED = 1,
		CYCLING_OPEN = 2,
		CYCLING_CLOSED = 3
	}

	protected enum CycleType
	{
		POSITIVE_OPENCLOSE = 0,
		NEGATIVE_OPENCLOSE = 1,
		POS_OPEN_NEG_CLOSE = 2,
		NEG_OPEN_POS_CLOSE = 3
	}

	public delegate void DoorEvent();

	[Space(10f)]
	[SerializeField]
	protected NomaiInterfaceOrb[] _listInterfaceOrb;

	[SerializeField]
	protected StartingRotationState _rotationOnStart;

	[SerializeField]
	protected CycleType _cycleType;

	[Space(10f)]
	[SerializeField]
	protected RotationObject[] _rotationObjectList;

	protected RotationState _currentRotationState;

	protected float _cycleTimer;

	protected float _totalCycleTime;

	protected Quaternion[] _startRotationQuats;

	protected Quaternion[] _stopRotationQuats;

	protected bool[] _rotationObjectsInMotion;

	public event DoorEvent OnOpen;

	public event DoorEvent OnClose;

	protected virtual void Awake()
	{
		switch (_rotationOnStart)
		{
		case StartingRotationState.OPEN:
			_currentRotationState = RotationState.OPEN;
			break;
		case StartingRotationState.CLOSED:
			_currentRotationState = RotationState.CLOSED;
			break;
		}
		_startRotationQuats = new Quaternion[_rotationObjectList.Length];
		_stopRotationQuats = new Quaternion[_rotationObjectList.Length];
		_rotationObjectsInMotion = new bool[_rotationObjectList.Length];
		float num = 0f;
		for (int i = 0; i < _rotationObjectList.Length; i++)
		{
			if (_rotationObjectList[i].rotationStartTimeOffset > num)
			{
				num = _rotationObjectList[i].rotationStartTimeOffset;
			}
			_rotationObjectsInMotion[i] = false;
		}
		_totalCycleTime = _cycleLength + num;
	}

	protected override void Start()
	{
		base.Start();
		base.enabled = false;
	}

	protected virtual void PlayMovementStartAudio()
	{
		if (_audioSource != null)
		{
			_audioSource.PlayOneShot(AudioType.NomaiDoorStart);
		}
	}

	protected virtual void PlayMovementStopAudio()
	{
		if (_audioSource != null)
		{
			_audioSource.PlayOneShot(AudioType.NomaiDoorStop);
		}
	}

	protected virtual void FixedUpdate()
	{
		for (int i = 0; i < _rotationObjectList.Length; i++)
		{
			RotationObject rotationObject = _rotationObjectList[i];
			if (_cycleTimer < _totalCycleTime)
			{
				float num = ((!(rotationObject.rotationStartTimeOffset > _cycleTimer)) ? ((_cycleTimer - rotationObject.rotationStartTimeOffset) / _cycleLength) : 0f);
				float t = rotationObject.rotationCurve.Evaluate(num);
				rotationObject.objectToRotate.transform.localRotation = Quaternion.Lerp(_startRotationQuats[i], _stopRotationQuats[i], t);
				if (num >= 1f)
				{
					if (_rotationObjectsInMotion[i])
					{
						_rotationObjectsInMotion[i] = false;
						PlayMovementStopAudio();
					}
				}
				else if (num > 0f && !_rotationObjectsInMotion[i])
				{
					_rotationObjectsInMotion[i] = true;
					PlayMovementStartAudio();
				}
			}
			else if (_currentRotationState == RotationState.CYCLING_OPEN)
			{
				_currentRotationState = RotationState.OPEN;
				rotationObject.objectToRotate.transform.localRotation = _stopRotationQuats[i];
				if (this.OnOpen != null)
				{
					this.OnOpen();
				}
			}
			else if (_currentRotationState == RotationState.CYCLING_CLOSED)
			{
				_currentRotationState = RotationState.CLOSED;
				rotationObject.objectToRotate.transform.localRotation = _stopRotationQuats[i];
				if (this.OnClose != null)
				{
					this.OnClose();
				}
			}
		}
		if (_cycleTimer < _totalCycleTime)
		{
			_cycleTimer += Time.deltaTime;
			return;
		}
		bool flag = false;
		for (int j = 0; j < _rotationObjectsInMotion.Length; j++)
		{
			if (_rotationObjectsInMotion[j])
			{
				_rotationObjectsInMotion[j] = false;
				flag = true;
			}
		}
		if (_audioSource != null)
		{
			_audioSource.Stop();
			if (flag)
			{
				PlayMovementStopAudio();
			}
		}
		for (int k = 0; k < _listInterfaceOrb.Length; k++)
		{
			if (_listInterfaceOrb[k].gameObject.activeInHierarchy)
			{
				_listInterfaceOrb[k].RemoveLock();
			}
		}
		base.enabled = false;
	}

	public override void Open(NomaiInterfaceSlot slot)
	{
		if (_currentRotationState != 0 && _currentRotationState != RotationState.CYCLING_OPEN)
		{
			Cycle(slot);
		}
	}

	public override void Close(NomaiInterfaceSlot slot)
	{
		if (_currentRotationState != RotationState.CLOSED && _currentRotationState != RotationState.CYCLING_CLOSED)
		{
			Cycle(slot);
		}
	}

	public override void Cycle(NomaiInterfaceSlot slot)
	{
		if (IsLocked() || IsCycling())
		{
			return;
		}
		_cycleTimer = 0f;
		base.enabled = true;
		_audioSource.Play();
		if (_currentRotationState == RotationState.OPEN)
		{
			_currentRotationState = RotationState.CYCLING_CLOSED;
			for (int i = 0; i < _rotationObjectList.Length; i++)
			{
				_startRotationQuats[i] = _rotationObjectList[i].objectToRotate.transform.localRotation;
				if (_rotationObjectList[i].rotationType == RotationObject.RotationType.OPEN_CLOSE)
				{
					_stopRotationQuats[i] = Quaternion.Euler(_rotationObjectList[i].closedRotation);
				}
				else if (_rotationObjectList[i].rotationType == RotationObject.RotationType.INTERVAL)
				{
					switch (_cycleType)
					{
					case CycleType.POSITIVE_OPENCLOSE:
					case CycleType.NEG_OPEN_POS_CLOSE:
						_stopRotationQuats[i] = _startRotationQuats[i] * Quaternion.Euler(_rotationObjectList[i].intervalRotation);
						break;
					case CycleType.NEGATIVE_OPENCLOSE:
					case CycleType.POS_OPEN_NEG_CLOSE:
						_stopRotationQuats[i] = _startRotationQuats[i] * Quaternion.Euler(_rotationObjectList[i].intervalRotation * -1f);
						break;
					default:
						_stopRotationQuats[i] = Quaternion.identity;
						break;
					}
				}
			}
		}
		else if (_currentRotationState == RotationState.CLOSED)
		{
			_currentRotationState = RotationState.CYCLING_OPEN;
			for (int j = 0; j < _rotationObjectList.Length; j++)
			{
				_startRotationQuats[j] = _rotationObjectList[j].objectToRotate.transform.localRotation;
				if (_rotationObjectList[j].rotationType == RotationObject.RotationType.OPEN_CLOSE)
				{
					_stopRotationQuats[j] = Quaternion.Euler(_rotationObjectList[j].openRotation);
				}
				else if (_rotationObjectList[j].rotationType == RotationObject.RotationType.INTERVAL)
				{
					switch (_cycleType)
					{
					case CycleType.POSITIVE_OPENCLOSE:
					case CycleType.POS_OPEN_NEG_CLOSE:
						_stopRotationQuats[j] = _startRotationQuats[j] * Quaternion.Euler(_rotationObjectList[j].intervalRotation);
						break;
					case CycleType.NEGATIVE_OPENCLOSE:
					case CycleType.NEG_OPEN_POS_CLOSE:
						_stopRotationQuats[j] = _startRotationQuats[j] * Quaternion.Euler(_rotationObjectList[j].intervalRotation * -1f);
						break;
					default:
						_stopRotationQuats[j] = Quaternion.identity;
						break;
					}
				}
			}
		}
		for (int k = 0; k < _openSwitches.Length; k++)
		{
			NomaiInterfaceOrb occupyingInterfaceOrb = _openSwitches[k].GetOccupyingInterfaceOrb();
			if (occupyingInterfaceOrb != null && occupyingInterfaceOrb.gameObject.activeInHierarchy)
			{
				occupyingInterfaceOrb.AddLock(_openSwitches[k].transform, _openSwitches[k].GetAttachedOWRigidbody());
			}
		}
		for (int l = 0; l < _closeSwitches.Length; l++)
		{
			NomaiInterfaceOrb occupyingInterfaceOrb = _closeSwitches[l].GetOccupyingInterfaceOrb();
			if (occupyingInterfaceOrb != null && occupyingInterfaceOrb.gameObject.activeInHierarchy)
			{
				occupyingInterfaceOrb.AddLock(_closeSwitches[l].transform, _closeSwitches[l].GetAttachedOWRigidbody());
			}
		}
		for (int m = 0; m < _cycleSwitches.Length; m++)
		{
			NomaiInterfaceOrb occupyingInterfaceOrb = _cycleSwitches[m].GetOccupyingInterfaceOrb();
			if (occupyingInterfaceOrb != null && occupyingInterfaceOrb.gameObject.activeInHierarchy)
			{
				occupyingInterfaceOrb.AddLock(_cycleSwitches[m].transform, _cycleSwitches[m].GetAttachedOWRigidbody());
			}
		}
		for (int n = 0; n < _listInterfaceOrb.Length; n++)
		{
			if (_listInterfaceOrb[n].gameObject.activeInHierarchy && !_listInterfaceOrb[n].HasLock())
			{
				_listInterfaceOrb[n].AddLock();
			}
		}
	}

	public override bool IsOpen()
	{
		return _currentRotationState == RotationState.OPEN;
	}

	public override bool IsCycling()
	{
		if (_currentRotationState != RotationState.CYCLING_CLOSED)
		{
			return _currentRotationState == RotationState.CYCLING_OPEN;
		}
		return true;
	}

	public override bool IsOpening()
	{
		return _currentRotationState == RotationState.CYCLING_OPEN;
	}

	public override bool IsClosing()
	{
		return _currentRotationState == RotationState.CYCLING_CLOSED;
	}
}
