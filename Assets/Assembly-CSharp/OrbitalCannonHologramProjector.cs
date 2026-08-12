using UnityEngine;

public class OrbitalCannonHologramProjector : HologramProjector
{
	[SerializeField]
	private NomaiInterfaceSlot[] _slots;

	[SerializeField]
	private GameObject[] _holograms;

	[SerializeField]
	private NomaiComputer[] _computers;

	[SerializeField]
	private NomaiEnergyCable[] _energyCables;

	private int _activeIndex = -1;

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

	protected override void OnHologramComplete(Hologram hologram)
	{
		if (_computers.Length > _activeIndex)
		{
			_computers[_activeIndex].DisplayAllEntries();
		}
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		_activeIndex = GetSlotIndex(slot);
		if (_activeIndex != -1)
		{
			if (_holograms.Length > _activeIndex)
			{
				CreateHologram(_holograms[_activeIndex]);
			}
			if (_energyCables.Length > _activeIndex)
			{
				_energyCables[_activeIndex].SetPowered(powered: true);
			}
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		if (_activeIndex != -1)
		{
			if (_computers.Length > _activeIndex)
			{
				_computers[_activeIndex].ClearAllEntries();
			}
			if (_energyCables.Length > _activeIndex)
			{
				_energyCables[_activeIndex].SetPowered(powered: false);
			}
			DestroyActiveHologram();
			_activeIndex = -1;
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
