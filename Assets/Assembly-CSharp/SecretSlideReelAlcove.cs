using System;
using UnityEngine;

public class SecretSlideReelAlcove : MonoBehaviour
{
	[SerializeField]
	private bool _closeDoorImmediately;

	[SerializeField]
	private AbstractDoor _door;

	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	private OWItemSocket _itemSocket;

	[SerializeField]
	private Transform _rotationPivot;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private OWAudioSource _oneShotAudio;

	private float _startRotationTime;

	private void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponentInParent<Sector>();
		}
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		if (_closeDoorImmediately)
		{
			OWItemSocket itemSocket = _itemSocket;
			itemSocket.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(itemSocket.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSlideReelRemoved));
		}
		else
		{
			OWItemSocket itemSocket2 = _itemSocket;
			itemSocket2.OnSocketableDoneRemoving = (OWItemSocket.SocketEvent)Delegate.Combine(itemSocket2.OnSocketableDoneRemoving, new OWItemSocket.SocketEvent(OnSlideReelRemoved));
		}
	}

	private void Start()
	{
		_itemSocket.EnableInteraction(value: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		OWItemSocket itemSocket = _itemSocket;
		itemSocket.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(itemSocket.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSlideReelRemoved));
		OWItemSocket itemSocket2 = _itemSocket;
		itemSocket2.OnSocketableDoneRemoving = (OWItemSocket.SocketEvent)Delegate.Remove(itemSocket2.OnSocketableDoneRemoving, new OWItemSocket.SocketEvent(OnSlideReelRemoved));
	}

	private void OnDetectDarkness()
	{
		if (_sector != null && _sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_oneShotAudio.PlayOneShot(AudioType.SecretPassage_Stop);
			_startRotationTime = Time.time;
			base.enabled = true;
		}
		else
		{
			_rotationPivot.localEulerAngles = Vector3.up * 180f;
		}
		_itemSocket.EnableInteraction(value: true);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void OnSlideReelRemoved(OWItem item)
	{
		OWItemSocket itemSocket = _itemSocket;
		itemSocket.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(itemSocket.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnSlideReelRemoved));
		OWItemSocket itemSocket2 = _itemSocket;
		itemSocket2.OnSocketableDoneRemoving = (OWItemSocket.SocketEvent)Delegate.Remove(itemSocket2.OnSocketableDoneRemoving, new OWItemSocket.SocketEvent(OnSlideReelRemoved));
		if (_door != null)
		{
			_door.Close();
		}
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_startRotationTime, _startRotationTime + 0.5f, Time.time);
		_rotationPivot.localEulerAngles = Vector3.Lerp(Vector3.zero, Vector3.up * 180f, num);
		if (num >= 1f)
		{
			base.enabled = false;
		}
	}
}
