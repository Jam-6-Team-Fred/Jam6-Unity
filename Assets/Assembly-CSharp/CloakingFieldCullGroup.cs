using UnityEngine;

public class CloakingFieldCullGroup : CullGroup
{
	[SerializeField]
	protected CloakFieldController _cloakingField;

	[SerializeField]
	protected Sector _exclusiveSector;

	protected bool _inCloakingField;

	protected bool _inExclusiveSector;

	protected bool _inMapView;

	protected bool _inDreamWorld;

	protected bool _isFastForwarding;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_cloakingField)
		{
			_cloakingField.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterCloakingField);
			_cloakingField.OnPlayerExit += new OWEvent.OWCallback(OnPlayerExitCloakingField);
		}
		else
		{
			Debug.LogWarning("CloakingFieldCullGroup has no specified CloakFieldController!", this);
		}
		if ((bool)_exclusiveSector)
		{
			_exclusiveSector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnEnterExclusiveSector);
			_exclusiveSector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnExitExclusiveSector);
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		SetVisible(visible: false, instant: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)_cloakingField)
		{
			_cloakingField.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterCloakingField);
			_cloakingField.OnPlayerExit -= new OWEvent.OWCallback(OnPlayerExitCloakingField);
		}
		if ((bool)_exclusiveSector)
		{
			_exclusiveSector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnEnterExclusiveSector);
			_exclusiveSector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnExitExclusiveSector);
		}
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	protected virtual void OnPlayerEnterCloakingField()
	{
		_inCloakingField = true;
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag)
		{
			SetVisible(flag);
		}
	}

	protected virtual void OnPlayerExitCloakingField()
	{
		_inCloakingField = false;
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag)
		{
			SetVisible(flag);
		}
	}

	protected virtual void OnEnterExclusiveSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_inExclusiveSector = true;
			bool flag = ShouldBeVisible();
			if (IsVisible() != flag)
			{
				SetVisible(flag);
			}
		}
	}

	protected virtual void OnExitExclusiveSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_inExclusiveSector = false;
			bool flag = ShouldBeVisible();
			if (IsVisible() != flag)
			{
				SetVisible(flag);
			}
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

	private void OnEnterDreamWorld()
	{
		_inDreamWorld = true;
		if (IsVisible() || IsCrossfading())
		{
			SetVisible(visible: false, instant: true);
		}
	}

	private void OnExitDreamWorld()
	{
		_inDreamWorld = false;
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag)
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
		if (_inExclusiveSector || _inMapView || _inDreamWorld || _isFastForwarding)
		{
			return false;
		}
		return _inCloakingField;
	}
}
