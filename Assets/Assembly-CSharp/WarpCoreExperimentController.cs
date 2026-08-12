using System;
using UnityEngine;

public class WarpCoreExperimentController : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _powerSlot;

	[SerializeField]
	private WarpCoreSocket _socketOne;

	[SerializeField]
	private WarpCoreSocket _socketTwo;

	[SerializeField]
	private NomaiExperimentBlackHole _blackHole;

	[SerializeField]
	private NomaiExperimentWhiteHole _whiteHole;

	private bool _singularitiesOpen;

	private void Awake()
	{
		_powerSlot.OnSlotActivated += OnPowerSlotActivated;
		_powerSlot.OnSlotDeactivated += OnPowerSlotDeactivated;
		WarpCoreSocket socketOne = _socketOne;
		socketOne.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(socketOne.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnWarpCorePlaced));
		WarpCoreSocket socketOne2 = _socketOne;
		socketOne2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(socketOne2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnWarpCoreRemoved));
		WarpCoreSocket socketTwo = _socketTwo;
		socketTwo.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Combine(socketTwo.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnWarpCorePlaced));
		WarpCoreSocket socketTwo2 = _socketTwo;
		socketTwo2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Combine(socketTwo2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnWarpCoreRemoved));
	}

	private void Start()
	{
		CheckActivation();
	}

	private void OnDestroy()
	{
		_powerSlot.OnSlotActivated -= OnPowerSlotActivated;
		_powerSlot.OnSlotDeactivated -= OnPowerSlotDeactivated;
		WarpCoreSocket socketOne = _socketOne;
		socketOne.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(socketOne.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnWarpCorePlaced));
		WarpCoreSocket socketOne2 = _socketOne;
		socketOne2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(socketOne2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnWarpCoreRemoved));
		WarpCoreSocket socketTwo = _socketTwo;
		socketTwo.OnSocketablePlaced = (OWItemSocket.SocketEvent)Delegate.Remove(socketTwo.OnSocketablePlaced, new OWItemSocket.SocketEvent(OnWarpCorePlaced));
		WarpCoreSocket socketTwo2 = _socketTwo;
		socketTwo2.OnSocketableRemoved = (OWItemSocket.SocketEvent)Delegate.Remove(socketTwo2.OnSocketableRemoved, new OWItemSocket.SocketEvent(OnWarpCoreRemoved));
	}

	private void OnPowerSlotActivated(NomaiInterfaceSlot slot)
	{
		_blackHole.SetTimeTravel(timeTravel: true);
	}

	private void OnPowerSlotDeactivated(NomaiInterfaceSlot slot)
	{
		_blackHole.SetTimeTravel(timeTravel: false);
	}

	private void CheckActivation()
	{
		int num = 0;
		if (_socketOne.GetWarpCoreType() == WarpCoreType.Black)
		{
			num++;
		}
		if (_socketTwo.GetWarpCoreType() == WarpCoreType.Black)
		{
			num++;
		}
		int num2 = 0;
		if (_socketOne.GetWarpCoreType() == WarpCoreType.White)
		{
			num2++;
		}
		if (_socketTwo.GetWarpCoreType() == WarpCoreType.White)
		{
			num2++;
		}
		if (!_singularitiesOpen && num == 1 && num2 == 1)
		{
			_singularitiesOpen = true;
			OpenSingularity(_socketOne);
			OpenSingularity(_socketTwo);
		}
		else if (_singularitiesOpen && (num != 1 || num2 != 1))
		{
			_singularitiesOpen = false;
			_blackHole.CloseSingularity();
			_whiteHole.CloseSingularity();
		}
	}

	private void OpenSingularity(WarpCoreSocket socket)
	{
		if (socket.GetWarpCoreType() == WarpCoreType.Black)
		{
			_blackHole.OpenSingularity();
			_blackHole.transform.position = socket.GetSingularitySocket().position;
			_blackHole.transform.rotation = socket.GetSingularitySocket().rotation;
		}
		if (socket.GetWarpCoreType() == WarpCoreType.White)
		{
			_whiteHole.OpenSingularity();
			_whiteHole.transform.position = socket.GetSingularitySocket().position;
			_whiteHole.transform.rotation = socket.GetSingularitySocket().rotation;
		}
	}

	private void OnWarpCorePlaced(OWItem warpCore)
	{
		CheckActivation();
	}

	private void OnWarpCoreRemoved(OWItem warpCore)
	{
		CheckActivation();
	}
}
