using UnityEngine;
using UnityEngine.UI;

public class MenuGammaSetting : Menu
{
	public delegate void GammaMenuEvent(bool firstTimeRun);

	[SerializeField]
	private SliderElement _gammaSlider;

	[SerializeField]
	private ButtonWithHotkeyImageElement _closeMenuButton;

	[SerializeField]
	private SubmitAction _closeMenuAction;

	[SerializeField]
	private Image _gammaImageReference;

	[Space(10f)]
	[SerializeField]
	private AnimationCurve _fadeCurve;

	[SerializeField]
	private float _menuFadeInDelay;

	[SerializeField]
	private float _menuFadeInDuration = 3f;

	[SerializeField]
	private float _menuFadeOutDelay;

	[SerializeField]
	private float _menuFadeOutDuration = 3f;

	[SerializeField]
	private CanvasGroupFadeController _menuFadeController;

	[Space(10f)]
	[SerializeField]
	private Text _instructionalText;

	[SerializeField]
	private Text _echoesGammaMessage;

	private float _menuFadeStartTime;

	private bool _fadingIn;

	private bool _fadingOut;

	private IInputCommands _exitMenuCommand;

	private ScreenPrompt _exitMenuPrompt;

	private Selectable _unitySelectable;

	private Material _uiGammaMaterial;

	private int _propID_CustomGamma;

	private GraphicSettings _gfxSettingsOnInitialStartup;

	private IProfileManager _profileManager;

	private bool _firstTimeSetup;

	private bool _completedFirstTimeGammaSetup;

	private int _gammaSliderValue;

	public event GammaMenuEvent OnGammaMenuFadeOutComplete;

	protected override void Awake()
	{
		_echoesGammaMessage.enabled = false;
		_fadingIn = false;
		_fadingOut = false;
		_firstTimeSetup = false;
		_menuFadeController.UseUnscaledTime();
		base.Awake();
	}

	private void StartFadeIn()
	{
		_menuFadeController.Reset();
		_fadingIn = true;
		Locator.GetMenuInputModule().DisableInputs();
		_menuFadeController.FadeTo(1f, _menuFadeInDuration, _menuFadeInDelay);
		_menuFadeStartTime = Time.unscaledTime;
	}

	private void StartFadeOut()
	{
		_menuFadeController.Reset(reversed: true);
		_fadingOut = true;
		Locator.GetMenuInputModule().DisableInputs();
		_menuFadeController.FadeTo(0f, _menuFadeOutDuration, _menuFadeOutDelay);
		_menuFadeStartTime = Time.unscaledTime;
		_closeMenuButton.gameObject.SetActive(value: false);
		_gammaSlider.gameObject.SetActive(value: false);
		_instructionalText.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (_fadingOut)
		{
			_menuFadeController.Update(_fadeCurve);
			if (Time.unscaledTime > _menuFadeStartTime + _menuFadeOutDuration + _menuFadeOutDelay)
			{
				FadeOutComplete();
			}
		}
		else if (_fadingIn)
		{
			_menuFadeController.Update(_fadeCurve);
			if (Time.unscaledTime > _menuFadeStartTime + _menuFadeInDuration + _menuFadeInDelay)
			{
				FadeInComplete();
			}
		}
		else if (_enabledMenu && _exitMenuCommand.IsNewlyPressed())
		{
			_closeMenuAction.Submit();
		}
	}

	public void ActivateAsFirstTimeSetup()
	{
		_firstTimeSetup = true;
		OWInput.ChangeInputMode(InputMode.Menu);
		base.OnDeactivateMenu += OnDeactivateMenuFirstTimeSetup;
		_gfxSettingsOnInitialStartup = PlayerData.GetGraphicSettings();
		PopulateGammaSliderUiValues();
		_gammaSlider.OnValueChanged += OnSliderValueChanged;
		EnableMenu(value: true);
		StartFadeIn();
	}

	private void FadeOutComplete()
	{
		Locator.GetMenuInputModule().EnableInputs();
		OWInput.RestorePreviousInputs();
		_fadingOut = false;
		_menuActivationRoot.gameObject.SetActive(value: false);
		if (this.OnGammaMenuFadeOutComplete != null)
		{
			this.OnGammaMenuFadeOutComplete(firstTimeRun: true);
		}
		_closeMenuButton.gameObject.SetActive(value: true);
		_gammaSlider.gameObject.SetActive(value: true);
		_instructionalText.gameObject.SetActive(value: true);
		_menuFadeController.Reset(reversed: true);
	}

	private void FadeInComplete()
	{
		Locator.GetMenuInputModule().EnableInputs();
		_fadingIn = false;
	}

	private void OnDeactivateMenuFirstTimeSetup()
	{
		base.OnDeactivateMenu -= OnDeactivateMenuFirstTimeSetup;
		StartFadeOut();
		_completedFirstTimeGammaSetup = true;
	}

	public override void EnableMenu(bool value)
	{
		if (value == _enabledMenu)
		{
			return;
		}
		_enabledMenu = value;
		if (_enabledMenu && !_initialized)
		{
			InitializeMenu();
		}
		if (_addToMenuStackManager)
		{
			if (_enabledMenu)
			{
				MenuStackManager.SharedInstance.Push(this, keepPreviousMenuVisible: false, muteSoundEffect: true);
			}
			else if (MenuStackManager.SharedInstance.Peek() == this)
			{
				MenuStackManager.SharedInstance.Pop();
			}
			else
			{
				Debug.LogError("Cannot disable Menu unless it is on the top the MenuLayerManager stack. Current menu on top: " + MenuStackManager.SharedInstance.Peek().gameObject.name);
			}
		}
		else if (_enabledMenu)
		{
			Activate();
			bool flag = false;
			for (int i = 0; i < _subMenus.Length; i++)
			{
				if (_subMenus[i].IsMenuEnabled() && _subMenus[i].GetSelectOnActivate() != null)
				{
					flag = true;
					break;
				}
			}
			if (_selectOnActivate != null && !flag)
			{
				SelectableAudioPlayer component = _selectOnActivate.GetComponent<SelectableAudioPlayer>();
				if (component != null)
				{
					component.SilenceNextSelectEvent();
				}
				Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnActivate);
				_lastSelected = _selectOnActivate;
			}
		}
		else
		{
			base.Deactivate();
		}
	}

	protected override void InitializeMenu()
	{
		_closeMenuButton.OnPointerPressAndMoveOut += ReselectLastMenuItem;
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		_propID_CustomGamma = Shader.PropertyToID("_CustomGamma");
		_unitySelectable = _gammaSlider.GetSelectable();
		_uiGammaMaterial = _gammaImageReference.material;
		base.InitializeMenu();
	}

	private void OnSliderValueChanged()
	{
		_gammaSliderValue = _gammaSlider.GetValue();
		_gfxSettingsOnInitialStartup.SetSliderValGamma(_gammaSliderValue);
		PopulateGammaSliderUiValues();
		GlobalMessenger<GraphicSettings>.FireEvent("GraphicSettingsUpdated", _gfxSettingsOnInitialStartup);
	}

	private void PopulateGammaSliderUiValues()
	{
		_gammaSlider.Initialize(_gfxSettingsOnInitialStartup.GetSliderValGamma());
		float num = _gfxSettingsOnInitialStartup.gammaValue * 2f - 1f;
		_gammaSlider.GetSecondaryTextField().text = ((num > 0f) ? ("+" + num.ToString("F1")) : num.ToString("F1"));
	}

	public override void Activate()
	{
		_completedFirstTimeGammaSetup = false;
		_exitMenuCommand = InputLibrary.GetInputCommand(InputConsts.InputCommandType.MENU_CONFIRM);
		_exitMenuPrompt = new ScreenPrompt(_exitMenuCommand, UITextLibrary.GetString(UITextType.KeyRebindingUpdatePopupContinueBtn));
		_closeMenuButton.SetPrompt(_exitMenuPrompt);
		OnGraphicSettingsUpdated(PlayerData.GetGraphicSettings());
		base.Activate();
	}

	public override void Deactivate(bool remainVisible)
	{
		if (_firstTimeSetup)
		{
			_firstTimeSetup = false;
			_gammaSlider.OnValueChanged -= OnSliderValueChanged;
			base.Deactivate(remainVisible: true);
		}
		else
		{
			base.Deactivate(remainVisible);
		}
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicsSettings)
	{
		if (_uiGammaMaterial != null)
		{
			float value = 2f / Mathf.Pow(2f, 0.4f + 2f * graphicsSettings.gammaValue * 0.6f);
			_uiGammaMaterial.SetFloat(_propID_CustomGamma, value);
		}
	}

	public bool HasRunFirstTimeGammaSetup()
	{
		return _completedFirstTimeGammaSetup;
	}

	public int GetFirstTimeGammaSetupValue()
	{
		return _gammaSliderValue;
	}

	public void ReselectLastMenuItem()
	{
		if (_unitySelectable != null)
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(_unitySelectable);
		}
	}

	private void OnLanguageChanged()
	{
		_exitMenuPrompt.SetText(UITextLibrary.GetString(UITextType.KeyRebindingUpdatePopupContinueBtn));
	}

	private void OnDestroy()
	{
		if (_initialized)
		{
			_closeMenuButton.OnPointerPressAndMoveOut -= ReselectLastMenuItem;
			GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", OnGraphicSettingsUpdated);
			TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
		}
	}
}
