using UnityEngine;

public class WarpCoreSocket : OWItemSocket
{
	[SerializeField]
	protected Transform _singularitySocket;

	[SerializeField]
	protected bool _isVesselClassSlot;

	protected WarpCoreType _acceptableWarpCoreTypes;

	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.WarpCore;
		if (_isVesselClassSlot)
		{
			_acceptableWarpCoreTypes = WarpCoreType.Vessel | WarpCoreType.VesselBroken;
		}
		else
		{
			_acceptableWarpCoreTypes = WarpCoreType.Black | WarpCoreType.White | WarpCoreType.SimpleBroken;
		}
	}

	public WarpCoreType GetWarpCoreType()
	{
		if (!(_socketedItem == null))
		{
			return ((WarpCoreItem)_socketedItem).GetWarpCoreType();
		}
		return WarpCoreType.Invalid;
	}

	public override bool AcceptsItem(OWItem item)
	{
		bool result = false;
		if (base.AcceptsItem(item))
		{
			WarpCoreItem warpCoreItem = item as WarpCoreItem;
			result = (warpCoreItem.GetWarpCoreType() & _acceptableWarpCoreTypes) == warpCoreItem.GetWarpCoreType();
		}
		return result;
	}

	public Transform GetSingularitySocket()
	{
		return _singularitySocket;
	}
}
