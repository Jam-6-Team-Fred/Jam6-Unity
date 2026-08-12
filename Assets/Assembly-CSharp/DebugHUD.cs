using UnityEngine;

public class DebugHUD : MonoBehaviour
{
	private ForceDetector _playerForceDetector;

	private PlayerCharacterController _playerController;

	private InputMode[] _inputModeArray;

	private MeshRenderer[] _thrusterArrowRenderers;

	private float _gForce;

	private void Awake()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			_playerForceDetector = gameObject.GetRequiredComponentInChildren<ForceDetector>();
			_playerController = gameObject.GetRequiredComponent<PlayerCharacterController>();
		}
		GlobalMessenger.AddListener("DisableGUI", OnDisableGUI);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("DisableGUI", OnDisableGUI);
	}

	private void OnDisableGUI()
	{
		Object.Destroy(this);
	}

	private void LateUpdate()
	{
		if (_playerController != null)
		{
			_gForce = _playerController.GetNormalAccelerationScalar();
		}
	}

	private void OnGUI()
	{
		if (GUIMode.IsHiddenMode() || PlayerState.UsingShipComputer())
		{
			return;
		}
		if (GUIMode.IsDebugMode())
		{
			GUI.Label(new Rect(10f, 10f, 200f, 20f), "Time Scale: " + Mathf.Round(Time.timeScale * 100f) / 100f);
			GUI.Label(new Rect(10f, 25f, 200f, 20f), "Time Remaining: " + Mathf.Floor(TimeLoop.GetSecondsRemaining() / 60f) + ":" + Mathf.Round(TimeLoop.GetSecondsRemaining() % 60f * 100f / 100f));
			GUI.Label(new Rect(10f, 40f, 200f, 20f), "Loop Count: " + TimeLoop.GetLoopCount());
			GUI.Label(new Rect(10f, 55f, 90f, 40f), "PauseFlags: ");
			GUI.Label(new Rect(100f, 55f, 50f, 40f), "MENU\n" + (OWTime.IsPaused(OWTime.PauseType.Menu) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(150f, 55f, 50f, 40f), "LOAD\n" + (OWTime.IsPaused(OWTime.PauseType.Loading) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(200f, 55f, 50f, 40f), "READ\n" + (OWTime.IsPaused(OWTime.PauseType.Reading) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(250f, 55f, 50f, 40f), "SLP\n" + (OWTime.IsPaused(OWTime.PauseType.Sleeping) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(300f, 55f, 50f, 40f), "INIT\n" + (OWTime.IsPaused(OWTime.PauseType.Initializing) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(350f, 55f, 50f, 40f), "STRM\n" + (OWTime.IsPaused(OWTime.PauseType.Streaming) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(400f, 55f, 50f, 40f), "SYS\n" + (OWTime.IsPaused(OWTime.PauseType.System) ? "TRUE " : "FALSE"));
			GUI.Label(new Rect(10f, 85f, 200f, 20f), "Input Mode: " + OWInput.GetInputMode());
			_inputModeArray = OWInput.GetInputModeStack();
			GUI.Label(new Rect(10f, 100f, 200f, 20f), "Input Mode Stack: ");
			int num = 150;
			for (int i = 0; i < _inputModeArray.Length; i++)
			{
				GUI.Label(new Rect(num, 100f, 200f, 20f), _inputModeArray[i].ToString());
				num += 75;
			}
			if (_playerForceDetector != null)
			{
				GUI.Label(new Rect(10f, 115f, 200f, 20f), "Net Force Accel: " + Mathf.Round(_playerForceDetector.GetForceAcceleration().magnitude * 100f) / 100f);
			}
			GUI.Label(new Rect(10f, 130f, 200f, 20f), "G-Force: " + Mathf.Round(_gForce * 100f) / 100f);
			GUI.Label(new Rect(10f, 145f, 200f, 20f), "Load Time: " + LoadTimeTracker.GetLatestLoadTime());
			if (DynamicResolutionManager.isActive)
			{
				GUI.Label(new Rect(10f, 160f, 200f, 20f), "Resolution Scale: " + DynamicResolutionManager.currentResolutionScale);
			}
		}
		if (GUIMode.IsInputMode())
		{
			GUI.Label(new Rect(10f, 10f, 300f, 2500f), ReadInputManager.ReadCommandInputs(verbose: false));
		}
		if (GUIMode.IsInputVerboseMode())
		{
			GUI.Label(new Rect(0f, 0f, 300f, 2500f), ReadInputManager.ReadCommandInputs(verbose: false));
			GUI.Label(new Rect(300f, 0f, 300f, 2500f), ReadInputManager.ReadCommandInputs(verbose: true));
		}
		if (GUIMode.IsInputRawMode())
		{
			GUI.Label(new Rect(10f, 480f, 500f, 20f), "Raw Mode Output Deprecated");
		}
	}
}
