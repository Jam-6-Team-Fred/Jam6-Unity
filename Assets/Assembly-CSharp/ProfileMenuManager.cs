using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMenuManager : MonoBehaviour
{
	private class ProfileElementLookup
	{
		public string profileName;

		public DateTime lastModifiedTime;

		public SubmitActionConfirm confirmSwitchAction;

		public SubmitActionConfirm confirmDeleteAction;
	}

	[SerializeField]
	private Menu _profileMenu;

	[SerializeField]
	private Text _currenProfileLabel;

	[SerializeField]
	private Button _createProfileButton;

	[SerializeField]
	private PopupMenu _createProfileConfirmPopup;

	[SerializeField]
	private GameObject _profileListRoot;

	[SerializeField]
	private GameObject _profileItemTemplate;

	[SerializeField]
	private RectTransform _profileListScrollViewContent;

	[SerializeField]
	private FontAndLanguageController _fontController;

	[SerializeField]
	private ButtonWithHotkeyImageElement _closeMenuButton;

	private SubmitActionConfirmInput _createProfileAction;

	private PopupMenu _deleteProfileConfirmPopup;

	private PopupMenu _switchProfileConfirmPopup;

	private List<GameObject> _listProfileElements;

	private List<ProfileElementLookup> _listProfileUIElementLookup;

	private ScreenPrompt _closeMenuPrompt;

	private ScreenPrompt _cancelPrompt;

	private ScreenPrompt _cancelAndExitGamePrompt;

	private ScreenPrompt _gamepadCancelPrompt;

	private ScreenPrompt _gamepadCancelAndExitGamePrompt;

	private ScreenPrompt _confirmCreateProfilePrompt;

	private ScreenPrompt _confirmDeleteProfilePrompt;

	private ScreenPrompt _confirmSwitchProfilePrompt;

	private TwoButtonActionElement _lastSelectedProfileAction;

	private bool _initialized;

	private bool _firstTimeProfileCreation;

	private bool _inputPopupActivated;

	private bool _usingGamepad;

	private void Awake()
	{
		_profileMenu.OnActivateMenu += OnProfileMenuActivate;
		_createProfileAction = _createProfileButton.GetRequiredComponent<SubmitActionConfirmInput>();
		_createProfileConfirmPopup.OnActivateMenu += OnCreateProfilePopupActivate;
		_createProfileAction.OnCancelAction += OnCreateProfileCancel;
		TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
		OWInput.SharedInputManager.OnUpdateInputDevice += OnSwitchInputDevice;
		_lastSelectedProfileAction = null;
	}

	private void Start()
	{
		_closeMenuPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuOptionQuitPrompt));
		_cancelPrompt = new ScreenPrompt(InputLibrary.escape, UITextLibrary.GetString(UITextType.RebindCancel));
		_cancelAndExitGamePrompt = new ScreenPrompt(InputLibrary.escape, UITextLibrary.GetString(UITextType.ProfileManagementCancelAndExit));
		_gamepadCancelPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.RebindCancel));
		_gamepadCancelAndExitGamePrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.ProfileManagementCancelAndExit));
		_confirmCreateProfilePrompt = new ScreenPrompt(InputLibrary.confirm, UITextLibrary.GetString(UITextType.ProfileManagementCreateCommand));
		_confirmDeleteProfilePrompt = new ScreenPrompt(InputLibrary.confirm, UITextLibrary.GetString(UITextType.MenuProfileDelete));
		_confirmSwitchProfilePrompt = new ScreenPrompt(InputLibrary.menuConfirm, UITextLibrary.GetString(UITextType.ProfileManagementSwitchCommand));
	}

	private void Update()
	{
		if (_usingGamepad != OWInput.UsingGamepad())
		{
			_usingGamepad = OWInput.UsingGamepad();
			UpdatePopupPrompts();
		}
	}

	private void OnDestroy()
	{
		TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
		OWInput.SharedInputManager.OnUpdateInputDevice -= OnSwitchInputDevice;
	}

	private void OnLanguageChanged()
	{
		SetCurrentProfileLabel();
		RefreshCloseMenuButton();
		if (_closeMenuPrompt != null)
		{
			_closeMenuPrompt.SetText(UITextLibrary.GetString(UITextType.MenuOptionQuitPrompt));
			_cancelPrompt.SetText(UITextLibrary.GetString(UITextType.RebindCancel));
			_cancelAndExitGamePrompt.SetText(UITextLibrary.GetString(UITextType.ProfileManagementCancelAndExit));
			_gamepadCancelPrompt.SetText(UITextLibrary.GetString(UITextType.RebindCancel));
			_gamepadCancelAndExitGamePrompt.SetText(UITextLibrary.GetString(UITextType.ProfileManagementCancelAndExit));
			_confirmCreateProfilePrompt.SetText(UITextLibrary.GetString(UITextType.ProfileManagementCreateCommand));
			_confirmDeleteProfilePrompt.SetText(UITextLibrary.GetString(UITextType.MenuProfileDelete));
			_confirmSwitchProfilePrompt.SetText(UITextLibrary.GetString(UITextType.ProfileManagementSwitchCommand));
		}
	}

	private void OnSwitchInputDevice()
	{
		if (PlayerData.IsLoaded())
		{
			RefreshCloseMenuButton();
		}
	}

	private void UpdatePopupPrompts()
	{
		PopupInputMenu inputPopup = _createProfileAction.GetInputPopup();
		ScreenPrompt cancelPrompt;
		if (_firstTimeProfileCreation)
		{
			cancelPrompt = _cancelAndExitGamePrompt;
			if (_usingGamepad)
			{
				cancelPrompt = _gamepadCancelAndExitGamePrompt;
			}
		}
		else
		{
			cancelPrompt = _cancelPrompt;
			if (_usingGamepad)
			{
				cancelPrompt = _gamepadCancelPrompt;
			}
		}
		if (_usingGamepad)
		{
			if (_inputPopupActivated)
			{
				inputPopup.SetUpPopupCommands(InputLibrary.confirm, InputLibrary.cancel, _confirmCreateProfilePrompt, cancelPrompt);
			}
			if (_deleteProfileConfirmPopup != null)
			{
				_deleteProfileConfirmPopup.SetUpPopupCommands(InputLibrary.confirm, InputLibrary.cancel, _confirmDeleteProfilePrompt, cancelPrompt);
			}
			if (_switchProfileConfirmPopup != null)
			{
				_switchProfileConfirmPopup.SetUpPopupCommands(InputLibrary.menuConfirm, InputLibrary.cancel, _confirmSwitchProfilePrompt, cancelPrompt);
			}
		}
		else
		{
			if (_inputPopupActivated)
			{
				inputPopup.SetUpPopupCommands(InputLibrary.confirm, InputLibrary.escape, _confirmCreateProfilePrompt, cancelPrompt);
			}
			if (_deleteProfileConfirmPopup != null)
			{
				_deleteProfileConfirmPopup.SetUpPopupCommands(InputLibrary.confirm, InputLibrary.cancel, _confirmDeleteProfilePrompt, cancelPrompt);
			}
			if (_switchProfileConfirmPopup != null)
			{
				_switchProfileConfirmPopup.SetUpPopupCommands(InputLibrary.menuConfirm, InputLibrary.cancel, _confirmSwitchProfilePrompt, cancelPrompt);
			}
		}
	}

	private void RefreshCloseMenuButton()
	{
		_closeMenuButton.SetPrompt(_closeMenuPrompt);
	}

	private void OnProfileMenuActivate()
	{
		if (!_initialized)
		{
			SetCurrentProfileLabel();
			PopulateProfiles();
			_initialized = true;
		}
	}

	private void SetCurrentProfileLabel()
	{
		_currenProfileLabel.text = UITextLibrary.GetString(UITextType.MenuProfile) + " " + StandaloneProfileManager.SharedInstance.currentProfile.profileName;
	}

	public void InitiateFirstTimeProfileCreation()
	{
		_firstTimeProfileCreation = true;
		_createProfileAction.Submit();
	}

	private void OnCreateProfilePopupActivate()
	{
		_inputPopupActivated = true;
		PopupInputMenu inputPopup = _createProfileAction.GetInputPopup();
		MenuStackManager.SharedInstance.OnMenuPop += OnPostProfileCreateMenuPop;
		inputPopup.OnPopupConfirm += OnCreateProfileConfirm;
		inputPopup.OnPopupValidate += OnCreateProfileValidate;
		inputPopup.OnInputPopupValidateChar += OnValidateChar;
		string @string = UITextLibrary.GetString(UITextType.ProfileManagementCreateMessage);
		_usingGamepad = OWInput.UsingGamepad();
		ScreenPrompt cancelPrompt;
		if (_firstTimeProfileCreation)
		{
			cancelPrompt = _cancelAndExitGamePrompt;
			if (_usingGamepad)
			{
				cancelPrompt = _gamepadCancelAndExitGamePrompt;
			}
		}
		else
		{
			cancelPrompt = _cancelPrompt;
			if (_usingGamepad)
			{
				cancelPrompt = _gamepadCancelPrompt;
			}
		}
		if (_usingGamepad)
		{
			inputPopup.SetUpPopup(@string, InputLibrary.confirm, InputLibrary.cancel, _confirmCreateProfilePrompt, cancelPrompt, closeMenuOnOk: false);
		}
		else
		{
			inputPopup.SetUpPopup(@string, InputLibrary.confirm, InputLibrary.escape, _confirmCreateProfilePrompt, cancelPrompt, closeMenuOnOk: false);
		}
		inputPopup.SetInputFieldPlaceholderText(UITextLibrary.GetString(UITextType.ProfileManagementPlaceholderText));
	}

	private void OnCreateProfileCancel()
	{
		_inputPopupActivated = false;
		PopupInputMenu inputPopup = _createProfileAction.GetInputPopup();
		inputPopup.OnPopupValidate -= OnCreateProfileValidate;
		inputPopup.OnInputPopupValidateChar -= OnValidateChar;
		inputPopup.CloseMenuOnOk(value: true);
		_createProfileAction.OnSubmitAction -= OnCreateProfileConfirm;
		if (_firstTimeProfileCreation)
		{
			Application.Quit();
		}
	}

	private void OnCreateProfileConfirm()
	{
		_inputPopupActivated = false;
		PopupInputMenu inputPopup = _createProfileAction.GetInputPopup();
		inputPopup.OnPopupValidate -= OnCreateProfileValidate;
		inputPopup.OnInputPopupValidateChar -= OnValidateChar;
		_createProfileAction.OnSubmitAction -= OnCreateProfileConfirm;
		StandaloneProfileManager.SharedInstance.TryCreateProfile(_createProfileAction.GetInputString());
		inputPopup.CloseMenuOnOk(value: true);
		PopulateProfiles();
		SetCurrentProfileLabel();
		inputPopup.EnableMenu(value: false);
		if (_firstTimeProfileCreation)
		{
			_firstTimeProfileCreation = false;
			UpdatePopupPrompts();
		}
	}

	private bool OnCreateProfileValidate()
	{
		PopupInputMenu inputPopup = _createProfileAction.GetInputPopup();
		return StandaloneProfileManager.SharedInstance.ValidateProfileName(inputPopup.GetInputText());
	}

	private bool OnValidateChar(char c)
	{
		if (_createProfileAction.GetInputPopup().GetInputText().Length >= StandaloneProfileManager.SharedInstance.profileNameCharacterLimit)
		{
			return false;
		}
		return StandaloneProfileManager.SharedInstance.IsValidCharacterForProfileName(c);
	}

	private void OnPostProfileCreateMenuPop(Menu poppedMenu)
	{
		MenuStackManager.SharedInstance.OnMenuPop -= OnPostProfileCreateMenuPop;
		if (poppedMenu == _createProfileAction.GetInputPopup())
		{
			SelectableAudioPlayer component = _createProfileButton.GetComponent<SelectableAudioPlayer>();
			if (component != null)
			{
				component.SilenceNextSelectEvent();
			}
			Locator.GetMenuInputModule().SelectOnNextUpdate(_createProfileButton);
		}
	}

	private void PopulateProfiles()
	{
		if (_listProfileElements == null)
		{
			_listProfileElements = new List<GameObject>();
		}
		else
		{
			for (int i = 0; i < _listProfileElements.Count; i++)
			{
				TwoButtonActionElement requiredComponent = _listProfileElements[i].GetRequiredComponent<TwoButtonActionElement>();
				ClearProfileElementListeners(requiredComponent);
				UnityEngine.Object.Destroy(_listProfileElements[i]);
			}
			_listProfileElements.Clear();
		}
		if (_listProfileUIElementLookup == null)
		{
			_listProfileUIElementLookup = new List<ProfileElementLookup>();
		}
		else
		{
			_listProfileUIElementLookup.Clear();
		}
		StandaloneProfileManager.ProfileData[] array = StandaloneProfileManager.SharedInstance.profiles.ToArray();
		string profileName = StandaloneProfileManager.SharedInstance.currentProfile.profileName;
		int num = 0;
		Selectable selectable = null;
		for (int j = 0; j < array.Length; j++)
		{
			if (!(array[j].profileName == profileName))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_profileItemTemplate);
				gameObject.gameObject.SetActive(value: true);
				gameObject.transform.SetParent(_profileListRoot.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				Text[] componentsInChildren = gameObject.gameObject.GetComponentsInChildren<Text>();
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					_fontController.AddTextElement(componentsInChildren[k]);
				}
				num++;
				TwoButtonActionElement requiredComponent2 = gameObject.GetRequiredComponent<TwoButtonActionElement>();
				Selectable requiredComponent3 = requiredComponent2.GetRequiredComponent<Selectable>();
				SetUpProfileElementListeners(requiredComponent2);
				requiredComponent2.SetLabelText(array[j].profileName);
				Text component = requiredComponent2.GetButtonOne().GetComponent<Text>();
				if (component != null)
				{
					_fontController.AddTextElement(component);
				}
				component = requiredComponent2.GetButtonTwo().GetComponent<Text>();
				if (component != null)
				{
					_fontController.AddTextElement(component);
				}
				if (num == 1)
				{
					Navigation navigation = _createProfileButton.navigation;
					navigation.selectOnDown = gameObject.GetRequiredComponent<Selectable>();
					_createProfileButton.navigation = navigation;
					Navigation navigation2 = requiredComponent3.navigation;
					navigation2.selectOnUp = _createProfileButton;
					requiredComponent3.navigation = navigation2;
				}
				else
				{
					Navigation navigation3 = requiredComponent3.navigation;
					Navigation navigation4 = selectable.navigation;
					navigation3.selectOnUp = selectable;
					navigation3.selectOnDown = null;
					navigation4.selectOnDown = requiredComponent3;
					requiredComponent3.navigation = navigation3;
					selectable.navigation = navigation4;
				}
				_listProfileElements.Add(gameObject);
				selectable = requiredComponent3;
				ProfileElementLookup profileElementLookup = new ProfileElementLookup();
				profileElementLookup.profileName = array[j].profileName;
				profileElementLookup.lastModifiedTime = array[j].lastModifiedTime;
				profileElementLookup.confirmSwitchAction = requiredComponent2.GetSubmitActionOne() as SubmitActionConfirm;
				profileElementLookup.confirmDeleteAction = requiredComponent2.GetSubmitActionTwo() as SubmitActionConfirm;
				_listProfileUIElementLookup.Add(profileElementLookup);
			}
		}
	}

	private void SetUpProfileElementListeners(TwoButtonActionElement profileActionElement)
	{
		profileActionElement.OnActionElementSubmit += OnProfileItemActionTriggered;
		SubmitActionConfirm submitActionConfirm = profileActionElement.GetSubmitActionOne() as SubmitActionConfirm;
		SubmitActionConfirm obj = profileActionElement.GetSubmitActionTwo() as SubmitActionConfirm;
		submitActionConfirm.OnPostSetupPopup += OnConfirmSwitchProfilePopup;
		obj.OnPostSetupPopup += OnConfirmDeleteProfilePopup;
		submitActionConfirm.OnSubmitAction += OnSwitchProfile;
		obj.OnSubmitAction += OnDeleteProfile;
		submitActionConfirm.OnCancelAction += OnProfileActionCancel;
		obj.OnCancelAction += OnProfileActionCancel;
	}

	private void OnConfirmSwitchProfilePopup(SubmitActionConfirm sender, PopupMenu popup)
	{
		_switchProfileConfirmPopup = popup;
		string text = "";
		for (int i = 0; i < _listProfileUIElementLookup.Count; i++)
		{
			if (_listProfileUIElementLookup[i].confirmSwitchAction == sender)
			{
				text = _listProfileUIElementLookup[i].profileName;
			}
		}
		string message = UITextLibrary.GetString(UITextType.ProfileManagementSwitchProfile1) + text + UITextLibrary.GetString(UITextType.ProfileManagementSwitchProfile2);
		popup.SetUpPopup(message, InputLibrary.menuConfirm, InputLibrary.cancel, _confirmSwitchProfilePrompt, _cancelPrompt);
		UpdatePopupPrompts();
	}

	private void OnConfirmDeleteProfilePopup(SubmitActionConfirm sender, PopupMenu popup)
	{
		_deleteProfileConfirmPopup = popup;
		string text = "";
		for (int i = 0; i < _listProfileUIElementLookup.Count; i++)
		{
			if (_listProfileUIElementLookup[i].confirmDeleteAction == sender)
			{
				text = _listProfileUIElementLookup[i].profileName;
			}
		}
		string message = UITextLibrary.GetString(UITextType.ProfileManagementDeleteProfile1) + text + UITextLibrary.GetString(UITextType.ProfileManagementDeleteProfile2);
		popup.SetUpPopup(message, InputLibrary.confirm, InputLibrary.cancel, _confirmDeleteProfilePrompt, _cancelPrompt);
		UpdatePopupPrompts();
	}

	private void ClearProfileElementListeners(TwoButtonActionElement profileActionElement)
	{
		profileActionElement.OnActionElementSubmit -= OnProfileItemActionTriggered;
		Text[] componentsInChildren = profileActionElement.gameObject.GetComponentsInChildren<Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			_fontController.RemoveTextElement(componentsInChildren[i]);
		}
		SubmitActionConfirm submitActionConfirm = profileActionElement.GetSubmitActionOne() as SubmitActionConfirm;
		SubmitActionConfirm obj = profileActionElement.GetSubmitActionTwo() as SubmitActionConfirm;
		submitActionConfirm.OnPostSetupPopup -= OnConfirmSwitchProfilePopup;
		obj.OnPostSetupPopup -= OnConfirmDeleteProfilePopup;
		submitActionConfirm.OnSubmitAction -= OnSwitchProfile;
		obj.OnSubmitAction -= OnDeleteProfile;
		submitActionConfirm.OnCancelAction -= OnProfileActionCancel;
		obj.OnCancelAction -= OnProfileActionCancel;
	}

	private void OnProfileItemActionTriggered(TwoButtonActionElement profileActionElement)
	{
		_lastSelectedProfileAction = profileActionElement;
	}

	private void OnSwitchProfile()
	{
		if (_lastSelectedProfileAction != null)
		{
			_switchProfileConfirmPopup = null;
			if (StandaloneProfileManager.SharedInstance.SwitchProfile(_lastSelectedProfileAction.GetLabelText()))
			{
				PopulateProfiles();
				SetCurrentProfileLabel();
				_lastSelectedProfileAction = null;
				Locator.GetMenuInputModule().SelectOnNextUpdate(_createProfileButton);
			}
			else
			{
				StandaloneProfileManager.SharedInstance.OnBackupDataRestored += OnSwitchProfileDataRecoveryCompleted;
			}
		}
	}

	private void OnSwitchProfileDataRecoveryCompleted()
	{
		StandaloneProfileManager.SharedInstance.OnBackupDataRestored -= OnSwitchProfileDataRecoveryCompleted;
		PopulateProfiles();
		SetCurrentProfileLabel();
		_lastSelectedProfileAction = null;
		Locator.GetMenuInputModule().SelectOnNextUpdate(_createProfileButton);
	}

	private void OnDeleteProfile()
	{
		if (_lastSelectedProfileAction != null)
		{
			_deleteProfileConfirmPopup = null;
			StandaloneProfileManager.SharedInstance.DeleteProfile(_lastSelectedProfileAction.GetLabelText());
			PopulateProfiles();
			_lastSelectedProfileAction = null;
			Locator.GetMenuInputModule().SelectOnNextUpdate(_createProfileButton);
		}
	}

	private void OnProfileActionCancel()
	{
		_deleteProfileConfirmPopup = null;
		_switchProfileConfirmPopup = null;
	}
}
