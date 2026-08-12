using UnityEngine;

public class ProbePromptTrigger : MonoBehaviour
{
	[SerializeField]
	private bool _cameraModePrompt;

	[SerializeField]
	private bool _showPromptOnEnter;

	[SerializeField]
	private CharacterDialogueTree _dialogue;

	private OWTriggerVolume _trigger;

	private ScreenPrompt _centerCameraPrompt;

	private void Awake()
	{
		_trigger = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		if (_dialogue != null)
		{
			_dialogue.OnEndConversation += OnEndConversation;
		}
		if (_cameraModePrompt)
		{
			_centerCameraPrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.PhotoModePrompt));
		}
		base.enabled = false;
	}

	private void Start()
	{
		if (_cameraModePrompt)
		{
			Locator.GetPromptManager().AddScreenPrompt(_centerCameraPrompt, PromptPosition.Center);
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
		if (_cameraModePrompt)
		{
			base.enabled = true;
		}
		else
		{
			GlobalMessenger.FireEvent("EnterProbePromptTrigger");
		}
	}

	private void Update()
	{
		if (_cameraModePrompt)
		{
			_centerCameraPrompt.SetVisibility(!Locator.GetToolModeSwapper().GetProbeLauncher().InPhotoMode() && !PlayerState.InConversation());
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (_showPromptOnEnter && hitObj.CompareTag("PlayerDetector"))
		{
			if (_cameraModePrompt)
			{
				base.enabled = true;
			}
			else
			{
				GlobalMessenger.FireEvent("EnterProbePromptTrigger");
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			if (_cameraModePrompt)
			{
				_centerCameraPrompt.SetVisibility(isVisible: false);
				base.enabled = false;
			}
			else
			{
				GlobalMessenger.FireEvent("ExitProbePromptTrigger");
			}
		}
	}
}
