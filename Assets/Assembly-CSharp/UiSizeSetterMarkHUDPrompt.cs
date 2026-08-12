using UnityEngine;

public class UiSizeSetterMarkHUDPrompt : BaseUiSizeSetter
{
	[SerializeField]
	private ScreenPromptList _promptList;

	[SerializeField]
	private IntBlock _fontSizes;

	[SerializeField]
	private FloatBlock _minHeights;

	[SerializeField]
	private RectTransform _promptRootTransform;

	[SerializeField]
	private Vector2Block _promptRootSizeDelta;

	[SerializeField]
	private Vector2Block _promptRootSizeDeltaWLineBreak;

	[SerializeField]
	private RectTransform _logImageTransform;

	[SerializeField]
	private Vector2Block _logImagePosition;

	[SerializeField]
	private Vector2Block _logImagePositionWLineBreak;

	[Space(10f)]
	[Header("AdditionalAdjustments")]
	[SerializeField]
	private float _lineSpacingRuPl = 0.8f;

	private ScreenPrompt _markOnHUDPrompt;

	protected override void Awake()
	{
		base.Awake();
		if (!_requiresExternalInitialization)
		{
			Debug.LogError("This code requires multiple references to other components to work properly");
		}
	}

	public void SetInitReferences(ScreenPrompt prompt)
	{
		_markOnHUDPrompt = prompt;
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		if (_markOnHUDPrompt == null)
		{
			Debug.LogError("Must set prompt before running Resize");
		}
		bool flag = _markOnHUDPrompt.GetText().Contains("\n");
		float num = 0f;
		float layoutMinWidth = 0f;
		int fontSize = 1;
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		Vector2 sizeDelta;
		Vector2 anchoredPosition;
		if (_markOnHUDPrompt.IsVisible())
		{
			if (uITextSize == UITextSize.LARGE)
			{
				fontSize = _fontSizes.largeVal;
				if (flag)
				{
					num = _minHeights.largeVal;
					sizeDelta = _promptRootSizeDeltaWLineBreak.largeVal;
					anchoredPosition = _logImagePositionWLineBreak.largeVal;
				}
				else
				{
					num = _minHeights.largeVal;
					sizeDelta = _promptRootSizeDelta.largeVal;
					anchoredPosition = _logImagePosition.largeVal;
				}
			}
			else
			{
				fontSize = _fontSizes.normalVal;
				if (flag)
				{
					num = _minHeights.normalVal;
					sizeDelta = _promptRootSizeDeltaWLineBreak.normalVal;
					anchoredPosition = _logImagePositionWLineBreak.normalVal;
				}
				else
				{
					num = _minHeights.normalVal;
					sizeDelta = _promptRootSizeDelta.normalVal;
					anchoredPosition = _logImagePosition.normalVal;
				}
			}
		}
		else
		{
			num = _minHeights.normalVal;
			sizeDelta = _promptRootSizeDelta.normalVal;
			anchoredPosition = _logImagePosition.normalVal;
		}
		_promptList.SetMinElementDimensionsAndFontSize(num, layoutMinWidth, fontSize);
		TextTranslation.Language language = TextTranslation.Get().GetLanguage();
		if (language == TextTranslation.Language.RUSSIAN || language == TextTranslation.Language.POLISH)
		{
			_promptList.SetCustomLineSpacing(_lineSpacingRuPl);
		}
		else
		{
			_promptList.ResetCustomLineSpacing();
		}
		_promptRootTransform.sizeDelta = sizeDelta;
		_logImageTransform.anchoredPosition = anchoredPosition;
	}
}
