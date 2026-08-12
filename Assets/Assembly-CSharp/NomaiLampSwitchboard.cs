using UnityEngine;

public class NomaiLampSwitchboard : MonoBehaviour
{
	[SerializeField]
	private float _fadeDuration = 3f;

	[SerializeField]
	private NomaiLamp[] _lamps;

	[SerializeField]
	private NomaiInterfaceSlot[] _slots;

	private void Awake()
	{
		if (_slots.Length != _lamps.Length)
		{
			Debug.LogError("Slot and lamp arrays must be the same length!");
			Debug.Break();
		}
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
		int num = FindSlotIndex(slot);
		if (num >= 0 && num < _lamps.Length)
		{
			_lamps[num].FadeTo(1f, _fadeDuration);
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		int num = FindSlotIndex(slot);
		if (num >= 0 && num < _lamps.Length)
		{
			_lamps[num].FadeTo(0f, _fadeDuration);
		}
	}

	private int FindSlotIndex(NomaiInterfaceSlot slot)
	{
		for (int i = 0; i < _slots.Length; i++)
		{
			if (_slots[i] == slot)
			{
				return i;
			}
		}
		return -1;
	}
}
