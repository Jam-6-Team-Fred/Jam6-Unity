using UnityEngine;

[AddComponentMenu("Streaming/Linked Sector Streaming", 200)]
public class LinkedSectorStreaming : SectorStreaming
{
	[Space]
	[SerializeField]
	private string _linkedSceneName = "";

	private StreamingGroup _linkedStreamingGroup;

	protected override void Start()
	{
		base.Start();
		_linkedStreamingGroup = StreamingGroup.GetStreamingGroup(_linkedSceneName);
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		base.OnSectorOccupantAdded(sectorDetector);
		if (base.enabled && !(_linkedStreamingGroup == null))
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_linkedStreamingGroup.RequestRequiredAssets(-10);
				_linkedStreamingGroup.RequestGeneralAssets(-10);
			}
			else if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
			{
				_linkedStreamingGroup.RequestRequiredAssets(-10);
			}
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		base.OnSectorOccupantRemoved(sectorDetector);
		if (base.enabled && !(_linkedStreamingGroup == null))
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_linkedStreamingGroup.ReleaseRequiredAssets();
				_linkedStreamingGroup.ReleaseGeneralAssets();
			}
			else if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
			{
				_linkedStreamingGroup.ReleaseRequiredAssets();
			}
		}
	}
}
