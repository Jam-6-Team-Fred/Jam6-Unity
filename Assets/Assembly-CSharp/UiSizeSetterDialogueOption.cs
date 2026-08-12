using UnityEngine;
using UnityEngine.UI;

public class UiSizeSetterDialogueOption : BaseUiSizeSetter
{
	[SerializeField]
	private Text _textField;

	[SerializeField]
	private RectTransform _optionSelectionMarkerTransform;

	[Header("Default Font")]
	[SerializeField]
	private IntBlock _entryFontSizeDefault;

	[SerializeField]
	private FloatBlock _lineSpacingDefault;

	[SerializeField]
	private FloatBlock _yPosSelectionMarkerDefault;

	[Space(10f)]
	[Header("CN Font")]
	[SerializeField]
	private IntBlock _entryFontSizeCn;

	[SerializeField]
	private FloatBlock _lineSpacingCn;

	[SerializeField]
	private FloatBlock _yPosSelectionMarkerCn;

	[Space(10f)]
	[Header("JP Font")]
	[SerializeField]
	private IntBlock _entryFontSizeJp;

	[SerializeField]
	private FloatBlock _lineSpacingJp;

	[SerializeField]
	private FloatBlock _yPosSelectionMarkerJp;

	[Space(10f)]
	[Header("KR Font")]
	[SerializeField]
	private IntBlock _entryFontSizeKr;

	[SerializeField]
	private FloatBlock _lineSpacingKr;

	[SerializeField]
	private FloatBlock _yPosSelectionMarkerKr;

	[Space(10f)]
	[Header("PL/RU Font")]
	[SerializeField]
	private IntBlock _entryFontSizePlRu;

	[SerializeField]
	private FloatBlock _lineSpacingPlRu;

	[SerializeField]
	private FloatBlock _yPosSelectionMarkerPlRu;

	[Space(10f)]
	[Header("TR Font")]
	[SerializeField]
	private IntBlock _entryFontSizeTr;

	[SerializeField]
	private FloatBlock _lineSpacingTr;

	[SerializeField]
	private FloatBlock _yPosSelectionMarkerTr;

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		bool flag = uITextSize == UITextSize.LARGE;
		TextTranslation.Language language = TextTranslation.Get().GetLanguage();
		bool flag2 = TextTranslation.Get().IsLanguageCJK();
		int fontSize;
		float lineSpacing;
		float y;
		switch (language)
		{
		case TextTranslation.Language.POLISH:
		case TextTranslation.Language.RUSSIAN:
			if (flag)
			{
				fontSize = _entryFontSizePlRu.largeVal;
				lineSpacing = _lineSpacingPlRu.largeVal;
				y = _yPosSelectionMarkerPlRu.largeVal;
			}
			else
			{
				fontSize = _entryFontSizePlRu.normalVal;
				lineSpacing = _lineSpacingPlRu.normalVal;
				y = _yPosSelectionMarkerPlRu.normalVal;
			}
			break;
		case TextTranslation.Language.TURKISH:
			if (flag)
			{
				fontSize = _entryFontSizeTr.largeVal;
				lineSpacing = _lineSpacingTr.largeVal;
				y = _yPosSelectionMarkerTr.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeTr.normalVal;
				lineSpacing = _lineSpacingTr.normalVal;
				y = _yPosSelectionMarkerTr.normalVal;
			}
			break;
		case TextTranslation.Language.CHINESE_SIMPLE:
			if (flag)
			{
				fontSize = _entryFontSizeCn.largeVal;
				lineSpacing = _lineSpacingCn.largeVal;
				y = _yPosSelectionMarkerCn.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeCn.normalVal;
				lineSpacing = _lineSpacingCn.normalVal;
				y = _yPosSelectionMarkerCn.normalVal;
			}
			break;
		case TextTranslation.Language.JAPANESE:
			if (flag)
			{
				fontSize = _entryFontSizeJp.largeVal;
				lineSpacing = _lineSpacingJp.largeVal;
				y = _yPosSelectionMarkerJp.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeJp.normalVal;
				lineSpacing = _lineSpacingJp.normalVal;
				y = _yPosSelectionMarkerJp.normalVal;
			}
			break;
		case TextTranslation.Language.KOREAN:
			if (flag)
			{
				fontSize = _entryFontSizeKr.largeVal;
				lineSpacing = _lineSpacingKr.largeVal;
				y = _yPosSelectionMarkerKr.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeKr.normalVal;
				lineSpacing = _lineSpacingKr.normalVal;
				y = _yPosSelectionMarkerKr.normalVal;
			}
			break;
		default:
			if (flag)
			{
				fontSize = _entryFontSizeDefault.largeVal;
				lineSpacing = _lineSpacingDefault.largeVal;
				y = _yPosSelectionMarkerDefault.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeDefault.normalVal;
				lineSpacing = _lineSpacingDefault.normalVal;
				y = _yPosSelectionMarkerDefault.normalVal;
			}
			break;
		}
		_textField.fontSize = fontSize;
		_textField.lineSpacing = lineSpacing;
		Vector2 anchoredPosition = _optionSelectionMarkerTransform.anchoredPosition;
		anchoredPosition.y = y;
		_optionSelectionMarkerTransform.anchoredPosition = anchoredPosition;
		if (flag)
		{
			if (language == TextTranslation.Language.CHINESE_SIMPLE || language == TextTranslation.Language.KOREAN)
			{
				_textField.horizontalOverflow = HorizontalWrapMode.Overflow;
			}
			else
			{
				_textField.horizontalOverflow = HorizontalWrapMode.Wrap;
			}
		}
		else if (flag2)
		{
			_textField.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
		else
		{
			_textField.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
	}
}
