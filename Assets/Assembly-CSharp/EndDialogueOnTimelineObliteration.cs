using UnityEngine;

public class EndDialogueOnTimelineObliteration : MonoBehaviour
{
	[SerializeField]
	private CharacterDialogueTree _dialogueTree;

	private void Start()
	{
		if (Locator.GetTimelineObliterationController() != null)
		{
			Locator.GetTimelineObliterationController().OnTimelineStartObliteration += OnTimelineStartObliteration;
		}
	}

	private void OnDestroy()
	{
		if (Locator.GetTimelineObliterationController() != null)
		{
			Locator.GetTimelineObliterationController().OnTimelineStartObliteration -= OnTimelineStartObliteration;
		}
	}

	private void OnTimelineStartObliteration()
	{
		if (_dialogueTree.InConversation())
		{
			_dialogueTree.EndConversation();
			_dialogueTree.GetInteractVolume().DisableInteraction();
		}
	}
}
