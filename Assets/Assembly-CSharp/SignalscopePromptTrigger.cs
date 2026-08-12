using UnityEngine;

public class SignalscopePromptTrigger : MonoBehaviour
{
	[SerializeField]
	private bool _showPromptOnEnter;

	[SerializeField]
	private CharacterDialogueTree _dialogue;

	[Space]
	[SerializeField]
	private bool _switchFrequency;

	[SerializeField]
	private SignalFrequency _frequency = SignalFrequency.Traveler;

	private OWTriggerVolume _trigger;

	private void Awake()
	{
		_trigger = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		if (_dialogue != null)
		{
			_dialogue.OnEndConversation += OnEndConversation;
		}
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
		if (_dialogue != null)
		{
			_dialogue.OnEndConversation -= OnEndConversation;
		}
	}

	private void OnEndConversation()
	{
		if (DialogueConditionManager.SharedInstance.GetConditionState("SHOW_SIGNALSCOPE_PROMPT"))
		{
			DialogueConditionManager.SharedInstance.SetConditionState("SHOW_SIGNALSCOPE_PROMPT");
			GlobalMessenger<bool>.FireEvent("EnterSignalscopePromptTrigger", arg1: true);
			if (_switchFrequency)
			{
				Locator.GetToolModeSwapper().GetSignalScope().SelectFrequency(_frequency);
			}
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (_showPromptOnEnter && hitObj.CompareTag("PlayerDetector"))
		{
			GlobalMessenger<bool>.FireEvent("EnterSignalscopePromptTrigger", arg1: true);
			if (_switchFrequency)
			{
				Locator.GetToolModeSwapper().GetSignalScope().SelectFrequency(_frequency);
			}
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
