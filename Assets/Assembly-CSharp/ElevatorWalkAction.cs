using GhostEnums;
using UnityEngine;

public class ElevatorWalkAction : GhostAction
{
	private bool _reachedEndOfPath;

	private bool _calledToElevator;

	private bool _hasUsedElevator;

	private GhostNode _elevatorNode;

	public bool reachedEndOfPath => _reachedEndOfPath;

	public override Name GetName()
	{
		return Name.ElevatorWalk;
	}

	public override float CalculateUtility()
	{
		if (_calledToElevator && !_hasUsedElevator && !_data.isPlayerLocationKnown)
		{
			return 100f;
		}
		if (_calledToElevator && !_hasUsedElevator)
		{
			return 70f;
		}
		return -100f;
	}

	public void UseElevator()
	{
		_hasUsedElevator = true;
	}

	public void CallToUseElevator()
	{
		_calledToElevator = true;
		if (_controller.GetNodeMap().GetPathNodes().Length > 1)
		{
			_elevatorNode = _controller.GetNodeMap().GetPathNodes()[1];
			_controller.PathfindToNode(_elevatorNode, MoveType.PATROL);
		}
		else
		{
			Debug.LogError("MissingElevatorNode");
		}
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(concealed: true);
		_controller.FaceVelocity();
		_effects.PlayDefaultAnimation();
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (_elevatorNode != null)
		{
			_controller.PathfindToNode(_elevatorNode, MoveType.PATROL);
		}
	}

	protected override void OnExitAction()
	{
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void OnArriveAtPosition()
	{
		_reachedEndOfPath = true;
	}
}
