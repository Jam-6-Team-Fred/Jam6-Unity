using System.Collections.Generic;
using UnityEngine;

public class RaftDock : RaftCarrier, IItemDropTarget
{
	[SerializeField]
	private RaftController _startRaft;

	[Space]
	[SerializeField]
	private InteractReceiver _interactReceiver;

	[SerializeField]
	private GearInterfaceEffects _gearInterface;

	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[SerializeField]
	private Transform _raftSocket;

	[SerializeField]
	private OWTriggerVolume _raftDetectorVolume;

	[SerializeField]
	private RaftDockLights _lightsController;

	[Header("Raft Proxy Collision")]
	[SerializeField]
	private Transform _raftProxyColliderRoot;

	[SerializeField]
	private OWCollider[] _raftProxyColliders;

	private OWRigidbody _parentBody;

	private float _currentRaftUndockDelay;

	private float _raftUndockCountDown;

	private int _raftInVolumeCounter;

	private bool _flooded;

	private List<OWItem> _itemsToParentOnRaft = new List<OWItem>(4);

	protected override void Awake()
	{
		base.Awake();
		if (_raftSocket == null)
		{
			_raftSocket = base.transform;
		}
		if (_startRaft != null)
		{
			_startRaft.SkipSuspendOnStart();
		}
		_parentBody = base.gameObject.GetAttachedOWRigidbody();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract += OnPressInteract;
		}
		if (_floodSensor != null && _floodSensor.gameObject.activeSelf)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
		_raftDetectorVolume.OnEntry += OnRaftDetectorEntry;
		_raftDetectorVolume.OnExit += OnRaftDetectorExit;
	}

	protected override void Start()
	{
		base.Start();
		if (_interactReceiver != null)
		{
			_interactReceiver.SetPromptText(UITextType.RotateGearPrompt);
		}
		ToggleProxyCollider(active: false);
		if (_startRaft != null)
		{
			_raft = _startRaft;
			FinishDocking(skipRailAnim: true);
		}
		_lightsController.SetLightsActivation(_raft != null, instant: true);
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_trigger.OnExit -= OnExit;
		if (_interactReceiver != null)
		{
			_interactReceiver.OnPressInteract -= OnPressInteract;
		}
		if (_raft != null)
		{
			_raft.OnArriveAtTarget -= new OWEvent.OWCallback(OnArriveAtTarget);
		}
		if (_floodSensor != null && _floodSensor.gameObject.activeSelf)
		{
			_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
		}
		if (_itemsToParentOnRaft.Count > 0)
		{
			for (int i = 0; i < _itemsToParentOnRaft.Count; i++)
			{
				_itemsToParentOnRaft[i].onPickedUp -= new OWEvent<OWItem>.OWCallback(OnRaftItemPickedUp);
			}
			_itemsToParentOnRaft.Clear();
		}
	}

	public OWRigidbody GetParentBody()
	{
		return _parentBody;
	}

	public Transform GetRaftSocket()
	{
		return _raftSocket;
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return base.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
		if (dropTarget.transform.IsChildOf(_raftProxyColliderRoot) && _raft != null)
		{
			_itemsToParentOnRaft.Add(item);
			item.onPickedUp += new OWEvent<OWItem>.OWCallback(OnRaftItemPickedUp);
		}
	}

	protected override Transform GetAlignDestination()
	{
		return _raftSocket;
	}

	protected override void MoveAfterAlign()
	{
		_raft.MoveToTarget(_raftSocket.position, _raftSocket.rotation, 3f);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_state == DockState.WaitForExit && _raftUndockCountDown > 0f)
		{
			_raftUndockCountDown -= Time.fixedDeltaTime;
			if (_raftUndockCountDown <= 0f)
			{
				UndockRaftAfterDelay();
			}
		}
	}

	protected override void OnArriveAtTarget()
	{
		base.OnArriveAtTarget();
		if (_state == DockState.LiftDelay)
		{
			_raft.GetOneShotAudio().PlayOneShot(AudioType.Raft_Reel_Start);
		}
		else if (_state == DockState.Lifting)
		{
			if (_loopingAudio.isPlaying)
			{
				_oneShotAudio.PlayOneShot(AudioType.Raft_Reel_Start);
				_loopingAudio.FadeIn(0.2f);
			}
			FinishDocking();
		}
	}

	protected override void OnEntry(GameObject hitObj)
	{
		base.OnEntry(hitObj);
		if (_state == DockState.AligningBelow)
		{
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("RaftDetector") && _state == DockState.WaitForExit)
		{
			_state = DockState.Ready;
			base.enabled = false;
			UpdateLightStatus();
		}
	}

	private void ToggleProxyCollider(bool active)
	{
		if (!active)
		{
			SurveyorProbe probe = Locator.GetProbe();
			if (probe != null && probe.IsAnchored() && probe.transform.IsChildOf(_raftProxyColliderRoot))
			{
				probe.Unanchor();
				probe.GetAnchor().AnchorToObject(_raft.gameObject, -probe.transform.forward, probe.transform.position);
			}
		}
		for (int i = 0; i < _raftProxyColliders.Length; i++)
		{
			_raftProxyColliders[i].SetActivation(active);
		}
	}

	private void FinishDocking(bool skipRailAnim = false)
	{
		_state = DockState.Docked;
		if (_loopingAudio.isPlaying)
		{
			_loopingAudio.FadeOut(0.2f);
			_oneShotAudio.PlayOneShot(AudioType.Raft_Socket);
		}
		_raft.Dock(this, skipRailAnim);
		ToggleProxyCollider(active: true);
		UpdateLightStatus();
		if (_gearInterface != null)
		{
			_gearInterface.AddRotation(-90f);
		}
		base.enabled = false;
	}

	private void UpdateLightStatus()
	{
		bool occupied = _state == DockState.Docked || _state == DockState.WaitForExit || (_flooded && _raftInVolumeCounter > 0);
		_lightsController.SetLightsActivation(occupied);
	}

	private void OnPressInteract()
	{
		if (_raft != null && _state == DockState.Docked)
		{
			_raftUndockCountDown = _raft.dropDelay;
			_state = DockState.WaitForExit;
			_raft.SetRailingRaised(raised: true);
			if (_gearInterface != null)
			{
				_gearInterface.AddRotation(90f);
			}
			base.enabled = true;
		}
		else if (_gearInterface != null)
		{
			_gearInterface.PlayFailure();
		}
	}

	private void OnRaftDetectorEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("RaftDetector"))
		{
			_raftInVolumeCounter++;
			UpdateLightStatus();
		}
	}

	private void OnRaftDetectorExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("RaftDetector"))
		{
			_raftInVolumeCounter = Mathf.Clamp(_raftInVolumeCounter - 1, 0, int.MaxValue);
			UpdateLightStatus();
		}
	}

	private void UndockRaftAfterDelay()
	{
		ToggleProxyCollider(active: false);
		_oneShotAudio.PlayOneShot(AudioType.Raft_Release);
		for (int i = 0; i < _itemsToParentOnRaft.Count; i++)
		{
			_itemsToParentOnRaft[i].SetSector(_raft.sector);
			_itemsToParentOnRaft[i].transform.SetParent(_raft.transform);
			_itemsToParentOnRaft[i].onPickedUp -= new OWEvent<OWItem>.OWCallback(OnRaftItemPickedUp);
		}
		_itemsToParentOnRaft.Clear();
		_raft.Undock();
		_raft.OnArriveAtTarget -= new OWEvent.OWCallback(OnArriveAtTarget);
		_raft = null;
		if (_flooded)
		{
			_state = DockState.Ready;
			base.enabled = false;
		}
		UpdateLightStatus();
	}

	private void OnFloodImpact()
	{
		_flooded = true;
		_lightsController.SetFlood(flood: true);
		UpdateLightStatus();
		OnPressInteract();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(6f, 1f, 6f));
	}

	private void OnRaftItemPickedUp(OWItem item)
	{
		for (int i = 0; i < _itemsToParentOnRaft.Count; i++)
		{
			if (_itemsToParentOnRaft[i] == item)
			{
				_itemsToParentOnRaft.RemoveAt(i);
				break;
			}
		}
		item.onPickedUp -= new OWEvent<OWItem>.OWCallback(OnRaftItemPickedUp);
	}
}
