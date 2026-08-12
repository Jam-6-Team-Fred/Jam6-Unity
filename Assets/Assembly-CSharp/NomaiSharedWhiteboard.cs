using System;
using UnityEngine;

public class NomaiSharedWhiteboard : NomaiShared
{
	[SerializeField]
	private NomaiWallText[] _nomaiTexts;

	[SerializeField]
	private NomaiRemoteCameraPlatform.ID[] _remoteIDs;

	private SharedStoneSocket _socket;

	private PedestalAnimator _pedestalAnimator;

	private void Awake()
	{
		for (int i = 0; i < _nomaiTexts.Length; i++)
		{
			_nomaiTexts[i].InitializeAsWhiteboardText();
		}
	}

	private void Start()
	{
		if (_nomaiTexts.Length != _remoteIDs.Length)
		{
			Debug.LogError("ERROR: Shared Whiteboards with more or less Nomai Arc Texts than remote IDs", this);
		}
		for (int i = 0; i < _nomaiTexts.Length; i++)
		{
			if (_nomaiTexts[i] == null)
			{
				Debug.LogWarning("Null Nomai Text in list. Check Nomai Texts list size to fix. Breaking out of Start.", this);
				return;
			}
			_nomaiTexts[i].HideImmediate();
		}
		_socket = GetComponentInChildren<SharedStoneSocket>();
		if (_socket == null)
		{
			Debug.LogWarning("SharedStoneSocket not found!", this);
			return;
		}
		SharedStoneSocket socket = _socket;
		socket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(socket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnSocketablePlaced));
		SharedStoneSocket socket2 = _socket;
		socket2.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Combine(socket2.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(OnSocketableDonePlacing));
		SharedStoneSocket socket3 = _socket;
		socket3.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(socket3.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
		_pedestalAnimator = _socket.GetComponent<PedestalAnimator>();
		PedestalAnimator pedestalAnimator = _pedestalAnimator;
		pedestalAnimator.OnPedestalContact = (PedestalAnimator.PedestalEvent)Delegate.Combine(pedestalAnimator.OnPedestalContact, new PedestalAnimator.PedestalEvent(OnPedestalContact));
		OWItem socketedItem = _socket.GetSocketedItem();
		if (!(socketedItem != null))
		{
			return;
		}
		SharedStone sharedStone = socketedItem as SharedStone;
		if (!(sharedStone != null))
		{
			return;
		}
		for (int j = 0; j < _remoteIDs.Length; j++)
		{
			if (sharedStone.GetRemoteCameraID() == _remoteIDs[j])
			{
				_nomaiTexts[j].ShowImmediate();
			}
		}
	}

	private void OnDestroy()
	{
		SharedStoneSocket socket = _socket;
		socket.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(socket.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnSocketablePlaced));
		SharedStoneSocket socket2 = _socket;
		socket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(socket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSocketableRemoved));
		PedestalAnimator pedestalAnimator = _pedestalAnimator;
		pedestalAnimator.OnPedestalContact = (PedestalAnimator.PedestalEvent)Delegate.Remove(pedestalAnimator.OnPedestalContact, new PedestalAnimator.PedestalEvent(OnPedestalContact));
	}

	private void OnPedestalContact()
	{
		if (_socket.GetSocketedItem() != null)
		{
			SetNomaiTextVisible(_socket.GetSocketedItem(), visible: true);
		}
	}

	private void OnSocketablePlaced(OWItem item)
	{
	}

	private void OnSocketableRemoved(OWItem item)
	{
		if (!(_sharedStone == null))
		{
			_socket.GetPedestalAnimator().PlayOpen();
			SetNomaiTextVisible(item, visible: false);
		}
	}

	private void OnSocketableDonePlacing(OWItem item)
	{
		_sharedStone = item as SharedStone;
		if (_sharedStone == null)
		{
			Debug.LogError("Placed an empty item or a non SharedStone in a NomaiRemoteCameraPlatform");
		}
		if (_sharedStone.GetRemoteCameraID() == _id)
		{
			_sharedStone = null;
			return;
		}
		_socket.GetPedestalAnimator().PlayClose();
		if (_pedestalAnimator.HasMadeContact())
		{
			SetNomaiTextVisible(item, visible: true);
		}
	}

	private void SetNomaiTextVisible(OWItem item, bool visible)
	{
		if (item == null)
		{
			return;
		}
		SharedStone sharedStone = item as SharedStone;
		for (int i = 0; i < _remoteIDs.Length; i++)
		{
			if (sharedStone.GetRemoteCameraID() == _remoteIDs[i])
			{
				if (visible)
				{
					_nomaiTexts[i].Show();
				}
				else
				{
					_nomaiTexts[i].Hide();
				}
			}
		}
	}
}
