using UnityEngine;

public class OWItemSocket : MonoBehaviour
{
	public delegate void SocketEvent(OWItem item);

	[SerializeField]
	protected Transform _socketTransform;

	[SerializeField]
	protected float _interactRange = 2f;

	[SerializeField]
	protected Sector _sector;

	protected OWItem _socketedItem;

	protected OWItem _removedItem;

	protected ItemType _acceptableType;

	protected bool _interactable = true;

	public SocketEvent OnSocketablePlaced;

	public SocketEvent OnSocketableRemoved;

	public SocketEvent OnSocketableDonePlacing;

	public SocketEvent OnSocketableDoneRemoving;

	private void Reset()
	{
		if (_socketTransform == null)
		{
			_socketTransform = base.transform;
		}
	}

	protected virtual void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponentInParent<Sector>();
		}
		if (_sector == null)
		{
			Debug.LogError("Could not find Sector in OWItemSocket parents", this);
			Debug.Break();
		}
		if (_socketTransform.childCount > 0)
		{
			_socketedItem = _socketTransform.GetComponentInChildren<OWItem>();
		}
	}

	protected virtual void Start()
	{
		if (_socketedItem != null)
		{
			_socketedItem.MoveAndChildToTransform(_socketTransform);
		}
		base.enabled = false;
	}

	public virtual bool UsesGiveTakePrompts()
	{
		return false;
	}

	public virtual bool PlaceIntoSocket(OWItem item)
	{
		if (!AcceptsItem(item) || _socketedItem != null)
		{
			return false;
		}
		_socketedItem = item;
		_socketedItem.SocketItem(_socketTransform, _sector);
		_socketedItem.PlaySocketAnimation();
		if (OnSocketablePlaced != null)
		{
			OnSocketablePlaced(_socketedItem);
		}
		base.enabled = true;
		return true;
	}

	public virtual OWItem RemoveFromSocket()
	{
		_removedItem = _socketedItem;
		_socketedItem = null;
		if (OnSocketableRemoved != null && _removedItem != null)
		{
			OnSocketableRemoved(_removedItem);
		}
		_removedItem.PlayUnsocketAnimation();
		_removedItem.SetColliderActivation(active: true);
		base.enabled = true;
		return _removedItem;
	}

	public virtual bool AcceptsItem(OWItem item)
	{
		ItemType itemType = item.GetItemType();
		if (_acceptableType == ItemType.Invalid)
		{
			Debug.LogWarning("Socket with no acceptable types set.");
		}
		return (itemType & _acceptableType) == itemType;
	}

	public virtual bool IsSocketOccupied()
	{
		return _socketedItem != null;
	}

	public virtual OWItem GetSocketedItem()
	{
		return _socketedItem;
	}

	public virtual float GetInteractRange()
	{
		return _interactRange;
	}

	public virtual bool IsInteractable()
	{
		return _interactable;
	}

	public virtual void EnableInteraction(bool value)
	{
		_interactable = value;
	}

	protected virtual void Update()
	{
		if (_removedItem != null && !_removedItem.IsAnimationPlaying())
		{
			if (OnSocketableDoneRemoving != null)
			{
				OnSocketableDoneRemoving(_removedItem);
			}
			_removedItem = null;
			base.enabled = false;
		}
		else if (_socketedItem != null && !_socketedItem.IsAnimationPlaying())
		{
			if (OnSocketableDonePlacing != null)
			{
				OnSocketableDonePlacing(_socketedItem);
			}
			base.enabled = false;
		}
	}
}
