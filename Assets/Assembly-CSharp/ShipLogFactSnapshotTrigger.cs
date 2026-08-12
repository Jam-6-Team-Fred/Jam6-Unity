using UnityEngine;

public class ShipLogFactSnapshotTrigger : MonoBehaviour
{
	[SerializeField]
	private string[] _factIDs;

	[SerializeField]
	private float _maxDistance = 200f;

	private VisibilityTracker _visibilityTracker;

	private void Awake()
	{
		_visibilityTracker = GetComponent<VisibilityTracker>();
		GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", OnProbeSnapshot);
	}

	private void OnDestroy()
	{
		GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
	}

	private void OnProbeSnapshot(ProbeCamera probeCamera)
	{
		if (_visibilityTracker != null && _visibilityTracker.IsVisibleToProbe(probeCamera.GetOWCamera()) && (_visibilityTracker.transform.position - probeCamera.transform.position).magnitude < _maxDistance)
		{
			for (int i = 0; i < _factIDs.Length; i++)
			{
				Locator.GetShipLogManager().RevealFact(_factIDs[i]);
			}
		}
	}
}
