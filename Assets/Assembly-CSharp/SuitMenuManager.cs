using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SuitMenuManager : MonoBehaviour
{
	public delegate void DeactivateSuitMenuEvent();

	[SerializeField]
	private Menu _mainMenu;

	[Space(10f)]
	[SerializeField]
	private SubmitAction _resetSettingsAction;

	[SerializeField]
	private SubmitAction _resetSettingsActionByCommand;

	[SerializeField]
	private SubmitAction _closeMenuAction;

	[SerializeField]
	private MenuCancelAction _onMenuCancelAction;

	[SerializeField]
	private ButtonWithHotkeyImageElement _resetSettingsFooterButton;

	[SerializeField]
	private ButtonWithHotkeyImageElement _closeMenuFooterButton;

	[SerializeField]
	private ScreenPromptList _promptList;

	private ScreenPrompt _exitPrompt;

	private ScreenPrompt _resetToDefaultsPrompt;

	private SettingsSave _gameSettingsOnActivate;

	private SettingsSave _updatedGameSettings;

	private List<SettingsMenuData> _listSettingsOptionData;

	public event DeactivateSuitMenuEvent OnDeactivateSuitMenu;

	private void Start()
	{
		MenuStackManager.SharedInstance.OnMenuPush += OnSettingsMenuPush;
		_resetSettingsActionByCommand.OnSubmitAction += OnResetSettingSubmit;
		_mainMenu.OnForceClosed += OnSettingsMenuForceClosed;
		_exitPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuOptionQuitPrompt));
		_resetToDefaultsPrompt = new ScreenPrompt(InputLibrary.setDefaults, UITextLibrary.GetString(UITextType.MenuResetToDefault));
		Locator.GetPromptManager().AddScreenPrompt(_exitPrompt, _promptList, TextAnchor.MiddleRight);
		if (_onMenuCancelAction != null)
		{
			_onMenuCancelAction.OnMenuCancel += OnMenuCancelEvent;
		}
		MenuOption[] menuOptions = _mainMenu.GetMenuOptions();
		_listSettingsOptionData = new List<SettingsMenuData>();
		foreach (MenuOption menuOption in menuOptions)
		{
			if (menuOption.ShouldEnable())
			{
				SettingsMenuData item = default(SettingsMenuData);
				item.uiMenuOption = menuOption;
				item.id = menuOption.GetSettingsID();
				item.labelTextField = menuOption.GetLabelField();
				item.secondaryTextField = menuOption.GetSecondaryTextField();
				MenuValueOption menuValueOption = menuOption as MenuValueOption;
				if (menuValueOption != null)
				{
					item.dependentMenuOption = menuValueOption.GetDependentMenuOption();
				}
				switch (item.id)
				{
				case SettingsID.VOL_MASTER:
				case SettingsID.VOL_MUSIC:
				case SettingsID.VOL_SFX:
				case SettingsID.INPUT_ACTIVE_CONTROLLER:
					Debug.LogError("The Preflight Checklist menu is not designed to update Volume or Active Controller in real time, double check");
					break;
				case SettingsID.GFX_FULLSCREEN:
				case SettingsID.GFX_ASPECT_RATIO:
				case SettingsID.GFX_RESOLUTION:
				case SettingsID.GFX_DISPLAY_NUM:
				case SettingsID.GFX_AA_TYPE:
				case SettingsID.GFX_AA_QUAL:
				case SettingsID.GFX_TEX_QUAL:
				case SettingsID.GFX_OCEAN_QUAL:
				case SettingsID.GFX_SHADOW_QUAL:
				case SettingsID.GFX_AO_QUAL:
				case SettingsID.GFX_GAMMA:
				case SettingsID.GFX_VSYNC:
				case SettingsID.GFX_FOV:
				case SettingsID.GFX_DITHER:
				case SettingsID.GFX_LIGHTING_QUAL:
					Debug.LogError("The Preflight Checklist menu is not designed to update any graphics settings, double check");
					break;
				}
				_listSettingsOptionData.Add(item);
			}
		}
		SettingsUiUtil.InitMenuOptionTextLabels(menuOptions);
		Locator.GetSceneMenuManager().RegisterSuitMenu(this);
	}

	private void OnDestroy()
	{
		MenuStackManager.SharedInstance.OnMenuPush -= OnSettingsMenuPush;
		_resetSettingsActionByCommand.OnSubmitAction -= OnResetSettingSubmit;
		_mainMenu.OnForceClosed -= OnSettingsMenuForceClosed;
		if (_onMenuCancelAction != null)
		{
			_onMenuCancelAction.OnMenuCancel -= OnMenuCancelEvent;
		}
	}

	private void Update()
	{
		if (_resetSettingsActionByCommand.gameObject.activeInHierarchy && OWInput.IsNewlyPressed(InputLibrary.setDefaults, InputMode.Menu))
		{
			_resetSettingsActionByCommand.Submit();
		}
	}

	public void OpenSuitMenu()
	{
		Locator.GetPauseCommandListener().AddPauseCommandLock();
		_mainMenu.OnDeactivateMenu += OnDeactivateMenu;
		OWTime.Pause(OWTime.PauseType.Menu);
		OWInput.ChangeInputMode(InputMode.Menu);
		_mainMenu.EnableMenu(value: true);
	}

	private void OnDeactivateMenu()
	{
		if (MenuStackManager.SharedInstance.GetMenuCount() == 0)
		{
			Locator.GetPauseCommandListener().RemovePauseCommandLock();
			_mainMenu.OnDeactivateMenu -= OnDeactivateMenu;
			OWTime.Unpause(OWTime.PauseType.Menu);
			OWInput.RestorePreviousInputs();
			if (this.OnDeactivateSuitMenu != null)
			{
				this.OnDeactivateSuitMenu();
			}
		}
	}

	private void OnSettingsMenuPush(Menu menu)
	{
		if (menu == _mainMenu)
		{
			_gameSettingsOnActivate = PlayerData.CloneSettingsData();
			_updatedGameSettings = _gameSettingsOnActivate.Clone();
			SettingsUiUtil.ApplySettingsToUi(_mainMenu.GetMenuOptions(), _gameSettingsOnActivate);
			_exitPrompt.SetVisibility(isVisible: true);
			_resetToDefaultsPrompt.SetVisibility(isVisible: true);
		}
	}

	private void ExitMenu()
	{
		_exitPrompt.SetVisibility(isVisible: false);
		CreateUpdatedSettingsSaves();
		SaveChanges();
	}

	private void CreateUpdatedSettingsSaves()
	{
		SettingsUiUtil.UpdateAllSettingsDataFromUi(_listSettingsOptionData.ToArray(), _updatedGameSettings);
	}

	private void RevertChanges()
	{
		SettingsUiUtil.ApplySettingsToUi(_mainMenu.GetMenuOptions(), _gameSettingsOnActivate);
	}

	private void SaveChanges()
	{
		bool flag = false;
		if (!_updatedGameSettings.Equals(_gameSettingsOnActivate))
		{
			flag = true;
			PlayerData.SetSettingsData(_updatedGameSettings);
		}
		if (flag)
		{
			PlayerData.SaveSettings();
		}
	}

	private void OnMenuCancelEvent(GameObject selectedObject, BaseEventData eventData)
	{
		ExitMenu();
	}

	private void OnCloseSubmit()
	{
		ExitMenu();
	}

	private void OnResetSettingSubmit()
	{
		SettingsID[] array = new SettingsID[_listSettingsOptionData.Count];
		for (int i = 0; i < _listSettingsOptionData.Count; i++)
		{
			if (_listSettingsOptionData[i].uiMenuOption.gameObject.activeSelf && _listSettingsOptionData[i].id != SettingsID.LANGUAGE && _listSettingsOptionData[i].id != SettingsID.INPUT_ACTIVE_CONTROLLER)
			{
				array[i] = _listSettingsOptionData[i].id;
			}
			else
			{
				array[i] = SettingsID.UNDEFINED;
			}
		}
		SettingsSave.SetToDefaults(_updatedGameSettings, array);
		SettingsUiUtil.ApplySettingsToUi(_mainMenu.GetMenuOptions(), _updatedGameSettings);
		if (Locator.GetEventSystem().currentSelectedGameObject == _resetSettingsFooterButton.gameObject)
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(_mainMenu.GetLastSelected());
		}
	}

	private void OnSettingsMenuForceClosed()
	{
		RevertChanges();
	}
}
