using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialogueBoxVer2 : MonoBehaviour
{
	private Canvas _canvas;

	[SerializeField]
	private LayoutGroup _mainLayoutRoot;

	[SerializeField]
	private Text _mainTextField;

	[SerializeField]
	private Text _nameTextField;

	[SerializeField]
	private GameObject _nameBox;

	[SerializeField]
	private GameObject _optionBox;

	[SerializeField]
	private VerticalLayoutGroup _optionsLayoutRoot;

	[SerializeField]
	private int _defaultOptionsLayoutSpacing;

	[SerializeField]
	private int _defaultOptionsLayoutSpacingLrg = 10;

	[FormerlySerializedAs("_cjkOptionsLayoutSpacing")]
	[SerializeField]
	private int _cnKrOptionsLayoutSpacing = 20;

	[SerializeField]
	private int _jpOptionsLayoutSpacing = 30;

	[SerializeField]
	private bool _turnOnNameField;

	[SerializeField]
	private GameObject _buttonPromptElement;

	[SerializeField]
	private Image _continueSprite;

	[SerializeField]
	private RectTransform _backgroundImageTransform;

	[SerializeField]
	private float _backgroundImageTopBottomMargin;

	[SerializeField]
	private float _backgroundImageLeftRightMargin;

	[Space(10f)]
	[SerializeField]
	private Font _defaultDialogueFont;

	[SerializeField]
	private Font _defaultDialogueFontDynamic;

	private Font _fontInUse;

	private Font _dynamicFontInUse;

	private float _fontSpacingInUse;

	private int _fontSizeInUse;

	private int _fontSizeOptionsInUse;

	private List<DialogueOptionUI> _optionsUIElements;

	private List<DialogueOption> _potentialOptions;

	private List<DialogueOption> _displayedOptions;

	private int _selectedOption;

	private TypeEffectText _mainFieldTextEffect;

	private List<TypeEffectText> _optionsTextEffectList;

	private bool _revealingOptions;

	private float _timeOnLastOptionReveal;

	private const float c_timeBetweenOptionReveal = 0.05f;

	private float _timeFullyRevealed;

	private const float c_maxTrackableRevealTime = 5f;

	private Sprite _dialogueInteractPromptSprite;

	private void Start()
	{
		InitializeOptionsUI();
		_canvas = this.GetRequiredComponent<Canvas>();
		_optionBox.SetActive(value: false);
		SetOptionsVisible(value: false);
		SetVisible(value: false);
		_displayedOptions = new List<DialogueOption>();
		_optionsTextEffectList = new List<TypeEffectText>();
		InitializeFont();
		ResizeBackgroundImage(resizeWidth: true);
		UpdatePromptImage();
		TextTranslation.Get().OnLanguageChanged += InitializeFont;
		Canvas.willRenderCanvases += OnWillRenderCanvases;
		PromptManager.OnUpdatePromptImages += UpdatePromptImage;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig += UpdatePromptImage;
		OWInput.SharedInputManager.OnUpdateInputCommands += UpdatePromptImage;
	}

	private void ResizeBackgroundImage(bool resizeWidth = false)
	{
		Vector2 sizeDelta = _backgroundImageTransform.sizeDelta;
		float num = _mainLayoutRoot.preferredHeight + _backgroundImageTopBottomMargin * 2f;
		if (sizeDelta.y != num)
		{
			sizeDelta.y = num;
			_backgroundImageTransform.sizeDelta = sizeDelta;
		}
		if (resizeWidth)
		{
			float num2 = _backgroundImageLeftRightMargin * 2f;
			if (sizeDelta.x != num2)
			{
				sizeDelta.x = num2;
				_backgroundImageTransform.sizeDelta = sizeDelta;
			}
		}
	}

	private void OnDestroy()
	{
		TextTranslation.Get().OnLanguageChanged -= InitializeFont;
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
		PromptManager.OnUpdatePromptImages -= UpdatePromptImage;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig -= UpdatePromptImage;
		OWInput.SharedInputManager.OnUpdateInputCommands -= UpdatePromptImage;
	}

	private void InitializeFont()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			_fontInUse = _defaultDialogueFont;
			_dynamicFontInUse = _defaultDialogueFontDynamic;
		}
		else
		{
			_fontInUse = TextTranslation.GetFont();
			_dynamicFontInUse = TextTranslation.GetFont(dynamicFont: true);
		}
		_mainTextField.font = _fontInUse;
		_nameTextField.font = _fontInUse;
		_optionBox.GetRequiredComponent<DialogueOptionUI>().textElement.font = _fontInUse;
	}

	private void UpdatePromptImage()
	{
		Texture2D texture2D = InputLibrary.interact.GetUITextures(OWInput.UsingGamepad(), forceRefresh: true)[0];
		_dialogueInteractPromptSprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), texture2D.width);
	}

	private void OnWillRenderCanvases()
	{
		bool flag = false;
		if (_revealingOptions && _mainFieldTextEffect.IsTextEffectComplete())
		{
			RevealOption();
			if (AreOptionsRevealed())
			{
				CompleteOptionsReveal();
			}
		}
		if (AreTextEffectsComplete())
		{
			if (_timeFullyRevealed < 5f)
			{
				_timeFullyRevealed += Time.unscaledDeltaTime;
				if (_timeFullyRevealed > 5f)
				{
					_timeFullyRevealed = 5f;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			ResizeBackgroundImage();
		}
	}

	public void SetVisible(bool value)
	{
		_canvas.enabled = value;
	}

	private void SetOptionsVisible(bool value)
	{
		for (int i = 0; i < _optionsUIElements.Count; i++)
		{
			_optionsUIElements[i].gameObject.SetActive(value);
		}
	}

	public void SetNameFieldVisible(bool value)
	{
		if (_turnOnNameField)
		{
			_nameBox.SetActive(value);
		}
		else
		{
			_nameBox.SetActive(value: false);
		}
	}

	public void SetNameField(string name)
	{
		_nameTextField.text = name;
	}

	private void ResetOptionsBoxes()
	{
		_displayedOptions.Clear();
		InitializeOptionsUI();
	}

	private void InitializeOptionsUI()
	{
		if (_optionsUIElements == null)
		{
			_optionsUIElements = new List<DialogueOptionUI>();
		}
		else
		{
			for (int i = 0; i < _optionsUIElements.Count; i++)
			{
				Object.Destroy(_optionsUIElements[i].gameObject);
			}
			_optionsUIElements.Clear();
		}
		_selectedOption = -1;
		if (TextTranslation.Get().IsLanguageCJK())
		{
			if (TextTranslation.Get().GetLanguage() == TextTranslation.Language.JAPANESE)
			{
				_optionsLayoutRoot.spacing = _jpOptionsLayoutSpacing;
			}
			else
			{
				_optionsLayoutRoot.spacing = _cnKrOptionsLayoutSpacing;
			}
		}
		else if (PlayerData.IsUILargeTextSize())
		{
			_optionsLayoutRoot.spacing = _defaultOptionsLayoutSpacingLrg;
		}
		else
		{
			_optionsLayoutRoot.spacing = _defaultOptionsLayoutSpacing;
		}
	}

	private void ResetAllText()
	{
		_displayedOptions.Clear();
		ResetOptionsBoxes();
		SetOptionsVisible(value: false);
		SetVisible(value: true);
		_optionsTextEffectList.Clear();
		_timeFullyRevealed = 0f;
	}

	private void SetMainFieldDialogueText(string richText)
	{
		if (richText.Contains("</i>") || richText.Contains("</b>") || richText.Contains("</u>") || richText.Contains("</size>"))
		{
			_mainTextField.font = _dynamicFontInUse;
		}
		else
		{
			_mainTextField.font = _fontInUse;
		}
		_mainTextField.text = richText;
		TypeEffectText component = _mainTextField.GetComponent<TypeEffectText>();
		if (component != null)
		{
			component.Init(richText, TextAnchor.MiddleLeft);
			_mainTextField.text = string.Empty;
		}
		_mainFieldTextEffect = component;
	}

	public void SetDialogueText(string richText, List<DialogueOption> listOptions)
	{
		_potentialOptions = listOptions;
		ResetAllText();
		SetMainFieldDialogueText(richText);
		Image requiredComponent = _continueSprite.transform.GetRequiredComponent<Image>();
		if (requiredComponent != null)
		{
			requiredComponent.sprite = _dialogueInteractPromptSprite;
		}
		_buttonPromptElement.gameObject.SetActive(value: false);
		if (_mainFieldTextEffect != null)
		{
			_mainFieldTextEffect.StartTextEffect();
		}
		if (_potentialOptions.Count == 0)
		{
			_buttonPromptElement.gameObject.SetActive(value: true);
			return;
		}
		SetUpOptions();
		_revealingOptions = true;
	}

	private void SetUpOptions()
	{
		if (_potentialOptions == null)
		{
			return;
		}
		SetOptionsVisible(value: true);
		int count = _potentialOptions.Count;
		for (int i = 0; i < count; i++)
		{
			bool flag = true;
			DialogueOption dialogueOption = _potentialOptions[i];
			if (dialogueOption.ConditionRequirement != string.Empty && !DialogueConditionManager.SharedInstance.GetConditionState(dialogueOption.ConditionRequirement))
			{
				flag = false;
			}
			for (int j = 0; j < dialogueOption.PersistentCondition.Count; j++)
			{
				if (!PlayerData.GetPersistentCondition(dialogueOption.PersistentCondition[j]))
				{
					flag = false;
				}
			}
			for (int k = 0; k < dialogueOption.LogConditionRequirement.Count; k++)
			{
				if (Locator.GetShipLogManager().GetFact(dialogueOption.LogConditionRequirement[k]) == null)
				{
					Debug.LogError("Trying to check ship log " + dialogueOption.LogConditionRequirement[k] + " which does not exist.");
				}
				else if (!Locator.GetShipLogManager().GetFact(dialogueOption.LogConditionRequirement[k]).IsRevealed())
				{
					flag = false;
				}
			}
			if (dialogueOption.CancelledRequirement != string.Empty && DialogueConditionManager.SharedInstance.GetConditionState(dialogueOption.CancelledRequirement))
			{
				flag = false;
			}
			for (int l = 0; l < dialogueOption.CancelledPersistentRequirement.Count; l++)
			{
				if (PlayerData.GetPersistentCondition(dialogueOption.CancelledPersistentRequirement[l]))
				{
					flag = false;
				}
			}
			if (flag)
			{
				GameObject obj = Object.Instantiate(_optionBox, _optionsLayoutRoot.transform);
				obj.SetActive(value: true);
				obj.transform.SetParent(_optionBox.transform.parent, worldPositionStays: false);
				DialogueOptionUI requiredComponent = obj.GetRequiredComponent<DialogueOptionUI>();
				Graphic[] componentsInChildren = obj.GetComponentsInChildren<Graphic>();
				if (dialogueOption.Text.Contains("</i>") || dialogueOption.Text.Contains("</b>") || dialogueOption.Text.Contains("</u>") || dialogueOption.Text.Contains("</size>"))
				{
					requiredComponent.textElement.font = _dynamicFontInUse;
				}
				else
				{
					requiredComponent.textElement.font = _fontInUse;
				}
				requiredComponent.textElement.text = dialogueOption.Text;
				requiredComponent.SetSelected(isSelected: false);
				_optionsUIElements.Add(requiredComponent);
				_displayedOptions.Add(dialogueOption);
				TypeEffectText component = requiredComponent.textElement.GetComponent<TypeEffectText>();
				if (component != null)
				{
					component.Init(dialogueOption.Text, TextAnchor.MiddleLeft);
					_optionsTextEffectList.Add(component);
				}
				for (int m = 0; m < componentsInChildren.Length; m++)
				{
					componentsInChildren[m].SetLayoutDirty();
				}
			}
		}
	}

	private void RevealOption()
	{
		if (!(Time.unscaledTime - _timeOnLastOptionReveal > 0.05f))
		{
			return;
		}
		for (int i = 0; i < _optionsTextEffectList.Count; i++)
		{
			if (!_optionsTextEffectList[i].IsTextEffectComplete())
			{
				_optionsTextEffectList[i].SetTextWithNoEffect();
				_timeOnLastOptionReveal = Time.unscaledTime;
				break;
			}
		}
	}

	private bool AreOptionsRevealed()
	{
		for (int i = 0; i < _optionsTextEffectList.Count; i++)
		{
			if (!_optionsTextEffectList[i].IsTextEffectComplete())
			{
				return false;
			}
		}
		return true;
	}

	private void CompleteOptionsReveal()
	{
		_timeOnLastOptionReveal = 0f;
		_revealingOptions = false;
		if (_optionsUIElements.Count > 0)
		{
			_selectedOption = 0;
			HighlightSelectedOption();
		}
	}

	public float TimeCompletelyRevealed()
	{
		return _timeFullyRevealed;
	}

	public bool AreTextEffectsComplete()
	{
		if (_mainFieldTextEffect != null && !_mainFieldTextEffect.IsTextEffectComplete())
		{
			return false;
		}
		for (int i = 0; i < _optionsTextEffectList.Count; i++)
		{
			if (!_optionsTextEffectList[i].IsTextEffectComplete())
			{
				return false;
			}
		}
		return true;
	}

	public void FinishAllTextEffects()
	{
		if (_mainFieldTextEffect != null)
		{
			_mainFieldTextEffect.CompleteTextEffect();
		}
		for (int i = 0; i < _optionsTextEffectList.Count; i++)
		{
			_optionsTextEffectList[i].CompleteTextEffect();
		}
	}

	public void OnUpPressed()
	{
		if (!_revealingOptions && _optionsUIElements.Count > 0)
		{
			int selectedOption = _selectedOption;
			if (_selectedOption - 1 > -1)
			{
				_selectedOption--;
			}
			else
			{
				_selectedOption = _optionsUIElements.Count - 1;
			}
			if (selectedOption != _selectedOption)
			{
				Locator.GetPlayerAudioController().PlayDialogueHighlightOption();
			}
			HighlightSelectedOption(selectedOption);
		}
	}

	public void OnDownPressed()
	{
		if (!_revealingOptions && _optionsUIElements.Count > 0)
		{
			int selectedOption = _selectedOption;
			if (_selectedOption + 1 < _optionsUIElements.Count)
			{
				_selectedOption++;
			}
			else
			{
				_selectedOption = 0;
			}
			if (selectedOption != _selectedOption)
			{
				Locator.GetPlayerAudioController().PlayDialogueHighlightOption();
			}
			HighlightSelectedOption(selectedOption);
		}
	}

	private void HighlightSelectedOption(int previousOption = -1)
	{
		if (previousOption == -1)
		{
			for (int i = 0; i < _optionsUIElements.Count; i++)
			{
				_optionsUIElements[i].SetSelected(isSelected: false);
			}
		}
		else
		{
			_optionsUIElements[previousOption].SetSelected(isSelected: false);
		}
		_optionsUIElements[_selectedOption].SetButtonPromptImage(_dialogueInteractPromptSprite);
		_optionsUIElements[_selectedOption].SetSelected(isSelected: true);
	}

	public int GetSelectedOption()
	{
		return _selectedOption;
	}

	public DialogueOption OptionFromUIIndex(int index)
	{
		return _displayedOptions[index];
	}

	public void OnEndDialogue()
	{
		InitializeOptionsUI();
		SetVisible(value: false);
	}
}
