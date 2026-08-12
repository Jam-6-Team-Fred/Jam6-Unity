using UnityEngine;

public class HideAndSeekController : MonoBehaviour
{
	[SerializeField]
	private GameObject _kidsPreGameRoot;

	[SerializeField]
	private GameObject _kidsHiddenRoot;

	private CharacterDialogueTree[] _dialoguesPreGame;

	private CharacterDialogueTree[] _dialoguesHidden;

	private float _gameStartTime;

	private bool _firstFrame = true;

	private OWTriggerVolume _trigger;

	private int _totalKidCount;

	private void Awake()
	{
		_dialoguesPreGame = _kidsPreGameRoot.GetComponentsInChildren<CharacterDialogueTree>();
		_dialoguesHidden = _kidsHiddenRoot.GetComponentsInChildren<CharacterDialogueTree>();
		_totalKidCount = _dialoguesHidden.Length;
		for (int i = 0; i < _dialoguesPreGame.Length; i++)
		{
			if (_dialoguesPreGame[i] != null)
			{
				_dialoguesPreGame[i].OnEndConversation += OnEndPreGameConversation;
			}
		}
		for (int j = 0; j < _dialoguesHidden.Length; j++)
		{
			if (_dialoguesHidden[j] != null)
			{
				_dialoguesHidden[j].OnEndConversation += OnEndHiddenConversation;
			}
		}
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnExit += OnExit;
		GlobalMessenger.AddListener("ResetSimulation", OnResetSimulation);
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
	}

	private void OnResetSimulation()
	{
		DialogueConditionManager.SharedInstance.SetConditionState("BeginHideAndSeek");
		DialogueConditionManager.SharedInstance.SetConditionState("EndHideAndSeek");
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		if (PlayerData.KnowsFrequency(SignalFrequency.HideAndSeek))
		{
			PlayerData.ForgetFrequency(SignalFrequency.HideAndSeek);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _dialoguesPreGame.Length; i++)
		{
			if (_dialoguesPreGame[i] != null)
			{
				_dialoguesPreGame[i].OnEndConversation -= OnEndPreGameConversation;
			}
		}
		for (int j = 0; j < _dialoguesHidden.Length; j++)
		{
			if (_dialoguesHidden[j] != null)
			{
				_dialoguesHidden[j].OnEndConversation -= OnEndHiddenConversation;
			}
		}
		_trigger.OnExit -= OnExit;
		GlobalMessenger.RemoveListener("ResetSimulation", OnResetSimulation);
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
	}

	private void Update()
	{
		if (_firstFrame)
		{
			_firstFrame = false;
			_kidsHiddenRoot.SetActive(value: false);
			base.enabled = false;
		}
		else if (Time.time > _gameStartTime)
		{
			base.enabled = false;
			Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OpenEyes(1f);
			ReticleController.Show();
			OWInput.ChangeInputMode(InputMode.Character);
			PlayerData.LearnFrequency(SignalFrequency.HideAndSeek);
			Locator.GetToolModeSwapper().GetSignalScope().SelectFrequency(SignalFrequency.HideAndSeek);
			GlobalMessenger<bool>.FireEvent("EnterSignalscopePromptTrigger", arg1: false);
			GlobalMessenger.FireEvent("StartHideAndSeek");
			_kidsPreGameRoot.SetActive(value: false);
			_kidsHiddenRoot.SetActive(value: true);
		}
	}

	private void OnEndPreGameConversation()
	{
		if (!DialogueConditionManager.SharedInstance.GetConditionState("BeginHideAndSeek") || DialogueConditionManager.SharedInstance.GetConditionState("EndHideAndSeek"))
		{
			return;
		}
		for (int i = 0; i < _dialoguesPreGame.Length; i++)
		{
			if (_dialoguesPreGame[i] != null)
			{
				_dialoguesPreGame[i].GetComponent<InteractReceiver>().DisableInteraction();
			}
		}
		Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().CloseEyes(2f);
		ReticleController.Hide();
		OWInput.ChangeInputMode(InputMode.None);
		_gameStartTime = Time.time + 4f;
		base.enabled = true;
	}

	private void OnEndHiddenConversation()
	{
		int num = 0;
		if (DialogueConditionManager.SharedInstance.GetConditionState("FoundKidOne"))
		{
			num++;
		}
		if (DialogueConditionManager.SharedInstance.GetConditionState("FoundKidTwo"))
		{
			num++;
		}
		if (DialogueConditionManager.SharedInstance.GetConditionState("FoundKidThree"))
		{
			num++;
		}
		if (num == _totalKidCount - 1)
		{
			DialogueConditionManager.SharedInstance.SetConditionState("LastKidToFind", conditionState: true);
		}
		if (DialogueConditionManager.SharedInstance.GetConditionState("EndHideAndSeek"))
		{
			Locator.GetToolModeSwapper().GetSignalScope().SelectFrequency(SignalFrequency.Traveler);
			GlobalMessenger.FireEvent("EndHideAndSeek");
			Object.Destroy(this);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			GlobalMessenger.FireEvent("ExitSignalscopePromptTrigger");
		}
	}
}
