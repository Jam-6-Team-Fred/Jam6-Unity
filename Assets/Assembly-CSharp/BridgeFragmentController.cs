using UnityEngine;

public class BridgeFragmentController : MonoBehaviour
{
	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	private Transform _snapTarget;

	[Space]
	[SerializeField]
	private Shape _detectorShape;

	private OWRigidbody _ringworldBody;

	private OWRigidbody _bridgeBody;

	private void Awake()
	{
		_bridgeBody = this.GetRequiredComponent<OWRigidbody>();
		_sector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
	}

	private void Start()
	{
		_ringworldBody = Locator.GetRingWorldController().GetAttachedOWRigidbody();
		if (!_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			if ((bool)_snapTarget)
			{
				_bridgeBody.transform.position = _snapTarget.position;
				_bridgeBody.transform.rotation = _snapTarget.rotation;
			}
			Suspend();
		}
		SectorCullGroup component = GetComponent<SectorCullGroup>();
		if (component != null)
		{
			component.RefreshSectorVisibilityState();
		}
	}

	private void OnDestroy()
	{
		_sector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
		_sector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
	}

	private void OnOccupantEnterSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			Unsuspend();
		}
	}

	private void OnOccupantExitSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			Suspend();
		}
	}

	private void Suspend()
	{
		_bridgeBody.Suspend(_ringworldBody);
		if (_detectorShape != null)
		{
			_detectorShape.SetActivation(newActive: false);
		}
	}

	private void Unsuspend()
	{
		_bridgeBody.Unsuspend();
		if (_detectorShape != null)
		{
			_detectorShape.SetActivation(newActive: true);
		}
	}
}
