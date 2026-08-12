using System.Collections.Generic;
using UnityEngine;

public class GhostNodeMap : MonoBehaviour
{
	public struct NodeSearchData
	{
		public GhostNode node;

		public float score;

		public bool searched;
	}

	private class PathData
	{
		public int cameFromIndex;

		public float gScore;

		public float fScore;

		public float neighborAngle;

		public float turnScore;

		public bool pruned;

		public bool visitedWhilePruning;

		public PathData()
		{
			Reset();
		}

		public void Reset()
		{
			cameFromIndex = -1;
			gScore = float.PositiveInfinity;
			fScore = float.PositiveInfinity;
			neighborAngle = 180f;
			turnScore = float.PositiveInfinity;
			pruned = false;
			visitedWhilePruning = false;
		}
	}

	[SerializeField]
	private List<GhostMarkerEdge> _markerEdges;

	[SerializeField]
	private Shape _mapBounds;

	private const float kNodeSnapDist = 2f;

	private GhostNode[] _nodes;

	private GhostEdge[] _edges;

	private GhostNode[] _pathNodes;

	private GhostNode[] _patrolNodes;

	private GhostNode[] _searchNodes;

	private GhostNode.NodeLayer _availableSearchNodes;

	private Transform _transform;

	private OWRigidbody _owRigidbody;

	private bool _initialized;

	private PathData[] _pathData;

	private GhostNode[] _startNodes = new GhostNode[2];

	private GhostNode[] _endNodes = new GhostNode[2];

	private GhostNode[] _singleEndNode = new GhostNode[1];

	private static List<int> s_openSet = new List<int>(128);

	private static List<int> s_closedSet = new List<int>(128);

	[ContextMenu("Rename Marker Nodes", false)]
	private void RenameMarkerNodes()
	{
		GhostNodeMarker[] componentsInChildren = GetComponentsInChildren<GhostNodeMarker>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.name = "GhostNode_" + i;
		}
	}

	[ContextMenu("Check for Overlapping Nodes", false)]
	private void CheckForOverlappingNodes()
	{
		GhostNodeMarker[] componentsInChildren = GetComponentsInChildren<GhostNodeMarker>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (i != j && Vector3.Distance(componentsInChildren[i].transform.position, componentsInChildren[j].transform.position) < 0.2f)
				{
					Debug.LogWarning("OVERLAPPING NODES DETECTED! " + componentsInChildren[i].name + " AND " + componentsInChildren[j].name, componentsInChildren[i]);
				}
			}
		}
	}

	[ContextMenu("Build Marker Visibility Sets", false)]
	private void BuildMarkerVisibilitySets()
	{
		GhostNodeMarker[] componentsInChildren = GetComponentsInChildren<GhostNodeMarker>();
		RaycastHit[] array = new RaycastHit[32];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			List<GhostNodeMarker> list = new List<GhostNodeMarker>();
			Vector3 vector = componentsInChildren[i].transform.position + new Vector3(0f, 3f, 0f);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (i == j)
				{
					continue;
				}
				Vector3 direction = componentsInChildren[j].transform.position + new Vector3(0f, 1.8f, 0f) - vector;
				if (direction.sqrMagnitude > 625f)
				{
					continue;
				}
				int num = Physics.RaycastNonAlloc(vector, direction, array, Mathf.Min(direction.magnitude, 25f), OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
				bool flag = false;
				for (int k = 0; k < num; k++)
				{
					if (!array[k].collider.CompareTag("IgnoredByGhostMap"))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(componentsInChildren[j]);
				}
			}
			componentsInChildren[i].visibleNodes = list.ToArray();
		}
	}

	private void Awake()
	{
		if (!_initialized)
		{
			InitializeMap();
		}
	}

	private void InitializeMap()
	{
		GhostNodeMarker[] componentsInChildren = GetComponentsInChildren<GhostNodeMarker>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].nodeIndex = i;
		}
		_nodes = new GhostNode[componentsInChildren.Length];
		GhostNode ghostNode = null;
		List<GhostNode> list = new List<GhostNode>();
		List<GhostNode> list2 = new List<GhostNode>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			_nodes[j] = new GhostNode(j, componentsInChildren[j]);
			if (_nodes[j].isPathNode && _nodes[j].pathData.isStartOfPath)
			{
				ghostNode = _nodes[j];
			}
			if (_nodes[j].isPatrolNode)
			{
				list.Add(_nodes[j]);
			}
			if (_nodes[j].isSearchNode)
			{
				list2.Add(_nodes[j]);
			}
		}
		_patrolNodes = list.ToArray();
		_searchNodes = list2.ToArray();
		_availableSearchNodes = (GhostNode.NodeLayer)0;
		for (int k = 0; k < _searchNodes.Length; k++)
		{
			_availableSearchNodes |= _searchNodes[k].searchData.searchLayer;
		}
		_pathData = new PathData[_nodes.Length];
		for (int l = 0; l < _pathData.Length; l++)
		{
			_pathData[l] = new PathData();
		}
		_edges = new GhostEdge[_markerEdges.Count];
		for (int m = 0; m < _markerEdges.Count; m++)
		{
			GhostNode ghostNode2 = _nodes[_markerEdges[m].markerOne.nodeIndex];
			GhostNode ghostNode3 = _nodes[_markerEdges[m].markerTwo.nodeIndex];
			if (ghostNode2 == ghostNode3)
			{
				Debug.LogError("Cannot link a node with itself!", _markerEdges[m].markerOne);
				Debug.Break();
			}
			_edges[m] = new GhostEdge(m, ghostNode2, ghostNode3);
			ghostNode2.AddNeighbor(ghostNode3, _edges[m]);
			ghostNode3.AddNeighbor(ghostNode2, _edges[m]);
		}
		List<GhostNode> list3 = new List<GhostNode>();
		if (ghostNode != null)
		{
			ghostNode.pathData.index = 0;
			list3.Add(ghostNode);
			int num = 1;
			GhostNode ghostNode4 = ghostNode;
			do
			{
				for (int n = 0; n < ghostNode4.neighbors.Count; n++)
				{
					if (ghostNode4.neighbors[n].isPathNode && !list3.Contains(ghostNode4.neighbors[n]))
					{
						ghostNode4.neighbors[n].pathData.index = num++;
						list3.Add(ghostNode4.neighbors[n]);
						ghostNode4 = ghostNode4.neighbors[n];
						break;
					}
				}
			}
			while (!ghostNode4.pathData.isEndOfPath);
		}
		_pathNodes = list3.ToArray();
		_transform = base.transform;
		_owRigidbody = _transform.GetAttachedOWRigidbody();
		_initialized = true;
	}

	public Vector3 WorldToLocalPosition(Vector3 worldPos)
	{
		return _transform.InverseTransformPoint(worldPos);
	}

	public Vector3 LocalToWorldPosition(Vector3 localPos)
	{
		return _transform.TransformPoint(localPos);
	}

	public Vector3 WorldToLocalDirection(Vector3 worldDir)
	{
		return _transform.InverseTransformDirection(worldDir);
	}

	public Vector3 LocalToWorldDirection(Vector3 localDir)
	{
		return _transform.TransformDirection(localDir);
	}

	public OWRigidbody GetOWRigidbody()
	{
		return _owRigidbody;
	}

	public bool CheckWorldPointInBounds(Vector3 worldPos)
	{
		if (_mapBounds == null)
		{
			Debug.LogWarning("Ghost node map does not have boundary shape!", this);
			return false;
		}
		return _mapBounds.PointInside(worldPos);
	}

	public bool CheckLocalPointInBounds(Vector3 localPos)
	{
		if (_mapBounds == null)
		{
			Debug.LogWarning("Ghost node map does not have boundary shape!", this);
			return false;
		}
		return _mapBounds.PointInside(LocalToWorldPosition(localPos));
	}

	public GhostNode GetRandomNode()
	{
		return _nodes[Random.Range(0, _nodes.Length)];
	}

	public int GetNodeCount()
	{
		return _nodes.Length;
	}

	public GhostNode GetNodeAtIndex(int index)
	{
		return _nodes[index];
	}

	public GhostNode[] GetPathNodes()
	{
		return _pathNodes;
	}

	public GhostNode[] GetPatrolNodes()
	{
		return _patrolNodes;
	}

	public GhostNode[] GetSearchNodes()
	{
		return _searchNodes;
	}

	public bool HasSearchNodes(GhostNode.NodeLayer layer = (GhostNode.NodeLayer)(-1))
	{
		return (layer & _availableSearchNodes) != 0;
	}

	public GhostNode GetRandomPatrolNode()
	{
		if (_patrolNodes.Length == 0)
		{
			Debug.LogWarning("Tried to get random patrol node, but no nodes in this map are marked as patrol!  Returning random general node instead.");
			return GetRandomNode();
		}
		return _patrolNodes[Random.Range(0, _patrolNodes.Length)];
	}

	public GhostNode[] GetSearchNodesOnLayer(GhostNode.NodeLayer layer)
	{
		List<GhostNode> list = new List<GhostNode>();
		for (int i = 0; i < _searchNodes.Length; i++)
		{
			if ((layer & _searchNodes[i].searchData.searchLayer) != 0)
			{
				list.Add(_searchNodes[i]);
			}
		}
		return list.ToArray();
	}

	public GhostEdge FindClosestEdge(Vector3 localPosition, out Vector3 closestPointOnEdge)
	{
		GhostEdge result = null;
		closestPointOnEdge = Vector3.zero;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _edges.Length; i++)
		{
			Vector3 vector = OWMath.ClosestPointOnSegment(localPosition, _edges[i].nodeOne.localPosition, _edges[i].nodeTwo.localPosition);
			float num2 = Vector3.Distance(localPosition, vector);
			if (num2 < num)
			{
				closestPointOnEdge = vector;
				num = num2;
				result = _edges[i];
			}
		}
		return result;
	}

	public GhostNode FindClosestNode(Vector3 localPosition)
	{
		GhostNode result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _nodes.Length; i++)
		{
			float sqrMagnitude = (localPosition - _nodes[i].localPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = _nodes[i];
			}
		}
		return result;
	}

	public GhostNode FindClosestPatrolNode(Vector3 localPosition)
	{
		GhostNode result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _patrolNodes.Length; i++)
		{
			float sqrMagnitude = (localPosition - _patrolNodes[i].localPosition).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = _patrolNodes[i];
			}
		}
		return result;
	}

	public int FindPossiblePlayerNodes(Vector3 startPosition, Vector3 localVelocity, float maxDistance, NodeSearchData[] results, bool pruning = true, float[] turnScores = null, Vector3[] cameFromPos = null, bool[] pruned = null)
	{
		if (_initialized)
		{
			InitializeMap();
		}
		for (int i = 0; i < _pathData.Length; i++)
		{
			_pathData[i].Reset();
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<GhostNode> list3 = new List<GhostNode>();
		Vector3 closestPointOnEdge;
		GhostEdge ghostEdge = FindClosestEdge(startPosition, out closestPointOnEdge);
		float sqrMagnitude = (ghostEdge.nodeOne.localPosition - startPosition).sqrMagnitude;
		float sqrMagnitude2 = (ghostEdge.nodeTwo.localPosition - startPosition).sqrMagnitude;
		Vector3 vector = ghostEdge.nodeTwo.localPosition - ghostEdge.nodeOne.localPosition;
		bool flag = localVelocity.sqrMagnitude > 0.010000001f;
		Vector3 vector2 = (flag ? localVelocity.normalized : Vector3.zero);
		bool flag2 = false;
		float num = Vector3.Dot(vector.normalized, vector2);
		if (sqrMagnitude < 4f && sqrMagnitude < sqrMagnitude2)
		{
			list.Add(ghostEdge.nodeOne.index);
			flag2 = true;
		}
		else if (sqrMagnitude2 < 4f && sqrMagnitude2 < sqrMagnitude)
		{
			list.Add(ghostEdge.nodeTwo.index);
			flag2 = true;
		}
		else if (flag && Mathf.Abs(num) > 0.5f)
		{
			if (num > 0f)
			{
				list.Add(ghostEdge.nodeTwo.index);
				list2.Add(ghostEdge.nodeOne.index);
			}
			else
			{
				list.Add(ghostEdge.nodeOne.index);
				list2.Add(ghostEdge.nodeTwo.index);
			}
		}
		else
		{
			list.Add(ghostEdge.nodeOne.index);
			list.Add(ghostEdge.nodeTwo.index);
		}
		for (int j = 0; j < list.Count; j++)
		{
			_pathData[list[j]].gScore = Vector3.Distance(closestPointOnEdge, _nodes[list[j]].localPosition);
			_pathData[list[j]].turnScore = 0f;
		}
		while (list.Count > 0)
		{
			int num2 = list[0];
			int cameFromIndex = _pathData[num2].cameFromIndex;
			list.RemoveAt(0);
			list2.Add(num2);
			if (_pathData[num2].gScore > maxDistance)
			{
				if (cameFromIndex == -1)
				{
					list3.Add(_nodes[num2]);
					continue;
				}
				float num3 = maxDistance - _pathData[cameFromIndex].gScore;
				float num4 = _pathData[num2].gScore - _pathData[cameFromIndex].gScore;
				if (num3 > num4 * 0.5f)
				{
					list3.Add(_nodes[num2]);
				}
				continue;
			}
			list3.Add(_nodes[num2]);
			float num5 = 180f;
			if (cameFromIndex == -1 && !flag && flag2)
			{
				for (int k = 0; k < _nodes[num2].neighbors.Count; k++)
				{
					int index = _nodes[num2].neighbors[k].index;
					_pathData[index].neighborAngle = 0f;
					_pathData[index].turnScore = 0f;
				}
				num5 = 0f;
			}
			else
			{
				Vector3 to = ((cameFromIndex != -1) ? (_nodes[num2].localPosition - _nodes[cameFromIndex].localPosition) : ((!flag) ? (_nodes[num2].localPosition - closestPointOnEdge) : vector2));
				for (int l = 0; l < _nodes[num2].neighbors.Count; l++)
				{
					int index2 = _nodes[num2].neighbors[l].index;
					if (index2 != cameFromIndex)
					{
						Vector3 vector3 = ((cameFromIndex == -1) ? startPosition : _nodes[num2].localPosition);
						float num6 = Vector3.SignedAngle(_nodes[index2].localPosition - vector3, to, _transform.up);
						_pathData[index2].neighborAngle = num6;
						num5 = Mathf.Min(num5, Mathf.Abs(num6));
					}
				}
			}
			for (int m = 0; m < _nodes[num2].neighbors.Count; m++)
			{
				int index3 = _nodes[num2].neighbors[m].index;
				if (index3 == cameFromIndex || list2.Contains(index3))
				{
					continue;
				}
				if (cameFromIndex == -1)
				{
					float num7 = Mathf.Abs(_pathData[index3].neighborAngle);
					if ((num5 <= 20f && num7 > 75f) || (num5 <= 45f && num7 > 85f) || (num5 <= 85f && num7 > 100f) || (num5 <= 100f && num7 > 120f) || (num5 <= 120f && num7 > 150f))
					{
						continue;
					}
				}
				float num8 = _pathData[num2].gScore + _nodes[num2].neighborEdges[m].length;
				if (!list.Contains(index3))
				{
					list.Add(index3);
				}
				else if (num8 >= _pathData[index3].gScore)
				{
					continue;
				}
				_pathData[index3].gScore = num8;
				_pathData[index3].cameFromIndex = num2;
				_pathData[index3].turnScore = _pathData[num2].turnScore + _pathData[index3].neighborAngle;
			}
		}
		for (int n = 0; n < list3.Count; n++)
		{
			PruneSearchTreeRecursive(list3[n].index);
		}
		if (pruning)
		{
			for (int num9 = list3.Count - 1; num9 >= 0; num9--)
			{
				if (_pathData[list3[num9].index].pruned)
				{
					list3.QuickRemoveAt(num9);
				}
			}
		}
		list3.Sort(SortSearchNodes);
		for (int num10 = 0; num10 < list3.Count; num10++)
		{
			results[num10].node = list3[num10];
			results[num10].score = _pathData[list3[num10].index].turnScore;
			results[num10].searched = false;
		}
		for (int num11 = 0; num11 < list3.Count; num11++)
		{
			if (turnScores != null)
			{
				turnScores[num11] = _pathData[list3[num11].index].turnScore;
			}
			if (cameFromPos != null)
			{
				int cameFromIndex2 = _pathData[list3[num11].index].cameFromIndex;
				if (cameFromIndex2 == -1)
				{
					cameFromPos[num11] = list3[num11].localPosition;
				}
				else
				{
					cameFromPos[num11] = _nodes[cameFromIndex2].localPosition;
				}
			}
			if (pruned != null)
			{
				pruned[num11] = _pathData[list3[num11].index].pruned;
			}
		}
		return list3.Count;
	}

	private bool PruneSearchTreeRecursive(int nodeIndex)
	{
		if (_pathData[nodeIndex].visitedWhilePruning)
		{
			return _pathData[nodeIndex].pruned;
		}
		_pathData[nodeIndex].visitedWhilePruning = true;
		if (_pathData[nodeIndex].cameFromIndex == -1)
		{
			_pathData[nodeIndex].pruned = false;
		}
		else if (Mathf.Abs(_pathData[nodeIndex].turnScore) > 135f)
		{
			_pathData[nodeIndex].pruned = true;
		}
		else
		{
			_pathData[nodeIndex].pruned = PruneSearchTreeRecursive(_pathData[nodeIndex].cameFromIndex);
		}
		return _pathData[nodeIndex].pruned;
	}

	private int SortSearchNodes(GhostNode x, GhostNode y)
	{
		float num = Mathf.Abs(_pathData[x.index].turnScore);
		float num2 = Mathf.Abs(_pathData[y.index].turnScore);
		if (num > num2)
		{
			return -1;
		}
		if (num2 > num)
		{
			return 1;
		}
		return 0;
	}

	public List<Vector3> FindPossiblePlayerPositions(Vector3 startPosition, Vector3 localVelocity, float maxDistance)
	{
		if (!_initialized)
		{
			InitializeMap();
		}
		for (int i = 0; i < _pathData.Length; i++)
		{
			_pathData[i].Reset();
		}
		Vector3 closestPointOnEdge;
		GhostEdge ghostEdge = FindClosestEdge(startPosition, out closestPointOnEdge);
		Vector3 vector = ghostEdge.nodeTwo.localPosition - ghostEdge.nodeOne.localPosition;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<Vector3> list3 = new List<Vector3>();
		float num = Vector3.Dot(vector.normalized, localVelocity.normalized);
		if (num > 0f)
		{
			list.Add(ghostEdge.nodeTwo.index);
			list2.Add(ghostEdge.nodeOne.index);
		}
		else if (num <= 0f)
		{
			list.Add(ghostEdge.nodeOne.index);
			list2.Add(ghostEdge.nodeTwo.index);
		}
		if (Mathf.Abs(num) < 0.5f)
		{
			Debug.LogWarning("Player moving perpendicular to closest edge!!!");
		}
		for (int j = 0; j < list.Count; j++)
		{
			_pathData[list[j]].gScore = Vector3.Distance(closestPointOnEdge, _nodes[list[j]].localPosition);
		}
		while (list.Count > 0)
		{
			int num2 = list[0];
			list.RemoveAt(0);
			list2.Add(num2);
			if (_pathData[num2].gScore > maxDistance)
			{
				int cameFromIndex = _pathData[num2].cameFromIndex;
				float num3 = _pathData[num2].gScore - maxDistance;
				Vector3 vector2;
				float num4;
				if (cameFromIndex == -1)
				{
					vector2 = closestPointOnEdge;
					num4 = _pathData[num2].gScore;
				}
				else
				{
					vector2 = _nodes[cameFromIndex].localPosition;
					num4 = _pathData[num2].gScore - _pathData[cameFromIndex].gScore;
				}
				Vector3 item = vector2 + (_nodes[num2].localPosition - vector2).normalized * (num4 - num3);
				list3.Add(item);
				continue;
			}
			bool flag = false;
			for (int k = 0; k < _nodes[num2].neighbors.Count; k++)
			{
				int index = _nodes[num2].neighbors[k].index;
				if (!list2.Contains(index))
				{
					float num5 = _pathData[num2].gScore + _nodes[num2].neighborEdges[k].length;
					if (!list.Contains(index))
					{
						list.Add(index);
					}
					else if (num5 >= _pathData[index].gScore)
					{
						continue;
					}
					_pathData[index].gScore = num5;
					_pathData[index].cameFromIndex = num2;
					flag = true;
				}
			}
			if (!flag)
			{
				list3.Add(_nodes[num2].localPosition);
			}
		}
		return list3;
	}

	public List<GhostNode> FindPath(Vector3 startLocalPosition, GhostEdge startEdge, GhostNode endNode)
	{
		startEdge.ToArray(_startNodes);
		_singleEndNode[0] = endNode;
		return FindPath(startLocalPosition, _startNodes, endNode.localPosition, _singleEndNode);
	}

	public List<GhostNode> FindPath(Vector3 startLocalPosition, GhostEdge startEdge, Vector3 endLocalPosition, GhostEdge endEdge)
	{
		startEdge.ToArray(_startNodes);
		endEdge.ToArray(_endNodes);
		return FindPath(startLocalPosition, _startNodes, endLocalPosition, _endNodes);
	}

	public float FindDistanceToNode(Vector3 startLocalPosition, GhostNode endNode)
	{
		Vector3 closestPointOnEdge;
		GhostEdge startEdge = FindClosestEdge(startLocalPosition, out closestPointOnEdge);
		List<GhostNode> list = FindPath(startLocalPosition, startEdge, endNode);
		if (list == null)
		{
			return float.PositiveInfinity;
		}
		float num = 0f;
		num += Vector3.Distance(startLocalPosition, closestPointOnEdge);
		for (int i = 0; i < list.Count - 1; i++)
		{
			for (int j = 0; j < list[i].neighborEdges.Count; j++)
			{
				if (list[i].neighborEdges[j].ContainsNode(list[i + 1]))
				{
					num += list[i].neighborEdges[j].length;
					break;
				}
			}
		}
		return num;
	}

	private List<GhostNode> FindPath(Vector3 startLocalPosition, GhostNode[] startNodes, Vector3 endLocalPosition, GhostNode[] endNodes)
	{
		if (!_initialized)
		{
			InitializeMap();
		}
		s_openSet.Clear();
		s_closedSet.Clear();
		for (int i = 0; i < _pathData.Length; i++)
		{
			_pathData[i].Reset();
		}
		for (int j = 0; j < startNodes.Length; j++)
		{
			s_openSet.Add(startNodes[j].index);
			_pathData[startNodes[j].index].gScore = Vector3.Distance(startLocalPosition, startNodes[j].localPosition);
			_pathData[startNodes[j].index].fScore = _pathData[startNodes[j].index].gScore + Vector3.Distance(startNodes[j].localPosition, endLocalPosition);
		}
		while (s_openSet.Count > 0)
		{
			int num = -1;
			float num2 = float.PositiveInfinity;
			for (int k = 0; k < s_openSet.Count; k++)
			{
				if (_pathData[s_openSet[k]].fScore < num2)
				{
					num = s_openSet[k];
					num2 = _pathData[s_openSet[k]].fScore;
				}
			}
			if (CheckEndConditions(num, endNodes))
			{
				return ReconstructPath(_pathData, _nodes[num]);
			}
			s_openSet.Remove(num);
			s_closedSet.Add(num);
			for (int l = 0; l < _nodes[num].neighbors.Count; l++)
			{
				int index = _nodes[num].neighbors[l].index;
				if (!s_closedSet.Contains(index))
				{
					float num3 = _pathData[num].gScore + _nodes[num].neighborEdges[l].length;
					if (!s_openSet.Contains(index))
					{
						s_openSet.Add(index);
					}
					else if (num3 >= _pathData[index].gScore)
					{
						continue;
					}
					_pathData[index].cameFromIndex = num;
					_pathData[index].gScore = num3;
					_pathData[index].fScore = num3 + Vector3.Distance(_nodes[num].neighbors[l].localPosition, endLocalPosition);
				}
			}
		}
		return null;
	}

	private bool CheckEndConditions(int currentIndex, GhostNode[] endNodes)
	{
		for (int i = 0; i < endNodes.Length; i++)
		{
			if (currentIndex == endNodes[i].index)
			{
				return true;
			}
		}
		return false;
	}

	private List<GhostNode> ReconstructPath(PathData[] pathData, GhostNode endNode)
	{
		List<GhostNode> list = new List<GhostNode> { endNode };
		while (pathData[list[list.Count - 1].index].cameFromIndex > -1)
		{
			GhostNode ghostNode = list[list.Count - 1];
			GhostNode item = _nodes[pathData[ghostNode.index].cameFromIndex];
			list.Add(item);
		}
		return list;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			Gizmos.color = Color.red;
			for (int i = 0; i < _markerEdges.Count; i++)
			{
				Gizmos.DrawLine(_markerEdges[i].markerOne.transform.position, _markerEdges[i].markerTwo.transform.position);
			}
		}
		else if (_nodes != null && _edges != null)
		{
			Gizmos.color = Color.green;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			for (int j = 0; j < _nodes.Length; j++)
			{
				Gizmos.DrawSphere(_nodes[j].localPosition, 0.2f);
			}
			for (int k = 0; k < _edges.Length; k++)
			{
				Gizmos.DrawLine(_edges[k].nodeOne.localPosition, _edges[k].nodeTwo.localPosition);
			}
		}
	}
}
