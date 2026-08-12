using UnityEngine;

[RequireComponent(typeof(Camera))]
public class NearViewCamera : MonoBehaviour
{
	private Camera _camera;

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		GlobalMessenger.AddListener("FlashbackStart", OnEventDisableCamera);
		GlobalMessenger.AddListener("TriggerDeathOutsideTimeLoop", OnEventDisableCamera);
		GlobalMessenger.AddListener("TriggerDeathOfReality", OnEventDisableCamera);
		GlobalMessenger.AddListener("TriggerDeathByVoid", OnEventDisableCamera);
		GlobalMessenger.AddListener("TriggerDeathByRingworldEscape", OnEventDisableCamera);
		GlobalMessenger.AddListener("TriggerDeathByDreamworldEscape", OnEventDisableCamera);
		GlobalMessenger.AddListener("TriggerDeathByQuantumMoon", OnEventDisableCamera);
		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("FlashbackStart", OnEventDisableCamera);
		GlobalMessenger.RemoveListener("TriggerDeathOutsideTimeLoop", OnEventDisableCamera);
		GlobalMessenger.RemoveListener("TriggerDeathOfReality", OnEventDisableCamera);
		GlobalMessenger.RemoveListener("TriggerDeathByVoid", OnEventDisableCamera);
		GlobalMessenger.RemoveListener("TriggerDeathByRingworldEscape", OnEventDisableCamera);
		GlobalMessenger.RemoveListener("TriggerDeathByDreamworldEscape", OnEventDisableCamera);
		GlobalMessenger.RemoveListener("TriggerDeathByQuantumMoon", OnEventDisableCamera);
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
	}

	private void OnSwitchActiveCamera(OWCamera camera)
	{
		if (camera.CompareTag("MainCamera"))
		{
			_camera.enabled = true;
		}
		else
		{
			_camera.enabled = false;
		}
	}

	private void OnEventDisableCamera()
	{
		_camera.enabled = false;
	}
}
