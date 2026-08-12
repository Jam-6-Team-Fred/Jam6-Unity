using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FontSizeFitter : MonoBehaviour
{
	private Text _text;

	private int _originalFontSize;

	private bool _initialized;

	private bool _canvasCallbackEnabled;

	private void Start()
	{
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		base.enabled = true;
	}

	private void OnDestroy()
	{
		TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
		if (_canvasCallbackEnabled)
		{
			Canvas.willRenderCanvases -= FontSizeFitting;
		}
	}

	private void Update()
	{
		if (!_initialized)
		{
			Initialize();
			base.enabled = false;
		}
	}

	private void OnLanguageChanged()
	{
		if (_text != null)
		{
			_text.fontSize = TextTranslation.GetModifiedFontSize(_originalFontSize);
		}
		_initialized = false;
		base.enabled = true;
	}

	private void Initialize()
	{
		_initialized = true;
		_text = this.GetRequiredComponent<Text>();
		_originalFontSize = _text.fontSize;
		_canvasCallbackEnabled = true;
		Canvas.willRenderCanvases += FontSizeFitting;
	}

	private void FontSizeFitting()
	{
		int num = _text.fontSize - 1;
		if ((_text.preferredHeight > _text.rectTransform.rect.height || _text.preferredWidth > _text.rectTransform.rect.width) && num > 0)
		{
			_text.fontSize = num;
			return;
		}
		if (num == 1)
		{
			Debug.LogWarning("FontSizeFitter using font size of 1. Please check the input string and rect transform dimensions");
		}
		_canvasCallbackEnabled = false;
		Canvas.willRenderCanvases -= FontSizeFitting;
	}
}
