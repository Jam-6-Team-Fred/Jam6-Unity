using GhostEnums;

public class SleepwalkAction : GhostAction
{
	public override Name GetName()
	{
		return Name.Sleepwalk;
	}

	public override float CalculateUtility()
	{
		if (!_data.hasWokenUp)
		{
			return 100f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		MoveToRandomPatrolNode();
		_controller.SetLanternConcealed(concealed: false);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void OnArriveAtPosition()
	{
		MoveToRandomPatrolNode();
	}

	private void MoveToRandomPatrolNode()
	{
		_controller.PathfindToNode(_controller.GetNodeMap().GetRandomPatrolNode(), MoveType.PATROL);
		_controller.FaceVelocity();
	}
}
