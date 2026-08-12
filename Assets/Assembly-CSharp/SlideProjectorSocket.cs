using UnityEngine;

public class SlideProjectorSocket : OWItemDoubleSocket
{
	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.SlideReel;
		_secondAcceptableType = ItemType.Lantern;
	}

	public override bool PlaceIntoSocket(OWItem item)
	{
		if (item is SlideReelItem)
		{
			(item as SlideReelItem).SetSocketLocalDir(Vector3.up);
		}
		return base.PlaceIntoSocket(item);
	}

	public override OWItem RemoveFromSocket()
	{
		if (_secondSocketedItem != null && _secondSocketedItem is SimpleLanternItem && !(_secondSocketedItem as SimpleLanternItem).IsLit())
		{
			_removedItem = _secondSocketedItem;
			_secondSocketedItem = null;
			if (OnSocketableRemoved != null && _removedItem != null)
			{
				OnSocketableRemoved(_removedItem);
			}
			_removedItem.PlayUnsocketAnimation();
			_removedItem.SetColliderActivation(active: true);
			base.enabled = true;
			return _removedItem;
		}
		if (_socketedItem != null && _socketedItem is SlideReelItem)
		{
			(_socketedItem as SlideReelItem).SetSocketLocalDir(Vector3.up);
		}
		return base.RemoveFromSocket();
	}

	public override OWItem GetSocketedItem()
	{
		if (_secondSocketedItem != null && _secondSocketedItem is SimpleLanternItem && !(_secondSocketedItem as SimpleLanternItem).IsLit())
		{
			return _secondSocketedItem;
		}
		return base.GetSocketedItem();
	}

	public SlideReelItem GetSocketedSlideReel()
	{
		if (GetFirstSocketedItem() != null && GetFirstSocketedItem().GetItemType() == ItemType.SlideReel)
		{
			return (SlideReelItem)GetFirstSocketedItem();
		}
		if (GetSecondSocketedItem() != null && GetSecondSocketedItem().GetItemType() == ItemType.SlideReel)
		{
			return (SlideReelItem)GetSecondSocketedItem();
		}
		return null;
	}
}
