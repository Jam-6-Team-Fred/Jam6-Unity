using UnityEngine;

public class RingWorldPromptController : MonoBehaviour
{
	[SerializeField]
	private AirlockInterface[] _airlocks;

	[SerializeField]
	private OWTriggerVolume[] _flashlightPromptVolumes;

	[SerializeField]
	private CloakFieldController _cloakController;

	private bool _probeInTrigger;

	private bool _waitingForProbeRetrieval;

	private bool _showFlashlightPrompt;

	private bool _showFlashlightPromptOnEntry;

	private bool _hasUsedFlashlightOnAirlock;

	private void Awake()
	{
		for (int i = 0; i < _flashlightPromptVolumes.Length; i++)
		{
			_flashlightPromptVolumes[i].OnEntry += OnEntry;
			_flashlightPromptVolumes[i].OnExit += OnExit;
		}
		_cloakController.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnterCloak);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _flashlightPromptVolumes.Length; i++)
		{
			_flashlightPromptVolumes[i].OnEntry -= OnEntry;
			_flashlightPromptVolumes[i].OnExit -= OnExit;
		}
		_cloakController.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterCloak);
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < _airlocks.Length; i++)
		{
			if (_airlocks[i].AreAnySensorsLit())
			{
				flag = true;
				break;
			}
		}
		if (!PlayerState.IsFlashlightOn() && _showFlashlightPromptOnEntry)
		{
			_showFlashlightPrompt = true;
			_showFlashlightPromptOnEntry = false;
			GlobalMessenger.FireEvent("EnterFlashlightPromptTrigger");
		}
		if (!_waitingForProbeRetrieval && !PlayerState.IsFlashlightOn() && flag && _probeInTrigger)
		{
			_waitingForProbeRetrieval = true;
		}
		else if (_waitingForProbeRetrieval && !_probeInTrigger)
		{
			_waitingForProbeRetrieval = false;
			if (!PlayerState.IsFlashlightOn() && !_showFlashlightPrompt)
			{
				_showFlashlightPrompt = true;
				GlobalMessenger.FireEvent("EnterFlashlightPromptTrigger");
			}
		}
		if (flag && PlayerState.IsFlashlightOn() && !_probeInTrigger)
		{
			base.enabled = false;
			_waitingForProbeRetrieval = false;
			_showFlashlightPromptOnEntry = false;
			_hasUsedFlashlightOnAirlock = true;
			if (_showFlashlightPrompt)
			{
				_showFlashlightPrompt = false;
				GlobalMessenger.FireEvent("ExitFlashlightPromptTrigger");
			}
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInTrigger = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = false;
			_waitingForProbeRetrieval = false;
			_showFlashlightPromptOnEntry = !_hasUsedFlashlightOnAirlock;
			if (_showFlashlightPrompt)
			{
				_showFlashlightPrompt = false;
				GlobalMessenger.FireEvent("ExitFlashlightPromptTrigger");
			}
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_probeInTrigger = false;
		}
	}

	private void OnPlayerEnterCloak()
	{
		if (!_cloakController.IsReferenceFrameVolumeActive())
		{
			PlayerData.SetPersistentCondition("MARK_ON_HUD_TUTORIAL_COMPLETE", state: false);
			PlayerData.SetPersistentCondition("COMPLETED_SHIPLOG_TUTORIAL", state: false);
		}
		_cloakController.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnterCloak);
	}
}
