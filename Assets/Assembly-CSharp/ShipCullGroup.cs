using UnityEngine;

public class ShipCullGroup : CullGroup, IShipGroup
{
	[SerializeField]
	private ShipLODTrigger _lodTrigger;

	private bool _inMapView;

	protected override void Awake()
	{
		base.Awake();
		if (_lodTrigger != null)
		{
			SetVisible(visible: false, instant: true);
			_lodTrigger.OnTriggerUpdated += new OWEvent.OWCallback(OnTriggerUpdated);
		}
		else
		{
			Debug.LogWarning("ShipCullGroup has no specificed ShipLODTrigger!");
		}
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated -= new OWEvent.OWCallback(OnTriggerUpdated);
		}
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void OnTriggerUpdated()
	{
		if (!_inMapView)
		{
			bool flag = ShouldBeVisible();
			if (IsVisible() != flag)
			{
				SetVisible(flag, instant: true);
			}
		}
	}

	private void OnEnterMapView()
	{
		_inMapView = true;
		bool flag = ShouldBeVisible();
		if (IsVisible() != flag)
		{
			SetVisible(flag, instant: true, updateSuspension: false);
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

	private bool ShouldBeVisible()
	{
		if (_inMapView)
		{
			return false;
		}
		if (_lodTrigger != null)
		{
			if (!_lodTrigger.isPlayerInTrigger)
			{
				return _lodTrigger.isProbeInTrigger;
			}
			return true;
		}
		return true;
	}

	public ShipLODTrigger GetLODTrigger()
	{
		return _lodTrigger;
	}

	public void SetLODTrigger(ShipLODTrigger lodTrigger)
	{
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated -= new OWEvent.OWCallback(OnTriggerUpdated);
		}
		_lodTrigger = lodTrigger;
		if (_lodTrigger != null)
		{
			_lodTrigger.OnTriggerUpdated += new OWEvent.OWCallback(OnTriggerUpdated);
		}
		OnTriggerUpdated();
	}
}
