using GhostEnums;
using UnityEngine;

public class PartyHouseAction : GhostAction
{
	private Vector3 _initialLocalPosition;

	private Vector3 _initialLocalDirection;

	private bool _allowChasePlayer;

	private bool _waitingToLookAtPlayer;

	private bool _lookingAtPlayer;

	private float _lookAtPlayerTime;

	private TurnSpeed _lookSpeed;

	public override Name GetName()
	{
		return Name.PartyHouse;
	}

	public override float CalculateUtility()
	{
		if (!_allowChasePlayer)
		{
			return 99f;
		}
		return 94f;
	}

	public override bool IsInterruptible()
	{
		return true;
	}

	public void ResetAllowChasePlayer()
	{
		_allowChasePlayer = false;
	}

	public void AllowChasePlayer()
	{
		_allowChasePlayer = true;
		_controller.SetLanternConcealed(concealed: true);
		_controller.FacePlayer(TurnSpeed.MEDIUM);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
	}

	public void LookAtPlayer(float delay, TurnSpeed lookSpeed = TurnSpeed.SLOWEST)
	{
		_waitingToLookAtPlayer = true;
		_lookAtPlayerTime = Time.time + delay;
		_lookSpeed = lookSpeed;
	}

	public override void Initialize(GhostData data, GhostController controller, GhostSensors sensors, GhostEffects effects)
	{
		base.Initialize(data, controller, sensors, effects);
		_initialLocalPosition = _controller.GetLocalFeetPosition();
		_initialLocalDirection = _controller.GetLocalForward();
	}

	protected override void OnEnterAction()
	{
		_controller.MoveToLocalPosition(_initialLocalPosition, MoveType.PATROL);
		_controller.FaceLocalPosition(_initialLocalPosition + _initialLocalDirection, TurnSpeed.MEDIUM);
		_controller.SetLanternConcealed(concealed: true);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		_waitingToLookAtPlayer = false;
		_lookingAtPlayer = false;
	}

	protected override void OnExitAction()
	{
		_allowChasePlayer = true;
	}

	public override bool Update_Action()
	{
		if (!_lookingAtPlayer)
		{
			bool isIlluminatedByPlayer = _data.sensor.isIlluminatedByPlayer;
			if ((_waitingToLookAtPlayer && Time.time > _lookAtPlayerTime) || isIlluminatedByPlayer)
			{
				_controller.FacePlayer(isIlluminatedByPlayer ? TurnSpeed.SLOW : _lookSpeed);
				_waitingToLookAtPlayer = false;
				_lookingAtPlayer = true;
			}
		}
		return true;
	}

	public override void FixedUpdate_Action()
	{
		if (_allowChasePlayer)
		{
			if (_controller.GetNodeMap().CheckLocalPointInBounds(_data.playerLocation.localPosition))
			{
				_controller.PathfindToLocalPosition(_data.playerLocation.localPosition, MoveType.SEARCH);
			}
			_controller.FaceLocalPosition(_data.playerLocation.localPosition, TurnSpeed.MEDIUM);
		}
	}
}
