using UnityEngine;

public class PrisonCellElevator : MonoBehaviour, IItemDropTarget
{
	[SerializeField]
	private DreamLibraryPedestal _pedestal;

	[SerializeField]
	private OWLightController _pedestalSymbol;

	[SerializeField]
	private OWRigidbody _elevatorBody;

	[SerializeField]
	private OWCollider _proxyCollider;

	[SerializeField]
	private OWTriggerVolume[] _killTriggers;

	[Header("Floors")]
	[SerializeField]
	private AbstractDoor _topDoor;

	[SerializeField]
	private OWCollider _topDoorProxyCollider;

	[SerializeField]
	private AbstractDoor _bottomDoor;

	[SerializeField]
	private OWCollider _bottomDoorProxyCollider;

	[SerializeField]
	private Transform[] _floors;

	[SerializeField]
	private int _startFloorIndex;

	[Header("Speed")]
	[SerializeField]
	private float _maxAccel = 1f;

	[SerializeField]
	private float _maxDecel = 3f;

	[SerializeField]
	private float _maxSpeed = 10f;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _oneShotAudio;

	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private OWAudioSource _musicAudio;

	private OWRigidbody _parentBody;

	private float _speed;

	private int _targetFloorIndex;

	private int _playerKillTriggerCount;

	private bool _hasPrisonerBeenRevealed;

	public OWEvent OnArriveAtTopFloor = new OWEvent(1);

	public OWEvent OnArriveAtBottomFloor = new OWEvent(1);

	public bool IsAtTopFloor => _targetFloorIndex == 1;

	public bool IsAtBottomFloor => _targetFloorIndex == 0;

	private void Start()
	{
		_pedestal.OnPowerOn += new OWEvent.OWCallback(OnPedestalPowerOn);
		_pedestal.OnPowerOff += new OWEvent.OWCallback(OnPedestalPowerOff);
		_pedestalSymbol.SetIntensity(0f);
		for (int i = 0; i < _killTriggers.Length; i++)
		{
			_killTriggers[i].OnEntry += OnEnterKillTrigger;
			_killTriggers[i].OnExit += OnExitKillTrigger;
		}
		_parentBody = _elevatorBody.GetOrigParentBody();
		_targetFloorIndex = _startFloorIndex;
		SuspendAtTargetFloor(reparentPedestal: false);
		_loopingAudio.SetLocalVolume(0f);
		_musicAudio.SetLocalVolume(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_pedestal.OnPowerOn -= new OWEvent.OWCallback(OnPedestalPowerOn);
		_pedestal.OnPowerOff -= new OWEvent.OWCallback(OnPedestalPowerOff);
		for (int i = 0; i < _killTriggers.Length; i++)
		{
			_killTriggers[i].OnEntry -= OnEnterKillTrigger;
			_killTriggers[i].OnExit -= OnExitKillTrigger;
		}
	}

	public void OnPrisonerReveal()
	{
		_musicAudio.FadeOut(1f);
		_hasPrisonerBeenRevealed = true;
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return _elevatorBody.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
	}

	public void CallToTopFloor()
	{
		CallElevatorToFloor(1);
	}

	public void CallToBottomFloor()
	{
		CallElevatorToFloor(0);
	}

	private void CallElevatorToFloor(int floorIndex)
	{
		if (_targetFloorIndex == floorIndex)
		{
			return;
		}
		_targetFloorIndex = floorIndex;
		if (_elevatorBody.IsSuspended())
		{
			Unsuspend();
			if (_oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Start);
				_loopingAudio.FadeIn(0.2f);
			}
		}
		if (_topDoor.IsOpen())
		{
			_topDoor.Close();
			_topDoorProxyCollider.SetActivation(active: true);
		}
		if (_bottomDoor.IsOpen())
		{
			_bottomDoor.Close();
			_bottomDoorProxyCollider.SetActivation(active: true);
		}
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		Transform transform = _floors[_targetFloorIndex];
		Vector3 vector = Vector3.Project(transform.position - _elevatorBody.GetPosition(), base.transform.up);
		float magnitude = vector.magnitude;
		bool flag = _elevatorBody.transform.InverseTransformPoint(transform.position).y > 0f;
		vector = (flag ? vector : (-vector));
		if (magnitude > 0.01f)
		{
			float num = (flag ? _maxAccel : (0f - _maxAccel));
			if (_speed > 0f == flag)
			{
				float num2 = Mathf.Pow(_speed, 2f) / _maxAccel;
				if (magnitude <= num2)
				{
					num = (flag ? (0f - _maxDecel) : _maxDecel);
				}
			}
			_speed = Mathf.Clamp(_speed + num * Time.fixedDeltaTime, 0f - _maxSpeed, _maxSpeed);
			float num3 = Mathf.Abs(_speed) * Time.fixedDeltaTime;
			if (magnitude < num3)
			{
				_speed = Mathf.Sign(_speed) * (magnitude / Time.fixedDeltaTime);
			}
			Vector3 pointVelocity = _parentBody.GetPointVelocity(_elevatorBody.GetPosition());
			Vector3 vector2 = vector.normalized * _speed;
			_elevatorBody.SetVelocity(pointVelocity + vector2);
			_elevatorBody.SetAngularVelocity(_parentBody.GetAngularVelocity());
		}
		else
		{
			SuspendAtTargetFloor();
			TryOpenDoor();
			base.enabled = false;
			if (_targetFloorIndex == 0)
			{
				OnArriveAtBottomFloor.Invoke();
			}
			else if (_targetFloorIndex == 1)
			{
				OnArriveAtTopFloor.Invoke();
				_musicAudio.FadeOut(4f);
			}
			if (_oneShotAudio != null && _loopingAudio != null)
			{
				_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Stop);
				_loopingAudio.FadeOut(0.2f);
			}
		}
	}

	private void SuspendAtTargetFloor(bool reparentPedestal = true)
	{
		_speed = 0f;
		if (reparentPedestal)
		{
			_pedestal.transform.parent = _floors[_targetFloorIndex];
		}
		_elevatorBody.Suspend();
		_elevatorBody.transform.parent = _floors[_targetFloorIndex];
		_elevatorBody.transform.localPosition = Vector3.zero;
		_elevatorBody.transform.localRotation = Quaternion.identity;
		_proxyCollider.transform.position = _floors[_targetFloorIndex].position;
		_proxyCollider.transform.rotation = _floors[_targetFloorIndex].rotation;
		_proxyCollider.SetActivation(active: true);
	}

	private void Unsuspend()
	{
		_pedestal.transform.parent = base.transform;
		_proxyCollider.SetActivation(active: false);
		_elevatorBody.transform.parent = null;
		_elevatorBody.Unsuspend();
	}

	private void OnPedestalPowerOn()
	{
		CallElevatorToFloor((_targetFloorIndex == 0) ? 1 : 0);
		if (_targetFloorIndex == 0 && !_hasPrisonerBeenRevealed)
		{
			_musicAudio.FadeInToLibraryVolume(6f);
		}
		Locator.GetDreamWorldController().UpdateSimulationSphereRadius(0f);
	}

	private void OnPedestalPowerOff()
	{
		TryOpenDoor();
	}

	private void TryOpenDoor()
	{
		if (_elevatorBody.IsSuspended() && !_pedestal.IsPowered())
		{
			if (!_topDoor.IsOpen() && _targetFloorIndex == 1)
			{
				_topDoor.Open();
				_topDoorProxyCollider.SetActivation(active: false);
				_pedestalSymbol.SetIntensity(1f);
			}
			else if (!_bottomDoor.IsOpen() && _targetFloorIndex == 0)
			{
				_bottomDoor.Open();
				_bottomDoorProxyCollider.SetActivation(active: false);
				Locator.GetDreamWorldController().UpdateSimulationSphereRadius(10f);
			}
		}
	}

	private void OnEnterKillTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerKillTriggerCount++;
			if (_speed < 0f && _playerKillTriggerCount == _killTriggers.Length)
			{
				Locator.GetDeathManager().KillPlayer(DeathType.CrushedByElevator);
				RumbleManager.PlayerCrushedByElevator();
			}
		}
	}

	private void OnExitKillTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerKillTriggerCount--;
		}
	}
}
