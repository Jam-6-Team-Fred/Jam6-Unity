using UnityEngine;

public class DreamArrivalPoint : MonoBehaviour
{
	public enum Location
	{
		Undefined = 0,
		Zone1 = 100,
		Zone2 = 200,
		Zone3 = 300,
		Zone4 = 400
	}

	[SerializeField]
	private Location _location;

	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private Campfire _campfire;

	[SerializeField]
	private DreamExplosionController _explosionController;

	[SerializeField]
	private OWTriggerVolume[] _entrywayVolumes;

	[SerializeField]
	private Transform _raftSpawn;

	private DreamCampfire _connectedDreamCampfire;

	private void Awake()
	{
		if (_location != 0)
		{
			Locator.RegisterDreamArrivalPoint(this, _location);
		}
	}

	private void Start()
	{
		_connectedDreamCampfire = Locator.GetDreamCampfire(_location);
		if (_connectedDreamCampfire != null)
		{
			_connectedDreamCampfire.OnDreamCampfireExtinguished += new OWEvent.OWCallback(OnDreamCampfireExtinguished);
		}
	}

	private void OnDestroy()
	{
		Locator.UnregisterDreamArrivalPoint(this, _location);
		if (_connectedDreamCampfire != null)
		{
			_connectedDreamCampfire.OnDreamCampfireExtinguished -= new OWEvent.OWCallback(OnDreamCampfireExtinguished);
		}
	}

	public Location GetLocation()
	{
		return _location;
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public Transform GetRaftSpawnTransform()
	{
		return _raftSpawn;
	}

	public void OnEnterDreamWorld(DreamLanternItem lantern)
	{
		for (int i = 0; i < _entrywayVolumes.Length; i++)
		{
			_entrywayVolumes[i].AddObjectToVolume(Locator.GetPlayerDetector().gameObject);
			_entrywayVolumes[i].AddObjectToVolume(Locator.GetPlayerCameraDetector().gameObject);
			_entrywayVolumes[i].AddObjectToVolume(lantern.GetFluidDetector().gameObject);
		}
		if (_explosionController != null)
		{
			_explosionController.OnEnterDreamWorld(lantern);
		}
	}

	public void OnExitDreamWorld()
	{
		if (_explosionController != null)
		{
			_explosionController.OnExitDreamWorld();
		}
	}

	private void OnDreamCampfireExtinguished()
	{
		_campfire.SetState(Campfire.State.UNLIT);
		_campfire.PlayOneShot(AudioType.DreamFire_Extinguish);
	}
}
