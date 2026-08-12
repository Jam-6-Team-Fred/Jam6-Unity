public class DreamLanternSocket : OWItemSocket
{
	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.DreamLantern;
	}

	public override bool PlaceIntoSocket(OWItem item)
	{
		if (base.PlaceIntoSocket(item))
		{
			Locator.GetDreamWorldController().SetPlayerLanternSocket(this);
			return true;
		}
		return false;
	}

	public override OWItem RemoveFromSocket()
	{
		OWItem oWItem = base.RemoveFromSocket();
		if (oWItem != null)
		{
			Locator.GetDreamWorldController().SetPlayerLanternSocket(null);
		}
		return oWItem;
	}
}
