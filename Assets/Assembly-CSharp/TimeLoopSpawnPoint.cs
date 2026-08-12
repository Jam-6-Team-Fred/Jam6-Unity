using UnityEngine;

public class TimeLoopSpawnPoint : SpawnPoint
{
	[SerializeField]
	private Light _ambientLight;

	public override void OnSpawnPlayer()
	{
		GlobalMessenger<OWRigidbody>.FireEvent("EnterTimeLoopCentral", Locator.GetPlayerBody());
		_ambientLight.gameObject.SetActive(value: true);
	}
}
