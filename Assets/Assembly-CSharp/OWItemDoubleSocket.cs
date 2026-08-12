using UnityEngine;

public class OWItemDoubleSocket : OWItemSocket
{
	[Space]
	[SerializeField]
	private Transform _secondSocketTransform;

	protected OWItem _secondSocketedItem;

	protected OWItem _secondRemovedItem;

	protected ItemType _secondAcceptableType;

	protected override void Awake()
	{
		base.Awake();
		if (_secondSocketTransform.childCount > 0)
		{
			_secondSocketedItem = _secondSocketTransform.GetComponentInChildren<OWItem>();
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_secondSocketedItem != null)
		{
			_secondSocketedItem.MoveAndChildToTransform(_secondSocketTransform);
		}
	}

	public override bool PlaceIntoSocket(OWItem item)
	{
		if (!AcceptsItem(item))
		{
			return false;
		}
		ItemType itemType = item.GetItemType();
		if ((itemType == _acceptableType && _socketedItem != null) || (itemType == _secondAcceptableType && _secondSocketedItem != null))
		{
			return false;
		}
		if (itemType == _acceptableType)
		{
			_socketedItem = item;
			_socketedItem.SocketItem(_socketTransform, _sector);
			_socketedItem.PlaySocketAnimation();
		}
		else
		{
			_secondSocketedItem = item;
			_secondSocketedItem.SocketItem(_secondSocketTransform, _sector);
			_secondSocketedItem.PlaySocketAnimation();
		}
		if (OnSocketablePlaced != null)
		{
			OnSocketablePlaced(item);
		}
		base.enabled = true;
		return true;
	}

	public override OWItem RemoveFromSocket()
	{
		OWItem oWItem = null;
		if (_socketedItem != null)
		{
			_removedItem = _socketedItem;
			_socketedItem = null;
			oWItem = _removedItem;
		}
		else if (_secondSocketedItem != null)
		{
			_secondRemovedItem = _secondSocketedItem;
			_secondSocketedItem = null;
			oWItem = _secondRemovedItem;
		}
		if (OnSocketableRemoved != null && oWItem != null)
		{
			OnSocketableRemoved(oWItem);
		}
		oWItem.PlayUnsocketAnimation();
		oWItem.SetColliderActivation(active: true);
		base.enabled = true;
		return oWItem;
	}

	public override bool AcceptsItem(OWItem item)
	{
		return (item.GetItemType() & (_acceptableType | _secondAcceptableType)) > ItemType.Invalid;
	}

	public virtual bool AcceptsItemInFirstSocket(OWItem item)
	{
		return (item.GetItemType() & _acceptableType) > ItemType.Invalid;
	}

	public virtual bool AcceptsItemInSecondSocket(OWItem item)
	{
		return (item.GetItemType() & _secondAcceptableType) > ItemType.Invalid;
	}

	public override bool IsSocketOccupied()
	{
		if (!(_socketedItem != null))
		{
			return _secondSocketedItem != null;
		}
		return true;
	}

	public virtual bool IsFirstSocketOccupied()
	{
		return _socketedItem != null;
	}

	public virtual bool IsSecondSocketOccupied()
	{
		return _secondSocketedItem != null;
	}

	public override OWItem GetSocketedItem()
	{
		if (!(_socketedItem != null))
		{
			return _secondSocketedItem;
		}
		return _socketedItem;
	}

	public virtual OWItem GetFirstSocketedItem()
	{
		return _socketedItem;
	}

	public virtual OWItem GetSecondSocketedItem()
	{
		return _secondSocketedItem;
	}

	protected override void Update()
	{
		bool num = (_removedItem != null && _removedItem.IsAnimationPlaying()) || (_socketedItem != null && _socketedItem.IsAnimationPlaying()) || (_secondRemovedItem != null && _secondRemovedItem.IsAnimationPlaying()) || (_secondSocketedItem != null && _secondSocketedItem.IsAnimationPlaying());
		if (_removedItem != null && !_removedItem.IsAnimationPlaying())
		{
			if (OnSocketableDoneRemoving != null)
			{
				OnSocketableDoneRemoving(_removedItem);
			}
			_removedItem = null;
		}
		else if (_socketedItem != null && !_socketedItem.IsAnimationPlaying() && OnSocketableDonePlacing != null)
		{
			OnSocketableDonePlacing(_socketedItem);
		}
		if (_secondRemovedItem != null && !_secondRemovedItem.IsAnimationPlaying())
		{
			if (OnSocketableDoneRemoving != null)
			{
				OnSocketableDoneRemoving(_secondRemovedItem);
			}
			_secondRemovedItem = null;
		}
		else if (_secondSocketedItem != null && !_secondSocketedItem.IsAnimationPlaying() && OnSocketableDonePlacing != null)
		{
			OnSocketableDonePlacing(_secondSocketedItem);
		}
		if (!num)
		{
			base.enabled = false;
		}
	}
}
