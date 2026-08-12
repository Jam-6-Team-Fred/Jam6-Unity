using UnityEngine;

[RequireComponent(typeof(ScreenPromptList))]
public class UiSizeSetterScreenPromptList : BaseUiSizeSetter
{
	[SerializeField]
	private bool _enableChildrenFontSizeChange;

	[SerializeField]
	private IntBlock _fontSizes;

	[SerializeField]
	private bool _enableMinElementHeightChange;

	[SerializeField]
	private FloatBlock _minHeights;

	[SerializeField]
	private bool _enableMinElementWidthChange;

	[SerializeField]
	private FloatBlock _minWidths;

	private ScreenPromptList _targetPromptList;

	protected override void Awake()
	{
		base.Awake();
		_targetPromptList = this.GetRequiredComponent<ScreenPromptList>();
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		float layoutMinHeight = 0f;
		float layoutMinWidth = 0f;
		int fontSize = 1;
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		if (uITextSize == UITextSize.LARGE)
		{
			if (_enableChildrenFontSizeChange)
			{
				fontSize = _fontSizes.largeVal;
			}
			if (_enableMinElementHeightChange)
			{
				layoutMinHeight = _minHeights.largeVal;
			}
			if (_enableMinElementWidthChange)
			{
				layoutMinWidth = _minWidths.largeVal;
			}
		}
		else
		{
			if (_enableChildrenFontSizeChange)
			{
				fontSize = _fontSizes.normalVal;
			}
			if (_enableMinElementHeightChange)
			{
				layoutMinHeight = _minHeights.normalVal;
			}
			if (_enableMinElementWidthChange)
			{
				layoutMinWidth = _minWidths.normalVal;
			}
		}
		_targetPromptList.SetMinElementDimensionsAndFontSize(layoutMinHeight, layoutMinWidth, fontSize);
	}
}
