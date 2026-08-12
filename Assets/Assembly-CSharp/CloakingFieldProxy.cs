using UnityEngine;

public class CloakingFieldProxy : SectorProxy
{
	[Space]
	[SerializeField]
	private CloakFieldController _cloakingField;

	private bool _playerInCloakingField;

	private bool _playerInDreamWorld;

	protected override void Awake()
	{
		base.Awake();
		if (_cloakingField != null)
		{
			_cloakingField.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterCloakingField);
			_cloakingField.OnPlayerExit += new OWEvent.OWCallback(OnPlayerExitCloakingField);
		}
		else
		{
			Debug.LogWarning("CloakingFieldProxy has no specified CloakFieldController!", this);
		}
		SetProxyHidden(hidden: true);
		SetProxyActive(proxyActive: true, instant: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_cloakingField != null)
		{
			_cloakingField.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterCloakingField);
			_cloakingField.OnPlayerExit -= new OWEvent.OWCallback(OnPlayerExitCloakingField);
		}
	}

	private void OnPlayerEnterCloakingField()
	{
		_playerInCloakingField = true;
		bool flag = ShouldBeHidden();
		if (_proxyHidden != flag)
		{
			SetProxyHidden(flag);
		}
		bool flag2 = ShouldBeActive();
		if (_proxyActive == flag2)
		{
			return;
		}
		if (!flag2)
		{
			if (CanSwitch())
			{
				SetProxyActive(proxyActive: false, instant: true);
				return;
			}
			_waitingForCullGroups = true;
			base.enabled = true;
		}
		else
		{
			SetProxyActive(proxyActive: true, instant: true);
			_waitingForCullGroups = false;
		}
	}

	private void OnPlayerExitCloakingField()
	{
		_playerInCloakingField = false;
		if (!_proxyHidden)
		{
			SetProxyHidden(hidden: true);
		}
		if (!_proxyActive)
		{
			SetProxyActive(proxyActive: true, instant: true);
			_waitingForCullGroups = false;
		}
	}

	protected override void OnEnterDreamWorld()
	{
		_playerInDreamWorld = true;
		if (!_proxyHidden)
		{
			SetProxyHidden(hidden: true);
		}
		if (!_proxyActive)
		{
			SetProxyActive(proxyActive: true, instant: true);
			_waitingForCullGroups = false;
		}
	}

	protected override void OnExitDreamWorld()
	{
		_playerInDreamWorld = false;
		bool flag = ShouldBeHidden();
		if (_proxyHidden != flag)
		{
			SetProxyHidden(flag);
		}
		bool flag2 = ShouldBeActive();
		if (_proxyActive == flag2)
		{
			return;
		}
		if (!flag2)
		{
			if (CanSwitch())
			{
				SetProxyActive(proxyActive: false, instant: true);
				return;
			}
			_waitingForCullGroups = true;
			base.enabled = true;
		}
		else
		{
			SetProxyActive(proxyActive: true, instant: true);
			_waitingForCullGroups = false;
		}
	}

	protected override bool ShouldBeActive()
	{
		if (!_playerInCloakingField)
		{
			return true;
		}
		if ((bool)_sector && !_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			return true;
		}
		return false;
	}

	protected override bool ShouldBeHidden()
	{
		if (!_playerInCloakingField || _playerInDreamWorld)
		{
			return true;
		}
		return base.ShouldBeHidden();
	}
}
