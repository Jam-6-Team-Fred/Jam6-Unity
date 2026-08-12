using UnityEngine;

[RequireComponent(typeof(NomaiInterfaceSlot))]
public class ShipLogFactOrbSlotTrigger : MonoBehaviour
{
	[SerializeField]
	private string[] _factIDs;

	private NomaiInterfaceSlot _slot;

	private void Awake()
	{
		_slot = GetComponent<NomaiInterfaceSlot>();
		_slot.OnSlotActivated += OnSlotActivated;
	}

	private void OnDestroy()
	{
		_slot.OnSlotActivated -= OnSlotActivated;
	}

	private void OnSlotActivated(NomaiInterfaceSlot slot)
	{
		for (int i = 0; i < _factIDs.Length; i++)
		{
			Locator.GetShipLogManager().RevealFact(_factIDs[i]);
		}
		_slot.OnSlotActivated -= OnSlotActivated;
	}
}
