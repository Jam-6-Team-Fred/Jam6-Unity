using UnityEngine;

public class MapReferenceFrameVolume : ReferenceFrameVolume
{
	[Space]
	[SerializeField]
	private float _mapMaxTargetDistance = 50000f;

	private float _defaultMaxTargetDistance;

	protected override void Awake()
	{
		base.Awake();
		_defaultMaxTargetDistance = _referenceFrame.GetMaxTargetDistance();
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void OnEnterMapView()
	{
		_referenceFrame.SetMaxTargetDistance(_mapMaxTargetDistance);
	}

	private void OnExitMapView()
	{
		_referenceFrame.SetMaxTargetDistance(_defaultMaxTargetDistance);
	}
}
