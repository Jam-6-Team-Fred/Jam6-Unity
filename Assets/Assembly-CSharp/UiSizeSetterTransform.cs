using UnityEngine;

[RequireComponent(typeof(Transform))]
public class UiSizeSetterTransform : BaseUiSizeSetter
{
	[SerializeField]
	private bool _enablePositionChange;

	[SerializeField]
	private Vector3Block _positions;

	[SerializeField]
	private bool _enableScaleChange;

	[SerializeField]
	private Vector3Block _scales;

	private Transform _targetTransform;

	protected override void Awake()
	{
		base.Awake();
		_targetTransform = this.GetRequiredComponent<Transform>();
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
			if (_enablePositionChange)
			{
				_targetTransform.position = _positions.largeVal;
			}
			if (_enableScaleChange)
			{
				_targetTransform.localScale = _scales.largeVal;
			}
		}
		else
		{
			if (_enablePositionChange)
			{
				_targetTransform.position = _positions.normalVal;
			}
			if (_enableScaleChange)
			{
				_targetTransform.localScale = _scales.normalVal;
			}
		}
	}
}
