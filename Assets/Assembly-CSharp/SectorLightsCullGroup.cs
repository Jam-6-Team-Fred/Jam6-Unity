using UnityEngine;

[AddComponentMenu("Sectors/Sector Lights Cull Group", 200)]
public class SectorLightsCullGroup : LightsCullGroup, ISectorGroup
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private float _fadeLength = 1f;

	private bool _inMapView;

	protected bool _isFastForwarding;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("SectorLightsCullGroup has no specified Sector!", this);
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
	}

	private void OnDestroy()
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	private void OnSectorOccupantsUpdated()
	{
		if (!_inMapView && !_isFastForwarding)
		{
			bool flag = ShouldBeVisible();
			if (IsShining() != flag)
			{
				SetShining(flag, _fadeLength);
			}
		}
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		if (IsShining() || IsCrossfading())
		{
			SetShining(shining: false);
		}
	}

	private void OnExitMapView()
	{
		_inMapView = false;
		bool flag = ShouldBeVisible();
		if (IsShining() != flag)
		{
			SetShining(flag);
		}
	}

	private void OnStartFastForward()
	{
		_isFastForwarding = true;
		if (IsShining() || IsCrossfading())
		{
			SetShining(shining: false);
		}
	}

	private void OnEndFastForward()
	{
		_isFastForwarding = false;
		bool flag = ShouldBeVisible();
		if (IsShining() != flag)
		{
			SetShining(flag);
		}
	}

	private bool ShouldBeVisible()
	{
		if (_inMapView || _isFastForwarding)
		{
			return false;
		}
		if ((bool)_sector)
		{
			return _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		}
		return true;
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public void SetSector(Sector sector)
	{
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_sector = sector;
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		OnSectorOccupantsUpdated();
	}
}
