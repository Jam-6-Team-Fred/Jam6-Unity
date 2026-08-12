using UnityEngine;

public class MuseumVolume : MonoBehaviour
{
	[SerializeField]
	private Collider _floorCollider;

	[SerializeField]
	private OWTriggerVolume _observatorySectorTrigger;

	[SerializeField]
	private GameObject _sequenceBreakColliderRoot;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = GetComponent<OWTriggerVolume>();
		GlobalMessenger.AddListener("ResumeSimulation", OnResumeSimulation);
		_trigger.OnEntry += OnEntry;
	}

	private void Start()
	{
		_sequenceBreakColliderRoot.SetActive(!TimeLoop.IsTimeFlowing());
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ResumeSimulation", OnResumeSimulation);
		_trigger.OnEntry -= OnEntry;
	}

	private void OnResumeSimulation()
	{
		_floorCollider.enabled = true;
		_trigger.AddObjectToVolume(Locator.GetPlayerDetector());
		_trigger.AddObjectToVolume(Locator.GetPlayerCameraDetector());
		_observatorySectorTrigger.AddObjectToVolume(Locator.GetPlayerDetector());
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() != null && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem()
			.GetItemType() == ItemType.WarpCore)
		{
			Achievements.Earn(Achievements.Type.MUSEUM);
		}
	}
}
