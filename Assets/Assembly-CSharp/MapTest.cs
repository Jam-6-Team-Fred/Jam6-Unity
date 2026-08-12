using UnityEngine;

public class MapTest : MonoBehaviour
{
	[SerializeField]
	private GhostNodeMap _map;

	[SerializeField]
	private bool _check;

	[SerializeField]
	private bool _editorChecks;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private float _maxDist = 40f;

	[Space]
	[SerializeField]
	private bool _visitOrderColors;

	[SerializeField]
	private bool _turnScoreColors;

	[SerializeField]
	private bool _printTurnScores;

	private int _numNodes;

	private GhostNodeMap.NodeSearchData[] _nodes;

	private float[] _turnScores;

	private Vector3[] _cameFromPos;

	private bool[] _pruned;

	private void Start()
	{
		_numNodes = 0;
		_nodes = new GhostNodeMap.NodeSearchData[_map.GetNodeCount()];
		_turnScores = new float[_nodes.Length];
		_cameFromPos = new Vector3[_nodes.Length];
		_pruned = new bool[_nodes.Length];
	}

	private void Update()
	{
		if (_check || _editorChecks)
		{
			_check = false;
			_numNodes = _map.FindPossiblePlayerNodes(base.transform.localPosition, _map.transform.parent.InverseTransformVector(base.transform.forward * _speed), _maxDist, _nodes, pruning: false, _turnScores, _cameFromPos, _pruned);
		}
	}

	private void OnDrawGizmos()
	{
		if (_map == null || _numNodes == 0 || _nodes == null) return; // CHANGED
		
		Gizmos.matrix = _map.transform.localToWorldMatrix;
		Gizmos.color = Color.cyan;
		for (int i = 0; i < _numNodes; i++)
		{
			if (_visitOrderColors)
			{
				float t = (float)i / (float)_numNodes;
				Gizmos.color = Color.HSVToRGB(Mathf.Lerp(0.5f, 0f, t), 1f, 1f);
			}
			else if (_turnScoreColors)
			{
				float t2 = Mathf.Abs(_turnScores[i]) / 180f;
				Gizmos.color = Color.HSVToRGB(Mathf.Lerp(0.5f, 0f, t2), 1f, 1f);
			}
			if (_pruned[i])
			{
				Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, (Time.unscaledTime % 0.5f > 0.15f) ? 1f : 0f);
			}
			Gizmos.DrawWireSphere(_nodes[i].node.localPosition, 1f);
			Gizmos.DrawLine(_nodes[i].node.localPosition + Vector3.up, _cameFromPos[i] + Vector3.up);
			_ = _printTurnScores;
		}
	}
}
