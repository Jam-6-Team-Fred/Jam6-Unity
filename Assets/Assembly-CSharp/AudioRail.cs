using UnityEngine;

[AddComponentMenu("Audio/Audio Rail", 300)]
public class AudioRail : SectoredMonoBehaviour
{
	[SerializeField]
	private Transform _audioTransform;

	[SerializeField]
	private Transform _railPointsRoot;

	[SerializeField]
	[HideInInspector]
	private bool _prebuilt;

	[SerializeField]
	[HideInInspector]
	private Vector3[] _railPoints;

	[SerializeField]
	[HideInInspector]
	private LineSegmentDistanceTracker[] _lineSegments;

	private bool _initialized;

	protected override void Awake()
	{
		base.Awake();
		if (!_prebuilt)
		{
			BuildAudioRail();
		}
		if (_audioTransform.parent != base.transform)
		{
			Debug.LogError("TransformToMove must be on a child transform");
		}
		else if (_railPoints.Length < 2)
		{
			Debug.LogError("Rail requires at least two points");
		}
		else
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		if (!_initialized)
		{
			Initialize();
		}
	}

	private void BuildAudioRail()
	{
		_railPoints = new Vector3[_railPointsRoot.childCount];
		for (int i = 0; i < _railPointsRoot.childCount; i++)
		{
			_railPoints[i] = _railPointsRoot.GetChild(i).localPosition;
		}
		if (_railPoints.Length > 1)
		{
			_lineSegments = new LineSegmentDistanceTracker[_railPoints.Length - 1];
			for (int j = 0; j < _railPoints.Length - 1; j++)
			{
				_lineSegments[j] = base.gameObject.AddComponent<LineSegmentDistanceTracker>();
			}
		}
	}

	private void Initialize()
	{
		Transform playerTransform = Locator.GetPlayerTransform();
		for (int i = 1; i < _railPoints.Length; i++)
		{
			_lineSegments[i - 1].Initialize(_railPoints[i - 1], _railPoints[i], playerTransform, _railPointsRoot);
		}
		_initialized = true;
	}

	private void Update()
	{
		float num = float.MaxValue;
		LineSegmentDistanceTracker lineSegmentDistanceTracker = null;
		for (int i = 0; i < _lineSegments.Length; i++)
		{
			float distanceToTarget = _lineSegments[i].GetDistanceToTarget();
			if (distanceToTarget < num)
			{
				num = distanceToTarget;
				lineSegmentDistanceTracker = _lineSegments[i];
			}
		}
		Vector3 closestPointOnSegment = lineSegmentDistanceTracker.GetClosestPointOnSegment();
		_audioTransform.position = closestPointOnSegment;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			if (!_initialized)
			{
				Initialize();
			}
			for (int i = 0; i < _lineSegments.Length; i++)
			{
				_lineSegments[i].SetPersistentTarget(Locator.GetPlayerTransform());
			}
			base.enabled = true;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			for (int i = 0; i < _lineSegments.Length; i++)
			{
				_lineSegments[i].SetPersistentTarget(null);
			}
			base.enabled = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_railPointsRoot == null)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		for (int i = 0; i < _railPointsRoot.childCount; i++)
		{
			Gizmos.DrawWireSphere(_railPointsRoot.GetChild(i).position, 1f);
			if (i > 0)
			{
				Gizmos.DrawLine(_railPointsRoot.GetChild(i - 1).position, _railPointsRoot.GetChild(i).position);
			}
		}
		Gizmos.DrawSphere(_audioTransform.position, 1f);
	}
}
