using UnityEngine;

public class ModelShipController : ThrusterController
{
	[SerializeField]
	private GameObject _detector;

	protected override void Awake()
	{
		base.Awake();
		_detector.SetActive(value: false);
		base.enabled = false;
		GlobalMessenger<OWRigidbody>.AddListener("EnterRemoteFlightConsole", OnEnterRemoteFlightConsole);
		GlobalMessenger.AddListener("ExitRemoteFlightConsole", OnExitRemoteFlightConsole);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterRemoteFlightConsole", OnEnterRemoteFlightConsole);
		GlobalMessenger.RemoveListener("ExitRemoteFlightConsole", OnExitRemoteFlightConsole);
	}

	protected override Vector3 ReadTranslationalInput()
	{
		if (!OWInput.IsInputMode(InputMode.ModelShip))
		{
			return Vector3.zero;
		}
		Vector3 result = new Vector3(OWInput.GetValue(InputLibrary.thrustX), 0f, OWInput.GetValue(InputLibrary.thrustZ));
		if (result.sqrMagnitude > 1f)
		{
			result.Normalize();
		}
		result.y = OWInput.GetValue(InputLibrary.thrustUp) - OWInput.GetValue(InputLibrary.thrustDown);
		return result;
	}

	protected override Vector3 ReadRotationalInput()
	{
		if (!OWInput.IsInputMode(InputMode.ModelShip))
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		zero.z -= OWInput.GetValue(InputLibrary.yaw);
		zero.x -= OWInput.GetValue(InputLibrary.pitch);
		return zero;
	}

	private void OnEnterRemoteFlightConsole(OWRigidbody modelShipBody)
	{
		base.enabled = true;
		_detector.SetActive(value: true);
	}

	private void OnExitRemoteFlightConsole()
	{
		base.enabled = false;
		_detector.SetActive(value: false);
	}
}
