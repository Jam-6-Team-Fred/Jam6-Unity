using UnityEngine;

public class JumpPromptTrigger : MonoBehaviour
{
	private ScreenPrompt _jumpPrompt;

	private void Start()
	{
		_jumpPrompt = new ScreenPrompt(InputLibrary.jump, UITextLibrary.GetString(UITextType.JumpPrompt) + " <CMD>" + UITextLibrary.GetString(UITextType.HoldReleasePrompt));
		base.enabled = false;
	}

	private void Update()
	{
		_jumpPrompt.SetVisibility(Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None) && OWInput.IsInputMode(InputMode.Character));
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (hitCollider.CompareTag("PlayerDetector"))
		{
			Locator.GetPromptManager().AddScreenPrompt(_jumpPrompt, PromptPosition.UpperRight);
			base.enabled = true;
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (hitCollider.CompareTag("PlayerDetector"))
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_jumpPrompt);
			base.enabled = false;
		}
	}
}
