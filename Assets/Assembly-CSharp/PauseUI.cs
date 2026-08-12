using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private Image _backdrop;

	[SerializeField]
	private Image _readingVignette;

	[SerializeField]
	private float _vignetteFadeSpeed = 1f;

	private Color _readingVignetteBaseColor;

	private float _readingVignetteFade;

	private void Awake()
	{
		_canvas.enabled = false;
		_backdrop.enabled = false;
		_readingVignette.enabled = false;
		base.enabled = false;
		_readingVignetteBaseColor = _readingVignette.color;
		_readingVignetteFade = 0f;
		GlobalMessenger.AddListener("GamePauseUpdated", OnGamePauseUpdated);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("GamePauseUpdated", OnGamePauseUpdated);
	}

	private void OnGamePauseUpdated()
	{
		OnGameStateUpdated();
	}

	private void OnGameStateUpdated()
	{
		if (OWTime.IsPaused())
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	private void Enable()
	{
		_canvas.enabled = true;
		base.enabled = true;
		_backdrop.enabled = OWTime.IsPaused(OWTime.PauseType.Menu);
		if (OWTime.IsPaused(OWTime.PauseType.Reading))
		{
			_readingVignette.enabled = true;
		}
	}

	private void Disable()
	{
		_backdrop.enabled = false;
	}

	private void LateUpdate()
	{
		if (_readingVignette.enabled)
		{
			if (OWTime.IsPaused(OWTime.PauseType.Reading))
			{
				_readingVignetteFade = Mathf.MoveTowards(_readingVignetteFade, 1f, 1f / _vignetteFadeSpeed * Time.unscaledDeltaTime);
			}
			else
			{
				_readingVignetteFade = Mathf.MoveTowards(_readingVignetteFade, 0f, 1f / _vignetteFadeSpeed * Time.unscaledDeltaTime);
				if (_readingVignetteFade <= 0f)
				{
					_readingVignette.enabled = false;
				}
			}
			_readingVignette.color = new Color(_readingVignetteBaseColor.r, _readingVignetteBaseColor.g, _readingVignetteBaseColor.b, _readingVignetteBaseColor.a * _readingVignetteFade);
		}
		if (!_readingVignette.enabled)
		{
			_canvas.enabled = false;
			base.enabled = false;
		}
	}
}
