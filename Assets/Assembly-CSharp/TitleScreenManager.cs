using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
	[SerializeField]
	private TitleScreenAnimation _cameraController;

	[SerializeField]
	private ProfileMenuManager _profileMenuManager;

	[SerializeField]
	private TitleButtonPrompt _titleButtonPrompt;

	[SerializeField]
	private TitleAnimationController _gfxController;

	[SerializeField]
	private OWMenuInputModule _inputModule;

	[SerializeField]
	private CanvasGroup _titleMenuRaycastBlocker;

	[SerializeField]
	private InputUpdateNotifier _inputUpdateNotifier;

	[SerializeField]
	private TitleCodeInputManager _titleCodeManager;

	[SerializeField]
	private MenuGammaSetting _firstTimeGammaSetup;

	[Space(10f)]
	[SerializeField]
	private Menu _mainMenu;

	[SerializeField]
	private Menu _optionsMenu;

	[SerializeField]
	private PopupMenu _okCancelPopup;

	[Space(10f)]
	[Header("Main Menu Selectables")]
	[SerializeField]
	private Selectable _newGameButton;

	[SerializeField]
	private Selectable _resumeGameButton;

	[SerializeField]
	private Selectable _profileMenuButton;

	[SerializeField]
	private Selectable _accountPickerButton;

	[SerializeField]
	private Selectable _optionsButton;

	[SerializeField]
	private Selectable _creditsButton;

	[Header("Main Menu Submit Actions")]
	[SerializeField]
	private SubmitActionLoadScene _newGameAction;

	[SerializeField]
	private SubmitActionLoadScene _resumeGameAction;

	[SerializeField]
	private SubmitActionConfirmInput _profileInputSubmitAction;

	[SerializeField]
	private SubmitActionAccountPicker _accountPickerSubmitAction;

	[SerializeField]
	private SubmitActionLoadScene _creditsAction;

	[SerializeField]
	private SubmitAction _resetGameAction;

	[Header("Main Menu Game Objects")]
	[SerializeField]
	private GameObject _resumeGameObject;

	[SerializeField]
	private GameObject _newGameObject;

	[SerializeField]
	private GameObject _profileMenuObject;

	[SerializeField]
	private GameObject _accountPickerObject;

	[SerializeField]
	private GameObject _creditsObject;

	[SerializeField]
	private GameObject _resetGameObject;

	[SerializeField]
	private GameObject _exitGameObject;

	[Space]
	[SerializeField]
	private Text _gameVersionTextDisplay;

	[SerializeField]
	private Text _copyrightTextDisplay;

	[SerializeField]
	private Text _gamertagDisplay;

	[SerializeField]
	private Text[] _mainMenuTextFields;

	[Space]
	[SerializeField]
	private GameObject _disconnectedGameObject;

	protected ResumeGameLocalizedText _resumeGameTextSetter;

	protected ScreenPrompt _confirmActionPrompt;

	protected ScreenPrompt _cancelActionPrompt;

	protected ScreenPrompt _continuePrompt;

	private IProfileManager _profileManager;

	private AsyncOperation _asyncGameLoad;

	private bool _waitingOnReadDone;

	private bool _sceneLoadStarted;

	private bool _waitingOnBrokenDataResponse;

	private bool _waitingOnMainMenuActivation;

	private const string c_copyrightText = "";

	private bool _showPopupsOnReturnToMainMenu;

	private StartupPopups _popupsToShow;

	private StartupPopups _activePopup;

	private bool _setGammaMenuCallback;

	private bool _autoResumeExpedition;

	private bool _allowDisconnectedMessage;

	private void Awake()
	{
		_profileManager = StandaloneProfileManager.SharedInstance;
		_titleCodeManager.enabled = true;
		_profileManager.PreInitialize();
		LoadManager.OnStartSceneLoad += OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
		MenuStackManager.SharedInstance.OnMenuPush += OnMenuPush;
		MenuStackManager.SharedInstance.OnMenuPop += OnMenuPop;
		_resumeGameTextSetter = _resumeGameObject.GetComponentInChildren<ResumeGameLocalizedText>();
		InitializePopupPrompts();
		_disconnectedGameObject.SetActive(value: false);
		_allowDisconnectedMessage = false;
	}

	private void OnMenuPop(Menu poppedMenu)
	{
		if (poppedMenu == _optionsMenu)
		{
			_copyrightTextDisplay.enabled = true;
			_gameVersionTextDisplay.enabled = true;
			_gamertagDisplay.enabled = true;
			EnableMainMenuTextFields(enable: true);
		}
		if (_showPopupsOnReturnToMainMenu && MainMenuIsActive())
		{
			TryShowStartupPopupsAndShowMenu(firstTimeRun: false);
			_showPopupsOnReturnToMainMenu = false;
		}
		else if (_waitingOnMainMenuActivation && MainMenuIsActive())
		{
			SelectDefaultMainMenuSelection();
			_waitingOnMainMenuActivation = false;
		}
	}

	private void OnMenuPush(Menu pushedMenu)
	{
		if (pushedMenu == _optionsMenu)
		{
			_copyrightTextDisplay.enabled = false;
			_gameVersionTextDisplay.enabled = false;
			_gamertagDisplay.enabled = false;
			EnableMainMenuTextFields(enable: false);
		}
	}

	private bool MainMenuIsActive()
	{
		return MenuStackManager.SharedInstance.Peek() == null;
	}

	private void EnableMainMenuTextFields(bool enable)
	{
		for (int i = 0; i < _mainMenuTextFields.Length; i++)
		{
			_mainMenuTextFields[i].enabled = enable;
		}
	}

	private void Start()
	{
		OWInput.ChangeInputMode(InputMode.Menu);
		InitializeUICallbacks();
		_titleMenuRaycastBlocker.blocksRaycasts = true;
		_cameraController.OnLogoPanComplete += FadeInTitleLogo;
	}

	private void InitializeUICallbacks()
	{
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		_newGameAction.OnSubmitAction += OnNewGameSubmit;
		_newGameAction.OnPostSetupPopup += OnNewGameSetupPopup;
		_resetGameAction.OnSubmitAction += OnResetGameSubmit;
		_accountPickerSubmitAction.OnAccountPickerSubmitEvent += OnAccountPickerSubmitEvent;
	}

	private void FadeInTitleLogo()
	{
		string text = "v";
		string[] array = Application.version.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length >= 3)
		{
			text = text + array[0] + "." + array[1] + "." + array[2];
		}
		else
		{
			Debug.LogWarning("Version is not in the correct format of x.x.x.x, displaying raw version string from Unity Project Settings");
			text += Application.version;
		}
		_gameVersionTextDisplay.text = text;
		_copyrightTextDisplay.text = "";
		SetUserAccountDisplayInfo();
		_inputModule.DisableInputs();
		_gfxController.OnTitleLogoAnimationComplete += OnTitleLogoAnimationComplete;
		_gfxController.FadeInTitleLogo(_autoResumeExpedition);
	}

	private void InitializePopupPrompts()
	{
		_confirmActionPrompt = new ScreenPrompt(InputLibrary.confirm, UITextLibrary.GetString(UITextType.MenuConfirm));
		_cancelActionPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuCancel));
		_continuePrompt = new ScreenPrompt(InputLibrary.menuConfirm, UITextLibrary.GetString(UITextType.KeyRebindingUpdatePopupContinueBtn));
	}

	private void OnLanguageChanged()
	{
		InitializePopupPrompts();
		SetUserAccountDisplayInfo();
	}

	private void OnTitleLogoAnimationComplete()
	{
		InitializeProfileManagerCallbacks();
		if (_titleButtonPrompt != null && _titleButtonPrompt.ShouldEnable())
		{
			if (_autoResumeExpedition)
			{
				_titleButtonPrompt.AutoLoginPS5();
				return;
			}
			_titleButtonPrompt.EnableButtonPrompt();
			_titleCodeManager.enabled = true;
		}
	}

	private void InitializeProfileManagerCallbacks()
	{
		StandaloneProfileManager.SharedInstance.OnNoProfilesExist += OnNoStandaloneProfilesExist;
		StandaloneProfileManager.SharedInstance.OnBrokenDataExists += OnBrokenDataExists;
		StandaloneProfileManager.SharedInstance.OnUpdatePlayerProfiles += OnUpdatePlayerProfiles;
		_profileManager.OnProfileSignInStart += OnProfileSignInStart;
		_profileManager.OnProfileSignInComplete += OnProfileSignInComplete;
		_profileManager.OnProfileSignOutStart += OnProfileSignOutStart;
		_profileManager.OnProfileSignOutComplete += OnProfileSignOutComplete;
		_profileManager.OnProfileReadDone += OnProfileManagerReadDone;
		_profileManager.Initialize();
	}

	private void OnDestroy()
	{
		StandaloneProfileManager.SharedInstance.OnNoProfilesExist -= OnNoStandaloneProfilesExist;
		StandaloneProfileManager.SharedInstance.OnBrokenDataExists -= OnBrokenDataExists;
		StandaloneProfileManager.SharedInstance.OnUpdatePlayerProfiles -= OnUpdatePlayerProfiles;
		_profileManager.OnProfileSignInStart -= OnProfileSignInStart;
		_profileManager.OnProfileSignInComplete -= OnProfileSignInComplete;
		_profileManager.OnProfileSignOutStart -= OnProfileSignOutStart;
		_profileManager.OnProfileSignOutComplete -= OnProfileSignOutComplete;
		_profileManager.OnProfileReadDone -= OnProfileManagerReadDone;
		LoadManager.OnStartSceneLoad -= OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad -= OnCompleteSceneLoad;
		_titleButtonPrompt.OnTitleButtonPromptClose += OnTitleButtonPromptClose;
		TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
		_newGameAction.OnSubmitAction -= OnNewGameSubmit;
		_newGameAction.OnPostSetupPopup -= OnNewGameSetupPopup;
		_resetGameAction.OnSubmitAction -= OnResetGameSubmit;
		_accountPickerSubmitAction.OnAccountPickerSubmitEvent -= OnAccountPickerSubmitEvent;
		MenuStackManager.SharedInstance.OnMenuPush -= OnMenuPush;
		MenuStackManager.SharedInstance.OnMenuPop -= OnMenuPop;
		if (_autoResumeExpedition)
		{
			SpinnerUI.Hide();
		}
	}

	private void ForceResetTitleScreen()
	{
		if (_activePopup != 0)
		{
			_okCancelPopup.OnPopupConfirm -= OnUserConfirmStartupPopup;
			_activePopup = StartupPopups.None;
		}
		MenuStackManager.SharedInstance.ForceClearStack();
		EventSystem.current.SetSelectedGameObject(null);
		_gfxController.ResetMenuOptions();
		ClearUserAccountDisplayInfo();
		if (_titleButtonPrompt.ShouldEnable())
		{
			_titleButtonPrompt.EnableButtonPrompt();
			_titleCodeManager.enabled = true;
		}
	}

	private void OnTitleButtonPromptClose()
	{
		_titleCodeManager.enabled = false;
	}

	public void OnSystemSuspend()
	{
	}

	public void OnSystemResume()
	{
		if (!_sceneLoadStarted)
		{
			ForceResetTitleScreen();
		}
	}

	private void OnDlcInstalled()
	{
		if (!_sceneLoadStarted)
		{
			ForceResetTitleScreen();
			_gfxController.CheckDlcEntitlement();
		}
	}

	private void OnCompleteSceneLoad(OWScene originalScene, OWScene loadScene)
	{
		_sceneLoadStarted = false;
	}

	private void OnStartSceneLoad(OWScene originalScene, OWScene loadScene)
	{
		_sceneLoadStarted = true;
	}

	private void OnNoStandaloneProfilesExist()
	{
		_titleMenuRaycastBlocker.blocksRaycasts = false;
		_inputModule.EnableInputs();
		_profileMenuManager.InitiateFirstTimeProfileCreation();
	}

	private void OnUpdatePlayerProfiles()
	{
		_profileManager.currentProfileGameSettings.ApplyAllSettings();
	}

	private void OnBrokenDataExists()
	{
		_titleMenuRaycastBlocker.blocksRaycasts = false;
		_inputModule.EnableInputs();
		_waitingOnBrokenDataResponse = true;
		bool flag = false;
		flag = StandaloneProfileManager.SharedInstance.BackupExistsForBrokenData();
		string text = UITextLibrary.GetString(UITextType.SaveRestore_CorruptedMsg);
		if (flag)
		{
			text = text + " " + UITextLibrary.GetString(UITextType.SaveRestore_LoadPreviousMsg);
		}
		_okCancelPopup.ResetPopup();
		_okCancelPopup.SetUpPopup(text, InputLibrary.confirm, InputLibrary.cancel, _confirmActionPrompt, _cancelActionPrompt, closeMenuOnOk: true, flag);
		_okCancelPopup.OnPopupConfirm += OnUserConfirmRestoreData;
		_okCancelPopup.OnPopupCancel += OnUserCancelRestoreData;
		_okCancelPopup.EnableMenu(value: true);
	}

	private void OnUserConfirmRestoreData()
	{
		_waitingOnBrokenDataResponse = false;
		StandaloneProfileManager.SharedInstance.RestoreCurrentProfileBackup();
		OnProfileManagerReadDone();
		_okCancelPopup.OnPopupConfirm -= OnUserConfirmRestoreData;
		_okCancelPopup.OnPopupCancel -= OnUserCancelRestoreData;
	}

	private void OnUserCancelRestoreData()
	{
		_waitingOnBrokenDataResponse = false;
		OnProfileManagerReadDone();
		_okCancelPopup.OnPopupConfirm -= OnUserConfirmRestoreData;
		_okCancelPopup.OnPopupCancel -= OnUserCancelRestoreData;
	}

	private void OnProfileManagerReadDone()
	{
		Debug.Log("Titlescreenmanager.OnProfileManagerReadDone");
		if (!_sceneLoadStarted && !_waitingOnBrokenDataResponse)
		{
			_waitingOnReadDone = false;
			SpinnerUI.Hide();
			_profileManager.currentProfileGraphicsSettings.ApplyAllGraphicSettings();
			SetUserAccountDisplayInfo();
			InitializeGameSaveDependencies();
			if (_autoResumeExpedition && (_profileManager.currentProfileGameSave.loopCount <= 1 || PlayerData.GetLastDeathType() == DeathType.BigBang || PlayerData.GetPersistentCondition("GAME_OVER_LAST_SAVE") || _popupsToShow != 0))
			{
				_autoResumeExpedition = false;
			}
			TryShowStartupPopupsAndShowMenu(firstTimeRun: true);
			_profileManager.currentProfileGameSettings.ApplyAllSettings();
			if (CheckInputValidationResult(default(InputRebindableData.ValidationResult), out var listToDisplay))
			{
				_inputUpdateNotifier.DisplayUserRebindables(listToDisplay);
				_inputUpdateNotifier.OnPlayerRectificationComplete += OnPlayerInputRectificationComplete;
			}
			if (_autoResumeExpedition)
			{
				_resumeGameAction.Submit();
				SpinnerUI.Show();
			}
		}
	}

	private void OnPlayerInputRectificationComplete()
	{
		_inputUpdateNotifier.OnPlayerRectificationComplete -= OnPlayerInputRectificationComplete;
		_waitingOnMainMenuActivation = true;
	}

	private bool CheckInputValidationResult(InputRebindableData.ValidationResult validationRes, out IInputCommands[] listToDisplay)
	{
		if (!validationRes.notifyPlayerOfChanges)
		{
			listToDisplay = null;
			return false;
		}
		listToDisplay = new IInputCommands[4]
		{
			InputCommandManager.MappedInputActions[InputConsts.InputCommandType.PROBELAUNCH],
			InputCommandManager.MappedInputActions[InputConsts.InputCommandType.SIGNALSCOPE],
			InputCommandManager.MappedInputActions[InputConsts.InputCommandType.TOOL_PRIMARY],
			InputCommandManager.MappedInputActions[InputConsts.InputCommandType.TOOL_SECONDARY]
		};
		return !listToDisplay[0].HasSameBinding(listToDisplay[1], usingGamepad: false) || !listToDisplay[0].HasSameBinding(listToDisplay[1], usingGamepad: true);
	}

	private void InitializeGameSaveDependencies()
	{
		Debug.Log("TitleScreenManager.InitializeGameSaveDependencies");
		_titleButtonPrompt.DisableButtonPrompt();
		if (_firstTimeGammaSetup.HasRunFirstTimeGammaSetup())
		{
			_profileManager.currentProfileGraphicsSettings.SetSliderValGamma(_firstTimeGammaSetup.GetFirstTimeGammaSetupValue());
		}
		PlayerData.Init(_profileManager.currentProfileGameSave, _profileManager.currentProfileGameSettings, _profileManager.currentProfileGraphicsSettings, _profileManager.currentProfileInputJSON);
		if (_titleCodeManager.ResetInputsToDefaults)
		{
			((InputManager)OWInput.SharedInputManager).commandManager.LoadDefaultInputActions();
			_titleCodeManager.ResetInputsToDefaults = false;
			Locator.GetMenuAudioController().PlayKonamiCode();
		}
		DetermineStartupPopups();
	}

	private void SetUserAccountDisplayInfo()
	{
		_gamertagDisplay.text = "";
	}

	private void ClearUserAccountDisplayInfo()
	{
		_gamertagDisplay.text = "";
	}

	private void DetermineStartupPopups()
	{
		_popupsToShow = StartupPopups.None;
		if (_profileManager.currentProfileGameSave.version == "NONE")
		{
			_popupsToShow |= StartupPopups.ResetInputs;
		}
		bool num = EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
		if (num && (_profileManager.currentProfileGameSave.shownPopups & StartupPopups.ReducedFrights) == 0)
		{
			_popupsToShow |= StartupPopups.ReducedFrights;
		}
		if (num && (_profileManager.currentProfileGameSave.shownPopups & StartupPopups.NewExhibit) == 0)
		{
			_popupsToShow |= StartupPopups.NewExhibit;
		}
	}

	private void OnGammaMenuDeactivate()
	{
		_inputModule.DisableInputs();
	}

	private void TryShowStartupPopupsAndShowMenu(bool firstTimeRun)
	{
		if (_setGammaMenuCallback)
		{
			PlayerData.SetRanFirstRunGammaSetup(val: true);
			_setGammaMenuCallback = false;
			_firstTimeGammaSetup.OnGammaMenuFadeOutComplete -= TryShowStartupPopupsAndShowMenu;
		}
		if (!PlayerData.RanFirstRunGammaSetup() && MainMenuIsActive())
		{
			_inputModule.EnableInputs();
			_setGammaMenuCallback = true;
			_firstTimeGammaSetup.OnGammaMenuFadeOutComplete += TryShowStartupPopupsAndShowMenu;
			_firstTimeGammaSetup.OnDeactivateMenu += OnGammaMenuDeactivate;
			_firstTimeGammaSetup.ActivateAsFirstTimeSetup();
		}
		else if (_popupsToShow == StartupPopups.None)
		{
			_okCancelPopup.ResetPopup();
			SetUpMainMenu();
			if (!_autoResumeExpedition && firstTimeRun)
			{
				FadeInMenuOptions();
			}
		}
		else
		{
			TryShowStartupPopups();
		}
	}

	private void TryShowStartupPopups()
	{
		string message = "AAAAGGGGGHH";
		if ((_popupsToShow & StartupPopups.ResetInputs) != 0)
		{
			_activePopup = StartupPopups.ResetInputs;
			message = UITextLibrary.GetString(UITextType.MenuMessage_InputUpdate);
		}
		else if ((_popupsToShow & StartupPopups.ReducedFrights) != 0)
		{
			_activePopup = StartupPopups.ReducedFrights;
			message = UITextLibrary.GetString(UITextType.MenuMessage_ReducedFrightOptionAvail);
		}
		else if ((_popupsToShow & StartupPopups.NewExhibit) != 0)
		{
			_activePopup = StartupPopups.NewExhibit;
			message = UITextLibrary.GetString(UITextType.MenuMessage_NewExhibit);
		}
		_inputModule.EnableInputs();
		_titleMenuRaycastBlocker.blocksRaycasts = false;
		if (!MainMenuIsActive())
		{
			_showPopupsOnReturnToMainMenu = true;
			return;
		}
		_okCancelPopup.ResetPopup();
		_okCancelPopup.SetUpPopup(message, InputLibrary.menuConfirm, null, _continuePrompt, null, closeMenuOnOk: true, setCancelButtonActive: false);
		_okCancelPopup.OnPopupConfirm += OnUserConfirmStartupPopup;
		_okCancelPopup.EnableMenu(value: true);
	}

	private void OnUserConfirmStartupPopup()
	{
		_popupsToShow &= ~_activePopup;
		PlayerData.SetShownPopups(_activePopup);
		PlayerData.SaveCurrentGame();
		_activePopup = StartupPopups.None;
		_okCancelPopup.OnPopupConfirm -= OnUserConfirmStartupPopup;
		_inputModule.DisableInputs();
		_titleMenuRaycastBlocker.blocksRaycasts = true;
		TryShowStartupPopupsAndShowMenu(firstTimeRun: true);
	}

	private void SetUpMainMenu()
	{
		bool num = _profileManager.currentProfileGameSave.loopCount > 1;
		_creditsAction.EnableConfirm(value: false);
		_resetGameObject.SetActive(value: false);
		if (num)
		{
			if (_resumeGameTextSetter != null)
			{
				_resumeGameTextSetter.SetText();
			}
			_newGameAction.EnableConfirm(value: true);
			_resumeGameAction.EnableConfirm(value: false);
			_resumeGameObject.SetActive(value: true);
			if (PlayerData.GetWarpedToTheEye())
			{
				_resumeGameAction.SetSceneToLoad(SubmitActionLoadScene.LoadableScenes.EYE);
			}
			else
			{
				_resumeGameAction.SetSceneToLoad(SubmitActionLoadScene.LoadableScenes.GAME);
			}
		}
		else
		{
			_newGameAction.EnableConfirm(value: false);
			_resumeGameObject.SetActive(value: false);
		}
		_accountPickerObject.SetActive(value: false);
		_profileMenuObject.SetActive(value: true);
		_exitGameObject.SetActive(value: true);
	}

	private void FadeInMenuOptions()
	{
		_titleMenuRaycastBlocker.blocksRaycasts = true;
		_inputModule.EnableInputs();
		_gfxController.ResetMenuOptions();
		_gfxController.OnTitleMenuAnimationComplete += OnTitleMenuAnimationComplete;
		EnableMainMenuTextFields(enable: false);
		EnableMainMenuTextFields(enable: true);
		_gfxController.FadeInMenuOptions();
	}

	private void OnTitleButtonPromptPS4DataLoaded()
	{
		_titleButtonPrompt.SetCompleted();
	}

	private void OnTitleButtonPromptPS5DataLoaded()
	{
		_titleButtonPrompt.SetCompleted();
	}

	private void OnAccountPickerSubmitEvent()
	{
		if (!_sceneLoadStarted)
		{
			SpinnerUI.Show();
			EventSystem.current.SetSelectedGameObject(null);
			_gfxController.ResetMenuOptions();
		}
	}

	private void OnTitleMenuAnimationComplete()
	{
		_titleMenuRaycastBlocker.blocksRaycasts = false;
		_inputModule.EnableInputs();
		_gfxController.OnTitleMenuAnimationComplete -= OnTitleMenuAnimationComplete;
		if (MenuStackManager.SharedInstance.GetMenuCount() == 0)
		{
			SelectDefaultMainMenuSelection();
		}
	}

	private void SelectDefaultMainMenuSelection()
	{
		EventSystem.current.SetSelectedGameObject(null);
		if (_profileManager.currentProfileGameSave.loopCount > 1)
		{
			SelectableAudioPlayer component = _resumeGameButton.GetComponent<SelectableAudioPlayer>();
			if (component != null)
			{
				component.SilenceNextSelectEvent();
			}
			Locator.GetMenuInputModule().SelectOnNextUpdate(_resumeGameButton);
		}
		else
		{
			SelectableAudioPlayer component = _newGameButton.GetComponent<SelectableAudioPlayer>();
			if (component != null)
			{
				component.SilenceNextSelectEvent();
			}
			Locator.GetMenuInputModule().SelectOnNextUpdate(_newGameButton);
		}
	}

	private void OnProfileSignInStart()
	{
		if (_sceneLoadStarted)
		{
			Debug.Log("TitleScreenManager.OnProfileSignInStart");
			_titleButtonPrompt.DisableButtonPrompt();
		}
	}

	private void OnProfileSignInComplete(ProfileManagerSignInResult result)
	{
		Debug.Log("TitleScreenManager.OnProfileSignInComplete");
		if (!_sceneLoadStarted && result == ProfileManagerSignInResult.COMPLETE)
		{
			_waitingOnReadDone = true;
		}
	}

	private void OnProfileSignOutStart()
	{
		if (!_sceneLoadStarted)
		{
			Debug.Log("TitleScreenManager.OnProfileSignOutStart");
			SpinnerUI.Show();
			EventSystem.current.SetSelectedGameObject(null);
			_gfxController.ResetMenuOptions();
		}
	}

	private void OnProfileSignOutComplete()
	{
		if (!_sceneLoadStarted)
		{
			Debug.Log("TitleScreenManager.OnProfileSignOutComplete");
			SpinnerUI.Hide();
			ClearUserAccountDisplayInfo();
		}
	}

	private void OnNewGameSetupPopup(SubmitActionConfirm sender, PopupMenu popupToOpen)
	{
		string @string = UITextLibrary.GetString(UITextType.MenuResetGameConfirm);
		popupToOpen.SetUpPopup(@string, InputLibrary.confirm, InputLibrary.cancel, _confirmActionPrompt, _cancelActionPrompt);
	}

	private void OnNewGameSubmit()
	{
		PlayerData.ResetGame();
	}

	private void OnResetGameSubmit()
	{
		PlayerData.ResetGame();
		InitializeGameSaveDependencies();
		TryShowStartupPopupsAndShowMenu(firstTimeRun: true);
	}
}
