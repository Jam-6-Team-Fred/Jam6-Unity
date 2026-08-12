using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TooltipDisplay : MonoBehaviour
{
	private Text _textDisplay;

	private void Awake()
	{
		_textDisplay = this.GetRequiredComponent<Text>();
		_textDisplay.text = "";
	}

	public void SetTooltipText(string text)
	{
		_textDisplay.text = text;
	}
}
