using UnityEngine;
using UnityEngine.UI;

public class LayoutValueSetter : MonoBehaviour
{
	public enum SetterType
	{
		NONE = 0,
		OWN_HEIGHT_DRIVES_WIDTH = 1
	}

	[SerializeField]
	private SetterType _setterType;

	private AspectRatioFitter _aspectRatioFitter;

	private float _cachedHeight;

	private float _aspectRatio;

	private LayoutElement _mLayoutElement;

	private RectTransform _mRectTransform;

	private bool _pendingInit;

	private bool _initialized;

	public bool IsDrivenByHeight()
	{
		return _setterType == SetterType.OWN_HEIGHT_DRIVES_WIDTH;
	}

	private void Awake()
	{
		if (_setterType == SetterType.NONE)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		if (_setterType == SetterType.NONE)
		{
			Debug.LogError("LayoutValueSetter not initialized");
			return;
		}
		_mLayoutElement = GetComponent<LayoutElement>();
		_mRectTransform = this.GetRequiredComponent<RectTransform>();
		_initialized = true;
		if (_pendingInit)
		{
			UpdateValues();
		}
	}

	private void Update()
	{
		UpdateValues();
	}

	public void SetSelfFitter(SetterType type)
	{
		if (type == SetterType.NONE)
		{
			Debug.LogError("Invalid SetterType");
			return;
		}
		_setterType = type;
		Image component = base.gameObject.GetComponent<Image>();
		if (component != null)
		{
			_aspectRatio = component.sprite.rect.width / component.sprite.rect.height;
		}
		UpdateValues();
		base.enabled = true;
	}

	private void UpdateValues()
	{
		if (!_initialized)
		{
			_pendingInit = true;
			return;
		}
		SetterType setterType = _setterType;
		if (setterType != SetterType.OWN_HEIGHT_DRIVES_WIDTH)
		{
			return;
		}
		float height = base.gameObject.GetRequiredComponent<RectTransform>().rect.height;
		if (_cachedHeight == height)
		{
			return;
		}
		_cachedHeight = height;
		if (_mLayoutElement != null)
		{
			if (_aspectRatio != 0f)
			{
				_mLayoutElement.preferredWidth = _cachedHeight * _aspectRatio;
			}
			else
			{
				_mLayoutElement.preferredWidth = _cachedHeight;
			}
			return;
		}
		Vector2 sizeDelta = _mRectTransform.sizeDelta;
		if (_aspectRatio != 0f)
		{
			sizeDelta.x = _cachedHeight * _aspectRatio;
		}
		else
		{
			sizeDelta.x = _cachedHeight;
		}
		_mRectTransform.sizeDelta = sizeDelta;
	}
}
