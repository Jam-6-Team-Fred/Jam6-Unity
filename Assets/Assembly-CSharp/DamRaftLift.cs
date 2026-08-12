using UnityEngine;

public class DamRaftLift : RaftCarrier
{
	[SerializeField]
	private Transform[] _liftNodes;

	[SerializeField]
	private Transform _craneRoot;

	[SerializeField]
	private Transform[] _craneWheels;

	[SerializeField]
	private float _hookReturnSpeed = 8f;

	[SerializeField]
	private float _craneReturnSpeed = 5f;

	[SerializeField]
	private RaftDockLights _raftDockLights;

	[SerializeField]
	private OWRenderer[] _hookRenderers;

	private const float WHEEL_RADIUS = 0.441f;

	private int _nodeIndex;

	private Vector3 _craneRootStartPos;

	private Vector3 _cranePrevPos;

	protected override void Awake()
	{
		base.Awake();
		GlobalMessenger.AddListener("DamBroken", OnDamBroken);
	}

	protected override void Start()
	{
		base.Start();
		_craneRootStartPos = _craneRoot.localPosition;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_raft != null)
		{
			_raft.OnArriveAtTarget -= new OWEvent.OWCallback(OnArriveAtTarget);
		}
		GlobalMessenger.RemoveListener("DamBroken", OnDamBroken);
	}

	protected override Transform GetAlignDestination()
	{
		return _liftNodes[0];
	}

	protected override void MoveAfterAlign()
	{
		MoveRaftToNextNode();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_state == DockState.Lifting)
		{
			Vector3 vector = base.transform.InverseTransformPoint(_raft.transform.position);
			_craneRoot.localPosition = new Vector3(vector.x, _craneRoot.localPosition.y, vector.z);
			RotateWheels(Vector3.right);
		}
		else if (_state == DockState.ResettingHook)
		{
			if (MoveHookToReturn(_hookReturnSpeed))
			{
				_state = DockState.ResettingCrane;
				_craneHookRoot.localPosition = _hookStartPosLocal;
			}
		}
		else if (_state == DockState.ResettingCrane)
		{
			Vector3 vector2 = _craneRootStartPos - _craneRoot.localPosition;
			_craneRoot.Translate(vector2.normalized * Time.deltaTime * _craneReturnSpeed);
			RotateWheels(-Vector3.right);
			if (Vector3.SqrMagnitude(vector2) <= 0.01f)
			{
				_craneRoot.localPosition = _craneRootStartPos;
				_state = DockState.Ready;
				if (_loopingAudio.isPlaying)
				{
					_loopingAudio.FadeOut(0.2f);
					_oneShotAudio.PlayOneShot(AudioType.Raft_Reel_End);
				}
			}
		}
		else if (_state == DockState.Ready)
		{
			_raftDockLights.SetLightsActivation(occupied: false);
			base.enabled = false;
		}
		_cranePrevPos = _craneRoot.localPosition;
	}

	protected override void OnEntry(GameObject hitObj)
	{
		base.OnEntry(hitObj);
		if (_state == DockState.AligningBelow)
		{
			base.enabled = true;
			for (int i = 0; i < _liftNodes.Length; i++)
			{
				_liftNodes[i].localEulerAngles = GetAlignDestination().localEulerAngles;
			}
			_nodeIndex = 1;
			_raftDockLights.SetLightsActivation(occupied: true);
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
			_oneShotAudio.PlayOneShot(AudioType.Raft_Reel_End);
			MoveRaftToNextNode();
		}
	}

	private void RotateWheels(Vector3 axis)
	{
		float magnitude = (_craneRoot.localPosition - _cranePrevPos).magnitude;
		for (int i = 0; i < _craneWheels.Length; i++)
		{
			_craneWheels[i].Rotate(axis, 57.29578f * (magnitude / 0.441f), Space.Self);
		}
	}

	private void MoveRaftToNextNode()
	{
		if (_liftNodes.Length > _nodeIndex)
		{
			_raft.MoveToTarget(_liftNodes[_nodeIndex].position, _liftNodes[_nodeIndex].rotation, 5f);
			_nodeIndex++;
			return;
		}
		_nodeIndex = 0;
		PlayHookAnimation();
		_raft.StopMovingToTarget();
		_raft.GetOneShotAudio().PlayOneShot(AudioType.Raft_Release);
		_raft.OnArriveAtTarget -= new OWEvent.OWCallback(OnArriveAtTarget);
		_raft = null;
		_state = DockState.ResettingHook;
	}

	private void OnDamBroken()
	{
		if (_raft != null)
		{
			_raft.OnArriveAtTarget -= new OWEvent.OWCallback(OnArriveAtTarget);
			_raft.StopMovingToTarget();
			_craneHookRoot.parent = null;
			for (int i = 0; i < _hookRenderers.Length; i++)
			{
				_hookRenderers[i].SetActivation(active: false);
			}
			_raft = null;
			_trigger.SetTriggerActivation(active: false);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		for (int i = 0; i < _liftNodes.Length; i++)
		{
			Gizmos.DrawSphere(_liftNodes[i].position, 1f);
			if (_liftNodes.Length > i + 1)
			{
				Gizmos.DrawLine(_liftNodes[i].position, _liftNodes[i + 1].position);
			}
		}
	}
}
