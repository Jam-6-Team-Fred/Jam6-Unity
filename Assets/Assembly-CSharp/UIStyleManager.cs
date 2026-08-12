using UnityEngine;

public class UIStyleManager : MonoBehaviour
{
	[Header("Menu")]
	[SerializeField]
	private Font _menuFont;

	[SerializeField]
	private Font _cnMenuFont;

	[SerializeField]
	private Font _jpMenuFont;

	[SerializeField]
	private Font _krMenuFont;

	[SerializeField]
	private Font _ruMenuFont;

	[SerializeField]
	private float _defaultLetterSpacing;

	[Space]
	[SerializeField]
	private Color _popupBlockerColor;

	[Space]
	[SerializeField]
	private Color _menuForegroundColorNormal;

	[SerializeField]
	private Color _menuForegroundColorIntermediateSelect;

	[SerializeField]
	private Color _menuForegroundColorSelected;

	[SerializeField]
	private Color _menuForegroundColorPressed;

	[SerializeField]
	private Color _menuForegroundColorDisabled;

	[Space]
	[SerializeField]
	private Color _menuBackgroundColorNormal;

	[SerializeField]
	private Color _menuBackgroundColorIntermediateSelect;

	[SerializeField]
	private Color _menuBackgroundColorSelected;

	[SerializeField]
	private Color _menuBackgroundColorPressed;

	[SerializeField]
	private Color _menuBackgroundColorDisabled;

	[Space]
	[SerializeField]
	private Color _menuSecondaryForegroundColorNormal;

	[SerializeField]
	private Color _menuSecondaryForegroundColorIntermediateSelect;

	[SerializeField]
	private Color _menuSecondaryForegroundColorSelected;

	[SerializeField]
	private Color _menuSecondaryForegroundColorPressed;

	[SerializeField]
	private Color _menuSecondaryForegroundColorDisabled;

	[Space]
	[SerializeField]
	private Color _menuSecondaryBackgroundColorNormal;

	[SerializeField]
	private Color _menuSecondaryBackgroundColorIntermediateSelect;

	[SerializeField]
	private Color _menuSecondaryBackgroundColorSelected;

	[SerializeField]
	private Color _menuSecondaryBackgroundColorPressed;

	[SerializeField]
	private Color _menuSecondaryBackgroundColorDisabled;

	[Space]
	[SerializeField]
	private Color _menuButtonForegroundColorNormal;

	[SerializeField]
	private Color _menuButtonForegroundColorIntermediateSelect;

	[SerializeField]
	private Color _menuButtonForegroundColorSelected;

	[SerializeField]
	private Color _menuButtonForegroundColorPressed;

	[SerializeField]
	private Color _menuButtonForegroundColorDisabled;

	[Space]
	[SerializeField]
	private Color _menuButtonBackgroundColorNormal;

	[SerializeField]
	private Color _menuButtonBackgroundColorIntermediateSelect;

	[SerializeField]
	private Color _menuButtonBackgroundColorSelected;

	[SerializeField]
	private Color _menuButtonBackgroundColorPressed;

	[SerializeField]
	private Color _menuButtonBackgroundColorDisabled;

	[Space]
	[Header("Preflight")]
	[SerializeField]
	private Color _menuPreflightColorNormal;

	[SerializeField]
	private Color _menuPreflightColorIntermediateSelect;

	[SerializeField]
	private Color _menuPreflightColorSelected;

	[SerializeField]
	private Color _menuPreflightColorPressed;

	[SerializeField]
	private Color _menuPreflightColorDisabled;

	[Space]
	[Header("Translator")]
	[SerializeField]
	private Font _translatorFont;

	[SerializeField]
	private Color _primaryHighlightColor;

	[SerializeField]
	private Color _secondaryHighlightColor;

	[Space]
	[Header("Ship Log")]
	[SerializeField]
	private Font _shipLogFont;

	[SerializeField]
	private Font _shipLogCardFont;

	[SerializeField]
	private float _shipLogCardLineSpacing;

	[SerializeField]
	private Color _shipLogRumorColor;

	[SerializeField]
	private Color _shipLogSelectionColor;

	[Space]
	[SerializeField]
	private Color _neutralColor;

	[SerializeField]
	private Color _neutralHighlight;

	[SerializeField]
	private Color _vesselColor;

	[SerializeField]
	private Color _vesselHighlight;

	[SerializeField]
	private Color _timeLoopColor;

	[SerializeField]
	private Color _timeLoopHighlight;

	[SerializeField]
	private Color _sunkenModuleColor;

	[SerializeField]
	private Color _sunkenModuleHighlight;

	[SerializeField]
	private Color _quantumMoonColor;

	[SerializeField]
	private Color _quantumMoonHighlight;

	[SerializeField]
	private Color _invisiblePlanetColor;

	[SerializeField]
	private Color _invisiblePlanetHighlight;

	public Font GetMenuFont()
	{
		switch (PlayerData.GetSavedLanguage())
		{
		case TextTranslation.Language.CHINESE_SIMPLE:
			return _cnMenuFont;
		case TextTranslation.Language.JAPANESE:
			return _jpMenuFont;
		case TextTranslation.Language.KOREAN:
			return _krMenuFont;
		case TextTranslation.Language.POLISH:
		case TextTranslation.Language.RUSSIAN:
			return _ruMenuFont;
		default:
			return _menuFont;
		}
	}

	public float GetMenuLetterSpacing()
	{
		return _defaultLetterSpacing;
	}

	public Color GetPopupBlockerColor()
	{
		return _popupBlockerColor;
	}

	public Color GetForegroundMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuForegroundColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuForegroundColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuForegroundColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuForegroundColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuForegroundColorDisabled;
			break;
		}
		return result;
	}

	public Color GetBackgroundMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuBackgroundColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuBackgroundColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuBackgroundColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuBackgroundColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuBackgroundColorDisabled;
			break;
		}
		return result;
	}

	public Color GetPreflightMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuPreflightColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuPreflightColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuPreflightColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuPreflightColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuPreflightColorDisabled;
			break;
		}
		return result;
	}

	public Color GetSecondaryForegroundMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuSecondaryForegroundColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuSecondaryForegroundColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuSecondaryForegroundColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuSecondaryForegroundColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuSecondaryForegroundColorDisabled;
			break;
		}
		return result;
	}

	public Color GetSecondaryBackgroundMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuSecondaryBackgroundColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuSecondaryBackgroundColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuSecondaryBackgroundColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuSecondaryBackgroundColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuSecondaryBackgroundColorDisabled;
			break;
		}
		return result;
	}

	public Color GetButtonForegroundMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuButtonForegroundColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuButtonForegroundColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuButtonForegroundColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuButtonForegroundColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuButtonForegroundColorDisabled;
			break;
		}
		return result;
	}

	public Color GetButtonBackgroundMenuColor(UIElementState state)
	{
		Color result = Color.black;
		switch (state)
		{
		case UIElementState.NORMAL:
			result = _menuButtonBackgroundColorNormal;
			break;
		case UIElementState.INTERMEDIATELY_HIGHLIGHTED:
			result = _menuButtonBackgroundColorIntermediateSelect;
			break;
		case UIElementState.HIGHLIGHTED:
		case UIElementState.ROLLOVER_HIGHLIGHT:
			result = _menuButtonBackgroundColorSelected;
			break;
		case UIElementState.PRESSED:
			result = _menuButtonBackgroundColorPressed;
			break;
		case UIElementState.DISABLED:
			result = _menuButtonBackgroundColorDisabled;
			break;
		}
		return result;
	}

	public Font GetTranslatorFont()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			return _translatorFont;
		}
		return TextTranslation.GetFont();
	}

	public Color GetPrimaryHighlightColor()
	{
		return _primaryHighlightColor;
	}

	public Color GetSecondaryHighlightColor()
	{
		return _secondaryHighlightColor;
	}

	public Font GetShipLogFont()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			return _shipLogFont;
		}
		return TextTranslation.GetFont();
	}

	public Font GetShipLogCardFont()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			return _shipLogCardFont;
		}
		return TextTranslation.GetFont();
	}

	public float GetShipLogCardSpacing()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			return _shipLogCardLineSpacing;
		}
		return TextTranslation.GetDefaultFontSpacing();
	}

	public Color GetShipLogRumorColor()
	{
		return _shipLogRumorColor;
	}

	public Color GetShipLogSelectionColor()
	{
		return _shipLogSelectionColor;
	}

	public Color GetShipLogNeutralColor(bool highlight = false)
	{
		if (!highlight)
		{
			return _neutralColor;
		}
		return _neutralHighlight;
	}

	public Color GetCuriosityColor(CuriosityName curiosityName, bool highlight = false)
	{
		switch (curiosityName)
		{
		case CuriosityName.Vessel:
			if (!highlight)
			{
				return _vesselColor;
			}
			return _vesselHighlight;
		case CuriosityName.TimeLoop:
			if (!highlight)
			{
				return _timeLoopColor;
			}
			return _timeLoopHighlight;
		case CuriosityName.SunkenModule:
			if (!highlight)
			{
				return _sunkenModuleColor;
			}
			return _sunkenModuleHighlight;
		case CuriosityName.QuantumMoon:
			if (!highlight)
			{
				return _quantumMoonColor;
			}
			return _quantumMoonHighlight;
		case CuriosityName.InvisiblePlanet:
			if (!highlight)
			{
				return _invisiblePlanetColor;
			}
			return _invisiblePlanetHighlight;
		default:
			if (!highlight)
			{
				return _neutralColor;
			}
			return _neutralHighlight;
		}
	}
}
