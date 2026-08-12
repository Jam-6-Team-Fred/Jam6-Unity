using GhostEnums;

public class SentryAction : GhostAction
{
	private GhostNode _targetSentryNode;

	private bool _spotlighting;

	public override Name GetName()
	{
		return Name.Sentry;
	}

	public override float CalculateUtility()
	{
		if (_data.threatAwareness >= GhostData.ThreatAwareness.SomeoneIsInHere)
		{
			return 50f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		_spotlighting = false;
		_controller.SetLanternConcealed(concealed: true);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Stalk);
		GhostNode[] searchNodesOnLayer = _controller.GetNodeMap().GetSearchNodesOnLayer(_controller.GetNodeLayer());
		_targetSentryNode = searchNodesOnLayer[0];
		_controller.PathfindToNode(_targetSentryNode, MoveType.PATROL);
		_controller.FaceVelocity();
	}

	public override bool Update_Action()
	{
		return true;
	}

	public override void FixedUpdate_Action()
	{
		if (_data.isPlayerLocationKnown && !_spotlighting)
		{
			_spotlighting = true;
			_controller.ChangeLanternFocus(1f);
		}
		if (_spotlighting)
		{
			if (_data.timeSincePlayerLocationKnown > 3f)
			{
				_spotlighting = false;
				_controller.SetLanternConcealed(concealed: true);
				_controller.FaceLocalPosition(_targetSentryNode.localPosition + _targetSentryNode.localForward * 10f, TurnSpeed.MEDIUM);
			}
			else
			{
				_controller.FaceLocalPosition(_data.lastKnownPlayerLocation.localPosition, TurnSpeed.FAST);
			}
		}
	}

	public override void OnArriveAtPosition()
	{
		_controller.FaceLocalPosition(_targetSentryNode.localPosition + _targetSentryNode.localForward * 10f, TurnSpeed.MEDIUM);
	}
}
