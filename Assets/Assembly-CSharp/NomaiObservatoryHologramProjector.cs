using UnityEngine;

public class NomaiObservatoryHologramProjector : HologramProjector
{
	[SerializeField]
	private NomaiInterfaceSlot[] _slots;

	[SerializeField]
	private GameObject _hologramPrefab;

	[SerializeField]
	private NomaiComputer _eyeComputer;

	private int _slotOnCount;

	private bool _activateEyeComputerAfterDelay;

	private float _activateEyeComputerTime;

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

	protected override void Update()
	{
		base.Update();
		if (_activateEyeComputerAfterDelay && Time.time > _activateEyeComputerTime)
		{
			_activateEyeComputerAfterDelay = false;
			_eyeComputer.DisplayAllEntries();
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		if (_activeHologram == null)
		{
			CreateHologram(_hologramPrefab);
		}
		_slotOnCount++;
		int slotIndex = GetSlotIndex(slot);
		((EyeOrbitHologram)_activeHologram).SetOrbitVisibility(slotIndex, visible: true);
		if (slotIndex == 5 && _eyeComputer != null)
		{
			_activateEyeComputerTime = Time.time + 5f;
			_activateEyeComputerAfterDelay = true;
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		int slotIndex = GetSlotIndex(slot);
		((EyeOrbitHologram)_activeHologram).SetOrbitVisibility(slotIndex, visible: false);
		_slotOnCount--;
		if (_slotOnCount == 0)
		{
			DestroyActiveHologram();
		}
		if ((_slotOnCount == 0 || slotIndex == 5) && _eyeComputer != null)
		{
			_eyeComputer.ClearAllEntries();
			_activateEyeComputerAfterDelay = false;
		}
	}

	private int GetSlotIndex(NomaiInterfaceSlot slot)
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
