using UnityEngine;

public class SecretTempleDoor : MonoBehaviour
{
	[SerializeField]
	private AbstractGhostDoorInterface _interface;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWRendererFadeController _lightBeamController;

	[SerializeField]
	private AbstractDoor _door;

	[SerializeField]
	private CageElevator _elevator;

	[SerializeField]
	private OWTriggerVolume _basementShipLogTrigger;

	private void Awake()
	{
		_interface.OnOpen += OnOpen;
		_interface.OnClose += OnClose;
	}

	private void Start()
	{
		_lightController.SetIntensity(1f);
		_lightBeamController.SetFade(1f);
		if (!Locator.GetShipLogManager().IsFactRevealed("IP_ZONE_3_LAB_X4"))
		{
			PlayerData.SetPersistentCondition("HIDE_TEMPLE_BASEMENT_ENTRIES", state: true);
			_basementShipLogTrigger.OnEntry += OnEnterBasementShipLogTrigger;
		}
	}

	private void OnDestroy()
	{
		_interface.OnOpen -= OnOpen;
		_interface.OnClose -= OnClose;
		_basementShipLogTrigger.OnEntry -= OnEnterBasementShipLogTrigger;
	}

	private void OnOpen()
	{
		_lightController.FadeTo(0f, 0.5f);
		_lightBeamController.FadeTo(0f, 0.5f);
		_elevator.GoToDestination(1);
		_door.Open();
	}

	private void OnClose()
	{
		_lightController.FadeTo(1f, 0.5f);
		_lightBeamController.FadeTo(1f, 0.5f);
		_elevator.GoToDestination(0);
		_door.Close();
	}

	private void OnEnterBasementShipLogTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetShipLogManager().RevealFact("IP_ZONE_3_LAB_X4");
			PlayerData.SetPersistentCondition("HIDE_TEMPLE_BASEMENT_ENTRIES", state: false);
			_basementShipLogTrigger.OnEntry -= OnEnterBasementShipLogTrigger;
		}
	}
}
