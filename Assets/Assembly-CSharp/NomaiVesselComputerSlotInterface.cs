using UnityEngine;

[RequireComponent(typeof(NomaiVesselComputer))]
public class NomaiVesselComputerSlotInterface : MonoBehaviour
{
	private NomaiVesselComputer _computer;

	[SerializeField]
	private NomaiInterfaceSlot _slot;

	[SerializeField]
	private TextAsset _overrideText;

	private void Awake()
	{
		_computer = GetComponent<NomaiVesselComputer>();
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
		if (_overrideText != null)
		{
			_computer.RebootWithNewText(_overrideText);
		}
		else
		{
			_computer.TurnOn();
		}
	}

	private void OnSlotDeactivated(NomaiInterfaceSlot slot)
	{
		_computer.TurnOff();
	}
}
