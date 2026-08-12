using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NomaiTranslatorProp : MonoBehaviour, ILateInitializer
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_EmissionColor;

	[SerializeField]
	private GameObject _translatorProp;

	[SerializeField]
	private MeshRenderer _leftPageArrowRenderer;

	[SerializeField]
	private MeshRenderer _rightPageArrowRenderer;

	[SerializeField]
	private Font _defaultPropFont;

	[SerializeField]
	private Font _defaultPropFontDynamic;

	[SerializeField]
	private float _defaultFontSpacing = 0.7f;

	private Font _fontInUse;

	private Font _dynamicFontInUse;

	private float _fontSpacingInUse;

	private Color _baseEmissionColor;

	private float _totalTranslateTime = 0.2f;

	private float _totalAlreadyTranslatedTime;

	private float _translationTimeElapsed;

	private float _perWordTranslationTime;

	private float _fullTextTranslationTime;

	private float _equipTime;

	private string _lastTextNodeToDisplay;

	private string _textNodeToDisplay;

	private string _translatedText;

	private TranslatorWordLengthComparer _lengthComparer;

	private List<TranslatorWord> _listDisplayWordsByLength;

	private List<TranslatorWord> _listDisplayWords;

	private int _numTranslatedWords;

	private Canvas _canvas;

	private ScrollRect _scrollRect;

	[SerializeField]
	private Text _textField;

	[SerializeField]
	private Text _pageNumberTextField;

	private StringBuilder _strBuilder;

	private StringBuilder _pageNumberStrBuilder;

	private NomaiText _nomaiTextComponent;

	private int _currentTextID;

	private TranslatorTargetBeam _targetBeam;

	private TranslatorScanBeam[] _scanBeams;

	private PlayerAudioController _audioController;

	private ThrusterModel _jetpackModel;

	private bool _screenPromptsInitialized;

	private ScreenPrompt _unequipPrompt;

	private ScreenPrompt _translatePrompt;

	private ScreenPrompt _centerTranslatePrompt;

	private ScreenPrompt _scrollPrompt;

	private ScreenPrompt _pagePrompt;

	private bool _isTooCloseToTarget;

	private bool _isTargetingGhostText;

	private bool _inNomaiAudioVolume;

	private bool _isTranslating;

	private bool _hasUsedTranslator;

	private bool _isTimeFrozen;

	private void Awake()
	{
		_listDisplayWords = new List<TranslatorWord>(1024);
		_listDisplayWordsByLength = new List<TranslatorWord>(1024);
		_lengthComparer = new TranslatorWordLengthComparer();
		_textNodeToDisplay = null;
		_strBuilder = new StringBuilder();
		_pageNumberStrBuilder = new StringBuilder();
		_canvas = base.transform.GetComponentInChildren<Canvas>();
		_scrollRect = base.transform.GetComponentInChildren<ScrollRect>();
		_targetBeam = base.transform.GetComponentInChildren<TranslatorTargetBeam>();
		_scanBeams = base.transform.GetComponentsInChildren<TranslatorScanBeam>();
		if (s_matPropBlock == null)
		{
			s_matPropBlock = new MaterialPropertyBlock();
			s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
		}
		if (_rightPageArrowRenderer != null)
		{
			Material sharedMaterial = _rightPageArrowRenderer.sharedMaterial;
			_baseEmissionColor = sharedMaterial.GetColor(s_propID_EmissionColor);
		}
		TurnOffArrowEmission();
		_canvas.enabled = false;
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].enabled = false;
		}
		_translatorProp.SetActive(value: false);
		_screenPromptsInitialized = false;
		LateInitializerManager.RegisterLateInitializer(this);
		_unequipPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.TranslatorExitPrompt) + "   <CMD>");
		_translatePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, UITextLibrary.GetString(UITextType.TranslatorUsePrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));
		_centerTranslatePrompt = new ScreenPrompt(InputLibrary.toolActionPrimary, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.TranslatorUsePrompt));
		_scrollPrompt = new ScreenPrompt(InputLibrary.toolOptionUp, InputLibrary.toolOptionDown, "<CMD>   " + UITextLibrary.GetString(UITextType.TranslatorScrollPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
		_pagePrompt = new ScreenPrompt(InputLibrary.toolOptionLeft, InputLibrary.toolOptionRight, "<CMD>   " + UITextLibrary.GetString(UITextType.TranslatorPagePrompt), ScreenPrompt.MultiCommandType.POS_NEG);
	}

	private void Start()
	{
		_audioController = Locator.GetPlayerAudioController();
		_jetpackModel = Locator.GetPlayerTransform().GetComponent<ThrusterModel>();
		_hasUsedTranslator = PlayerData.GetPersistentCondition("HAS_USED_TRANSLATOR");
		InitializeFont();
		TextTranslation.Get().OnLanguageChanged += InitializeFont;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (!_screenPromptsInitialized)
		{
			LateInitializerManager.UnregisterLateInitializer(this);
		}
		TextTranslation.Get().OnLanguageChanged -= InitializeFont;
	}

	public void LateInitialize()
	{
		_screenPromptsInitialized = true;
		Locator.GetPromptManager().AddScreenPrompt(_unequipPrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_translatePrompt, PromptPosition.UpperRight);
		Locator.GetPromptManager().AddScreenPrompt(_centerTranslatePrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_scrollPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_pagePrompt, PromptPosition.Center);
	}

	private void InitializeFont()
	{
		if (TextTranslation.Get().IsLanguageLatin())
		{
			_fontInUse = _defaultPropFont;
			_dynamicFontInUse = _defaultPropFontDynamic;
			_fontSpacingInUse = _defaultFontSpacing;
		}
		else
		{
			_fontInUse = TextTranslation.GetFont();
			_dynamicFontInUse = TextTranslation.GetFont(dynamicFont: true);
			_fontSpacingInUse = TextTranslation.GetDefaultFontSpacing();
		}
		_textField.font = _fontInUse;
		_textField.lineSpacing = _fontSpacingInUse;
	}

	public void OnEquipTool()
	{
		base.enabled = true;
		_equipTime = Time.time;
		_unequipPrompt.SetVisibility(isVisible: true);
		_canvas.enabled = true;
		if ((bool)_targetBeam)
		{
			_targetBeam.Activate();
		}
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].enabled = true;
		}
		_translatorProp.SetActive(value: true);
	}

	public void OnUnequipTool()
	{
		base.enabled = false;
		StopTranslating();
		if (_isTimeFrozen)
		{
			OWTime.Unpause(OWTime.PauseType.Reading);
			_isTimeFrozen = false;
		}
		TurnOffArrowEmission();
		_scrollPrompt.SetVisibility(isVisible: false);
		_pagePrompt.SetVisibility(isVisible: false);
		_translatePrompt.SetVisibility(isVisible: false);
		_unequipPrompt.SetVisibility(isVisible: false);
		_centerTranslatePrompt.SetVisibility(isVisible: false);
	}

	public void OnFinishUnequipAnimation()
	{
		if ((bool)_targetBeam)
		{
			_targetBeam.Deactivate();
		}
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].enabled = false;
		}
		_canvas.enabled = false;
		_translatorProp.SetActive(value: false);
	}

	public void SetTooCloseToTarget(bool value)
	{
		_isTooCloseToTarget = value;
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetTooCloseToTarget(value);
		}
	}

	public bool HasNomaiText()
	{
		if (_textNodeToDisplay == null)
		{
			return false;
		}
		return true;
	}

	public bool SetNomaiAudio(NomaiAudioVolume audio, int textPage = 1)
	{
		_inNomaiAudioVolume = true;
		_nomaiTextComponent = audio.GetAudioText();
		_currentTextID = textPage;
		_textNodeToDisplay = _nomaiTextComponent.GetTextNode(textPage);
		SetNomaiAudioArrowEmissions();
		return HasNomaiText();
	}

	private void SetNomaiAudioArrowEmissions()
	{
		if (!_inNomaiAudioVolume || !(_nomaiTextComponent != null) || !_nomaiTextComponent.IsTranslated(1) || _isTranslating || _nomaiTextComponent.GetNumTextBlocks() <= 1)
		{
			return;
		}
		int numTextBlocks = _nomaiTextComponent.GetNumTextBlocks();
		if (_leftPageArrowRenderer != null)
		{
			if (numTextBlocks >= 2 && _currentTextID > 1)
			{
				SetMaterialEmissionEnabled(_leftPageArrowRenderer, emissionEnabled: true);
			}
			else
			{
				SetMaterialEmissionEnabled(_leftPageArrowRenderer, emissionEnabled: false);
			}
		}
		if (_rightPageArrowRenderer != null)
		{
			if (numTextBlocks >= 2 && _currentTextID < numTextBlocks)
			{
				SetMaterialEmissionEnabled(_rightPageArrowRenderer, emissionEnabled: true);
			}
			else
			{
				SetMaterialEmissionEnabled(_rightPageArrowRenderer, emissionEnabled: false);
			}
		}
	}

	private void TurnOffArrowEmission()
	{
		if (_leftPageArrowRenderer != null)
		{
			SetMaterialEmissionEnabled(_leftPageArrowRenderer, emissionEnabled: false);
		}
		if (_rightPageArrowRenderer != null)
		{
			SetMaterialEmissionEnabled(_rightPageArrowRenderer, emissionEnabled: false);
		}
	}

	private void SetMaterialEmissionEnabled(MeshRenderer emissiveRenderer, bool emissionEnabled)
	{
		if (emissionEnabled)
		{
			s_matPropBlock.SetColor(s_propID_EmissionColor, _baseEmissionColor * 1f);
			emissiveRenderer.SetPropertyBlock(s_matPropBlock);
		}
		else
		{
			s_matPropBlock.SetColor(s_propID_EmissionColor, _baseEmissionColor * 0f);
			emissiveRenderer.SetPropertyBlock(s_matPropBlock);
		}
	}

	public void ClearNomaiAudio()
	{
		_textNodeToDisplay = null;
		TurnOffArrowEmission();
	}

	public void SetNomaiTextLine(NomaiTextLine line)
	{
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(line);
			_scanBeams[i].SetNomaiComputerRing(null);
			_scanBeams[i].SetNomaiVesselComputerRing(null);
		}
	}

	public void ClearNomaiTextLine()
	{
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(null);
		}
	}

	public void SetNomaiComputerRing(NomaiComputerRing ring)
	{
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(null);
			_scanBeams[i].SetNomaiComputerRing(ring);
			_scanBeams[i].SetNomaiVesselComputerRing(null);
		}
	}

	public void ClearNomaiComputerRing()
	{
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiComputerRing(null);
		}
	}

	public void SetNomaiVesselComputerRing(NomaiVesselComputerRing ring)
	{
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiTextLine(null);
			_scanBeams[i].SetNomaiComputerRing(null);
			_scanBeams[i].SetNomaiVesselComputerRing(ring);
		}
	}

	public void ClearNomaiVesselComputerRing()
	{
		for (int i = 0; i < _scanBeams.Length; i++)
		{
			_scanBeams[i].SetNomaiVesselComputerRing(null);
		}
	}

	public void SetNomaiText(NomaiText text, int textID)
	{
		if (_nomaiTextComponent != text || _currentTextID != textID)
		{
			_inNomaiAudioVolume = false;
			_nomaiTextComponent = text;
			_currentTextID = textID;
			_textNodeToDisplay = _nomaiTextComponent.GetTextNode(_currentTextID);
			_scrollRect.verticalNormalizedPosition = 1f;
			TurnOffArrowEmission();
		}
	}

	public void SetNomaiText(NomaiText text)
	{
		if (_nomaiTextComponent != text)
		{
			_inNomaiAudioVolume = false;
			_nomaiTextComponent = text;
			_currentTextID = -1;
			_textNodeToDisplay = _nomaiTextComponent.GetTextNode(_currentTextID);
			_scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	public void ClearNomaiText()
	{
		_nomaiTextComponent = null;
		_textNodeToDisplay = null;
	}

	public void SetTargetingGhostText(bool isTargetingGhostText)
	{
		_isTargetingGhostText = isTargetingGhostText;
	}

	protected virtual void Update()
	{
		bool flag = _nomaiTextComponent != null && _nomaiTextComponent.IsTranslated(_currentTextID);
		bool flag2 = !_isTargetingGhostText && !flag && _textNodeToDisplay != null;
		_centerTranslatePrompt.SetVisibility(flag2 && !_hasUsedTranslator);
		_translatePrompt.SetVisibility(flag2 && _hasUsedTranslator);
		if (!_hasUsedTranslator && flag && Locator.GetPlayerSuit().IsWearingHelmet())
		{
			PlayerData.SetPersistentCondition("HAS_USED_TRANSLATOR", state: true);
			_hasUsedTranslator = true;
		}
		UpdateTimeFreeze(flag, _nomaiTextComponent);
		if (_textNodeToDisplay != null && _textNodeToDisplay != _lastTextNodeToDisplay)
		{
			StopTranslating();
			SwitchTextNode(_textNodeToDisplay);
		}
		_lastTextNodeToDisplay = _textNodeToDisplay;
		if (_isTargetingGhostText)
		{
			_textField.text = UITextLibrary.GetString(_isTooCloseToTarget ? UITextType.TranslatorTooCloseWarning : UITextType.TranslatorUntranslatableWarning);
		}
		else if (_textNodeToDisplay == null)
		{
			_textField.text = "";
			_pageNumberTextField.text = "";
			StopTranslating();
		}
		else if (_isTooCloseToTarget)
		{
			_textField.text = UITextLibrary.GetString(UITextType.TranslatorTooCloseWarning);
		}
		else
		{
			DisplayTextNode();
		}
		_textField.rectTransform.sizeDelta = new Vector2(_textField.rectTransform.sizeDelta.x, _textField.preferredHeight);
		_scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, _textField.preferredHeight);
		float num = (float)_textField.fontSize / (_textField.preferredHeight - _scrollRect.viewport.sizeDelta.y);
		if (OWInput.IsNewlyPressed(InputLibrary.toolOptionUp))
		{
			_scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + num);
		}
		if (OWInput.IsNewlyPressed(InputLibrary.toolOptionDown))
		{
			_scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition - num);
		}
		_scrollPrompt.SetVisibility(_textField.preferredHeight > _scrollRect.viewport.sizeDelta.y);
		bool flag3 = _inNomaiAudioVolume && _nomaiTextComponent != null && _nomaiTextComponent.IsTranslated(1) && !_isTranslating && _nomaiTextComponent.GetNumTextBlocks() > 1;
		int num2 = _currentTextID;
		if (flag3)
		{
			int numTextBlocks = _nomaiTextComponent.GetNumTextBlocks();
			if (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft))
			{
				num2 = Mathf.Max(1, _currentTextID - 1);
			}
			if (OWInput.IsNewlyPressed(InputLibrary.toolOptionRight))
			{
				num2 = Mathf.Min(numTextBlocks, _currentTextID + 1);
			}
			if (num2 != _currentTextID)
			{
				_currentTextID = num2;
				if (_nomaiTextComponent != null && _inNomaiAudioVolume)
				{
					_textNodeToDisplay = _nomaiTextComponent.GetTextNode(_currentTextID);
					SetNomaiAudioArrowEmissions();
				}
			}
			if (_pageNumberTextField != null)
			{
				_pageNumberStrBuilder.Length = 0;
				_pageNumberStrBuilder.Append(num2);
				_pageNumberStrBuilder.Append(" / ");
				_pageNumberStrBuilder.Append(numTextBlocks);
				if (_pageNumberTextField.text != _pageNumberStrBuilder.ToString())
				{
					_pageNumberTextField.text = _pageNumberStrBuilder.ToString();
				}
			}
		}
		else if (_pageNumberTextField != null && _pageNumberTextField.text != "")
		{
			_pageNumberTextField.text = "";
		}
		_pagePrompt.SetVisibility(flag3);
	}

	protected void DisplayTextNode()
	{
		if (!_nomaiTextComponent.IsTranslated(_currentTextID))
		{
			if (OWInput.IsPressed(InputLibrary.toolActionPrimary) || (_inNomaiAudioVolume && _currentTextID > 1))
			{
				_translationTimeElapsed += Time.deltaTime;
				if (!_isTranslating)
				{
					_isTranslating = true;
					_audioController.PlayTranslateAudio();
				}
			}
			else if (_isTranslating)
			{
				StopTranslating();
			}
		}
		else if (_nomaiTextComponent.IsTranslated(_currentTextID))
		{
			_translationTimeElapsed += Time.deltaTime;
		}
		string text = "";
		bool flag = false;
		if (_translationTimeElapsed == 0f && !_nomaiTextComponent.IsTranslated(_currentTextID))
		{
			text = (_inNomaiAudioVolume ? UITextLibrary.GetString(UITextType.TranslatorUntranslatedRecordingWarning) : UITextLibrary.GetString(UITextType.TranslatorUntranslatedWritingWarning));
		}
		else
		{
			if (_translationTimeElapsed > (float)(_numTranslatedWords + 1) * _perWordTranslationTime && _numTranslatedWords < _listDisplayWords.Count)
			{
				_listDisplayWordsByLength[_numTranslatedWords].BeginTranslation(_perWordTranslationTime);
				_numTranslatedWords++;
			}
			for (int i = 0; i < _listDisplayWords.Count; i++)
			{
				_listDisplayWordsByLength[i].UpdateDisplayText(Time.deltaTime);
			}
			_strBuilder.Length = 0;
			bool flag2 = true;
			for (int j = 0; j < _listDisplayWords.Count; j++)
			{
				TranslatorWord translatorWord = _listDisplayWords[j];
				_strBuilder.Append(translatorWord.DisplayText);
				if (!translatorWord.IsTranslated())
				{
					flag2 = false;
				}
				if (translatorWord.TranslatedText.Contains("</i>") || translatorWord.TranslatedText.Contains("</b>") || translatorWord.TranslatedText.Contains("</u>") || translatorWord.TranslatedText.Contains("</size>"))
				{
					flag = true;
				}
			}
			text = _strBuilder.ToString();
			if (flag2)
			{
				StopTranslating();
				_nomaiTextComponent.SetAsTranslated(_currentTextID);
				SetNomaiAudioArrowEmissions();
			}
		}
		if (_nomaiTextComponent.UseDefaultFontOverride(_currentTextID))
		{
			if (_textField.font != _defaultPropFontDynamic)
			{
				_textField.font = _defaultPropFontDynamic;
				_textField.lineSpacing = _defaultFontSpacing;
			}
		}
		else if (flag && _textField.font != _dynamicFontInUse)
		{
			_textField.font = _dynamicFontInUse;
			_textField.lineSpacing = _fontSpacingInUse;
		}
		else if (!flag && _textField.font != _fontInUse)
		{
			_textField.font = _fontInUse;
			_textField.lineSpacing = _fontSpacingInUse;
		}
		_textField.text = text;
	}

	private void UpdateTimeFreeze(bool isTargetTextTranslated, NomaiText nomaiText)
	{
		bool flag = PlayerData.GetFreezeTimeWhileReadingTranslator() && isTargetTextTranslated && !Locator.GetGlobalMusicController().IsEndTimesPlaying() && !_isTooCloseToTarget;
		if (flag)
		{
			if (PlayerState.InZeroG())
			{
				OWRigidbody attachedOWRigidbody = nomaiText.GetAttachedOWRigidbody();
				Vector3 vector = Locator.GetPlayerBody().GetVelocity() - attachedOWRigidbody.GetVelocity();
				if (_jetpackModel.GetAngularVelocity().magnitude > 0.01f || vector.magnitude > 0.1f)
				{
					flag = false;
				}
			}
			if (Time.time < _equipTime + 1.3f || OWInput.GetAxisValue(InputLibrary.look).magnitude > 0f || OWInput.GetAxisValue(InputLibrary.moveXZ).magnitude > 0f || OWInput.GetValue(InputLibrary.thrustUp) > 0f || OWInput.GetValue(InputLibrary.thrustDown) > 0f)
			{
				flag = false;
			}
			for (int i = 0; i < _scanBeams.Length; i++)
			{
				if (_scanBeams[i].IsSwitching())
				{
					flag = false;
					break;
				}
			}
		}
		if (!_isTimeFrozen && flag)
		{
			OWTime.Pause(OWTime.PauseType.Reading);
			_isTimeFrozen = true;
		}
		else if (_isTimeFrozen && !flag)
		{
			OWTime.Unpause(OWTime.PauseType.Reading);
			_isTimeFrozen = false;
		}
	}

	private void SwitchTextNode(string textNode)
	{
		_translationTimeElapsed = 0f;
		_fullTextTranslationTime = (_nomaiTextComponent.IsTranslated(_currentTextID) ? _totalAlreadyTranslatedTime : _totalTranslateTime);
		_translatedText = CleanupText(textNode);
		_listDisplayWords.Clear();
		_listDisplayWordsByLength.Clear();
		int num = 0;
		while (num >= 0 && num < _translatedText.Length)
		{
			int num2 = _translatedText.IndexOf(' ', num);
			if (num2 == -1)
			{
				num2 = _translatedText.Length - 1;
			}
			TranslatorWord item = new TranslatorWord(_translatedText.Substring(num, num2 + 1 - num), num, num2 + 1, _nomaiTextComponent.IsTranslated(_currentTextID), _fullTextTranslationTime);
			_listDisplayWords.Add(item);
			_listDisplayWordsByLength.Add(item);
			num = num2 + 1;
		}
		_listDisplayWordsByLength.Sort(_lengthComparer);
		_numTranslatedWords = 0;
		_perWordTranslationTime = _fullTextTranslationTime / (float)_listDisplayWords.Count;
	}

	private void StopTranslating()
	{
		if (_isTranslating)
		{
			_audioController.StopTranslateAudio();
			_isTranslating = false;
			_translationTimeElapsed = 0f;
		}
	}

	private string CleanupText(string text)
	{
		return text.Trim();
	}
}
