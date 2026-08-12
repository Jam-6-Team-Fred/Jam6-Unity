using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class SettingsMenuView : MonoBehaviour
{
	[SerializeField]
	private SettingsMenuModel _model;

	[SerializeField]
	private TabbedMenu _mainSettingsMenu;

	[SerializeField]
	private Menu _gameplayMenu;

	[SerializeField]
	private Menu _audioLangMenu;

	[SerializeField]
	private Menu _controlsMenu;

	[SerializeField]
	private Menu _graphicsMenu;

	[SerializeField]
	private MenuConfirmCancelAction _confirmCancelAction;

	[SerializeField]
	private MenuOption[] _listSettingsOptions;

	[SerializeField]
	private MenuValueOption[] _listGraphicsSettingsOptions;

	[SerializeField]
	private MenuValueOption _confirmToggleOption;

	[SerializeField]
	private KeyRebindingElement[] _listRebindableOptions;

	[Space(20f)]
	[SerializeField]
	private ButtonWithHotkeyImageElement _resetToDefaultButton;

	[SerializeField]
	private ButtonWithHotkeyImageElement _closeMenuButton;

	[SerializeField]
	private ButtonWithHotkeyImageElement _cancelRebindingButton;

	[Space(10f)]
	[SerializeField]
	private SubmitAction _resetSettingsActionByCommand;

	[SerializeField]
	private SubmitAction _resetSettingsAction;

	[SerializeField]
	private SubmitAction _cancelRebindingActionByCommand;

	[SerializeField]
	private SubmitAction _cancelRebindingAction;

	[SerializeField]
	private SubmitAction _closeMenuAction;

	[Space(10f)]
	[SerializeField]
	private Image _consoleConfirmOptionImgOne;

	[SerializeField]
	private Image _consoleConfirmOptionImgTwo;

	[Space(10f)]
	[SerializeField]
	private InputEventListener _cancelRebindingButtonListener;

	[SerializeField]
	private GameObject _raycastBlocker;

	private MultiSelectionListElement _controllerSelectOption;

	private RebindingState _rebindState;

	private float _rebindStateEndTime;

	private Dictionary<SettingsID, SettingsMenuData> _settingsDataWithEvents;

	private List<SettingsMenuData> _listSettingsOptionData;

	private List<SettingsMenuData> _listGfxSettingsOptionData;

	private ScreenPrompt _tabPrompt;

	private ScreenPrompt _exitPrompt;

	private ScreenPrompt _resetToDefaultsPrompt;

	private UITextType _resetToDefaultsPromptText;

	private ScreenPrompt _cancelRebindingPrompt;

	private bool _pointerUpOverCancelButton;

	private bool _initialized;

	private void Start()
	{
		MenuStackManager.SharedInstance.OnMenuPush += OnSettingsMenuPush;
		_cancelRebindingButtonListener.OnPointerEnterEvent += OnPointerEnterCancelRebindingButton;
		_cancelRebindingButtonListener.OnPointerExitEvent += OnPointerExitCancelRebindingButton;
		EnableRaycastBlocker(value: false);
	}

	private void Update()
	{
		if (OWInput.IsInputMode(InputMode.Menu) && _resetSettingsActionByCommand.gameObject.activeInHierarchy && OWInput.IsNewlyPressed(InputLibrary.setDefaults))
		{
			_resetSettingsActionByCommand.Submit();
		}
		if (_rebindState != null && _rebindState.IsValid)
		{
			UpdateRebinding();
		}
	}

	private void UpdateRebinding()
	{
		if (OWInput.IsPressed(InputLibrary.setDefaults))
		{
			InputLibrary.setDefaults.BlockNextRelease();
		}
		if (OWInput.IsPressed(InputLibrary.cancel))
		{
			InputLibrary.cancel.BlockNextRelease();
		}
		if (OWInput.IsPressed(InputLibrary.cancelRebinding1, InputMode.Rebinding) && OWInput.IsPressed(InputLibrary.cancelRebinding2, InputMode.Rebinding))
		{
			_cancelRebindingActionByCommand.Submit();
		}
		else if (_rebindState.HasInputEvents())
		{
			_rebindState.ProcessInputCandidates();
		}
	}

	private void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		OWInput.SharedInputManager.OnUpdateInputMode += OnUpdateInputMode;
		ButtonPromptLibrary.OnUpdateButtonPromptConfig += OnButtonImagesChanged;
		_settingsDataWithEvents = new Dictionary<SettingsID, SettingsMenuData>();
		_listSettingsOptionData = new List<SettingsMenuData>();
		for (int i = 0; i < _listSettingsOptions.Length; i++)
		{
			MenuOption menuOption = _listSettingsOptions[i];
			if (menuOption.ShouldEnable())
			{
				SettingsMenuData settingsMenuData = default(SettingsMenuData);
				settingsMenuData.uiMenuOption = menuOption;
				settingsMenuData.id = menuOption.GetSettingsID();
				settingsMenuData.labelTextField = menuOption.GetLabelField();
				settingsMenuData.secondaryTextField = menuOption.GetSecondaryTextField();
				MenuValueOption menuValueOption = menuOption as MenuValueOption;
				if (menuValueOption != null)
				{
					settingsMenuData.dependentMenuOption = menuValueOption.GetDependentMenuOption();
				}
				if (settingsMenuData.id == SettingsID.INPUT_ACTIVE_CONTROLLER)
				{
					_controllerSelectOption = (MultiSelectionListElement)menuOption;
					_controllerSelectOption.OnListUpdated += OnControllerListUpdated;
				}
				else
				{
					menuValueOption.OnMenuOptionValueChanged += OnMenuOptionChanged;
					_settingsDataWithEvents.Add(settingsMenuData.id, settingsMenuData);
				}
				_listSettingsOptionData.Add(settingsMenuData);
			}
		}
		_listGfxSettingsOptionData = new List<SettingsMenuData>();
		for (int j = 0; j < _listGraphicsSettingsOptions.Length; j++)
		{
			MenuValueOption menuValueOption2 = _listGraphicsSettingsOptions[j];
			if (menuValueOption2.ShouldEnable())
			{
				SettingsMenuData settingsMenuData2 = default(SettingsMenuData);
				settingsMenuData2.uiMenuOption = menuValueOption2;
				settingsMenuData2.id = menuValueOption2.GetSettingsID();
				settingsMenuData2.labelTextField = menuValueOption2.GetLabelField();
				settingsMenuData2.secondaryTextField = menuValueOption2.GetSecondaryTextField();
				settingsMenuData2.dependentMenuOption = menuValueOption2.GetDependentMenuOption();
				menuValueOption2.OnMenuOptionValueChanged += OnMenuOptionChanged;
				_settingsDataWithEvents.Add(settingsMenuData2.id, settingsMenuData2);
				_listGfxSettingsOptionData.Add(settingsMenuData2);
			}
		}
		InitSettingsTextLabels();
		InitButtonPrompts();
		if (_confirmToggleOption.ShouldEnable())
		{
			InitConsoleConfirmToggle();
		}
		_gameplayMenu.OnActivateMenu += OnActivateGameplayMenu;
		_audioLangMenu.OnActivateMenu += OnActivateAudioLangMenu;
		_controlsMenu.OnActivateMenu += OnActivateControlsMenu;
		_graphicsMenu.OnActivateMenu += OnActivateGraphicsMenu;
		OWInput.SharedInputManager.OnUpdateInputDevice += RefreshMenuButtonDisplay;
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		_resetToDefaultButton.OnPointerPressAndMoveOut += ReselectLastMenuItem;
		_closeMenuButton.OnPointerPressAndMoveOut += ReselectLastMenuItem;
		if (_confirmCancelAction != null)
		{
			_confirmCancelAction.OnMenuCancel += OnMenuCancelEvent;
		}
		_resetSettingsActionByCommand.OnSubmitAction += OnResetSettingSubmit;
		_resetSettingsAction.OnSubmitAction += OnResetSettingSubmit;
		_cancelRebindingActionByCommand.OnSubmitAction += OnCancelRebindingSubmit;
		_cancelRebindingAction.OnSubmitAction += OnCancelRebindingSubmit;
		_closeMenuAction.OnSubmitAction += OnCloseSubmit;
		_cancelRebindingButton.gameObject.SetActive(value: false);
		_initialized = true;
	}

	private void InitButtonPrompts()
	{
		_tabPrompt = new ScreenPrompt(InputLibrary.tabL, InputLibrary.tabR, UITextLibrary.GetString(UITextType.MenuOptionTabPrompt), ScreenPrompt.MultiCommandType.POS_NEG);
		_exitPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuOptionQuitPrompt));
		_resetToDefaultsPromptText = UITextType.MenuPrompt_SetDefaultsGameplay;
		_resetToDefaultsPrompt = new ScreenPrompt(InputLibrary.setDefaults, UITextLibrary.GetString(_resetToDefaultsPromptText));
		_cancelRebindingPrompt = new ScreenPrompt(InputLibrary.cancelRebinding1, InputLibrary.cancelRebinding2, UITextLibrary.GetString(UITextType.KeyRebindingCancelRebinding), ScreenPrompt.MultiCommandType.HOLD_BOTH);
		RefreshMenuButtonDisplay();
	}

	private void InitSettingsTextLabels()
	{
		SettingsUiUtil.InitMenuOptionTextLabels(_listSettingsOptions, _confirmToggleOption);
		MenuOption[] listGraphicsSettingsOptions = _listGraphicsSettingsOptions;
		SettingsUiUtil.InitMenuOptionTextLabels(listGraphicsSettingsOptions);
		SettingsUiUtil.InitRebindableOptionTextLabels(_listRebindableOptions);
	}

	public void EnableRaycastBlocker(bool value)
	{
		_raycastBlocker.SetActive(value);
	}

	private void OnPointerEnterCancelRebindingButton(PointerEventData eventData, Selectable selectable)
	{
		_pointerUpOverCancelButton = true;
	}

	private void OnPointerExitCancelRebindingButton(PointerEventData eventData, Selectable selectable)
	{
		_pointerUpOverCancelButton = false;
	}

	public bool IsPointerOverCancelButton()
	{
		return _pointerUpOverCancelButton;
	}

	private void RefreshMenuButtonDisplay()
	{
		_cancelRebindingButton.SetPrompt(_cancelRebindingPrompt, InputMode.Rebinding);
		_resetToDefaultButton.SetPrompt(_resetToDefaultsPrompt);
		_closeMenuButton.SetPrompt(_exitPrompt);
	}

	public void RefreshRebindingDisplay(RebindableID rebindableId)
	{
		for (int i = 0; i < _listRebindableOptions.Length; i++)
		{
			if (_listRebindableOptions[i].GetRebindableID() == rebindableId)
			{
				_listRebindableOptions[i].UpdateDisplay();
			}
		}
	}

	public bool ReadyToRebind()
	{
		if (Time.realtimeSinceStartup - _rebindStateEndTime < 0.2f)
		{
			return false;
		}
		return true;
	}

	public void RegisterRebindingState(RebindingState rebindingState)
	{
		if (_rebindState != null && _rebindState.IsValid)
		{
			Debug.LogError("SettingsMenuView.RegisterRebindingState Cannot start rebinding operation when another in progress");
		}
		_rebindState = rebindingState;
	}

	public void UnregisterRebindingState(RebindingState rebindingState)
	{
		if (rebindingState != _rebindState)
		{
			Debug.LogError("SettingsMenuView.UnregisterRebindingState Invalid rebinding cancel request");
		}
		_rebindState = null;
		_rebindStateEndTime = Time.realtimeSinceStartup;
	}

	public void NotifyBindingChanged()
	{
		OWInput.NotifyBindingChanged();
		_model.InitializeInputRebindables(_listRebindableOptions);
	}

	private void OnCancelRebindingSubmit()
	{
		if (_rebindState.IsValid)
		{
			_rebindState.CancelRebinding();
		}
	}

	private void OnResetSettingSubmit()
	{
		ResetToDefaultSettings();
		PlayerData.SaveInputSettings();
		NotifyBindingChanged();
	}

	private void OnCloseSubmit()
	{
		ExitMenu();
	}

	private void OnActivateGameplayMenu()
	{
		_resetToDefaultsPromptText = UITextType.MenuPrompt_SetDefaultsGameplay;
		_resetToDefaultsPrompt.SetText(UITextLibrary.GetString(_resetToDefaultsPromptText));
		_resetToDefaultButton.RefreshTextAndImages();
	}

	private void OnActivateAudioLangMenu()
	{
		_resetToDefaultsPromptText = UITextType.MenuPrompt_SetDefaultsAudioLang;
		_resetToDefaultsPrompt.SetText(UITextLibrary.GetString(_resetToDefaultsPromptText));
		_resetToDefaultButton.RefreshTextAndImages();
	}

	private void OnActivateControlsMenu()
	{
		_resetToDefaultsPromptText = UITextType.MenuPrompt_SetDefaultsControls;
		_resetToDefaultsPrompt.SetText(UITextLibrary.GetString(_resetToDefaultsPromptText));
		_resetToDefaultButton.RefreshTextAndImages();
	}

	private void OnActivateGraphicsMenu()
	{
		_resetToDefaultsPromptText = UITextType.MenuPrompt_SetDefaultsGraphics;
		_resetToDefaultsPrompt.SetText(UITextLibrary.GetString(_resetToDefaultsPromptText));
		_resetToDefaultButton.RefreshTextAndImages();
	}

	private void OnLanguageChanged()
	{
		_resetToDefaultsPrompt.SetText(UITextLibrary.GetString(_resetToDefaultsPromptText));
		_exitPrompt.SetText(UITextLibrary.GetString(UITextType.MenuOptionQuitPrompt));
		_cancelRebindingPrompt.SetText(UITextLibrary.GetString(UITextType.KeyRebindingCancelRebinding));
		RefreshMenuButtonDisplay();
	}

	private void OnMenuOptionChanged(SettingsID id, MenuValueOption option)
	{
		SettingsMenuData settingsMenuData = _settingsDataWithEvents[id];
		_model.UpdateCachedSetting(settingsMenuData);
		_model.RealtimeOptionUpdate(settingsMenuData);
	}

	private void OnControllerListUpdated(MultiSelectionListElement.ListEntry[] listEntries)
	{
		Debug.Log("OnControllerListUpdated");
		_model.ChangeControllersEnabled(listEntries);
	}

	private void OnConsoleConfirmToggleUpdated(SettingsID id, MenuValueOption option)
	{
		_model.SwapConfirmAndCancelBinding();
	}

	private void OnDestroy()
	{
		MenuStackManager.SharedInstance.OnMenuPush -= OnSettingsMenuPush;
		_cancelRebindingButtonListener.OnPointerEnterEvent -= OnPointerEnterCancelRebindingButton;
		_cancelRebindingButtonListener.OnPointerExitEvent -= OnPointerExitCancelRebindingButton;
		if (!_initialized)
		{
			return;
		}
		ButtonPromptLibrary.OnUpdateButtonPromptConfig -= OnButtonImagesChanged;
		OWInput.SharedInputManager.OnUpdateInputMode -= OnUpdateInputMode;
		_gameplayMenu.OnActivateMenu -= OnActivateGameplayMenu;
		_audioLangMenu.OnActivateMenu -= OnActivateAudioLangMenu;
		_controlsMenu.OnActivateMenu -= OnActivateControlsMenu;
		_graphicsMenu.OnActivateMenu -= OnActivateGraphicsMenu;
		foreach (KeyValuePair<SettingsID, SettingsMenuData> settingsDataWithEvent in _settingsDataWithEvents)
		{
			if (settingsDataWithEvent.Key == SettingsID.INPUT_ACTIVE_CONTROLLER)
			{
				((MultiSelectionListElement)settingsDataWithEvent.Value.uiMenuOption).OnListUpdated -= OnControllerListUpdated;
			}
			else
			{
				((MenuValueOption)settingsDataWithEvent.Value.uiMenuOption).OnMenuOptionValueChanged -= OnMenuOptionChanged;
			}
		}
		OWInput.SharedInputManager.OnUpdateInputDevice -= RefreshMenuButtonDisplay;
		TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
		_resetToDefaultButton.OnPointerPressAndMoveOut -= ReselectLastMenuItem;
		_closeMenuButton.OnPointerPressAndMoveOut -= ReselectLastMenuItem;
		_resetSettingsActionByCommand.OnSubmitAction -= OnResetSettingSubmit;
		_resetSettingsAction.OnSubmitAction -= OnResetSettingSubmit;
		_closeMenuAction.OnSubmitAction -= OnCloseSubmit;
	}

	private void OnSettingsMenuPush(Menu menu)
	{
		if (menu == _mainSettingsMenu)
		{
			Initialize();
			_model.Initialize();
			_model.InitializeInputRebindables(_listRebindableOptions);
		}
	}

	private void GetActiveMenuOptions(ref List<SettingsMenuData> menuOptionsData, ref List<KeyRebindingElement> rebindingOptions)
	{
		if (_gameplayMenu.IsMenuEnabled())
		{
			GetSettingsMenuDataFromMenu(_gameplayMenu, ref menuOptionsData);
		}
		else if (_audioLangMenu.IsMenuEnabled())
		{
			GetSettingsMenuDataFromMenu(_audioLangMenu, ref menuOptionsData);
		}
		else if (_graphicsMenu.IsMenuEnabled())
		{
			GetSettingsMenuDataFromMenu(_graphicsMenu, ref menuOptionsData);
		}
		else if (_controlsMenu.IsMenuEnabled())
		{
			rebindingOptions.AddRange(_listRebindableOptions);
			if (_confirmToggleOption.ShouldEnable())
			{
				SettingsMenuData item = default(SettingsMenuData);
				item.uiMenuOption = _confirmToggleOption;
				item.id = _confirmToggleOption.GetSettingsID();
				item.labelTextField = _confirmToggleOption.GetLabelField();
				item.secondaryTextField = _confirmToggleOption.GetSecondaryTextField();
				item.dependentMenuOption = _confirmToggleOption.GetDependentMenuOption();
				menuOptionsData.Add(item);
			}
		}
	}

	private void GetSettingsMenuDataFromMenu(Menu menu, ref List<SettingsMenuData> menuOptionsData)
	{
		MenuOption[] menuOptions = menu.GetMenuOptions();
		List<SettingsMenuData> list = new List<SettingsMenuData>();
		for (int i = 0; i < menuOptions.Length; i++)
		{
			MenuValueOption menuValueOption = menuOptions[i] as MenuValueOption;
			if (menuValueOption != null)
			{
				SettingsMenuData item = default(SettingsMenuData);
				item.uiMenuOption = menuValueOption;
				item.id = menuValueOption.GetSettingsID();
				item.labelTextField = menuValueOption.GetLabelField();
				item.secondaryTextField = menuValueOption.GetSecondaryTextField();
				item.dependentMenuOption = menuValueOption.GetDependentMenuOption();
				list.Add(item);
			}
		}
		menuOptionsData.AddRange(list);
		Menu[] subMenus = menu.GetSubMenus();
		for (int j = 0; j < subMenus.Length; j++)
		{
			GetSettingsMenuDataFromMenu(subMenus[j], ref menuOptionsData);
		}
	}

	private void InitConsoleConfirmToggle()
	{
		_confirmToggleOption.OnMenuOptionValueChanged += OnConsoleConfirmToggleUpdated;
		_confirmToggleOption.GetRequiredComponent<Selectable>();
		Texture2D buttonTexture = ButtonPromptLibrary.SharedInstance.GetButtonTexture(JoystickButton.FaceDown);
		_consoleConfirmOptionImgOne.sprite = Sprite.Create(buttonTexture, new Rect(0f, 0f, buttonTexture.width, buttonTexture.height), new Vector2(0.5f, 0.5f), buttonTexture.width);
		buttonTexture = ButtonPromptLibrary.SharedInstance.GetButtonTexture(JoystickButton.FaceRight);
		_consoleConfirmOptionImgTwo.sprite = Sprite.Create(buttonTexture, new Rect(0f, 0f, buttonTexture.width, buttonTexture.height), new Vector2(0.5f, 0.5f), buttonTexture.width);
		if (!(InputLibrary.menuConfirm is ISingleInputCommand singleInputCommand) || !singleInputCommand.TryCastAction<ISingleAction>(out var castAction))
		{
			return;
		}
		ReadOnlyArray<UnityEngine.InputSystem.InputBinding> bindings = castAction.Action.bindings;
		bool inputBool = true;
		foreach (UnityEngine.InputSystem.InputBinding item in bindings)
		{
			if (item.path.StartsWith("<Gamepad>"))
			{
				inputBool = (item.path.EndsWith("buttonSouth") ? true : false);
				break;
			}
		}
		_confirmToggleOption.Initialize(inputBool);
	}

	public void ApplySettingsToUI(SettingsSave settingsToApply)
	{
		SettingsUiUtil.ApplySettingsToUi(_listSettingsOptions, settingsToApply);
		if (_controllerSelectOption != null)
		{
			List<MultiSelectionListElement.ListEntry> controllerUIList = _model.GetControllerUIList();
			if (controllerUIList.Count == 0)
			{
				_controllerSelectOption.GetSelectable().gameObject.SetActive(value: false);
				return;
			}
			_controllerSelectOption.GetSelectable().gameObject.SetActive(value: false);
			_controllerSelectOption.Initialize(controllerUIList.ToArray());
		}
	}

	public void ApplyGraphicSettingsToUI(GraphicSettings settingsToApply)
	{
		SettingsUiUtil.ApplyGraphicSettingsToUi(_listGraphicsSettingsOptions, settingsToApply);
	}

	public void UpdateKeyRebindingElementDisplays()
	{
		for (int i = 0; i < _listRebindableOptions.Length; i++)
		{
			_listRebindableOptions[i].UpdateDisplay();
		}
		if (!(InputLibrary.menuConfirm is ISingleInputCommand singleInputCommand) || !singleInputCommand.TryCastAction<ISingleAction>(out var castAction))
		{
			return;
		}
		ReadOnlyArray<UnityEngine.InputSystem.InputBinding> bindings = castAction.Action.bindings;
		bool inputBool = true;
		foreach (UnityEngine.InputSystem.InputBinding item in bindings)
		{
			if (item.path.StartsWith("<Gamepad>"))
			{
				inputBool = (item.path.EndsWith("buttonSouth") ? true : false);
				break;
			}
		}
		_confirmToggleOption.Initialize(inputBool);
	}

	public void ResetToDefaultSettings()
	{
		List<SettingsMenuData> menuOptionsData = new List<SettingsMenuData>();
		List<KeyRebindingElement> rebindingOptions = new List<KeyRebindingElement>();
		GetActiveMenuOptions(ref menuOptionsData, ref rebindingOptions);
		_model.ResetToDefaultSettings(menuOptionsData, rebindingOptions);
		if (Locator.GetEventSystem().currentSelectedGameObject == _resetToDefaultButton.gameObject)
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(_mainSettingsMenu.GetLastSelectedTabButton().GetMenu().GetSelectOnActivate());
		}
		Locator.GetMenuAudioController().PlayResetDefaults();
	}

	public void ReselectLastMenuItem()
	{
		Selectable lastSelected = _mainSettingsMenu.GetLastSelectedTabButton().GetMenu().GetLastSelected();
		if (lastSelected != null)
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(lastSelected);
		}
	}

	private void OnMenuCancelEvent(GameObject selectedObject, BaseEventData eventData)
	{
		ExitMenu();
	}

	public void ExitMenu()
	{
		List<SettingsMenuData> list = new List<SettingsMenuData>();
		list.AddRange(_listSettingsOptionData);
		list.AddRange(_listGfxSettingsOptionData);
		_model.UpdateAllCachedSettings(list.ToArray());
		_model.SaveChanges();
		_confirmCancelAction.EnableConfirmPopup(enable: false);
	}

	private void OnUpdateInputMode()
	{
		if (OWInput.SharedInputManager.GetInputMode() == InputMode.Rebinding)
		{
			_cancelRebindingButton.gameObject.SetActive(value: true);
			_resetToDefaultButton.gameObject.SetActive(value: false);
			_closeMenuButton.gameObject.SetActive(value: false);
		}
		else
		{
			_cancelRebindingButton.gameObject.SetActive(value: false);
			_resetToDefaultButton.gameObject.SetActive(value: true);
			_closeMenuButton.gameObject.SetActive(value: true);
		}
	}

	private void OnButtonImagesChanged()
	{
		for (int i = 0; i < _listRebindableOptions.Length; i++)
		{
			_listRebindableOptions[i].UpdateDisplay(forceRefresh: true);
		}
	}
}
