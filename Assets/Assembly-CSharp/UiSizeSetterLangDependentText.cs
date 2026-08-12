using UnityEngine;
using UnityEngine.UI;

public class UiSizeSetterLangDependentText : BaseUiSizeSetter
{
	private Text _textField;

	[SerializeField]
	private bool _shouldUpdateFonts;

	[Header("Default Font")]
	[SerializeField]
	private IntBlock _entryFontSizeDefault;

	[SerializeField]
	private FloatBlock _lineSpacingDefault;

	[SerializeField]
	private BoolBlock _enableHorzOverflowDefault;

	[Space(10f)]
	[Header("CN Font")]
	[SerializeField]
	private IntBlock _entryFontSizeCn;

	[SerializeField]
	private FloatBlock _lineSpacingCn;

	[SerializeField]
	private BoolBlock _enableHorzOverflowCn;

	[Space(10f)]
	[Header("JP Font")]
	[SerializeField]
	private IntBlock _entryFontSizeJp;

	[SerializeField]
	private FloatBlock _lineSpacingJp;

	[SerializeField]
	private BoolBlock _enableHorzOverflowJp;

	[Space(10f)]
	[Header("KR Font")]
	[SerializeField]
	private IntBlock _entryFontSizeKr;

	[SerializeField]
	private FloatBlock _lineSpacingKr;

	[SerializeField]
	private BoolBlock _enableHorzOverflowKr;

	[Space(10f)]
	[Header("PL/RU Font")]
	[SerializeField]
	private IntBlock _entryFontSizePlRu;

	[SerializeField]
	private FloatBlock _lineSpacingPlRu;

	[SerializeField]
	private BoolBlock _enableHorzOverflowPlRu;

	[Space(10f)]
	[Header("TR Font")]
	[SerializeField]
	private IntBlock _entryFontSizeTr;

	[SerializeField]
	private FloatBlock _lineSpacingTr;

	[SerializeField]
	private BoolBlock _enableHorzOverflowTr;

	private Font _originalFont;

	protected override void Awake()
	{
		base.Awake();
		_textField = this.GetRequiredComponent<Text>();
		_originalFont = _textField.font;
	}

	protected override void Start()
	{
		base.Start();
		if (_shouldUpdateFonts)
		{
			InitializeFont();
			TextTranslation.Get().OnLanguageChanged += InitializeFont;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_shouldUpdateFonts)
		{
			TextTranslation.Get().OnLanguageChanged -= InitializeFont;
		}
	}

	private void InitializeFont()
	{
		Font font = ((!TextTranslation.Get().IsLanguageLatin()) ? TextTranslation.GetFont(_originalFont.dynamic) : _originalFont);
		_textField.font = font;
		DoResizeAction(PlayerData.GetTextSize());
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		bool flag = uITextSize == UITextSize.LARGE;
		int fontSize;
		float lineSpacing;
		bool flag2;
		switch (TextTranslation.Get().GetLanguage())
		{
		case TextTranslation.Language.POLISH:
		case TextTranslation.Language.RUSSIAN:
			if (flag)
			{
				fontSize = _entryFontSizePlRu.largeVal;
				lineSpacing = _lineSpacingPlRu.largeVal;
				flag2 = _enableHorzOverflowPlRu.largeVal;
			}
			else
			{
				fontSize = _entryFontSizePlRu.normalVal;
				lineSpacing = _lineSpacingPlRu.normalVal;
				flag2 = _enableHorzOverflowPlRu.normalVal;
			}
			break;
		case TextTranslation.Language.TURKISH:
			if (flag)
			{
				fontSize = _entryFontSizeTr.largeVal;
				lineSpacing = _lineSpacingTr.largeVal;
				flag2 = _enableHorzOverflowTr.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeTr.normalVal;
				lineSpacing = _lineSpacingTr.normalVal;
				flag2 = _enableHorzOverflowTr.normalVal;
			}
			break;
		case TextTranslation.Language.CHINESE_SIMPLE:
			if (flag)
			{
				fontSize = _entryFontSizeCn.largeVal;
				lineSpacing = _lineSpacingCn.largeVal;
				flag2 = _enableHorzOverflowCn.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeCn.normalVal;
				lineSpacing = _lineSpacingCn.normalVal;
				flag2 = _enableHorzOverflowCn.normalVal;
			}
			break;
		case TextTranslation.Language.JAPANESE:
			if (flag)
			{
				fontSize = _entryFontSizeJp.largeVal;
				lineSpacing = _lineSpacingJp.largeVal;
				flag2 = _enableHorzOverflowJp.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeJp.normalVal;
				lineSpacing = _lineSpacingJp.normalVal;
				flag2 = _enableHorzOverflowJp.normalVal;
			}
			break;
		case TextTranslation.Language.KOREAN:
			if (flag)
			{
				fontSize = _entryFontSizeKr.largeVal;
				lineSpacing = _lineSpacingKr.largeVal;
				flag2 = _enableHorzOverflowKr.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeKr.normalVal;
				lineSpacing = _lineSpacingKr.normalVal;
				flag2 = _enableHorzOverflowKr.normalVal;
			}
			break;
		default:
			if (flag)
			{
				fontSize = _entryFontSizeDefault.largeVal;
				lineSpacing = _lineSpacingDefault.largeVal;
				flag2 = _enableHorzOverflowDefault.largeVal;
			}
			else
			{
				fontSize = _entryFontSizeDefault.normalVal;
				lineSpacing = _lineSpacingDefault.normalVal;
				flag2 = _enableHorzOverflowDefault.normalVal;
			}
			break;
		}
		_textField.fontSize = fontSize;
		_textField.lineSpacing = lineSpacing;
		_textField.horizontalOverflow = (flag2 ? HorizontalWrapMode.Overflow : HorizontalWrapMode.Wrap);
	}
}
