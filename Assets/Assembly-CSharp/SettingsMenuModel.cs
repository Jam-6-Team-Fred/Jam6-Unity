using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Utilities;

public class SettingsMenuModel : MonoBehaviour
{
	private struct CachedGamepadState
	{
		public int deviceId;

		public bool connected;

		public int displayIndex;

		public bool isWhitelisted;

		public SettingsSave.UserDeviceInfo persistentInfo;
	}

	[SerializeField]
	private SettingsMenuView _view;

	[Space(10f)]
	private SettingsSave _gameSettingsOnActivate;

	private GraphicSettings _gfxSettingsOnActivate;

	private SettingsSave _updatedGameSettings;

	private GraphicSettings _updatedGfxSettings;

	private List<CachedGamepadState> _listCachedGamepadState;

	private RebindingState _rebindState;

	private Resolution[] _resolutions;

	private string[] _aspectRatioDisplayStrings;

	public void Initialize()
	{
		_gameSettingsOnActivate = PlayerData.CloneSettingsData();
		_updatedGameSettings = _gameSettingsOnActivate.Clone();
		_gfxSettingsOnActivate = PlayerData.GetGraphicSettings();
		_updatedGfxSettings = _gfxSettingsOnActivate.Clone();
		_view.ApplySettingsToUI(_gameSettingsOnActivate);
		_view.ApplyGraphicSettingsToUI(_gfxSettingsOnActivate);
		AspectRatio aspectRatio = _gfxSettingsOnActivate.aspectRatio;
		_resolutions = SystemDisplay.GetResolutionsWithAspect(aspectRatio);
		InitializeAspectRatioOptionList();
	}

	public void SaveChanges()
	{
		bool flag = false;
		if (!_updatedGameSettings.Equals(_gameSettingsOnActivate))
		{
			flag = true;
			PlayerData.SetSettingsData(_updatedGameSettings);
		}
		if (!_updatedGfxSettings.Equals(_gfxSettingsOnActivate))
		{
			flag = true;
			PlayerData.SetGraphicSettings(_updatedGfxSettings);
		}
		if (flag)
		{
			PlayerData.SaveSettings();
		}
		else
		{
			PlayerData.SaveInputSettings();
		}
	}

	private void InitializeAspectRatioOptionList()
	{
		AspectRatio[] availableAspectRatioList = SystemDisplay.GetAvailableAspectRatioList();
		_aspectRatioDisplayStrings = new string[availableAspectRatioList.Length];
		for (int i = 0; i < _aspectRatioDisplayStrings.Length; i++)
		{
			_aspectRatioDisplayStrings[i] = GraphicSettings.GetAspectRatioString(availableAspectRatioList[i]);
		}
	}

	public void RevertChanges()
	{
		TextTranslation.Get().SetLanguage(_gameSettingsOnActivate.language);
		Locator.GetAudioMixer().SetMasterVolume(_gameSettingsOnActivate.masterVolume);
		Locator.GetAudioMixer().SetMasterMusicVolume(_gameSettingsOnActivate.musicVolume);
		Locator.GetAudioMixer().SetMasterSFXVolume(_gameSettingsOnActivate.sfxVolume);
		_view.ApplySettingsToUI(_gameSettingsOnActivate);
		_view.ApplyGraphicSettingsToUI(_gfxSettingsOnActivate);
	}

	public void ResetToDefaultSettings(List<SettingsMenuData> settingsToReset, List<KeyRebindingElement> rebindingElementsToReset)
	{
		List<RebindableID> list = null;
		bool flag = false;
		SettingsID[] array = new SettingsID[settingsToReset.Count];
		for (int i = 0; i < settingsToReset.Count; i++)
		{
			if (settingsToReset[i].uiMenuOption.gameObject.activeSelf)
			{
				if (settingsToReset[i].id != SettingsID.LANGUAGE && settingsToReset[i].id != SettingsID.INPUT_ACTIVE_CONTROLLER)
				{
					array[i] = settingsToReset[i].id;
				}
				else
				{
					array[i] = SettingsID.UNDEFINED;
				}
				if (settingsToReset[i].id == SettingsID.REBINDABLE_OPTION_CONFIRM_TOGGLE)
				{
					flag = true;
				}
			}
		}
		for (int j = 0; j < rebindingElementsToReset.Count; j++)
		{
			if (list == null)
			{
				list = new List<RebindableID>();
			}
			if (rebindingElementsToReset[j].gameObject.activeSelf)
			{
				list.Add(rebindingElementsToReset[j].GetRebindableID());
			}
		}
		if (rebindingElementsToReset.Count > 0)
		{
			list.Add(RebindableID.SET_DEFAULTS);
		}
		SettingsSave.SetToDefaults(_updatedGameSettings, array);
		GraphicSettings.SetToDefaults(_updatedGfxSettings, array);
		if (flag && !OWInput.SharedInputManager.IsBindingSameAsDefault(RebindableID.MENU_CANCEL, gamepad: true))
		{
			SwapConfirmAndCancelBinding();
		}
		if (list != null)
		{
			OWInput.SharedInputManager.ModifyBindingsToDefaults(list.ToArray());
		}
		ApplyAudioSettings(_updatedGameSettings);
		_view.ApplySettingsToUI(_updatedGameSettings);
		_view.ApplyGraphicSettingsToUI(_updatedGfxSettings);
		_view.UpdateKeyRebindingElementDisplays();
	}

	public bool InitializeRebindingAction(IRebindableInputAction rebindableInputAction, out RebindingState rebindingState)
	{
		rebindingState = null;
		if (_rebindState != null && _rebindState.IsValid)
		{
			Debug.LogError("Rebinding Action still in progress. Cancelling Rebind request");
			return false;
		}
		if (!_view.ReadyToRebind())
		{
			return false;
		}
		rebindingState = new RebindingState(rebindableInputAction, _view);
		if (rebindingState.IsValid)
		{
			RebindingState obj = rebindingState;
			obj.OnBindingApplied = (Action<string, string, int, bool>)Delegate.Combine(obj.OnBindingApplied, new Action<string, string, int, bool>(OnBindingApplied));
			RebindingState obj2 = rebindingState;
			obj2.OnFinishedRebinding = (Action)Delegate.Combine(obj2.OnFinishedRebinding, new Action(OnFinishedRebinding));
			RebindingState obj3 = rebindingState;
			obj3.OnCancelledRebinding = (Action)Delegate.Combine(obj3.OnCancelledRebinding, new Action(OnCancelledRebinding));
			_view.EnableRaycastBlocker(value: true);
		}
		else
		{
			Debug.LogError("Error finding control scheme for input binding " + rebindableInputAction.RebindableID);
		}
		return rebindingState.IsValid;
	}

	protected void OnBindingApplied(string oldPath, string newPath, int bindingIndex, bool usingGamepad)
	{
		_view.UpdateKeyRebindingElementDisplays();
	}

	protected void OnFinishedRebinding()
	{
		_rebindState = null;
		_view.EnableRaycastBlocker(value: false);
		PlayerData.SaveInputSettings();
		_view.NotifyBindingChanged();
		Locator.GetMenuAudioController().PlayRebindKey();
	}

	protected void OnCancelledRebinding()
	{
		_view.EnableRaycastBlocker(value: false);
		_rebindState = null;
	}

	public void SwapConfirmAndCancelBinding()
	{
		if (!(InputLibrary.cancel is ISingleInputCommand singleInputCommand) || !(InputLibrary.menuConfirm is ISingleInputCommand singleInputCommand2))
		{
			return;
		}
		_rebindState = new RebindingState(singleInputCommand.Action as IRebindableInputAction, _view);
		InputControl inputControl = null;
		if (!singleInputCommand2.TryCastAction<ISingleAction>(out var castAction))
		{
			return;
		}
		InputAction action = castAction.Action;
		for (int i = 0; i < action.controls.Count; i++)
		{
			inputControl = action.controls[i];
			if (inputControl.device is Gamepad)
			{
				break;
			}
			inputControl = null;
		}
		if (inputControl == null)
		{
			Debug.LogError("Could not find a compatible InputControl to rebind menuConfirm/Cancel");
			return;
		}
		RebindingState rebindState = _rebindState;
		rebindState.OnBindingApplied = (Action<string, string, int, bool>)Delegate.Combine(rebindState.OnBindingApplied, new Action<string, string, int, bool>(OnCancelActionBindingApplied));
		RebindingState rebindState2 = _rebindState;
		rebindState2.OnFinishedRebinding = (Action)Delegate.Combine(rebindState2.OnFinishedRebinding, new Action(OnFinishedRebindingCancelAction));
		_rebindState.ApplyGamepadBindingNoInputChecks(inputControl);
	}

	private void OnCancelActionBindingApplied(string oldPath, string newPath, int bindingIndex, bool usingGamepad)
	{
		bool usingGamepad2 = usingGamepad;
		if (InputLibrary.cancel is ISingleInputCommand singleInputCommand && singleInputCommand.TryCastAction<RebindableAxisInputAction>(out var castAction))
		{
			RebindingUtil.ResolveConflicts(ref castAction, oldPath, newPath, bindingIndex, usingGamepad2, consoleCancelConfirmCheck: true);
			_view.UpdateKeyRebindingElementDisplays();
			RebindingState rebindState = _rebindState;
			rebindState.OnBindingApplied = (Action<string, string, int, bool>)Delegate.Remove(rebindState.OnBindingApplied, new Action<string, string, int, bool>(OnCancelActionBindingApplied));
		}
	}

	private void OnFinishedRebindingCancelAction()
	{
		RebindingState rebindState = _rebindState;
		rebindState.OnFinishedRebinding = (Action)Delegate.Remove(rebindState.OnFinishedRebinding, new Action(OnFinishedRebindingCancelAction));
		PlayerData.SaveInputSettings();
		_view.NotifyBindingChanged();
		_view.UpdateKeyRebindingElementDisplays();
	}

	public void InitializeInputRebindables(KeyRebindingElement[] rebindingElements)
	{
		for (int i = 0; i < rebindingElements.Length; i++)
		{
			rebindingElements[i].Initialize(this);
		}
	}

	private void ApplyAudioSettings(SettingsSave saveData)
	{
		if (Locator.GetAudioMixer() != null)
		{
			Locator.GetAudioMixer().SetMasterVolume(saveData.masterVolume);
			Locator.GetAudioMixer().SetMasterMusicVolume(saveData.musicVolume);
			Locator.GetAudioMixer().SetMasterSFXVolume(saveData.sfxVolume);
		}
	}

	public void UpdateAllCachedSettings(SettingsMenuData[] uiOptions)
	{
		SettingsUiUtil.UpdateAllSettingsDataFromUi(uiOptions, _updatedGameSettings, _resolutions, _updatedGfxSettings);
	}

	public void UpdateCachedSetting(SettingsMenuData uiOption)
	{
		SettingsUiUtil.UpdateAnySettingDataFromUi(uiOption, _updatedGameSettings, _resolutions, _updatedGfxSettings);
	}

	private void RefreshConnectedDeviceList()
	{
		ReadOnlyArray<Gamepad> gamepadList = OWInput.GetGamepadList();
		if (_listCachedGamepadState == null)
		{
			_listCachedGamepadState = new List<CachedGamepadState>();
		}
		else
		{
			_listCachedGamepadState.Clear();
		}
		long[] enableDeviceWhitelistIds = InputUtil.GetEnableDeviceWhitelistIds();
		foreach (Gamepad item2 in gamepadList)
		{
			HID.HIDDeviceDescriptor hIDDeviceDescriptor = HID.HIDDeviceDescriptor.FromJson(item2.description.capabilities);
			CachedGamepadState item = default(CachedGamepadState);
			item.persistentInfo = default(SettingsSave.UserDeviceInfo);
			item.persistentInfo.userEnabled = item2.enabled;
			item.persistentInfo.unityDeviceName = item2.name;
			if (InputUtil.IsXInputController(item2.name))
			{
				item.persistentInfo.productId = -1;
				item.persistentInfo.vendorId = 1118;
			}
			else
			{
				item.persistentInfo.productId = hIDDeviceDescriptor.productId;
				item.persistentInfo.vendorId = hIDDeviceDescriptor.vendorId;
			}
			item.persistentInfo.manufacturer = item2.description.manufacturer;
			item.persistentInfo.productName = item2.description.product;
			item.deviceId = item2.deviceId;
			item.connected = item2.added;
			item.displayIndex = -1;
			item.isWhitelisted = false;
			long num = InputUtil.ConcatBitwiseId(item.persistentInfo.vendorId, item.persistentInfo.productId);
			for (int i = 0; i < enableDeviceWhitelistIds.Length; i++)
			{
				if (num == enableDeviceWhitelistIds[i])
				{
					item.isWhitelisted = true;
					break;
				}
			}
			_listCachedGamepadState.Add(item);
		}
	}

	public List<MultiSelectionListElement.ListEntry> GetControllerUIList()
	{
		RefreshConnectedDeviceList();
		List<MultiSelectionListElement.ListEntry> list = new List<MultiSelectionListElement.ListEntry>();
		if (_listCachedGamepadState != null)
		{
			for (int i = 0; i < _listCachedGamepadState.Count; i++)
			{
				MultiSelectionListElement.ListEntry item = default(MultiSelectionListElement.ListEntry);
				if (!_listCachedGamepadState[i].connected)
				{
					continue;
				}
				CachedGamepadState value = _listCachedGamepadState[i];
				item.itemIndex = i;
				item.itemLabel = value.persistentInfo.unityDeviceName;
				if (InputUtil.GamepadDisplayNameLookup.TryGetValue(item.itemLabel, out var value2))
				{
					item.itemLabel = value2;
				}
				else
				{
					char[] trimChars = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
					string key = item.itemLabel.TrimEnd(trimChars);
					if (InputUtil.GamepadDisplayNameLookup.TryGetValue(key, out value2))
					{
						item.itemLabel = value2;
					}
				}
				if (!value.isWhitelisted)
				{
					ref string itemLabel = ref item.itemLabel;
					itemLabel = itemLabel + " " + UITextLibrary.GetString(UITextType.MenuOption_UnsupportedController);
				}
				item.itemBoolVal = value.persistentInfo.userEnabled;
				value.displayIndex = i;
				_listCachedGamepadState[i] = value;
				list.Add(item);
			}
		}
		return list;
	}

	public void RealtimeOptionUpdate(SettingsMenuData menuData)
	{
		switch (menuData.id)
		{
		case SettingsID.BUTTON_PROMPTS:
			Locator.GetPromptManager().UpdatePromptsEnabled(_updatedGameSettings.buttonPromptsEnabled);
			break;
		case SettingsID.INPUT_BUTTON_PROMPT:
			ButtonPromptLibrary.SharedInstance.SetConfigTextures(_updatedGameSettings.promptImgSet);
			break;
		case SettingsID.VOL_MASTER:
			Locator.GetAudioMixer().SetMasterVolume(_updatedGameSettings.masterVolume);
			break;
		case SettingsID.VOL_MUSIC:
			Locator.GetAudioMixer().SetMasterMusicVolume(_updatedGameSettings.musicVolume);
			break;
		case SettingsID.VOL_SFX:
			Locator.GetAudioMixer().SetMasterSFXVolume(_updatedGameSettings.sfxVolume);
			break;
		case SettingsID.GFX_ASPECT_RATIO:
		{
			AspectRatio aspectRatio = _updatedGfxSettings.aspectRatio;
			_resolutions = SystemDisplay.GetResolutionsWithAspect(aspectRatio);
			int index = _resolutions.Length - 1;
			string[] array = new string[_resolutions.Length];
			for (int i = 0; i < _resolutions.Length; i++)
			{
				array[i] = _resolutions[i].width + "x" + _resolutions[i].height;
			}
			((OptionsSelectorElement)menuData.dependentMenuOption).Initialize(index, array);
			break;
		}
		case SettingsID.GFX_GAMMA:
		{
			float num = _updatedGfxSettings.gammaValue * 2f - 1f;
			menuData.secondaryTextField.text = ((num > 0f) ? ("+" + num.ToString("F1")) : num.ToString("F1"));
			GlobalMessenger<GraphicSettings>.FireEvent("GraphicSettingsUpdated", _updatedGfxSettings);
			break;
		}
		case SettingsID.GFX_FOV:
			menuData.secondaryTextField.text = _updatedGfxSettings.fieldOfView.ToString("F0");
			GlobalMessenger<GraphicSettings>.FireEvent("GraphicSettingsUpdated", _updatedGfxSettings);
			break;
		}
	}

	public void ChangeControllersEnabled(MultiSelectionListElement.ListEntry[] listFromUiElement)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < listFromUiElement.Length; i++)
		{
			MultiSelectionListElement.ListEntry listEntry = listFromUiElement[i];
			for (int j = 0; j < _listCachedGamepadState.Count; j++)
			{
				CachedGamepadState value = _listCachedGamepadState[j];
				if (value.displayIndex == listEntry.itemIndex && value.persistentInfo.userEnabled != listEntry.itemBoolVal)
				{
					list.Add(value.deviceId);
					value.persistentInfo.userEnabled = listEntry.itemBoolVal;
					_listCachedGamepadState[j] = value;
					break;
				}
			}
		}
		OWInput.ChangeControllersEnabled(list);
		List<SettingsSave.UserDeviceInfo> list2 = new List<SettingsSave.UserDeviceInfo>();
		for (int k = 0; k < _listCachedGamepadState.Count; k++)
		{
			list2.Add(_listCachedGamepadState[k].persistentInfo);
		}
		_updatedGameSettings.deviceEnabledList = list2.ToArray();
	}
}
