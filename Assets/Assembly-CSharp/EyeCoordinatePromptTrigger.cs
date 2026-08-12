using UnityEngine;

public class EyeCoordinatePromptTrigger : MonoBehaviour
{
	[SerializeField]
	private VesselWarpController _warpController;

	private KeyInfoPromptController _promptController;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
	}

	private void Start()
	{
		_promptController = GameObject.FindWithTag("Global").GetComponent<KeyInfoPromptController>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		_promptController.SetEyeCoordinatesVisibility(_warpController.HasPower() && Locator.GetShipLogManager().IsFactRevealed("OPC_EYE_COORDINATES_X1"));
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = false;
			_promptController.SetEyeCoordinatesVisibility(visible: false);
		}
	}
}
