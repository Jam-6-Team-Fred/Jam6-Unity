using UnityEngine;

public class GabbroDialogueSwapper : MonoBehaviour
{
	[SerializeField]
	private GabbroConditionalDialogue[] _conditionalDialogues;

	private CharacterDialogueTree _dialogueTree;

	private GabbroConditionalDialogue _activeConditionDialogue;

	private void Start()
	{
		_dialogueTree = GetComponent<CharacterDialogueTree>();
		_dialogueTree.OnStartConversation += OnStartConversation;
		_dialogueTree.OnEndConversation += OnEndConversation;
		bool persistentCondition = PlayerData.GetPersistentCondition("TALKED_TO_GABBRO");
		bool persistentCondition2 = PlayerData.GetPersistentCondition("GABBRO_MERGE_TRIGGERED");
		for (int i = 0; i < _conditionalDialogues.Length; i++)
		{
			if (_conditionalDialogues[i].hasTalkedToGabbro == persistentCondition && _conditionalDialogues[i].hasMergeTriggered == persistentCondition2 && PlayerData.LoadLoopCount() >= _conditionalDialogues[i].minLoopCount)
			{
				_activeConditionDialogue = _conditionalDialogues[i];
			}
		}
		if (_activeConditionDialogue != null)
		{
			_dialogueTree.SetTextXml(_activeConditionDialogue.dialogueTextAsset);
		}
		else if (persistentCondition2)
		{
			_activeConditionDialogue = _conditionalDialogues[_conditionalDialogues.Length - 1];
			_dialogueTree.SetTextXml(_activeConditionDialogue.dialogueTextAsset);
		}
	}

	private void OnDestroy()
	{
		_dialogueTree.OnStartConversation -= OnStartConversation;
		_dialogueTree.OnEndConversation -= OnEndConversation;
	}

	private void OnStartConversation()
	{
		PlayerData.SetPersistentCondition("TALKED_TO_GABBRO", state: true);
		if (_activeConditionDialogue.triggerMerge)
		{
			PlayerData.SetPersistentCondition("GABBRO_MERGE_TRIGGERED", state: true);
		}
	}

	private void OnEndConversation()
	{
		if (DialogueConditionManager.SharedInstance.GetConditionState("BeginMeditation"))
		{
			PlayerData.SetPersistentCondition("KNOWS_MEDITATION", state: true);
			Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
		}
	}
}
