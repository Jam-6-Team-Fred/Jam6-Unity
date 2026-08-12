using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class OffScreenIndicator : MonoBehaviour
{
	private enum ScreenSide
	{
		BOTTOM = 0,
		TOP = 1,
		LEFT = 2,
		RIGHT = 3,
		NONE = 4
	}

	[SerializeField]
	private Text _textField;

	[SerializeField]
	private RectTransform _arrow;

	[SerializeField]
	private float _screenMargin = 20f;

	private float _screenMarginScaled;

	private Canvas _canvas;

	private Vector2 _customCanvasRectBounds;

	private ScreenSide _side;

	private RectTransform _rectTransform;

	private void Awake()
	{
		SetCanvas(_textField.canvas);
	}

	public void SetCanvas(Canvas canvas)
	{
		_canvas = canvas;
		_screenMarginScaled = _screenMargin * _canvas.scaleFactor;
		_rectTransform = this.GetRequiredComponent<RectTransform>();
		if (_canvas.renderMode == RenderMode.WorldSpace)
		{
			_customCanvasRectBounds = _canvas.GetRequiredComponent<RectTransform>().sizeDelta;
		}
		else
		{
			_customCanvasRectBounds = new Vector2(_canvas.pixelRect.width, _canvas.pixelRect.height);
		}
		_customCanvasRectBounds = new Vector2(_customCanvasRectBounds.x - _screenMarginScaled * 2f, _customCanvasRectBounds.y - _screenMarginScaled * 2f);
	}

	public void SetText(string text)
	{
		_textField.text = text;
	}

	public void SetCanvasPosition(Vector3 targetPos)
	{
		Vector2 zero = Vector2.zero;
		OWCamera activeCamera = Locator.GetActiveCamera();
		float num = 0f;
		Vector3 position = activeCamera.transform.position;
		Vector3 direction = targetPos - position;
		Vector3 vector = activeCamera.transform.InverseTransformDirection(direction);
		num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		num = OWMath.SetAnglePositive(num);
		float num2 = _customCanvasRectBounds.x / _canvas.scaleFactor;
		float num3 = _customCanvasRectBounds.y / _canvas.scaleFactor;
		float num4 = Mathf.Atan2(num3 / 2f, num2 / 2f) * 57.29578f;
		float num5 = 90f - num4;
		ScreenSide screenSide = ScreenSide.NONE;
		if (num > num4 && num < 90f + num5)
		{
			screenSide = ScreenSide.TOP;
			zero = new Vector2(num2 / 2f + num3 / 2f / Mathf.Tan(num * ((float)Math.PI / 180f)), num3 + _screenMarginScaled);
		}
		else if (num > 90f + num5 && num < 180f + num4)
		{
			screenSide = ScreenSide.LEFT;
			zero = new Vector2(_screenMarginScaled, num3 / 2f - num2 / 2f * Mathf.Tan(num * ((float)Math.PI / 180f)));
		}
		else if (num > 180f + num4 && num < 270f + num5)
		{
			screenSide = ScreenSide.BOTTOM;
			zero = new Vector2(num2 / 2f - num3 / 2f / Mathf.Tan(num * ((float)Math.PI / 180f)), _screenMarginScaled);
		}
		else
		{
			screenSide = ScreenSide.RIGHT;
			zero = new Vector2(num2 + _screenMarginScaled, num3 / 2f + num2 / 2f * Mathf.Tan(num * ((float)Math.PI / 180f)));
		}
		ApplyRotations(screenSide);
		_rectTransform.anchoredPosition = zero;
	}

	private void ApplyRotations(ScreenSide side)
	{
		if (_side != side)
		{
			switch (side)
			{
			case ScreenSide.BOTTOM:
				_rectTransform.pivot = new Vector2(0.5f, 0f);
				_arrow.anchorMin = new Vector2(0.5f, 0f);
				_arrow.anchorMax = _arrow.anchorMin;
				_arrow.localRotation = Quaternion.Euler(Vector3.zero);
				_textField.rectTransform.pivot = new Vector2(0.5f, 0f);
				_textField.alignment = TextAnchor.LowerCenter;
				_textField.rectTransform.anchoredPosition = Vector2.zero;
				_textField.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
				break;
			case ScreenSide.LEFT:
				_rectTransform.pivot = new Vector2(0f, 0.5f);
				_arrow.anchorMin = new Vector2(0f, 0.5f);
				_arrow.anchorMax = _arrow.anchorMin;
				_arrow.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
				_textField.rectTransform.pivot = new Vector2(0f, 0.5f);
				_textField.alignment = TextAnchor.MiddleLeft;
				_textField.rectTransform.anchoredPosition = new Vector2(0f, 5f);
				_textField.rectTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
				break;
			case ScreenSide.RIGHT:
				_rectTransform.pivot = new Vector2(1f, 0.5f);
				_arrow.anchorMin = new Vector2(1f, 0.5f);
				_arrow.anchorMax = _arrow.anchorMin;
				_arrow.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
				_textField.rectTransform.pivot = new Vector2(1f, 0.5f);
				_textField.alignment = TextAnchor.MiddleRight;
				_textField.rectTransform.anchoredPosition = new Vector2(0f, 5f);
				_textField.rectTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 270f));
				break;
			case ScreenSide.TOP:
				_rectTransform.pivot = new Vector2(0.5f, 1f);
				_arrow.anchorMin = new Vector2(0.5f, 1f);
				_arrow.anchorMax = _arrow.anchorMin;
				_arrow.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
				_textField.rectTransform.pivot = new Vector2(0.5f, 1f);
				_textField.alignment = TextAnchor.UpperCenter;
				_textField.rectTransform.anchoredPosition = Vector2.zero;
				_textField.rectTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
				break;
			}
			_side = side;
			_arrow.anchoredPosition = Vector2.zero;
		}
	}
}
