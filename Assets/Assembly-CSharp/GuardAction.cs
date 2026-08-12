using GhostEnums;
using UnityEngine;

public class GuardAction : GhostAction
{
	private GhostNode _targetSearchNode;

	private bool _searchingAtNode;

	private bool _watchingPlayer;

	private float _searchStartTime;

	private float _lastSawPlayer;

	private GhostNode[] _searchNodes;

	private bool _hasReachedAnySearchNode;

	public override Name GetName()
	{
		return Name.Guard;
	}

	public override float CalculateUtility()
	{
		if (!_controller.GetNodeMap().HasSearchNodes(_controller.GetNodeLayer()))
		{
			return -100f;
		}
		if (_data.threatAwareness >= GhostData.ThreatAwareness.SomeoneIsInHere)
		{
			if (_data.reduceGuardUtility)
			{
				return 60f;
			}
			return 90f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(concealed: true);
		_sensors.SetContactEdgeCatcherWidth(5f);
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		ContinueSearch();
	}

	protected override void OnExitAction()
	{
		if (_searchingAtNode)
		{
			_controller.FaceVelocity();
		}
		_sensors.ResetContactEdgeCatcherWidth();
		_targetSearchNode = null;
		_searchingAtNode = false;
		_watchingPlayer = false;
	}

	public override bool Update_Action()
	{
		if (_searchingAtNode && Time.time > _searchStartTime + 4f)
		{
			_controller.FaceVelocity();
			_targetSearchNode.searchData.lastSearchTime = Time.time;
			ContinueSearch();
		}
		bool flag = _hasReachedAnySearchNode && (_data.sensor.isPlayerVisible || _data.sensor.isPlayerHeldLanternVisible);
		if (flag)
		{
			_lastSawPlayer = Time.time;
		}
		if (!_watchingPlayer && flag)
		{
			_watchingPlayer = true;
			_searchingAtNode = false;
			_controller.StopMoving();
			_controller.FacePlayer(TurnSpeed.MEDIUM);
		}
		else if (_watchingPlayer && !flag && Time.time - _lastSawPlayer > 1f)
		{
			_watchingPlayer = false;
			ContinueSearch();
		}
		return true;
	}

	public override void OnArriveAtPosition()
	{
		if (_searchNodes != null && _searchNodes.Length == 1)
		{
			_controller.FaceLocalPosition(_targetSearchNode.localPosition + _targetSearchNode.localForward * 10f, TurnSpeed.MEDIUM);
		}
		else
		{
			_controller.Spin(TurnSpeed.MEDIUM);
		}
		_searchingAtNode = true;
		_hasReachedAnySearchNode = true;
		_searchStartTime = Time.time;
	}

	private void ContinueSearch()
	{
		_searchingAtNode = false;
		_targetSearchNode = GetHighestPriorityNodeToSearch();
		if (_targetSearchNode == null)
		{
			Debug.LogError("Failed to find any nodes to search!  Did we exhaust our existing options?", _controller);
			Debug.Break();
		}
		_controller.PathfindToNode(_targetSearchNode, MoveType.SEARCH);
		_controller.FaceVelocity();
	}

	private GhostNode GetHighestPriorityNodeToSearch()
	{
		_searchNodes = _controller.GetNodeMap().GetSearchNodesOnLayer(_controller.GetNodeLayer());
		float num = 0f;
		float time = Time.time;
		for (int i = 0; i < _searchNodes.Length; i++)
		{
			float num2 = time - _searchNodes[i].searchData.lastSearchTime;
			num += num2;
		}
		num /= (float)_searchNodes.Length;
		GhostNode ghostNode = null;
		for (int j = 0; j < 5; j++)
		{
			ghostNode = _searchNodes[Random.Range(0, _searchNodes.Length)];
			if (time - ghostNode.searchData.lastSearchTime > num)
			{
				break;
			}
		}
		return ghostNode;
	}
}
