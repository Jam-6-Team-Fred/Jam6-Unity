using UnityEngine;

[AddComponentMenu("Streaming/Sector Preload Streaming", 200)]
public class SectorPreloadStreaming : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private string _preloadSceneName = "";

	private StreamingGroup _preloadStreamingGroup;

	private void Awake()
	{
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
		_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
		if (!StreamingManager.isStreamingEnabled)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		_preloadStreamingGroup = StreamingGroup.GetStreamingGroup(_preloadSceneName);
		if (_preloadStreamingGroup == null)
		{
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantAdded);
		_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnSectorOccupantRemoved);
	}

	private void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (base.enabled)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_preloadStreamingGroup.RequestRequiredAssets(-10);
				_preloadStreamingGroup.RequestGeneralAssets(-10);
			}
			else if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
			{
				_preloadStreamingGroup.RequestRequiredAssets(-10);
			}
		}
	}

	private void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (base.enabled)
		{
			if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
			{
				_preloadStreamingGroup.ReleaseRequiredAssets();
				_preloadStreamingGroup.ReleaseGeneralAssets();
			}
			else if (sectorDetector.GetOccupantType() == DynamicOccupant.Probe)
			{
				_preloadStreamingGroup.ReleaseRequiredAssets();
			}
		}
	}
}
