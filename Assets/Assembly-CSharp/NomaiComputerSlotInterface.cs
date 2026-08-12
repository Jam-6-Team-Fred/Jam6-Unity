using System;
using UnityEngine;

[RequireComponent(typeof(NomaiComputer))]
public class NomaiComputerSlotInterface : MonoBehaviour
{
	[Serializable]
	private struct ComputerEvent
	{
		public enum Type
		{
			DisplayEntry = 0,
			DisplayAllEntries = 1,
			ClearEntry = 2,
			ClearAllEntries = 3,
			Wait = 4
		}

		public Type type;

		public int entryID;

		public float waitTime;
	}

	private NomaiComputer _computer;

	[SerializeField]
	private NomaiInterfaceSlot _slot;

	[SerializeField]
	private ComputerEvent[] _onActivate;

	[SerializeField]
	private ComputerEvent[] _onDeactivate;

	private void Awake()
	{
		_computer = GetComponent<NomaiComputer>();
	}

	private void OnEnable()
	{
		if (_slot != null)
		{
			_slot.OnSlotActivated += OnSlotActivated;
			_slot.OnSlotDeactivated += OnSlotDeactivated;
		}
	}

	private void OnDisable()
	{
		if (_slot != null)
		{
			_slot.OnSlotActivated -= OnSlotActivated;
			_slot.OnSlotDeactivated -= OnSlotDeactivated;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		FireEvents(ref _onActivate);
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		FireEvents(ref _onDeactivate);
	}

	private void FireEvents(ref ComputerEvent[] events)
	{
		for (int i = 0; i < events.Length; i++)
		{
			switch (events[i].type)
			{
			case ComputerEvent.Type.DisplayEntry:
				_computer.DisplayEntry(events[i].entryID);
				break;
			case ComputerEvent.Type.DisplayAllEntries:
				_computer.DisplayAllEntries();
				break;
			case ComputerEvent.Type.ClearEntry:
				_computer.ClearEntry(events[i].entryID);
				break;
			case ComputerEvent.Type.ClearAllEntries:
				_computer.ClearAllEntries();
				break;
			case ComputerEvent.Type.Wait:
				_computer.Wait(events[i].waitTime);
				break;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_slot != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(base.transform.position, _slot.transform.position);
		}
	}
}
