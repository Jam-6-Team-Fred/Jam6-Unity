using System;
using UnityEngine;

public class SecretSlideSocketDoor : MonoBehaviour
{
	[SerializeField]
	private SlideReelSocket _slideReelSocket;

	[SerializeField]
	private AbstractDoor _secretDoor;

	[SerializeField]
	private AbstractDoor _mainDoor;

	[SerializeField]
	private OWLightController _lightController;

	[Space]
	[SerializeField]
	private float _lightFadeLength = 1f;

	[SerializeField]
	private float _doorOpenDelay = 1f;

	private bool _socketed;

	private bool _doorOpened;

	private float _doorOpenTime;

	private void Awake()
	{
		SlideReelSocket slideReelSocket = _slideReelSocket;
		slideReelSocket.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Combine(slideReelSocket.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(RevealSecretDoor));
		SlideReelSocket slideReelSocket2 = _slideReelSocket;
		slideReelSocket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(slideReelSocket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(ConcealSecretDoor));
		SlideReelSocket slideReelSocket3 = _slideReelSocket;
		slideReelSocket3.OnSocketableDoneRemoving = (OWItemSocket.SocketEvent)Delegate.Combine(slideReelSocket3.OnSocketableDoneRemoving, new OWItemSocket.SocketEvent(ReEnableLights));
		base.enabled = false;
	}

	private void OnDestroy()
	{
		SlideReelSocket slideReelSocket = _slideReelSocket;
		slideReelSocket.OnSocketableDonePlacing = (OWItemSocket.SocketEvent)Delegate.Remove(slideReelSocket.OnSocketableDonePlacing, new OWItemSocket.SocketEvent(RevealSecretDoor));
		SlideReelSocket slideReelSocket2 = _slideReelSocket;
		slideReelSocket2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(slideReelSocket2.OnSocketableRemoved, new OWItemSocket.SocketEvent(ConcealSecretDoor));
		SlideReelSocket slideReelSocket3 = _slideReelSocket;
		slideReelSocket3.OnSocketableDoneRemoving = (OWItemSocket.SocketEvent)Delegate.Remove(slideReelSocket3.OnSocketableDoneRemoving, new OWItemSocket.SocketEvent(ReEnableLights));
	}

	private void Update()
	{
		if (_socketed && !_doorOpened && Time.time >= _doorOpenTime)
		{
			_secretDoor.Open();
			_mainDoor.Close();
			_doorOpened = true;
			base.enabled = false;
		}
	}

	private void RevealSecretDoor(OWItem item)
	{
		_lightController.FadeTo(0f, _lightFadeLength);
		_doorOpenTime = Time.time + _doorOpenDelay;
		_socketed = true;
		base.enabled = true;
	}

	private void ConcealSecretDoor(OWItem item)
	{
		_secretDoor.Close();
		_mainDoor.Open();
		_socketed = false;
		_doorOpened = false;
		base.enabled = false;
	}

	private void ReEnableLights(OWItem item)
	{
		_lightController.FadeTo(1f, _lightFadeLength);
	}
}
