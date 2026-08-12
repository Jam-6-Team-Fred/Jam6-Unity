using UnityEngine;
using UnityEngine.UI;

public class UiSizeSetterShipLogFact : BaseUiSizeSetter
{
	private Text _textField;

	[SerializeField]
	private RectTransform _bulletPointTransform;

	[Space(10f)]
	[SerializeField]
	private IntBlock _fontSizeDefault;

	[SerializeField]
	private FloatBlock _lineSpacingDefault;

	[SerializeField]
	private Vector2Block _bulletPointPositionDefault;

	[Space(10f)]
	[SerializeField]
	private IntBlock _fontSizeCn;

	[SerializeField]
	private FloatBlock _lineSpacingCn;

	[SerializeField]
	private Vector2Block _bulletPointPositionCn;

	[Space(10f)]
	[SerializeField]
	private IntBlock _fontSizeJp;

	[SerializeField]
	private FloatBlock _lineSpacingJp;

	[SerializeField]
	private Vector2Block _bulletPointPositionJp;

	[Space(10f)]
	[SerializeField]
	private IntBlock _fontSizeKr;

	[SerializeField]
	private FloatBlock _lineSpacingKr;

	[SerializeField]
	private Vector2Block _bulletPointPositionKr;

	[Space(10f)]
	[SerializeField]
	private IntBlock _fontSizePlRu;

	[SerializeField]
	private FloatBlock _lineSpacingPlRu;

	[SerializeField]
	private Vector2Block _bulletPointPositionPlRu;

	[Space(10f)]
	[SerializeField]
	private IntBlock _fontSizeTr;

	[SerializeField]
	private FloatBlock _lineSpacingTr;

	[SerializeField]
	private Vector2Block _bulletPointPositionTr;

	protected override void Awake()
	{
		base.Awake();
		_textField = this.GetRequiredComponent<Text>();
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		TextTranslation.Language language = TextTranslation.Get().GetLanguage();
		int num = 0;
		float num2 = 0f;
		Vector2 zero = Vector2.zero;
		if (uITextSize == UITextSize.SMALL)
		{
			switch (language)
			{
			case TextTranslation.Language.CHINESE_SIMPLE:
				num = _fontSizeCn.normalVal;
				num2 = _lineSpacingCn.normalVal;
				zero = _bulletPointPositionCn.normalVal;
				break;
			case TextTranslation.Language.JAPANESE:
				num = _fontSizeJp.normalVal;
				num2 = _lineSpacingJp.normalVal;
				zero = _bulletPointPositionJp.normalVal;
				break;
			case TextTranslation.Language.KOREAN:
				num = _fontSizeKr.normalVal;
				num2 = _lineSpacingKr.normalVal;
				zero = _bulletPointPositionKr.normalVal;
				break;
			case TextTranslation.Language.POLISH:
			case TextTranslation.Language.RUSSIAN:
				num = _fontSizePlRu.normalVal;
				num2 = _lineSpacingPlRu.normalVal;
				zero = _bulletPointPositionPlRu.normalVal;
				break;
			case TextTranslation.Language.TURKISH:
				num = _fontSizeTr.normalVal;
				num2 = _lineSpacingTr.normalVal;
				zero = _bulletPointPositionTr.normalVal;
				break;
			default:
				num = _fontSizeDefault.normalVal;
				num2 = _lineSpacingDefault.normalVal;
				zero = _bulletPointPositionDefault.normalVal;
				break;
			}
		}
		else
		{
			switch (language)
			{
			case TextTranslation.Language.CHINESE_SIMPLE:
				num = _fontSizeCn.largeVal;
				num2 = _lineSpacingCn.largeVal;
				zero = _bulletPointPositionCn.largeVal;
				break;
			case TextTranslation.Language.JAPANESE:
				num = _fontSizeJp.largeVal;
				num2 = _lineSpacingJp.largeVal;
				zero = _bulletPointPositionJp.largeVal;
				break;
			case TextTranslation.Language.KOREAN:
				num = _fontSizeKr.largeVal;
				num2 = _lineSpacingKr.largeVal;
				zero = _bulletPointPositionKr.largeVal;
				break;
			case TextTranslation.Language.POLISH:
			case TextTranslation.Language.RUSSIAN:
				num = _fontSizePlRu.largeVal;
				num2 = _lineSpacingPlRu.largeVal;
				zero = _bulletPointPositionPlRu.largeVal;
				break;
			case TextTranslation.Language.TURKISH:
				num = _fontSizeTr.largeVal;
				num2 = _lineSpacingTr.largeVal;
				zero = _bulletPointPositionTr.largeVal;
				break;
			default:
				num = _fontSizeDefault.largeVal;
				num2 = _lineSpacingDefault.largeVal;
				zero = _bulletPointPositionDefault.largeVal;
				break;
			}
		}
		_textField.fontSize = num;
		_textField.lineSpacing = num2;
		_bulletPointTransform.anchoredPosition = zero;
	}
}
