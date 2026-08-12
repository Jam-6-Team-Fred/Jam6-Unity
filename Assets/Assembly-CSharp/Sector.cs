using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Sectors/Sector", 100)]
public class Sector : MonoBehaviour
{
	public enum Name
	{
		Unnamed = 0,
		Sun = 1,
		HourglassTwin_A = 2,
		HourglassTwin_B = 3,
		TimberHearth = 4,
		BrittleHollow = 5,
		GiantsDeep = 6,
		DarkBramble = 7,
		Comet = 8,
		QuantumMoon = 9,
		TimberMoon = 10,
		BrambleDimension = 11,
		VolcanicMoon = 12,
		OrbitalProbeCannon = 13,
		EyeOfTheUniverse = 14,
		Ship = 15,
		SunStation = 16,
		WhiteHole = 17,
		TimeLoopDevice = 18,
		Vessel = 19,
		VesselDimension = 20,
		HourglassTwins = 21,
		InvisiblePlanet = 22,
		DreamWorld = 23
	}

	[SerializeField]
	private Name _name;

	[SerializeField]
	private GameObject _triggerRoot;

	[SerializeField]
	private List<Sector> _subsectors;

	[SerializeField]
	private string _idString;

	[Space]
	[SerializeField]
	private bool _subsectorOverlapSafetyCheck;

	private OWRigidbody _attachedOWRigidbody;

	private Sector _parentSector;

	private ProximityTrigger _proximityTrigger;

	private OWTriggerVolume _owTriggerVolume;

	private VolumeExcluder _volumeExcluder;

	private List<SectorDetector> _dynamicOccupants = new List<SectorDetector>(8);

	private List<SectorDetector> _excludedOccupants = new List<SectorDetector>(8);

	private List<SectorDetector> _occupantsTracked = new List<SectorDetector>(8);

	private DynamicOccupant _occupantMask;

	private int _frameCounter;

	private bool _firstUpdate = true;

	private bool _overriddenByQuantumMoon;

	private bool _playerInTriggerVolume;

	private bool _probeInTriggerVolume;

	private List<FogLight> _fogLightsInSector;

	public OWEvent OnSectorOccupantsUpdated = new OWEvent(128);

	public OWEvent<SectorDetector> OnOccupantEnterSector = new OWEvent<SectorDetector>(128);

	public OWEvent<SectorDetector> OnOccupantExitSector = new OWEvent<SectorDetector>(128);

	protected virtual void Awake()
	{
		if (_triggerRoot == null)
		{
			_triggerRoot = base.gameObject;
		}
		_proximityTrigger = _triggerRoot.GetComponent<ProximityTrigger>();
		if (_proximityTrigger != null)
		{
			_proximityTrigger.AddListeners(OnEntry, OnExit);
		}
		_owTriggerVolume = _triggerRoot.GetComponent<OWTriggerVolume>();
		if (_owTriggerVolume != null)
		{
			_owTriggerVolume.OnEntry += OnEntry;
			_owTriggerVolume.OnExit += OnExit;
		}
		_volumeExcluder = base.gameObject.GetComponent<VolumeExcluder>();
		if (_volumeExcluder != null)
		{
			_volumeExcluder.SetEvents(OnEntryExcluder, OnExitExcluder, OnEntryExcluder, OnExitExcluder);
		}
		if (_proximityTrigger == null && _owTriggerVolume == null)
		{
			Debug.LogError("Could not find any triggers for Sector", this);
			Debug.Break();
		}
		for (int i = 0; i < _subsectors.Count; i++)
		{
			_subsectors[i]._parentSector = this;
		}
		_attachedOWRigidbody = base.gameObject.GetAttachedOWRigidbody();
		SectorManager.RegisterSector(this);
		switch (_name)
		{
		case Name.TimberHearth:
		case Name.BrittleHollow:
		case Name.GiantsDeep:
		case Name.DarkBramble:
		case Name.Comet:
		case Name.TimberMoon:
		case Name.VolcanicMoon:
		case Name.OrbitalProbeCannon:
		case Name.WhiteHole:
		case Name.HourglassTwins:
			_overriddenByQuantumMoon = true;
			break;
		default:
			_overriddenByQuantumMoon = false;
			break;
		}
		if (_overriddenByQuantumMoon)
		{
			GlobalMessenger.AddListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
			GlobalMessenger.AddListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
			GlobalMessenger.AddListener("ProbeEnterQuantumMoon", OnProbeEnterQuantumMoon);
			GlobalMessenger.AddListener("ProbeExitQuantumMoon", OnProbeExitQuantumMoon);
		}
	}

	private void OnDestroy()
	{
		if (_proximityTrigger != null)
		{
			_proximityTrigger.RemoveListeners(OnEntry, OnExit);
		}
		if (_owTriggerVolume != null)
		{
			_owTriggerVolume.OnEntry -= OnEntry;
			_owTriggerVolume.OnExit -= OnExit;
		}
		if (_volumeExcluder != null)
		{
			_volumeExcluder.RemoveEvents(OnEntryExcluder, OnExitExcluder, OnEntryExcluder, OnExitExcluder);
		}
		SectorManager.UnregisterSector(this);
		if (_overriddenByQuantumMoon)
		{
			GlobalMessenger.RemoveListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
			GlobalMessenger.RemoveListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
			GlobalMessenger.RemoveListener("ProbeEnterQuantumMoon", OnProbeEnterQuantumMoon);
			GlobalMessenger.RemoveListener("ProbeExitQuantumMoon", OnProbeExitQuantumMoon);
		}
	}

	public string GetIDString()
	{
		return _idString;
	}

	public OWTriggerVolume GetTriggerVolume()
	{
		return _owTriggerVolume;
	}

	public bool IsBrambleDimension()
	{
		if (_name != Name.BrambleDimension)
		{
			return _name == Name.VesselDimension;
		}
		return true;
	}

	private void AddSubsector(Sector subsector)
	{
		_subsectors.SafeAdd(subsector);
	}

	private void RemoveSubsector(Sector subsector)
	{
		_subsectors.Remove(subsector);
	}

	public void SetParentSector(Sector newParentSector)
	{
		if (_parentSector == newParentSector)
		{
			return;
		}
		if ((bool)_parentSector)
		{
			_parentSector.RemoveSubsector(this);
		}
		if (_proximityTrigger != null)
		{
			for (int i = _dynamicOccupants.Count - 1; i >= 0; i++)
			{
				RemoveOccupant(_dynamicOccupants[i]);
			}
			for (int j = _excludedOccupants.Count - 1; j >= 0; j++)
			{
				RemoveOccupant(_excludedOccupants[j]);
			}
			for (int k = _occupantsTracked.Count - 1; k >= 0; k++)
			{
				RemoveOccupant(_occupantsTracked[k]);
			}
			_proximityTrigger.UntrackAll();
			if (newParentSector == null)
			{
				List<SectorDetector> registeredSectorDetectors = SectorManager.GetRegisteredSectorDetectors();
				for (int l = 0; l < registeredSectorDetectors.Count; l++)
				{
					TrackDetector(registeredSectorDetectors[l]);
				}
			}
			else
			{
				for (int m = 0; m < newParentSector._dynamicOccupants.Count; m++)
				{
					TrackDetector(newParentSector._dynamicOccupants[m]);
				}
			}
		}
		if ((bool)newParentSector)
		{
			newParentSector.AddSubsector(this);
		}
		_parentSector = newParentSector;
	}

	public Name GetName()
	{
		return _name;
	}

	public Sector GetParentSector()
	{
		return _parentSector;
	}

	public Sector GetRootSector()
	{
		if (IsRootSector())
		{
			return this;
		}
		Sector parentSector = _parentSector;
		while (parentSector._parentSector != null)
		{
			parentSector = parentSector._parentSector;
		}
		return parentSector;
	}

	public bool IsRootSector()
	{
		return _parentSector == null;
	}

	public bool IsSubsector(Sector sector)
	{
		if (sector == null)
		{
			return false;
		}
		for (int i = 0; i < _subsectors.Count; i++)
		{
			if (_subsectors[i] == sector)
			{
				return true;
			}
			if (_subsectors[i].IsSubsector(sector))
			{
				return true;
			}
		}
		return false;
	}

	public void SetOWRigidbody(OWRigidbody body)
	{
		_attachedOWRigidbody = body;
	}

	public OWRigidbody GetOWRigidbody()
	{
		return _attachedOWRigidbody;
	}

	public void RegisterFogLight(FogLight fogLight)
	{
		if (_fogLightsInSector == null)
		{
			_fogLightsInSector = new List<FogLight>(32);
		}
		_fogLightsInSector.Add(fogLight);
	}

	public List<FogLight> GetFogLights()
	{
		return _fogLightsInSector;
	}

	public DynamicOccupant GetOccupantMask()
	{
		return _occupantMask;
	}

	public bool ContainsOccupant(DynamicOccupant occupant)
	{
		return (_occupantMask & occupant) == occupant;
	}

	public bool ContainsAnyOccupants(DynamicOccupant occupantMask)
	{
		return (_occupantMask & occupantMask) != 0;
	}

	public bool AddOccupant(SectorDetector detector)
	{
		if (_dynamicOccupants != null && !_dynamicOccupants.Contains(detector))
		{
			_dynamicOccupants.Add(detector);
			RebuildOccupantMask();
			detector.AddSector(this);
			detector.GetOWCollider().OnColliderDisabled += OnSectorDetectorDisabled;
			for (int i = 0; i < _subsectors.Count; i++)
			{
				_subsectors[i].TrackDetector(detector);
			}
			if (_subsectorOverlapSafetyCheck)
			{
				for (int j = 0; j < _subsectors.Count; j++)
				{
					if (_subsectors[j]._owTriggerVolume.IsTrackingObject(detector.gameObject))
					{
						_subsectors[j].AddOccupant(detector);
					}
				}
			}
			OnOccupantEnterSector.Invoke(detector);
			if (!_firstUpdate)
			{
				OnSectorOccupantsUpdated.Invoke();
			}
			return true;
		}
		return false;
	}

	public bool RemoveOccupant(SectorDetector detector)
	{
		if (_dynamicOccupants != null && _dynamicOccupants.Remove(detector))
		{
			RebuildOccupantMask();
			detector.RemoveSector(this);
			detector.GetOWCollider().OnColliderDisabled -= OnSectorDetectorDisabled;
			for (int i = 0; i < _subsectors.Count; i++)
			{
				_subsectors[i].RemoveOccupant(detector);
				_subsectors[i].UntrackDetector(detector);
			}
			OnOccupantExitSector.Invoke(detector);
			if (!_firstUpdate)
			{
				OnSectorOccupantsUpdated.Invoke();
			}
			return true;
		}
		return false;
	}

	private void RebuildOccupantMask()
	{
		_occupantMask = DynamicOccupant.Undefined;
		for (int i = 0; i < _dynamicOccupants.Count; i++)
		{
			_occupantMask |= _dynamicOccupants[i].GetOccupantType();
		}
	}

	public List<SectorDetector> GetOccupants()
	{
		return _dynamicOccupants;
	}

	public void TrackDetector(SectorDetector detector)
	{
		if (_proximityTrigger != null)
		{
			_proximityTrigger.TrackObject(detector.gameObject);
		}
	}

	public void UntrackDetector(SectorDetector detector)
	{
		if (_proximityTrigger != null)
		{
			_proximityTrigger.UntrackObject(detector.gameObject);
		}
	}

	private void FixedUpdate()
	{
		_frameCounter++;
		if (_firstUpdate && _frameCounter >= 3)
		{
			OnSectorOccupantsUpdated.Invoke();
			_firstUpdate = false;
			base.enabled = false;
		}
	}

	private void OnSectorDetectorDisabled(OWCollider collider)
	{
		RemoveOccupant(collider.GetComponent<SectorDetector>());
	}

	private void OnEntry(GameObject hitObj)
	{
		SectorDetector component = hitObj.GetComponent<SectorDetector>();
		if (!(component != null))
		{
			return;
		}
		if (component.GetOccupantType() == DynamicOccupant.Player)
		{
			_playerInTriggerVolume = true;
			if (_overriddenByQuantumMoon && Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInside())
			{
				MonoBehaviour.print("Prevent player from entering " + base.gameObject.name + " while inside Quantum Moon");
				return;
			}
		}
		else if (component.GetOccupantType() == DynamicOccupant.Probe)
		{
			_probeInTriggerVolume = true;
			if (_overriddenByQuantumMoon && Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsProbeInside())
			{
				MonoBehaviour.print("Prevent probe from entering " + base.gameObject.name + " while inside Quantum Moon");
				return;
			}
		}
		if (_name != Name.QuantumMoon || component.GetOccupantType() != DynamicOccupant.Probe)
		{
			if (_excludedOccupants.Contains(component) && !_dynamicOccupants.Contains(component))
			{
				Debug.LogError("ERROR: " + component.gameObject.name + " was excluded despite not being a dynamic occupant.");
			}
			if (_occupantsTracked.Contains(component))
			{
				_excludedOccupants.Add(component);
			}
			else
			{
				AddOccupant(component);
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		SectorDetector component = hitObj.GetComponent<SectorDetector>();
		if (component != null)
		{
			if (component.GetOccupantType() == DynamicOccupant.Player)
			{
				_playerInTriggerVolume = false;
			}
			else if (component.GetOccupantType() == DynamicOccupant.Probe)
			{
				_probeInTriggerVolume = false;
			}
			RemoveOccupant(component);
			if (_excludedOccupants.Contains(component))
			{
				_excludedOccupants.Remove(component);
			}
		}
	}

	private void OnEntryExcluder(GameObject hitObj)
	{
		SectorDetector component = hitObj.GetComponent<SectorDetector>();
		if (component != null)
		{
			if (_dynamicOccupants.Contains(component) && !_excludedOccupants.Contains(component))
			{
				RemoveOccupant(component);
				_excludedOccupants.Add(component);
			}
			if (!_occupantsTracked.Contains(component))
			{
				_occupantsTracked.Add(component);
			}
		}
	}

	private void OnExitExcluder(GameObject hitObj)
	{
		SectorDetector component = hitObj.GetComponent<SectorDetector>();
		if (component != null)
		{
			if (_excludedOccupants.Contains(component))
			{
				AddOccupant(component);
				_excludedOccupants.Remove(component);
			}
			if (_occupantsTracked.Contains(component))
			{
				_occupantsTracked.Remove(component);
			}
		}
	}

	private void OnPlayerEnterQuantumMoon()
	{
		if (_playerInTriggerVolume)
		{
			RemoveOccupant(Locator.GetPlayerSectorDetector());
			MonoBehaviour.print("Removing player from " + base.gameObject.name + " on enter Quantum Moon");
		}
	}

	private void OnPlayerExitQuantumMoon()
	{
		if (_playerInTriggerVolume)
		{
			AddOccupant(Locator.GetPlayerSectorDetector());
			MonoBehaviour.print("Adding player to " + base.gameObject.name + " on exit Quantum Moon");
		}
	}

	private void OnProbeEnterQuantumMoon()
	{
		if (_probeInTriggerVolume)
		{
			RemoveOccupant(Locator.GetProbe().GetSectorDetector());
			MonoBehaviour.print("Removing probe from " + base.gameObject.name + " on enter Quantum Moon");
		}
	}

	private void OnProbeExitQuantumMoon()
	{
		if (_probeInTriggerVolume)
		{
			AddOccupant(Locator.GetProbe().GetSectorDetector());
			MonoBehaviour.print("Adding probe to " + base.gameObject.name + " on exit Quantum Moon");
		}
	}
}
