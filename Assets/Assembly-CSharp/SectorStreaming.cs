using UnityEngine;

[AddComponentMenu("Streaming/Sector Streaming", 200)]
public class SectorStreaming : SectoredMonoBehaviour
{
	protected static int s_playerSectorCount;

	[SerializeField]
	private StreamingGroup _streamingGroup;

	[SerializeField]
	private float _softLoadRadius = 2000f;

	private Transform _playerTransform;

	private SurveyorProbe _probe;

	private bool _playerInSoftLoadRadius;

	private bool _probeInSoftLoadRadius;

	protected override void Awake()
	{
		base.Awake();
		_playerInSoftLoadRadius = false;
		_probeInSoftLoadRadius = false;
		if (!StreamingManager.isStreamingEnabled)
		{
			base.enabled = false;
		}
	}

	protected virtual void Start()
	{
		_playerTransform = Locator.GetPlayerTransform();
		_probe = Locator.GetProbe();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected virtual void UpdateLoadingPriority()
	{
		if (OWTime.IsPaused())
		{
			StreamingManager.loadingPriority = StreamingManager.LoadingPriority.High;
		}
		else if (s_playerSectorCount == 0)
		{
			StreamingManager.loadingPriority = StreamingManager.LoadingPriority.Normal;
		}
		else
		{
			StreamingManager.loadingPriority = StreamingManager.LoadingPriority.Low;
		}
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (base.enabled)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_streamingGroup.RequestGeneralAssets();
				s_playerSectorCount++;
			}
			UpdateLoadingPriority();
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (base.enabled)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_streamingGroup.ReleaseGeneralAssets();
				s_playerSectorCount--;
			}
			UpdateLoadingPriority();
		}
	}

	protected virtual void FixedUpdate()
	{
		bool flag = (_playerTransform.position - _sector.transform.position).sqrMagnitude < _softLoadRadius * _softLoadRadius;
		bool flag2 = _probe != null && _probe.IsLaunched() && (_probe.transform.position - _sector.transform.position).sqrMagnitude < _softLoadRadius * _softLoadRadius;
		if (PlayerState.OnQuantumMoon() && Locator.GetQuantumMoon().IsPlayerInsideShrine() && _sector.GetName() != Sector.Name.QuantumMoon)
		{
			flag = false;
			flag2 = false;
		}
		if (!_playerInSoftLoadRadius && flag)
		{
			_streamingGroup.RequestRequiredAssets();
		}
		else if (_playerInSoftLoadRadius && !flag)
		{
			_streamingGroup.ReleaseRequiredAssets();
		}
		if (!_probeInSoftLoadRadius && flag2)
		{
			_streamingGroup.RequestRequiredAssets();
		}
		else if (_probeInSoftLoadRadius && !flag2)
		{
			_streamingGroup.ReleaseRequiredAssets();
		}
		_playerInSoftLoadRadius = flag;
		_probeInSoftLoadRadius = flag2;
	}

	protected virtual void LateUpdate()
	{
		if (OWTime.IsPaused())
		{
			FixedUpdate();
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(base.transform.position, _softLoadRadius);
		}
	}
}
