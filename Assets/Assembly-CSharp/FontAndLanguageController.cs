using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FontAndLanguageController : MonoBehaviour
{
	[Serializable]
	protected class TextItemsRootObject
	{
		public GameObject rootObj;

		public GameObject[] excludeObj;

		public bool controlScale;

		public bool useDefaultLineSpacing = true;

		public bool isLanguageFont;
	}

	[Serializable]
	protected class TextItem
	{
		public Text textElement;

		public bool controlScale;

		public bool useDefaultLineSpacing = true;

		public bool isLanguageFont;
	}

	[Serializable]
	protected struct TextContainer
	{
		public Text textElement;

		public bool shouldScale;

		public bool useDefaultLineSpacing;

		public bool isLanguageFont;

		public Font originalFont;

		public float originalSpacing;

		public int originalFontSize;

		public Vector3 originalScale;

		public Vector2 originalSizeDelta;

		public bool markForRemoval;
	}

	[SerializeField]
	protected List<TextItemsRootObject> _rootObjectsWithTextList;

	[SerializeField]
	protected List<TextItem> _textItemList;

	protected List<TextContainer> _textContainerList;

	protected virtual void Awake()
	{
		InitializeDefaults();
	}

	protected virtual void Start()
	{
		InitializeFont();
		TextTranslation.Get().OnLanguageChanged += InitializeFont;
	}

	protected virtual void OnDestroy()
	{
		TextTranslation.Get().OnLanguageChanged -= InitializeFont;
	}

	protected virtual void InitializeDefaults()
	{
		_textContainerList = new List<TextContainer>();
		for (int i = 0; i < _textItemList.Count; i++)
		{
			TextContainer item = default(TextContainer);
			item.textElement = _textItemList[i].textElement;
			item.shouldScale = _textItemList[i].controlScale;
			item.useDefaultLineSpacing = _textItemList[i].useDefaultLineSpacing;
			item.isLanguageFont = _textItemList[i].isLanguageFont;
			item.originalFont = _textItemList[i].textElement.font;
			item.originalSpacing = _textItemList[i].textElement.lineSpacing;
			item.originalFontSize = _textItemList[i].textElement.fontSize;
			item.originalScale = _textItemList[i].textElement.rectTransform.localScale;
			item.originalSizeDelta = _textItemList[i].textElement.rectTransform.sizeDelta;
			item.markForRemoval = false;
			_textContainerList.Add(item);
		}
		for (int j = 0; j < _rootObjectsWithTextList.Count; j++)
		{
			List<Text> list = new List<Text>();
			_rootObjectsWithTextList[j].rootObj.GetComponentsInChildren(includeInactive: true, list);
			List<TextContainer> list2 = new List<TextContainer>();
			for (int k = 0; k < list.Count; k++)
			{
				TextContainer item2 = default(TextContainer);
				item2.textElement = list[k];
				item2.shouldScale = _rootObjectsWithTextList[j].controlScale;
				item2.useDefaultLineSpacing = _rootObjectsWithTextList[j].useDefaultLineSpacing;
				item2.isLanguageFont = _rootObjectsWithTextList[j].isLanguageFont;
				item2.originalFont = list[k].font;
				item2.originalSpacing = list[k].lineSpacing;
				item2.originalFontSize = list[k].fontSize;
				item2.originalScale = list[k].rectTransform.localScale;
				item2.originalSizeDelta = list[k].rectTransform.sizeDelta;
				item2.markForRemoval = false;
				list2.Add(item2);
			}
			for (int l = 0; l < _rootObjectsWithTextList[j].excludeObj.Length; l++)
			{
				Transform parent = _rootObjectsWithTextList[j].excludeObj[l].transform;
				for (int m = 0; m < list2.Count; m++)
				{
					if (list2[m].textElement.transform.IsChildOf(parent))
					{
						TextContainer value = list2[m];
						value.markForRemoval = true;
						list2[m] = value;
					}
				}
			}
			list2.RemoveAll((TextContainer textContainer) => textContainer.markForRemoval);
			_textContainerList.AddRange(list2);
		}
	}

	protected virtual void InitializeFont()
	{
		bool flag = TextTranslation.Get().IsLanguageLatin();
		for (int i = 0; i < _textContainerList.Count; i++)
		{
			TextStyleApplier component = _textContainerList[i].textElement.GetComponent<TextStyleApplier>();
			if (_textContainerList[i].isLanguageFont)
			{
				Font languageFont = TextTranslation.GetLanguageFont();
				if (_textContainerList[i].originalFont == languageFont)
				{
					_textContainerList[i].textElement.font = languageFont;
					_textContainerList[i].textElement.lineSpacing = _textContainerList[i].originalSpacing;
					_textContainerList[i].textElement.fontSize = TextTranslation.GetModifiedFontSize(_textContainerList[i].originalFontSize);
					_textContainerList[i].textElement.rectTransform.localScale = _textContainerList[i].originalScale;
					_textContainerList[i].textElement.rectTransform.sizeDelta = _textContainerList[i].originalSizeDelta;
				}
				else
				{
					int modifiedFontSize = TextTranslation.GetModifiedFontSize(languageFont.fontSize);
					_textContainerList[i].textElement.font = languageFont;
					_textContainerList[i].textElement.lineSpacing = TextTranslation.GetDefaultFontSpacing();
					if (_textContainerList[i].shouldScale)
					{
						_textContainerList[i].textElement.fontSize = modifiedFontSize;
						Vector3 localScale = _textContainerList[i].originalScale * ((float)_textContainerList[i].originalFontSize / (float)modifiedFontSize);
						_textContainerList[i].textElement.rectTransform.localScale = localScale;
						_textContainerList[i].textElement.rectTransform.sizeDelta = new Vector2(_textContainerList[i].originalSizeDelta.x * _textContainerList[i].originalScale.x / localScale.x, _textContainerList[i].originalSizeDelta.y * _textContainerList[i].originalScale.y / localScale.y);
					}
					else
					{
						_textContainerList[i].textElement.fontSize = TextTranslation.GetModifiedFontSize(_textContainerList[i].originalFontSize);
					}
					if (_textContainerList[i].useDefaultLineSpacing)
					{
						_textContainerList[i].textElement.lineSpacing = TextTranslation.GetDefaultFontSpacing();
					}
					else
					{
						_textContainerList[i].textElement.lineSpacing = _textContainerList[i].originalSpacing;
					}
				}
			}
			else if (flag)
			{
				_textContainerList[i].textElement.font = _textContainerList[i].originalFont;
				_textContainerList[i].textElement.lineSpacing = _textContainerList[i].originalSpacing;
				_textContainerList[i].textElement.fontSize = _textContainerList[i].originalFontSize;
				_textContainerList[i].textElement.rectTransform.localScale = _textContainerList[i].originalScale;
				_textContainerList[i].textElement.rectTransform.sizeDelta = _textContainerList[i].originalSizeDelta;
			}
			else
			{
				Font font = TextTranslation.GetFont(_textContainerList[i].originalFont.dynamic);
				if (_textContainerList[i].originalFont == font)
				{
					_textContainerList[i].textElement.font = font;
					_textContainerList[i].textElement.lineSpacing = _textContainerList[i].originalSpacing;
					_textContainerList[i].textElement.fontSize = TextTranslation.GetModifiedFontSize(_textContainerList[i].originalFontSize);
					_textContainerList[i].textElement.rectTransform.localScale = _textContainerList[i].originalScale;
					_textContainerList[i].textElement.rectTransform.sizeDelta = _textContainerList[i].originalSizeDelta;
				}
				else if (font.dynamic)
				{
					_textContainerList[i].textElement.fontSize = TextTranslation.GetModifiedFontSize(_textContainerList[i].originalFontSize);
					_textContainerList[i].textElement.rectTransform.localScale = _textContainerList[i].originalScale;
					_textContainerList[i].textElement.rectTransform.sizeDelta = _textContainerList[i].originalSizeDelta;
					_textContainerList[i].textElement.font = font;
					if (_textContainerList[i].useDefaultLineSpacing)
					{
						_textContainerList[i].textElement.lineSpacing = TextTranslation.GetDefaultFontSpacing();
					}
					else
					{
						_textContainerList[i].textElement.lineSpacing = _textContainerList[i].originalSpacing;
					}
				}
				else
				{
					int modifiedFontSize2 = TextTranslation.GetModifiedFontSize(font.fontSize);
					_textContainerList[i].textElement.font = font;
					_textContainerList[i].textElement.lineSpacing = TextTranslation.GetDefaultFontSpacing();
					if (_textContainerList[i].shouldScale)
					{
						_textContainerList[i].textElement.fontSize = modifiedFontSize2;
						Vector3 localScale2 = _textContainerList[i].originalScale * ((float)_textContainerList[i].originalFontSize / (float)modifiedFontSize2);
						_textContainerList[i].textElement.rectTransform.localScale = localScale2;
						_textContainerList[i].textElement.rectTransform.sizeDelta = new Vector2(_textContainerList[i].originalSizeDelta.x * _textContainerList[i].originalScale.x / localScale2.x, _textContainerList[i].originalSizeDelta.y * _textContainerList[i].originalScale.y / localScale2.y);
					}
					else
					{
						_textContainerList[i].textElement.fontSize = TextTranslation.GetModifiedFontSize(_textContainerList[i].originalFontSize);
					}
					if (_textContainerList[i].useDefaultLineSpacing)
					{
						_textContainerList[i].textElement.lineSpacing = TextTranslation.GetDefaultFontSpacing();
					}
					else
					{
						_textContainerList[i].textElement.lineSpacing = _textContainerList[i].originalSpacing;
					}
				}
			}
			if (component != null)
			{
				component.font = _textContainerList[i].textElement.font;
				if (!TextTranslation.Get().IsLanguageLatin() && TextTranslation.Get().GetLanguage() != TextTranslation.Language.RUSSIAN && TextTranslation.Get().GetLanguage() != TextTranslation.Language.POLISH && TextTranslation.Get().GetLanguage() != TextTranslation.Language.TURKISH)
				{
					component.fixedWidth = _textContainerList[i].textElement.font.fontSize;
				}
				else
				{
					component.fixedWidth = 0f;
				}
			}
			_textContainerList[i].textElement.SetAllDirty();
		}
	}

	public virtual void AddTextElement(Text textElement, bool rescale = true, bool useDefaultLineSpacing = true, bool isDynamicFont = false)
	{
		for (int i = 0; i < _textContainerList.Count; i++)
		{
			if (_textContainerList[i].textElement == textElement)
			{
				return;
			}
		}
		TextContainer item = default(TextContainer);
		item.textElement = textElement;
		item.shouldScale = rescale;
		item.useDefaultLineSpacing = useDefaultLineSpacing;
		item.isLanguageFont = isDynamicFont;
		item.originalFont = textElement.font;
		item.originalSpacing = textElement.lineSpacing;
		item.originalFontSize = textElement.fontSize;
		item.originalScale = textElement.rectTransform.localScale;
		item.originalSizeDelta = textElement.rectTransform.sizeDelta;
		_textContainerList.Add(item);
		InitializeFont();
	}

	public virtual void RemoveTextElement(Text textElement)
	{
		int num = -1;
		for (int i = 0; i < _textContainerList.Count; i++)
		{
			if (_textContainerList[i].textElement == textElement)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			_textContainerList.RemoveAt(num);
		}
	}
}
