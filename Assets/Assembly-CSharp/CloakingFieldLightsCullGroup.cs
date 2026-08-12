using UnityEngine;

public class CloakingFieldLightsCullGroup : LightsCullGroup
{
	[SerializeField]
	protected CloakFieldController _cloakingField;

	[SerializeField]
	protected Sector _exclusiveSector;

	protected bool _inCloakingField;

	protected bool _inExclusiveSector;

	protected bool _inMapView;

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
			Debug.LogWarning("CloakingFieldLightsCullGroup has no specified CloakFieldController!", this);
		}
		if ((bool)_exclusiveSector)
		{
			_exclusiveSector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnEnterExclusiveSector);
			_exclusiveSector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnExitExclusiveSector);
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		SetShining(shining: false);
	}

	private void OnDestroy()
	{
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
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
	}

	private void OnPlayerEnterCloakingField()
	{
		_inCloakingField = true;
		bool flag = ShouldBeVisible();
		if (IsShining() != flag)
		{
			SetShining(flag);
		}
	}

	private void OnPlayerExitCloakingField()
	{
		_inCloakingField = false;
		bool flag = ShouldBeVisible();
		if (IsShining() != flag)
		{
			SetShining(flag);
		}
	}

	protected virtual void OnEnterExclusiveSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_inExclusiveSector = true;
			bool flag = ShouldBeVisible();
			if (IsShining() != flag)
			{
				SetShining(flag);
			}
		}
	}

	protected virtual void OnExitExclusiveSector(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			_inExclusiveSector = false;
			bool flag = ShouldBeVisible();
			if (IsShining() != flag)
			{
				SetShining(flag);
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
		if (_inExclusiveSector || _inMapView || _isFastForwarding)
		{
			return false;
		}
		return _inCloakingField;
	}
}
