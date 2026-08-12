using GhostEnums;
using UnityEngine;

public class PartyPathAction : GhostAction
{
	private int _pathIndex;

	private bool _allowFollowPath;

	private bool _reachedEndOfPath;

	private bool _isMovingToFinalPosition;

	private Vector3 _finalPosition;

	public int currentPathIndex => _pathIndex;

	public bool hasReachedEndOfPath => _reachedEndOfPath;

	public bool isMovingToFinalPosition => _isMovingToFinalPosition;

	public bool allowHearGhostCall
	{
		get
		{
			if (_allowFollowPath)
			{
				return !_isMovingToFinalPosition;
			}
			return false;
		}
	}

	public override Name GetName()
	{
		return Name.PartyPath;
	}

	public override float CalculateUtility()
	{
		if (_controller.GetNodeMap().GetPathNodes().Length == 0)
		{
			return -100f;
		}
		return 10f;
	}

	public void ResetPath()
	{
		_pathIndex = 0;
		_allowFollowPath = false;
		_reachedEndOfPath = false;
		_isMovingToFinalPosition = false;
		_controller.StopMoving();
		_controller.SetLanternConcealed(concealed: true, playAudio: false);
	}

	public void StartFollowPath()
	{
		_allowFollowPath = true;
		_controller.PathfindToNode(_controller.GetNodeMap().GetPathNodes()[_pathIndex], MoveType.PATROL);
		_controller.SetLanternConcealed(concealed: false, playAudio: false);
	}

	public void MoveToFinalPosition(Vector3 worldPosition)
	{
		_isMovingToFinalPosition = true;
		_finalPosition = _controller.WorldToLocalPosition(worldPosition);
		_controller.PathfindToLocalPosition(_finalPosition, MoveType.PATROL);
		_controller.SetLanternConcealed(concealed: true);
	}

	protected override void OnEnterAction()
	{
		_controller.FaceVelocity();
		_effects.PlayDefaultAnimation();
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (_allowFollowPath)
		{
			if (_isMovingToFinalPosition)
			{
				_controller.PathfindToLocalPosition(_finalPosition, MoveType.PATROL);
			}
			else
			{
				_controller.PathfindToNode(_controller.GetNodeMap().GetPathNodes()[_pathIndex], MoveType.PATROL);
			}
			_controller.SetLanternConcealed(_isMovingToFinalPosition);
			_controller.ChangeLanternFocus(0f);
		}
		else
		{
			_controller.SetLanternConcealed(concealed: true, playAudio: false);
		}
	}

	protected override void OnExitAction()
	{
		_reachedEndOfPath = false;
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void OnArriveAtPosition()
	{
		if (!_isMovingToFinalPosition)
		{
			GhostNode[] pathNodes = _controller.GetNodeMap().GetPathNodes();
			if (_pathIndex + 1 > pathNodes.Length || pathNodes[_pathIndex].pathData.isEndOfPath)
			{
				_reachedEndOfPath = true;
				return;
			}
			_pathIndex++;
			_controller.PathfindToNode(pathNodes[_pathIndex], MoveType.PATROL);
		}
	}
}
