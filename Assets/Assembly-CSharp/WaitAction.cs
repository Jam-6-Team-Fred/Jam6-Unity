using GhostEnums;
using UnityEngine;

public class WaitAction : GhostAction
{
	public override Name GetName()
	{
		return Name.Wait;
	}

	public override float CalculateUtility()
	{
		if (PlayerState.IsGrabbedByGhost() && (_running || _data.timeSincePlayerLocationKnown < 4f))
		{
			return 666f;
		}
		return 0f;
	}

	protected override void OnEnterAction()
	{
		if (PlayerState.IsGrabbedByGhost())
		{
			_effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
			_controller.FacePlayer(TurnSpeed.MEDIUM);
			if (_data.playerLocation.distanceXZ < 3f)
			{
				Vector3 toPositionXZ = _data.playerLocation.toPositionXZ;
				_controller.MoveToLocalPosition(_controller.GetLocalFeetPosition() - toPositionXZ * 3f, MoveType.SEARCH);
			}
			else
			{
				_controller.StopMoving();
			}
		}
		else
		{
			_controller.StopMoving();
			_controller.StopFacing();
		}
	}

	public override bool Update_Action()
	{
		return true;
	}
}
