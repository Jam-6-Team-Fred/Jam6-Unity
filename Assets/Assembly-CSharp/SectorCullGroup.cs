using UnityEngine;

[AddComponentMenu("Sectors/Sector Cull Group", 200)]
public class SectorCullGroup : CullGroup, ISectorGroup
{
	[SerializeField]
	protected Sector _sector;

	[SerializeField]
	protected SectorProxy _controllingProxy;

	protected bool _inMapView;

	protected bool _isFastForwarding;

	protected bool _firstUpdate = true;

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_controllingProxy)
		{
			_controllingProxy.AddControlledCullGroup(this);
			if ((bool)_sector)
			{
				Debug.LogWarning("SectorCullGroup should only use either a Sector or a controlling Proxy, not both!", this);
			}
			return;
		}
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		else
		{
			Debug.LogWarning("SectorCullGroup has no specified Sector!", this);
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger.AddListener("EnterDreamWorld", OnDreamWorldTransition);
		GlobalMessenger.AddListener("ExitDreamWorld", OnDreamWorldTransition);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)_sector)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_controllingProxy == null)
		{
			GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
			GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
			GlobalMessenger.RemoveListener("EnterDreamWorld", OnDreamWorldTransition);
			GlobalMessenger.RemoveListener("ExitDreamWorld", OnDreamWorldTransition);
			GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
			GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
		}
	}

	protected virtual void OnSectorOccupantsUpdated()
	{
		if (!_inMapView && !_isFastForwarding)
		{
			bool flag = ShouldBeVisible();
			if (IsVisible() != flag)
			{
				SetVisible(flag, _firstUpdate);
			}
			_firstUpdate = false;
		}
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		if (IsVisible() || IsCrossfading())
		{
			SetVisible(visible: false, instant: true, IsCrossfading());
		}
	}

	private void OnExitMapView()
	{
		_inMapView = false;
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag)
		{
			SetVisible(flag, instant: true, updateSuspension: false);
		}
	}

	private void OnDreamWorldTransition()
	{
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag || IsCrossfading())
		{
			SetVisible(flag, instant: true);
		}
	}

	private void OnStartFastForward()
	{
		_isFastForwarding = true;
		if (IsVisible() || IsCrossfading())
		{
			SetVisible(visible: false, instant: true);
		}
	}

	private void OnEndFastForward()
	{
		_isFastForwarding = false;
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag)
		{
			SetVisible(flag, instant: true);
		}
	}

	protected virtual bool ShouldBeVisible()
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
		if (!(_controllingProxy != null))
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

	public SectorProxy GetControllingProxy()
	{
		return _controllingProxy;
	}

	public void RefreshSectorVisibilityState()
	{
		if (_sector != null)
		{
			bool flag = ShouldBeVisible();
			if (IsVisible() != flag)
			{
				SetVisible(flag, _firstUpdate);
			}
			_firstUpdate = false;
		}
	}
}
