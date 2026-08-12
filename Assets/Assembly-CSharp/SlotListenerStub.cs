using UnityEngine;

public class SlotListenerStub : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot[] _slots;

	private void Awake()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].OnSlotActivated += OnSlotActivated;
			_slots[i].OnSlotDeactivated += OnSlotDeactivated;
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			_slots[i].OnSlotActivated -= OnSlotActivated;
			_slots[i].OnSlotDeactivated -= OnSlotDeactivated;
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
	}
}
