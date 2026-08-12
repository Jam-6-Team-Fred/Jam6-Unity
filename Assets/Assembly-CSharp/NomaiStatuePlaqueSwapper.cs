using UnityEngine;

public class NomaiStatuePlaqueSwapper : MonoBehaviour
{
	[SerializeField]
	private TextAsset _exploredCometText;

	private CharacterDialogueTree _dialogue;

	private void Start()
	{
		_dialogue = GetComponent<CharacterDialogueTree>();
		if (Locator.GetShipLogManager().IsFactRevealed("COMET_INTERIOR_X1"))
		{
			_dialogue.SetTextXml(_exploredCometText);
		}
	}
}
