using UnityEngine;

public class DreamWorldPromptController : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume[] _jumpPromptTriggers;

	[SerializeField]
	private float _promptDelay = 5f;

	private ScreenPrompt _jumpPrompt;

	private bool _jumpPromptAdded;

	private float _showPromptTime;

	private void Start()
	{
		base.enabled = false;
		_jumpPrompt = new ScreenPrompt(InputLibrary.jump, UITextLibrary.GetString(UITextType.JumpPrompt) + " <CMD>" + UITextLibrary.GetString(UITextType.HoldReleasePrompt));
		for (int i = 0; i < _jumpPromptTriggers.Length; i++)
		{
			_jumpPromptTriggers[i].OnEntry += OnEnterJumpPromptTrigger;
			_jumpPromptTriggers[i].OnExit += OnExitJumpPromptTrigger;
		}
	}

	private void OnDestroy()
	{
		RemoveJumpListeners();
	}

	private void RemoveJumpListeners()
	{
		for (int i = 0; i < _jumpPromptTriggers.Length; i++)
		{
			_jumpPromptTriggers[i].OnEntry -= OnEnterJumpPromptTrigger;
			_jumpPromptTriggers[i].OnExit -= OnExitJumpPromptTrigger;
		}
		if (Locator.GetPlayerController() != null)
		{
			Locator.GetPlayerController().OnJump -= OnJump;
		}
	}

	private void Update()
	{
		_jumpPrompt.SetVisibility(OWInput.IsInputMode(InputMode.Character) && Time.time > _showPromptTime);
	}

	private void OnJump()
	{
		_jumpPromptAdded = false;
		Locator.GetPromptManager().RemoveScreenPrompt(_jumpPrompt);
		RemoveJumpListeners();
	}

	private void OnEnterJumpPromptTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = true;
			Locator.GetPlayerController().OnJump += OnJump;
			if (!_jumpPromptAdded)
			{
				_jumpPromptAdded = true;
				_showPromptTime = Time.time + _promptDelay;
				Locator.GetPromptManager().AddScreenPrompt(_jumpPrompt, PromptPosition.Center);
			}
		}
	}

	private void OnExitJumpPromptTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			base.enabled = false;
			Locator.GetPlayerController().OnJump -= OnJump;
			_jumpPrompt.SetVisibility(isVisible: false);
		}
	}
}
