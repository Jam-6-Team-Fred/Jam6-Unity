using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DialogueOptionUI : MonoBehaviour
{
	[SerializeField]
	private Image _buttonPromptImage;

	[SerializeField]
	private Image _selectedOptionImage;

	[SerializeField]
	private Text _optionText;

	public Text textElement => _optionText;

	public void SetButtonPromptImage(Sprite s)
	{
		_buttonPromptImage.sprite = s;
	}

	public void SetSelected(bool isSelected)
	{
		_buttonPromptImage.enabled = isSelected;
		_selectedOptionImage.enabled = isSelected;
	}
}
