using UnityEngine;

public class MuseumDialogueSwapper : MonoBehaviour
{
	[SerializeField]
	private CharacterDialogueTree _dialogue;

	[SerializeField]
	private TextAsset _xmlToSwapWith;

	[SerializeField]
	private string _LogID;

	private void Start()
	{
		if (Locator.GetShipLogManager().GetFact(_LogID) == null)
		{
			Debug.LogError("Trying to check ship log " + _LogID + " which does not exist.");
		}
		else if (Locator.GetShipLogManager().GetFact(_LogID).IsRevealed())
		{
			_dialogue.SetTextXml(_xmlToSwapWith);
		}
	}
}
