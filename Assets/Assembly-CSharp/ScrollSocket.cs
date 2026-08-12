using System;
using UnityEngine;

public class ScrollSocket : OWItemSocket
{
	protected override void Awake()
	{
		base.Awake();
		_acceptableType = ItemType.Scroll;
	}

	protected override void Start()
	{
		base.Start();
		if (_socketedItem != null)
		{
			ScrollItem scrollItem = _socketedItem as ScrollItem;
			if (scrollItem != null)
			{
				scrollItem.ShowNomaiTextImmediate();
			}
			else
			{
				Debug.LogError("Socketed Item is not a scroll???");
				Debug.Break();
			}
		}
		OnSocketableDonePlacing = (SocketEvent)Delegate.Combine(OnSocketableDonePlacing, new SocketEvent(OnScrollPlaced));
	}

	private void OnDestroy()
	{
		OnSocketableDonePlacing = (SocketEvent)Delegate.Remove(OnSocketableDonePlacing, new SocketEvent(OnScrollPlaced));
	}

	public override OWItem RemoveFromSocket()
	{
		((ScrollItem)_socketedItem).HideNomaiText();
		return base.RemoveFromSocket();
	}

	private void OnScrollPlaced(OWItem socketable)
	{
		((ScrollItem)_socketedItem).ShowNomaiText();
	}
}
