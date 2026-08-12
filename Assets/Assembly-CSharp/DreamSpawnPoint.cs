using UnityEngine;

public class DreamSpawnPoint : SpawnPoint
{
	[Header("Dream World")]
	[SerializeField]
	private DreamArrivalPoint.Location _ringWorldSleepLocation;

	[SerializeField]
	private DreamWorldController _dreamController;

	[SerializeField]
	private DreamArrivalPoint _zoneArrivalPoint;

	public override void OnSpawnPlayer()
	{
		_dreamController.SpawnInDreamWorld(_ringWorldSleepLocation, _zoneArrivalPoint);
		AddObjectToTriggerVolumes(_dreamController.GetPlayerLantern().GetFluidDetector().gameObject);
	}
}
