using UnityEngine;

public class LightSwitchInterfaceOrb : LightSwitch
{
	private enum SlotAction
	{
		NONE = 0,
		TURN_ON = 1,
		TURN_OFF = 2
	}

	[SerializeField]
	private NomaiInterfaceSlot[] _slotsGroup1;

	[SerializeField]
	private SlotAction _group1SlotActivateAction;

	[SerializeField]
	private SlotAction _group1SlotDectivateAction;

	[SerializeField]
	private NomaiInterfaceSlot[] _slotsGroup2;

	[SerializeField]
	private SlotAction _group2SlotActivateAction;

	[SerializeField]
	private SlotAction _group2SlotDeactivateAction;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < _slotsGroup1.Length; i++)
		{
			_slotsGroup1[i].OnSlotActivated += OnSlot1Activated;
			_slotsGroup1[i].OnSlotDeactivated += OnSlot1Deactivated;
		}
		for (int j = 0; j < _slotsGroup2.Length; j++)
		{
			_slotsGroup2[j].OnSlotActivated += OnSlot2Activated;
			_slotsGroup2[j].OnSlotDeactivated += OnSlot2Deactivated;
		}
	}

	private void OnSlot1Activated(NomaiInterfaceSlot slot)
	{
		switch (_group1SlotActivateAction)
		{
		case SlotAction.TURN_ON:
			TurnOn();
			break;
		case SlotAction.TURN_OFF:
			TurnOff();
			break;
		}
	}

	private void OnSlot1Deactivated(NomaiInterfaceSlot slot)
	{
		switch (_group1SlotDectivateAction)
		{
		case SlotAction.TURN_ON:
			TurnOn();
			break;
		case SlotAction.TURN_OFF:
			TurnOff();
			break;
		}
	}

	private void OnSlot2Activated(NomaiInterfaceSlot slot)
	{
		switch (_group2SlotActivateAction)
		{
		case SlotAction.TURN_ON:
			TurnOn();
			break;
		case SlotAction.TURN_OFF:
			TurnOff();
			break;
		}
	}

	private void OnSlot2Deactivated(NomaiInterfaceSlot slot)
	{
		switch (_group2SlotDeactivateAction)
		{
		case SlotAction.TURN_ON:
			TurnOn();
			break;
		case SlotAction.TURN_OFF:
			TurnOff();
			break;
		}
	}
}
