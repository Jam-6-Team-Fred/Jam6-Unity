using UnityEngine;

public class WarpCoreItem : OWItem
{
	[SerializeField]
	protected WarpCoreType _warpCoreType;

	protected WarpCoreType _wcType;

	protected override void Awake()
	{
		base.Awake();
		_type = ItemType.WarpCore;
		switch (_warpCoreType)
		{
		case WarpCoreType.Invalid:
			_wcType = WarpCoreType.Invalid;
			break;
		case WarpCoreType.Vessel:
			_wcType = WarpCoreType.Vessel;
			break;
		case WarpCoreType.VesselBroken:
			_wcType = WarpCoreType.VesselBroken;
			break;
		case WarpCoreType.Black:
			_wcType = WarpCoreType.Black;
			break;
		case WarpCoreType.White:
			_wcType = WarpCoreType.White;
			break;
		case WarpCoreType.SimpleBroken:
			_wcType = WarpCoreType.SimpleBroken;
			break;
		}
	}

	public WarpCoreType GetWarpCoreType()
	{
		return _wcType;
	}

	public bool IsVesselCoreType()
	{
		if (_wcType != WarpCoreType.Vessel)
		{
			return _wcType == WarpCoreType.VesselBroken;
		}
		return true;
	}

	public override string GetDisplayName()
	{
		return UITextLibrary.GetString(UITextType.ItemWarpCorePrompt);
	}
}
