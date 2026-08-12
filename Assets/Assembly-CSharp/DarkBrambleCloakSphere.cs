using UnityEngine;

public class DarkBrambleCloakSphere : MonoBehaviour
{
	[SerializeField]
	private Sector[] _sectors;

	private Renderer _renderer;

	private int _playerCount;

	private int _probeCount;

	private void Awake()
	{
		base.enabled = false;
		_renderer = GetComponent<Renderer>();
		_playerCount = 0;
		_probeCount = 0;
		for (int i = 0; i < _sectors.Length; i++)
		{
			_sectors[i].OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
			_sectors[i].OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _sectors.Length; i++)
		{
			_sectors[i].OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantEnterSector);
			_sectors[i].OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnOccupantExitSector);
		}
	}

	private void OnOccupantEnterSector(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerCount++;
		}
		else if (detector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeCount++;
		}
		_renderer.enabled = true;
		base.enabled = false;
	}

	private void OnOccupantExitSector(SectorDetector detector)
	{
		if (detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerCount--;
		}
		else if (detector.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeCount--;
		}
		if (_playerCount == 0 && _probeCount == 0)
		{
			_renderer.enabled = false;
		}
	}
}
