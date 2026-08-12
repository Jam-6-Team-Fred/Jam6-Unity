using System.Collections.Generic;
using UnityEngine;

public class SectorDetector : MonoBehaviour
{
	public delegate void EnterSectorEvent(Sector sector);

	public delegate void ExitSectorEvent(Sector sector);

	[SerializeField]
	private DynamicOccupant _occupantType;

	protected List<Sector> _sectorList;

	private OWCollider _attachedCollider;

	private OWRigidbody _attachedRigidbody;

	public event EnterSectorEvent OnEnterSector;

	public event ExitSectorEvent OnExitSector;

	private void Awake()
	{
		_sectorList = new List<Sector>(32);
		if (GetComponent<Collider>() != null)
		{
			if (GetComponent<Collider>().isTrigger)
			{
				Debug.LogError("Colliders attached to detectors should not be triggers!", base.gameObject);
				Debug.Break();
			}
			_attachedCollider = base.gameObject.GetAddComponent<OWCollider>();
			_attachedRigidbody = ((_attachedCollider != null) ? _attachedCollider.GetAttachedOWRigidbody() : null);
			if (_attachedRigidbody != null)
			{
				_attachedRigidbody.OnSuspendOWRigidbody += OnAttachedRigidbodySuspended;
				_attachedRigidbody.OnPreUnsuspendOWRigidbody += OnAttachedRigidbodyResumed;
			}
		}
		if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.advancedDetectorMask))
		{
			Debug.Log("This SectorDetector is not on the AdvancedDetector layer!", base.gameObject);
			Debug.Break();
		}
	}

	private void Start()
	{
		SectorManager.RegisterSectorDetector(this);
	}

	private void OnDestroy()
	{
		if (_attachedRigidbody != null)
		{
			_attachedRigidbody.OnSuspendOWRigidbody -= OnAttachedRigidbodySuspended;
			_attachedRigidbody.OnPreUnsuspendOWRigidbody -= OnAttachedRigidbodyResumed;
		}
		SectorManager.UnregisterSectorDetector(this);
	}

	public OWCollider GetOWCollider()
	{
		return _attachedCollider;
	}

	public ReferenceFrame GetPassiveReferenceFrame()
	{
		for (int num = _sectorList.Count - 1; num >= 0; num--)
		{
			if (_sectorList[num].GetName() != 0 && _sectorList[num].GetName() != Sector.Name.Ship && _sectorList[num].GetName() != Sector.Name.Sun && _sectorList[num].GetName() != Sector.Name.HourglassTwins)
			{
				return _sectorList[num].GetOWRigidbody().GetReferenceFrame();
			}
		}
		return null;
	}

	public void AddDetectorToAllOccupiedSectors(SectorDetector detector)
	{
		for (int i = 0; i < _sectorList.Count; i++)
		{
			_sectorList[i].AddOccupant(detector);
		}
	}

	public DynamicOccupant GetOccupantType()
	{
		return _occupantType;
	}

	public void SetOccupantType(DynamicOccupant occupantType)
	{
		_occupantType = occupantType;
	}

	public bool IsWithinASector()
	{
		return _sectorList.Count > 0;
	}

	public bool IsWithinSector(Sector.Name sectorName)
	{
		for (int i = 0; i < _sectorList.Count; i++)
		{
			if (_sectorList[i].GetName() == sectorName)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumberOccupiedSectors()
	{
		return _sectorList.Count;
	}

	public bool IsWithinSector(string sectorIdString)
	{
		for (int i = 0; i < _sectorList.Count; i++)
		{
			if (_sectorList[i].GetIDString() == sectorIdString)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void AddSector(Sector sector)
	{
		if (_sectorList.SafeAdd(sector))
		{
			OnAddSector(sector);
			if (this.OnEnterSector != null)
			{
				this.OnEnterSector(sector);
			}
		}
	}

	public void RemoveSector(Sector sector)
	{
		if (_sectorList.Remove(sector))
		{
			OnRemoveSector(sector);
			if (this.OnExitSector != null)
			{
				this.OnExitSector(sector);
			}
		}
	}

	protected virtual void OnAddSector(Sector sector)
	{
	}

	protected virtual void OnRemoveSector(Sector sector)
	{
	}

	public bool DetectorOccupiesSameSectors(SectorDetector detector)
	{
		if (_sectorList.Count == detector._sectorList.Count)
		{
			for (int i = 0; i < _sectorList.Count; i++)
			{
				if (!detector._sectorList.Contains(_sectorList[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public Sector GetLastEnteredSector()
	{
		if (_sectorList.Count > 0)
		{
			return _sectorList[_sectorList.Count - 1];
		}
		return null;
	}

	public Sector GetLastEnteredSector(Sector.Name[] ignoreList)
	{
		for (int num = _sectorList.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			for (int i = 0; i < ignoreList.Length; i++)
			{
				if (_sectorList[num].GetName() == ignoreList[i])
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return _sectorList[num];
			}
		}
		return null;
	}

	private void OnAttachedRigidbodySuspended(OWRigidbody suspendedBody)
	{
		SectorManager.UnregisterSectorDetector(this);
	}

	private void OnAttachedRigidbodyResumed(OWRigidbody resumedBody)
	{
		SectorManager.RegisterSectorDetector(this);
	}
}
