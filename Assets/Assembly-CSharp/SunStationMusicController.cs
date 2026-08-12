using UnityEngine;

public class SunStationMusicController : MonoBehaviour
{
	[SerializeField]
	private AudioVolume _musicVolume;

	[SerializeField]
	private OWTriggerVolume _controlModuleVolume;

	[SerializeField]
	private NomaiInterfaceSlot _openHatchSlot;

	private void Awake()
	{
		_controlModuleVolume.OnEntry += OnEnterControlModule;
		_openHatchSlot.OnSlotActivated += OnOpenEmergencyHatch;
	}

	private void Start()
	{
		_musicVolume.SetVolumeActivation(active: false);
	}

	private void OnDestroy()
	{
		_controlModuleVolume.OnEntry -= OnEnterControlModule;
		_openHatchSlot.OnSlotActivated -= OnOpenEmergencyHatch;
	}

	private void OnEnterControlModule(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_musicVolume.SetVolumeActivation(active: true);
		}
	}

	private void OnOpenEmergencyHatch(NomaiInterfaceSlot slot)
	{
		_musicVolume.SetVolumeActivation(active: true);
	}
}
