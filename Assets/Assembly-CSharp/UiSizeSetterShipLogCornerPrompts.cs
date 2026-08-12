using System;
using UnityEngine;
using UnityEngine.UI;

public class UiSizeSetterShipLogCornerPrompts : BaseUiSizeSetter
{
	protected enum LayoutGroupType
	{
		HORIZONTAL = 0,
		VERTICAL = 1
	}

	[Serializable]
	protected struct LayoutGroupBlock
	{
		public LayoutGroupType normalVal;

		public LayoutGroupType largeVal;
	}

	[Serializable]
	protected struct RectOffsetBlock
	{
		public RectOffset normalVal;

		public RectOffset largeVal;
	}

	[Header("Layout Group Settings")]
	[SerializeField]
	private bool _enableLayoutGroupTypeChange;

	[SerializeField]
	private LayoutGroupBlock _layoutGroupTypes;

	[SerializeField]
	private bool _enablePaddingChange;

	[SerializeField]
	private RectOffsetBlock _paddings;

	[SerializeField]
	private bool _enableSpacingChange;

	[SerializeField]
	private FloatBlock _spacings;

	[SerializeField]
	private bool _enableChildAlignmentChange;

	[SerializeField]
	private TextAnchorBlock _childAlignments;

	[SerializeField]
	private bool _enableControlChildSizeWidthChange;

	[SerializeField]
	private BoolBlock _controlChildWidths;

	[SerializeField]
	private bool _enableControlChildSizeHeightChange;

	[SerializeField]
	private BoolBlock _controlChildHeights;

	[SerializeField]
	private bool _enableForceExpandWidthChange;

	[SerializeField]
	private BoolBlock _forceExpandWidths;

	[SerializeField]
	private bool _enableForceExpandHeightChange;

	[SerializeField]
	private BoolBlock _forceExpandHeights;

	[Space(10f)]
	[Header("ScreenPromptList Settings")]
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

	private HorizontalOrVerticalLayoutGroup _targetLayoutGroup;

	private bool _doResizeOnNextUpdate;

	private UITextSize _cachedSetting;

	private ScreenPromptList _screenPromptList;

	protected override void Awake()
	{
		base.Awake();
		_targetLayoutGroup = this.GetRequiredComponent<HorizontalOrVerticalLayoutGroup>();
		if (_targetLayoutGroup == null)
		{
			Debug.LogError("This component requires a layout group to work", this);
		}
		_screenPromptList = GetComponent<ScreenPromptList>();
		if (_screenPromptList == null)
		{
			Debug.LogError("This component requires a ScreenPromptList to work", this);
		}
	}

	protected void Update()
	{
		if (_doResizeOnNextUpdate)
		{
			DoResizeAction(_cachedSetting);
		}
	}

	public override void DoResizeAction(UITextSize textSizeSetting)
	{
		if (_targetLayoutGroup != null && _enableLayoutGroupTypeChange)
		{
			if (!_doResizeOnNextUpdate)
			{
				UnityEngine.Object.Destroy(_targetLayoutGroup);
				_doResizeOnNextUpdate = true;
				_cachedSetting = textSizeSetting;
			}
		}
		else
		{
			_doResizeOnNextUpdate = false;
			ResizeLayoutGroup(textSizeSetting);
			ResizeScreenPromptList(textSizeSetting);
		}
	}

	private void ResizeLayoutGroup(UITextSize textSizeSetting)
	{
		UITextSize uITextSize = textSizeSetting;
		if (uITextSize == UITextSize.AUTO)
		{
			uITextSize = ((!PlayerData.IsUILargeTextSize()) ? UITextSize.SMALL : UITextSize.LARGE);
		}
		if (uITextSize == UITextSize.LARGE)
		{
			if (_enableLayoutGroupTypeChange)
			{
				if (_layoutGroupTypes.largeVal == LayoutGroupType.VERTICAL)
				{
					_targetLayoutGroup = base.gameObject.GetAddComponent<VerticalLayoutGroup>();
				}
				else
				{
					_targetLayoutGroup = base.gameObject.GetAddComponent<HorizontalLayoutGroup>();
				}
			}
			if (_enablePaddingChange)
			{
				_targetLayoutGroup.padding = _paddings.largeVal;
			}
			if (_enableSpacingChange)
			{
				_targetLayoutGroup.spacing = _spacings.largeVal;
			}
			if (_enableChildAlignmentChange)
			{
				_targetLayoutGroup.childAlignment = _childAlignments.largeVal;
			}
			if (_enableControlChildSizeHeightChange)
			{
				_targetLayoutGroup.childControlHeight = _controlChildHeights.largeVal;
			}
			if (_enableControlChildSizeWidthChange)
			{
				_targetLayoutGroup.childControlWidth = _controlChildWidths.largeVal;
			}
			if (_enableForceExpandHeightChange)
			{
				_targetLayoutGroup.childForceExpandHeight = _forceExpandHeights.largeVal;
			}
			if (_enableForceExpandWidthChange)
			{
				_targetLayoutGroup.childForceExpandWidth = _forceExpandWidths.largeVal;
			}
			return;
		}
		if (_enableLayoutGroupTypeChange)
		{
			if (_layoutGroupTypes.normalVal == LayoutGroupType.VERTICAL)
			{
				_targetLayoutGroup = base.gameObject.GetAddComponent<VerticalLayoutGroup>();
			}
			else
			{
				_targetLayoutGroup = base.gameObject.GetAddComponent<HorizontalLayoutGroup>();
			}
		}
		if (_enablePaddingChange)
		{
			_targetLayoutGroup.padding = _paddings.normalVal;
		}
		if (_enableSpacingChange)
		{
			_targetLayoutGroup.spacing = _spacings.normalVal;
		}
		if (_enableChildAlignmentChange)
		{
			_targetLayoutGroup.childAlignment = _childAlignments.normalVal;
		}
		if (_enableControlChildSizeHeightChange)
		{
			_targetLayoutGroup.childControlHeight = _controlChildHeights.normalVal;
		}
		if (_enableControlChildSizeWidthChange)
		{
			_targetLayoutGroup.childControlWidth = _controlChildWidths.normalVal;
		}
		if (_enableForceExpandHeightChange)
		{
			_targetLayoutGroup.childForceExpandHeight = _forceExpandHeights.normalVal;
		}
		if (_enableForceExpandWidthChange)
		{
			_targetLayoutGroup.childForceExpandWidth = _forceExpandWidths.normalVal;
		}
	}

	private void ResizeScreenPromptList(UITextSize textSizeSetting)
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
		_screenPromptList.SetMinElementDimensionsAndFontSize(layoutMinHeight, layoutMinWidth, fontSize);
	}
}
