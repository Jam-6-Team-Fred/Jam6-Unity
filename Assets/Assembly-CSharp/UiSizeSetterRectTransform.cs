using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiSizeSetterRectTransform : BaseUiSizeSetter
{
	[SerializeField]
	private bool _enableAnchoredPositionChange;

	[SerializeField]
	private Vector2Block _anchoredPositions;

	[SerializeField]
	private bool _enableSizeDeltaChange;

	[SerializeField]
	private Vector2Block _sizeDeltas;

	[SerializeField]
	private bool _enableScaleChange;

	[SerializeField]
	private Vector3Block _scales;

	private RectTransform _targetTransform;

	protected override void Awake()
	{
		base.Awake();
		_targetTransform = this.GetRequiredComponent<RectTransform>();
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
			if (_enableAnchoredPositionChange)
			{
				_targetTransform.anchoredPosition = _anchoredPositions.largeVal;
			}
			if (_enableSizeDeltaChange)
			{
				_targetTransform.sizeDelta = _sizeDeltas.largeVal;
			}
			if (_enableScaleChange)
			{
				_targetTransform.localScale = _scales.largeVal;
			}
		}
		else
		{
			if (_enableAnchoredPositionChange)
			{
				_targetTransform.anchoredPosition = _anchoredPositions.normalVal;
			}
			if (_enableSizeDeltaChange)
			{
				_targetTransform.sizeDelta = _sizeDeltas.normalVal;
			}
			if (_enableScaleChange)
			{
				_targetTransform.localScale = _scales.normalVal;
			}
		}
	}
}
