using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
	[SerializeField]
	protected UITextType _textID;

	protected Text text;

	protected bool _languageUpdateEnabled;

	protected virtual void Start()
	{
		UpdateLanguage();
		if (!_languageUpdateEnabled)
		{
			TextTranslation.Get().OnLanguageChanged += UpdateLanguage;
			_languageUpdateEnabled = true;
		}
	}

	protected virtual void OnDestroy()
	{
		if (_languageUpdateEnabled)
		{
			_languageUpdateEnabled = false;
			TextTranslation.Get().OnLanguageChanged -= UpdateLanguage;
		}
	}

	protected virtual void UpdateLanguage()
	{
		if (text == null)
		{
			text = GetComponent<Text>();
		}
		text.text = UITextLibrary.GetString(_textID);
		text.SetAllDirty();
	}

	public void SetTextID(UITextType textID)
	{
		if (_textID != textID)
		{
			_textID = textID;
			UpdateLanguage();
		}
	}

	public void EnableLanguageUpdate(bool enable)
	{
		if (_languageUpdateEnabled != enable)
		{
			_languageUpdateEnabled = enable;
			if (_languageUpdateEnabled)
			{
				TextTranslation.Get().OnLanguageChanged += UpdateLanguage;
			}
			else
			{
				TextTranslation.Get().OnLanguageChanged -= UpdateLanguage;
			}
		}
	}
}
