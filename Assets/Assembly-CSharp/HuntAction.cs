using System.Collections.Generic;
using GhostEnums;
using UnityEngine;

public class HuntAction : GhostAction
{
	private int _numNodesToSearch;

	private GhostNodeMap.NodeSearchData[] _nodesToSearch;

	private int _currentNodeIndex;

	private GhostNode _closestNode;

	private bool _startAtClosestNode;

	private bool _huntStarted;

	private float _huntStartTime;

	private bool _huntFailed;

	private float _huntFailTime;

	private List<int> _spotlightIndexList = new List<int>(16);

	private int _spotlightIndex = -1;

	private Vector3 _DEBUG_localPos;

	private Vector3 _DEBUG_localVel;

	public override void Initialize(GhostData data, GhostController controller, GhostSensors sensors, GhostEffects effects)
	{
		base.Initialize(data, controller, sensors, effects);
		_numNodesToSearch = 0;
		_nodesToSearch = new GhostNodeMap.NodeSearchData[_controller.GetNodeMap().GetNodeCount()];
		_currentNodeIndex = 0;
		_huntStarted = false;
		_huntStartTime = 0f;
		_huntFailed = false;
		_huntFailTime = 0f;
		controller.OnNodeMapChanged += new OWEvent.OWCallback(OnNodeMapChanged);
	}

	private void OnNodeMapChanged()
	{
		if (_running)
		{
			Debug.LogError("Changing node maps while the Hunt action is running is almost definitely not supported!");
			_huntFailed = true;
		}
		_numNodesToSearch = 0;
		_nodesToSearch = new GhostNodeMap.NodeSearchData[_controller.GetNodeMap().GetNodeCount()];
		_currentNodeIndex = 0;
	}

	public override Name GetName()
	{
		return Name.Hunt;
	}

	public override float CalculateUtility()
	{
		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			return -100f;
		}
		if (_huntFailed && _huntFailTime > _data.timeLastSawPlayer)
		{
			return -100f;
		}
		if (_running || _data.timeSincePlayerLocationKnown < 60f)
		{
			return 80f;
		}
		return -100f;
	}

	protected override void OnEnterAction()
	{
		_controller.SetLanternConcealed(concealed: true);
		_controller.FaceVelocity();
		_effects.SetMovementStyle(GhostEffects.MovementStyle.Normal);
		if (!_huntStarted || _data.timeLastSawPlayer > _huntStartTime)
		{
			Vector3 vector = (_data.lastKnownSensor.knowsPlayerVelocity ? _data.lastKnownPlayerLocation.localVelocity : Vector3.zero);
			_numNodesToSearch = _controller.GetNodeMap().FindPossiblePlayerNodes(_data.lastKnownPlayerLocation.localPosition, vector, 30f, _nodesToSearch);
			_currentNodeIndex = 0;
			_startAtClosestNode = false;
			_closestNode = null;
			_huntStarted = true;
			_huntStartTime = Time.time;
			_huntFailed = false;
			if (_numNodesToSearch == 0)
			{
				Debug.LogError("Failed to find nodes to hunt player!", _controller);
				_huntFailed = true;
				_huntFailTime = Time.time;
			}
			_DEBUG_localPos = _data.lastKnownPlayerLocation.localPosition;
			_DEBUG_localVel = vector;
		}
		if (_huntFailed)
		{
			return;
		}
		_closestNode = _controller.GetNodeMap().FindClosestNode(_controller.GetLocalFeetPosition());
		for (int i = 0; i < _closestNode.visibleNodes.Count; i++)
		{
			for (int j = 0; j < _numNodesToSearch; j++)
			{
				if (_closestNode.visibleNodes[i] == _nodesToSearch[j].node.index)
				{
					_startAtClosestNode = true;
					break;
				}
			}
		}
		if (_startAtClosestNode)
		{
			_controller.PathfindToNode(_closestNode, MoveType.SEARCH);
		}
		else
		{
			_controller.PathfindToNode(_nodesToSearch[_currentNodeIndex].node, MoveType.SEARCH);
		}
		_effects.PlayVoiceAudioNear(AudioType.Ghost_Hunt);
	}

	protected override void OnExitAction()
	{
		if (_huntFailed && !_data.isPlayerLocationKnown)
		{
			_effects.PlayVoiceAudioNear(AudioType.Ghost_HuntFail);
		}
	}

	public override bool Update_Action()
	{
		if (!_huntFailed)
		{
			return !_data.isPlayerLocationKnown;
		}
		return false;
	}

	public override void FixedUpdate_Action()
	{
		if (!_huntStarted || _huntFailed || _spotlightIndexList.Count <= 0 || _controller.GetDreamLanternController().IsConcealed())
		{
			return;
		}
		for (int i = 0; i < _spotlightIndexList.Count; i++)
		{
			if (!_nodesToSearch[_spotlightIndexList[i]].searched)
			{
				Vector3 from = _nodesToSearch[_spotlightIndexList[i]].node.localPosition - _controller.GetLocalFeetPosition();
				OWLight2 light = _controller.GetDreamLanternController().GetLight();
				Vector3 to = _controller.WorldToLocalDirection(light.transform.forward);
				if (Vector3.Angle(from, to) < light.GetLight().spotAngle * 0.5f - 5f && from.sqrMagnitude < light.range * light.range)
				{
					_nodesToSearch[_spotlightIndexList[i]].searched = true;
				}
			}
		}
	}

	public override void OnTraversePathNode(GhostNode node)
	{
		for (int i = 0; i < _numNodesToSearch; i++)
		{
			if (node == _nodesToSearch[i].node)
			{
				_nodesToSearch[i].searched = true;
			}
		}
	}

	public override void OnArriveAtPosition()
	{
		GhostNode node;
		if (_startAtClosestNode)
		{
			_startAtClosestNode = false;
			node = _closestNode;
			for (int i = 0; i < _numNodesToSearch; i++)
			{
				if (_closestNode == _nodesToSearch[i].node)
				{
					_nodesToSearch[i].searched = true;
					break;
				}
			}
		}
		else
		{
			node = _nodesToSearch[_currentNodeIndex].node;
			_nodesToSearch[_currentNodeIndex].searched = true;
		}
		GenerateSpotlightList(node);
		if (_spotlightIndexList.Count > 0)
		{
			_controller.SetLanternConcealed(concealed: false);
			SpotlightNextNode();
		}
		else
		{
			TryContinueSearch();
		}
	}

	public override void OnFaceNode(GhostNode node)
	{
		int num = _spotlightIndexList[_spotlightIndex];
		if (node != _nodesToSearch[num].node)
		{
			Debug.LogError("Why are we facing this node??? " + node.name);
			Debug.Break();
			return;
		}
		_nodesToSearch[num].searched = true;
		for (int num2 = _spotlightIndexList.Count - 1; num2 >= 0; num2--)
		{
			if (_nodesToSearch[_spotlightIndexList[num2]].searched)
			{
				_spotlightIndexList.RemoveAt(num2);
			}
		}
		if (_spotlightIndexList.Count > 0)
		{
			SpotlightNextNode();
			return;
		}
		_controller.SetLanternConcealed(concealed: true);
		_controller.FaceVelocity();
		TryContinueSearch();
	}

	private void SpotlightNextNode()
	{
		_spotlightIndex = 0;
		int num = _spotlightIndexList[_spotlightIndex];
		_controller.FaceNode(_nodesToSearch[num].node, TurnSpeed.MEDIUM, 1f, autoFocusLantern: true);
	}

	private void TryContinueSearch()
	{
		if (Time.time > _enterTime + 60f)
		{
			_huntFailed = true;
			_huntFailTime = Time.time;
			return;
		}
		while (_nodesToSearch[_currentNodeIndex].searched && _currentNodeIndex < _numNodesToSearch)
		{
			_currentNodeIndex++;
		}
		if (_currentNodeIndex < _numNodesToSearch)
		{
			_controller.PathfindToNode(_nodesToSearch[_currentNodeIndex].node, MoveType.SEARCH);
			return;
		}
		_huntFailed = true;
		_huntFailTime = Time.time;
	}

	private void GenerateSpotlightList(GhostNode node)
	{
		_spotlightIndexList.Clear();
		for (int i = 0; i < node.visibleNodes.Count; i++)
		{
			for (int j = 0; j < _numNodesToSearch; j++)
			{
				if (!_nodesToSearch[j].searched && node.visibleNodes[i] == _nodesToSearch[j].node.index)
				{
					_spotlightIndexList.Add(j);
				}
			}
		}
	}

	public override void DrawGizmos(bool isGhostSelected)
	{
		if (isGhostSelected)
		{
			Gizmos.matrix = _controller.GetNodeRoot().localToWorldMatrix;
			Gizmos.color = Color.white;
			Gizmos.DrawCube(_DEBUG_localPos, Vector3.one);
			Gizmos.DrawRay(_DEBUG_localPos, _DEBUG_localVel);
			for (int i = 0; i < _numNodesToSearch; i++)
			{
				float t = Mathf.Abs(_nodesToSearch[i].score) / 180f;
				Gizmos.color = (_nodesToSearch[i].searched ? Color.black : Color.HSVToRGB(Mathf.Lerp(0.5f, 0f, t), 1f, 1f));
				Gizmos.DrawWireSphere(_nodesToSearch[i].node.localPosition, (i < _currentNodeIndex) ? 0.5f : 2f);
			}
		}
	}
}
