using UnityEngine;

public class EyeVortexTrigger : MonoBehaviour
{
	[SerializeField]
	private ProbeSafetyVolume _tunnelSafetyVolume;

	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private GameObject _tunnelObject;

	[Space]
	[SerializeField]
	private OWTriggerVolume _vortexTrigger;

	[SerializeField]
	private OWTriggerVolume _heatLightningTrigger;

	[SerializeField]
	private HeatLightningController _heatLightningController;

	[SerializeField]
	private OWTriggerVolume _entryAudioTrigger;

	[SerializeField]
	private OWTriggerVolume _exitAudioTrigger;

	private void Awake()
	{
		_vortexTrigger.OnEntry += OnEnterVortex;
		_vortexTrigger.OnExit += OnExitVortex;
		_heatLightningTrigger.OnEntry += OnEnterHeatLightningTrigger;
		_entryAudioTrigger.OnEntry += OnEnterEntryAudioTrigger;
		_exitAudioTrigger.OnEntry += OnEnterExitAudioTrigger;
	}

	private void Start()
	{
		_tunnelSafetyVolume.SetVolumeActivation(active: false);
	}

	private void OnDestroy()
	{
		_vortexTrigger.OnEntry -= OnEnterVortex;
		_vortexTrigger.OnExit -= OnExitVortex;
		_heatLightningTrigger.OnEntry -= OnEnterHeatLightningTrigger;
		_entryAudioTrigger.OnEntry -= OnEnterEntryAudioTrigger;
		_exitAudioTrigger.OnEntry -= OnEnterExitAudioTrigger;
	}

	private void OnEnterVortex(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_musicSource.SetLocalVolume(0f);
			_musicSource.FadeIn(0.5f);
			_tunnelSafetyVolume.SetVolumeActivation(active: true);
			RumbleManager.StartEyeVortex();
			Locator.GetEyeStateManager().SetState(EyeState.IntoTheVortex);
			Locator.GetPlayerCamera().renderSkybox = false;
		}
	}

	private void OnExitVortex(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_tunnelObject.SetActive(value: false);
			RumbleManager.StopEyeVortex();
		}
	}

	private void OnEnterHeatLightningTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_heatLightningController.TriggerFlash(1.5f);
		}
	}

	private void OnEnterEntryAudioTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_oneShotSource.pitch = 0.9f;
			_oneShotSource.PlayOneShot(AudioType.EyeVortexEntry);
		}
	}

	private void OnEnterExitAudioTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_oneShotSource.pitch = 1.1f;
			_oneShotSource.PlayOneShot(AudioType.EyeVortexExit);
		}
	}
}
