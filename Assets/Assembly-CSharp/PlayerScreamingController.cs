using UnityEngine;

public class PlayerScreamingController : MonoBehaviour
{
	[SerializeField]
	private CharacterDialogueTree _playerDialogueTree;

	[SerializeField]
	private Animator _playerAnimator;

	private void Awake()
	{
		_playerDialogueTree.OnStartConversation += ResetScreamingFlags;
		_playerDialogueTree.OnSelectDialogueOption += OnSelectDialogueOption;
	}

	private void OnDestroy()
	{
		_playerDialogueTree.OnStartConversation -= ResetScreamingFlags;
		_playerDialogueTree.OnSelectDialogueOption -= OnSelectDialogueOption;
	}

	private void ResetScreamingFlags()
	{
		_playerAnimator.SetBool("Screaming", value: false);
		DialogueConditionManager.SharedInstance.SetConditionState("PLAYER_TIMECLONE_FREAKOUT");
	}

	private void OnSelectDialogueOption()
	{
		_playerAnimator.SetBool("Screaming", DialogueConditionManager.SharedInstance.GetConditionState("PLAYER_TIMECLONE_FREAKOUT"));
	}
}
