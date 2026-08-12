using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ShipLogEntryListItem))]
public class UiSizeSetterShipLogEntry : BaseUiSizeSetter
{
	private ShipLogEntryListItem _shipLogEntryListItem;

	private LayoutElement _listItemLayoutElement;

	private Text _textField;

	[SerializeField]
	private RectTransform _bulletPointTransform;

	[Space(10f)]
	[Header("Default Font")]
	[SerializeField]
	private IntBlock _entryFontSizeDefault;

	[SerializeField]
	private IntBlock _subEntryFontSizeDefault;

	[SerializeField]
	private FloatBlock _minRootObjLayoutHeightDefault;

	[Space(10f)]
	[Header("CN Font")]
	[SerializeField]
	private IntBlock _entryFontSizeCn;

	[SerializeField]
	private IntBlock _subEntryFontSizeCn;

	[SerializeField]
	private FloatBlock _minRootObjLayoutHeightCn;

	[Space(10f)]
	[Header("JP Font")]
	[SerializeField]
	private IntBlock _entryFontSizeJp;

	[SerializeField]
	private IntBlock _subEntryFontSizeJp;

	[SerializeField]
	private FloatBlock _minRootObjLayoutHeightJp;

	[Space(10f)]
	[Header("KR Font")]
	[SerializeField]
	private IntBlock _entryFontSizeKr;

	[SerializeField]
	private IntBlock _subEntryFontSizeKr;

	[SerializeField]
	private FloatBlock _minRootObjLayoutHeightKr;

	[Space(10f)]
	[Header("PL/RU Font")]
	[SerializeField]
	private IntBlock _entryFontSizePlRu;

	[SerializeField]
	private IntBlock _subEntryFontSizePlRu;

	[SerializeField]
	private FloatBlock _minRootObjLayoutHeightPlRu;

	[Space(10f)]
	[Header("TR Font")]
	[SerializeField]
	private IntBlock _entryFontSizeTr;

	[SerializeField]
	private IntBlock _subEntryFontSizeTr;

	[SerializeField]
	private FloatBlock _minRootObjLayoutHeightTr;

	protected override void Awake()
	{
		base.Awake();
		if (!_requiresExternalInitialization)
		{
			Debug.LogError("ShipLogEntries have delayed initialization. This must be enabled");
		}
		_shipLogEntryListItem = this.GetRequiredComponent<ShipLogEntryListItem>();
		_textField = _shipLogEntryListItem.GetNameField();
		_listItemLayoutElement = _shipLogEntryListItem.GetRootLayoutElement();
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		bool flag = _shipLogEntryListItem.IsSubEntry();
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		TextTranslation.Language language = TextTranslation.Get().GetLanguage();
		int num = 0;
		float num2 = 0f;
		if (uITextSize == UITextSize.LARGE)
		{
			switch (language)
			{
			case TextTranslation.Language.CHINESE_SIMPLE:
				num = (flag ? _subEntryFontSizeCn.largeVal : _entryFontSizeCn.largeVal);
				num2 = _minRootObjLayoutHeightCn.largeVal;
				break;
			case TextTranslation.Language.JAPANESE:
				num = (flag ? _subEntryFontSizeJp.largeVal : _entryFontSizeJp.largeVal);
				num2 = _minRootObjLayoutHeightJp.largeVal;
				break;
			case TextTranslation.Language.KOREAN:
				num = (flag ? _subEntryFontSizeKr.largeVal : _entryFontSizeKr.largeVal);
				num2 = _minRootObjLayoutHeightKr.largeVal;
				break;
			case TextTranslation.Language.POLISH:
			case TextTranslation.Language.RUSSIAN:
				num = (flag ? _subEntryFontSizePlRu.largeVal : _entryFontSizePlRu.largeVal);
				num2 = _minRootObjLayoutHeightPlRu.largeVal;
				break;
			case TextTranslation.Language.TURKISH:
				num = (flag ? _subEntryFontSizeTr.largeVal : _entryFontSizeTr.largeVal);
				num2 = _minRootObjLayoutHeightTr.largeVal;
				break;
			default:
				num = (flag ? _subEntryFontSizeDefault.largeVal : _entryFontSizeDefault.largeVal);
				num2 = _minRootObjLayoutHeightDefault.largeVal;
				break;
			}
		}
		else
		{
			switch (language)
			{
			case TextTranslation.Language.CHINESE_SIMPLE:
				num = (flag ? _subEntryFontSizeCn.normalVal : _entryFontSizeCn.normalVal);
				num2 = _minRootObjLayoutHeightCn.normalVal;
				break;
			case TextTranslation.Language.JAPANESE:
				num = (flag ? _subEntryFontSizeJp.normalVal : _entryFontSizeJp.normalVal);
				num2 = _minRootObjLayoutHeightJp.normalVal;
				break;
			case TextTranslation.Language.KOREAN:
				num = (flag ? _subEntryFontSizeKr.normalVal : _entryFontSizeKr.normalVal);
				num2 = _minRootObjLayoutHeightKr.normalVal;
				break;
			case TextTranslation.Language.POLISH:
			case TextTranslation.Language.RUSSIAN:
				num = (flag ? _subEntryFontSizePlRu.normalVal : _entryFontSizePlRu.normalVal);
				num2 = _minRootObjLayoutHeightPlRu.normalVal;
				break;
			case TextTranslation.Language.TURKISH:
				num = (flag ? _subEntryFontSizeTr.normalVal : _entryFontSizeTr.normalVal);
				num2 = _minRootObjLayoutHeightTr.normalVal;
				break;
			default:
				num = (flag ? _subEntryFontSizeDefault.normalVal : _entryFontSizeDefault.normalVal);
				num2 = _minRootObjLayoutHeightDefault.normalVal;
				break;
			}
		}
		_textField.fontSize = num;
		_listItemLayoutElement.minHeight = num2;
	}
}
