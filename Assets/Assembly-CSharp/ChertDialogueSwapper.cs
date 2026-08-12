using UnityEngine;

public class ChertDialogueSwapper : MonoBehaviour
{
	[SerializeField]
	private ChertConditionalDialogue[] _conditionalDialogues;

	private CharacterDialogueTree _dialogueTree;

	private ChertConditionalDialogue _activeConditionDialogue;

	private void Start()
	{
		_dialogueTree = GetComponent<CharacterDialogueTree>();
		_dialogueTree.OnStartConversation += OnStartConversation;
	}

	private void SelectMood()
	{
		float num = -1f;
		TextAsset textXml = null;
		for (int i = 0; i < _conditionalDialogues.Length; i++)
		{
			if (TimeLoop.GetMinutesElapsed() >= _conditionalDialogues[i].startMinute && _conditionalDialogues[i].startMinute > num)
			{
				textXml = _conditionalDialogues[i].dialogueTextAsset;
				num = _conditionalDialogues[i].startMinute;
			}
		}
		_dialogueTree.SetTextXml(textXml);
	}

	private void OnStartConversation()
	{
		SelectMood();
	}
}
