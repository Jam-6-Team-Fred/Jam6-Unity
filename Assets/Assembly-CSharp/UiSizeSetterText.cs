using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UiSizeSetterText : BaseUiSizeSetter
{
	[SerializeField]
	private bool _enableFontSizeChange;

	[SerializeField]
	private IntBlock _fontSizes;

	[SerializeField]
	private bool _enableLineSpacingChange;

	[SerializeField]
	private FloatBlock _lineSpacings;

	private Text _targetText;

	protected override void Awake()
	{
		base.Awake();
		_targetText = this.GetRequiredComponent<Text>();
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		if (uITextSize == UITextSize.LARGE)
		{
			if (_enableFontSizeChange)
			{
				_targetText.fontSize = _fontSizes.largeVal;
			}
			if (_enableLineSpacingChange)
			{
				_targetText.lineSpacing = _lineSpacings.largeVal;
			}
		}
		else
		{
			if (_enableFontSizeChange)
			{
				_targetText.fontSize = _fontSizes.normalVal;
			}
			if (_enableLineSpacingChange)
			{
				_targetText.lineSpacing = _lineSpacings.normalVal;
			}
		}
	}
}
