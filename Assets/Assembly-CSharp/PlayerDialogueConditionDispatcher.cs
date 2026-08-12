using UnityEngine;

public class PlayerDialogueConditionDispatcher : MonoBehaviour
{
	private void Awake()
	{
		GlobalMessenger.AddListener("ShipLiftoff", OnShipLiftoff);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ShipLiftoff", OnShipLiftoff);
	}

	private void OnShipLiftoff()
	{
		DialogueConditionManager.SharedInstance.SetConditionState("Space", conditionState: true);
	}
}
