using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

public static class PlayerData
{
	private static GameSave _currentGameSave;

	private static SettingsSave _settingsSave;

	private static GraphicSettings _graphicsSettings;

	private static bool s_ranGammaSetupThisSession;

	private static string inputJSON => ((InputManager)OWInput.SharedInputManager).commandManager.InputActions.ToJson();

	static PlayerData()
	{
		_currentGameSave = null;
		_settingsSave = null;
		_graphicsSettings = null;
	}

	public static void Init(GameSave saveData, SettingsSave settingsData, GraphicSettings graphicSettings, string jsonInputActions)
	{
		_currentGameSave = saveData;
		_settingsSave = settingsData;
		_graphicsSettings = graphicSettings;
		_graphicsSettings.ApplyAllGraphicSettings();
		if (!((InputManager)OWInput.SharedInputManager).commandManager.LoadActions(jsonInputActions))
		{
			Debug.LogError("Input Actions loading failed, resetting to defaults");
			((InputManager)OWInput.SharedInputManager).commandManager.LoadDefaultInputActions();
		}
		_settingsSave.ApplyAllSettings();
		OWInput.SharedInputManager.LateInitialize();
		DialogueConditionManager.SharedInstance.ReadPlayerData();
	}

	public static bool IsLoaded()
	{
		return _settingsSave != null;
	}

	public static void SetSettingsData(SettingsSave settingsData)
	{
		_settingsSave = settingsData;
		_settingsSave.ApplyAllSettings();
	}

	public static void CopySettingsSaveData(SettingsSave targetSettings)
	{
		_settingsSave.CopyTo(targetSettings);
	}

	public static SettingsSave CloneSettingsData()
	{
		return _settingsSave.Clone();
	}

	public static void SetRebindingSettings(InputRebindableData rebindingData, int activeGamepad)
	{
	}

	public static void OnSetFirstControllerAfterStartup(int newActiveGamepad, InputUtil.GamePadPresetConfig gamepadConfig, InputUtil.ButtonPromptPresetConfig promptConfig)
	{
	}

	public static void SetGraphicSettings(GraphicSettings graphicSettings)
	{
		_graphicsSettings = graphicSettings;
		_graphicsSettings.ApplyAllGraphicSettings();
	}

	public static GraphicSettings GetGraphicSettings()
	{
		if (_graphicsSettings == null)
		{
			Debug.LogWarning("Getting graphics settings before settings are initialized!");
			_graphicsSettings = new GraphicSettings(init: true);
		}
		return _graphicsSettings;
	}

	public static void SaveCurrentGame()
	{
		_currentGameSave.version = Application.version;
		StandaloneProfileManager.SharedInstance.SaveGame(_currentGameSave, null, null, null);
	}

	public static void ResetGame()
	{
		bool didRunInitGammaSetting = false;
		if (_currentGameSave != null)
		{
			didRunInitGammaSetting = _currentGameSave.didRunInitGammaSetting;
		}
		_currentGameSave = new GameSave();
		_currentGameSave.didRunInitGammaSetting = didRunInitGammaSetting;
		StandaloneProfileManager.SharedInstance.SaveGame(_currentGameSave, null, null, null);
	}

	public static void SaveSettings()
	{
		StandaloneProfileManager.SharedInstance.SaveGame(null, _settingsSave, _graphicsSettings, inputJSON);
	}

	public static void SaveInputSettings()
	{
		StandaloneProfileManager.SharedInstance.SaveGame(null, null, null, inputJSON);
	}

	public static bool IsBusy()
	{
		return StandaloneProfileManager.SharedInstance.isBusyWithFileOps;
	}

	public static InputUtil.GamePadPresetConfig GetGamepadConfig()
	{
		Gamepad current = Gamepad.current;
		if (current is DualShockGamepad)
		{
			return InputUtil.GamePadPresetConfig.PS4;
		}
		if (current is XInputController)
		{
			return InputUtil.GamePadPresetConfig.XBOX;
		}
		if (current is SwitchProControllerHID)
		{
			return InputUtil.GamePadPresetConfig.SWITCH_PRO;
		}
		return InputUtil.GamePadPresetConfig.XBOX;
	}

	public static ButtonPromptImgSet GetButtonPromptImageSetting()
	{
		if (_settingsSave == null)
		{
			return GetDefaultButtonPromptImageSetting();
		}
		ButtonPromptImgSet promptImgSet = _settingsSave.promptImgSet;
		if (promptImgSet == ButtonPromptImgSet.DEFAULT)
		{
			return GetDefaultButtonPromptImageSetting();
		}
		return _settingsSave.promptImgSet;
	}

	public static ButtonPromptImgSet GetDefaultButtonPromptImageSetting()
	{
		Gamepad current = Gamepad.current;
		if (current is DualShockGamepad)
		{
			return ButtonPromptImgSet.DUALSHOCK_4;
		}
		if (current is XInputController)
		{
			return ButtonPromptImgSet.XBOX_ONE;
		}
		if (current is SwitchProControllerHID)
		{
			return ButtonPromptImgSet.SWITCH_PRO;
		}
		if (SteamUtils.IsSteamRunningOnSteamDeck())
		{
			return ButtonPromptImgSet.STEAMDECK;
		}
		return ButtonPromptImgSet.XBOX_ONE;
	}

	public static string GetLastUsedDeviceName()
	{
		return Gamepad.current.displayName;
	}

	public static int GetLastUsedDeviceIndex()
	{
		return Gamepad.current.deviceId;
	}

	public static bool RanFirstRunGammaSetup()
	{
		if (s_ranGammaSetupThisSession)
		{
			return true;
		}
		return _currentGameSave.didRunInitGammaSetting;
	}

	public static void SetRanFirstRunGammaSetup(bool val)
	{
		if (val)
		{
			s_ranGammaSetupThisSession = true;
		}
		_currentGameSave.didRunInitGammaSetting = val;
		SaveCurrentGame();
	}

	public static void AddShipLogFactSave(ShipLogFactSave factSave)
	{
		if (_currentGameSave.shipLogFactSaves.ContainsKey(factSave.id))
		{
			Debug.LogError("Save file already contains a fact called " + factSave.id);
			Debug.Break();
		}
		else
		{
			_currentGameSave.shipLogFactSaves.Add(factSave.id, factSave);
		}
	}

	public static ShipLogFactSave GetShipLogFactSave(string id)
	{
		if (_currentGameSave.shipLogFactSaves.ContainsKey(id))
		{
			return _currentGameSave.shipLogFactSaves[id];
		}
		return null;
	}

	public static void AddNewlyRevealedFactID(string id)
	{
		_currentGameSave.newlyRevealedFactIDs.Add(id);
	}

	public static List<string> GetNewlyRevealedFactIDs()
	{
		return _currentGameSave.newlyRevealedFactIDs;
	}

	public static void ClearNewlyRevealedFactIDs()
	{
		_currentGameSave.newlyRevealedFactIDs.Clear();
	}

	public static void SetLastDeathType(DeathType deathType)
	{
		_currentGameSave.lastDeathType = deathType;
		SaveCurrentGame();
	}

	public static DeathType GetLastDeathType()
	{
		return _currentGameSave.lastDeathType;
	}

	public static bool KnowsAboutWarpReceivers()
	{
		return false;
	}

	public static bool KnowsFrequency(SignalFrequency frequency)
	{
		return _currentGameSave.knownFrequencies[AudioSignal.FrequencyToIndex(frequency)];
	}

	public static bool KnowsMultipleFrequencies()
	{
		int num = 0;
		for (int i = 1; i < _currentGameSave.knownFrequencies.Length; i++)
		{
			if (_currentGameSave.knownFrequencies[i])
			{
				num++;
			}
		}
		return num > 1;
	}

	public static void LearnSignal(SignalName signalName)
	{
		if (!_currentGameSave.knownSignals.ContainsKey(100))
		{
			_currentGameSave.knownSignals.Add(100, value: false);
		}
		if (!_currentGameSave.knownSignals.ContainsKey(101))
		{
			_currentGameSave.knownSignals.Add(101, value: false);
		}
		if (_currentGameSave.knownSignals.ContainsKey((int)signalName) && !_currentGameSave.knownSignals[(int)signalName])
		{
			_currentGameSave.knownSignals[(int)signalName] = true;
			SaveCurrentGame();
		}
	}

	public static bool KnowsSignal(SignalName signalName)
	{
		if (_currentGameSave.knownSignals.ContainsKey((int)signalName))
		{
			return _currentGameSave.knownSignals[(int)signalName];
		}
		return false;
	}

	public static void LearnFrequency(SignalFrequency frequency)
	{
		int num = AudioSignal.FrequencyToIndex(frequency);
		if (!_currentGameSave.knownFrequencies[num])
		{
			_currentGameSave.knownFrequencies[num] = true;
			SaveCurrentGame();
		}
	}

	public static void ForgetFrequency(SignalFrequency frequency)
	{
		_currentGameSave.knownFrequencies[AudioSignal.FrequencyToIndex(frequency)] = false;
		SaveCurrentGame();
	}

	public static void LearnLaunchCodes()
	{
		bool flag = false;
		if (!_currentGameSave.PersistentConditionExists("LAUNCH_CODES_GIVEN"))
		{
			flag = true;
		}
		else if (_currentGameSave.GetPersistentCondition("LAUNCH_CODES_GIVEN"))
		{
			flag = true;
		}
		if (flag)
		{
			DialogueConditionManager.SharedInstance.SetConditionState("SCIENTIST_3", conditionState: true);
			_currentGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", state: true);
			GlobalMessenger.FireEvent("LearnLaunchCodes");
		}
	}

	public static bool KnowsLaunchCodes()
	{
		return _currentGameSave.GetPersistentCondition("LAUNCH_CODES_GIVEN");
	}

	public static void SetPersistentCondition(string condition, bool state)
	{
		_currentGameSave.SetPersistentCondition(condition, state);
		switch (condition)
		{
		case "PLAYER_ENTERED_TIMELOOPCORE":
			return;
		case "PROBE_ENTERED_TIMELOOPCORE":
			return;
		case "PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE":
			return;
		}
		SaveCurrentGame();
	}

	public static bool GetPersistentCondition(string condition)
	{
		return _currentGameSave.GetPersistentCondition(condition);
	}

	public static bool PersistentConditionExists(string condition)
	{
		return _currentGameSave.PersistentConditionExists(condition);
	}

	public static void SaveLoopCount(int loopCount)
	{
		_currentGameSave.loopCount = loopCount;
		SaveCurrentGame();
	}

	public static int LoadLoopCount()
	{
		return _currentGameSave.loopCount;
	}

	public static void SaveWarpedToTheEye(float secondsRemaining)
	{
		_currentGameSave.warpedToTheEye = true;
		_currentGameSave.secondsRemainingOnWarp = secondsRemaining;
		SaveCurrentGame();
	}

	public static void SaveEyeCompletion()
	{
		_currentGameSave.warpedToTheEye = false;
		SaveCurrentGame();
	}

	public static bool GetWarpedToTheEye()
	{
		return _currentGameSave.warpedToTheEye;
	}

	public static float GetSecondsRemainingOnWarp()
	{
		return _currentGameSave.secondsRemainingOnWarp;
	}

	public static int GetLoopCountOnParadoxStart()
	{
		return _currentGameSave.loopCountOnParadox;
	}

	public static void SetLoopCountOnParadoxStart()
	{
		if (_currentGameSave.loopCountOnParadox > 0)
		{
			Debug.LogWarning("Paradox loop count has already been set!  Retaining existing loop count (" + _currentGameSave.loopCountOnParadox + ").");
		}
		else
		{
			_currentGameSave.loopCountOnParadox = _currentGameSave.loopCount;
		}
	}

	public static void RevertParadoxLoopCountStates()
	{
		if (_currentGameSave.loopCountOnParadox <= 0)
		{
			Debug.LogError("Tried to restore pre-paradox loop count with an invalid value (" + _currentGameSave.loopCountOnParadox + ")!");
			return;
		}
		_currentGameSave.loopCount = _currentGameSave.loopCountOnParadox;
		_currentGameSave.loopCountOnParadox = 0;
		SaveCurrentGame();
	}

	public static void SetShownPopups(StartupPopups shownPopup)
	{
		_currentGameSave.shownPopups |= shownPopup;
	}

	public static bool GetAutoEquipTranslator()
	{
		return _settingsSave.autoEquipTranslator;
	}

	public static bool GetShowShipLogNotifications()
	{
		return _settingsSave.showShipLogNotifications;
	}

	public static bool GetDetectiveModeEnabled()
	{
		return _settingsSave.detectiveModeEnabled;
	}

	public static bool GetFreezeTimeWhileReadingTranslator()
	{
		if (_settingsSave.freezeTimeWhileReading && !GUIMode.IsCaptureMode())
		{
			return !GUIMode.IsHiddenMode();
		}
		return false;
	}

	public static bool GetFreezeTimeWhileReadingShipLog()
	{
		if (_settingsSave.freezeTimeWhileReadingShipLog && !GUIMode.IsCaptureMode())
		{
			return !GUIMode.IsHiddenMode();
		}
		return false;
	}

	public static bool GetFreezeTimeWhileReadingConversations()
	{
		if (_settingsSave.freezeTimeWhileReadingConversations && !GUIMode.IsCaptureMode())
		{
			return !GUIMode.IsHiddenMode();
		}
		return false;
	}

	public static TextTranslation.Language GetSavedLanguage()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.language;
		}
		return TextTranslation.Language.UNKNOWN;
	}

	public static bool GetAutoRun()
	{
		return _settingsSave.autoRun;
	}

	public static void SetAutoRun(bool autoRun)
	{
		_settingsSave.autoRun = autoRun;
	}

	public static bool GetAutopilotEnabled()
	{
		return _settingsSave.autopilotEnabled;
	}

	public static bool GetPromptsEnabled()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.buttonPromptsEnabled;
		}
		return true;
	}

	public static bool GetAutoBoost()
	{
		return _settingsSave.autoBoost;
	}

	public static void SetAutoBoost(bool autoBoost)
	{
		_settingsSave.autoBoost = autoBoost;
	}

	public static bool GetReducedFrights()
	{
		return _settingsSave.reducedFrights;
	}

	public static int GetLookInversionFactor()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.inversionFactor;
		}
		return 1;
	}

	public static int GetShipLookInversionFactor()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.shipInversionFactor;
		}
		return 1;
	}

	public static void SetLookInversionFactor(int inversionFactor)
	{
		_settingsSave.inversionFactor = inversionFactor;
	}

	public static int GetLookSensitivity()
	{
		return _settingsSave.lookSensitivity;
	}

	public static void SetLookSensitivity(int lookSensitivity)
	{
		_settingsSave.lookSensitivity = lookSensitivity;
	}

	public static int GetFlightSensitivity()
	{
		return _settingsSave.flightSensitivity;
	}

	public static void SetFlightSensitivity(int flightSensitivity)
	{
		_settingsSave.flightSensitivity = flightSensitivity;
	}

	public static TextSpeed LoadTextSpeed()
	{
		return _settingsSave.textSpeed;
	}

	public static void SetTextSpeed(TextSpeed textSpeed)
	{
		_settingsSave.textSpeed = textSpeed;
	}

	public static bool IsUILargeTextSize()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.IsLargeTextSize();
		}
		return false;
	}

	public static void SetUiSizeSetting(UITextSize textSize)
	{
		_settingsSave.uiTextSize = textSize;
	}

	public static UITextSize GetTextSize()
	{
		return _settingsSave.uiTextSize;
	}

	public static float GetMasterVolume()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.masterVolume;
		}
		return 1f;
	}

	public static float GetSFXVolume()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.sfxVolume;
		}
		return 1f;
	}

	public static float GetMusicVolume()
	{
		if (_settingsSave != null)
		{
			return _settingsSave.musicVolume;
		}
		return 1f;
	}

	public static int EatBurnedMarshmallow()
	{
		_currentGameSave.burnedMarshmallowEaten++;
		return _currentGameSave.burnedMarshmallowEaten;
	}

	public static void EatPerfectMarshmallow()
	{
		_currentGameSave.perfectMarshmallowsEaten++;
		Achievements.SetHeroStat(Achievements.HeroStat.PERFECT_MARSHMALLOW, _currentGameSave.perfectMarshmallowsEaten);
	}

	public static void CompletedFullTimeLoop()
	{
		_currentGameSave.fullTimeloops++;
		Achievements.SetHeroStat(Achievements.HeroStat.FULL_TIMELOOP, _currentGameSave.fullTimeloops);
	}

	public static uint GetFullTimeLoopsCompleted()
	{
		return _currentGameSave.fullTimeloops;
	}

	public static bool GetResumeGameActivityCardAvailable()
	{
		if (_currentGameSave != null)
		{
			return _currentGameSave.ps5Activity_canResumeExpedition;
		}
		return false;
	}

	public static List<string> GetAvailbleShipLogActivityCards()
	{
		if (_currentGameSave != null)
		{
			return _currentGameSave.ps5Activity_availableShipLogCards;
		}
		return null;
	}

	public static void UpdateAvailableActivityCards(bool canResumeExpedition, List<string> shipLogCards)
	{
		if (_currentGameSave != null)
		{
			_currentGameSave.ps5Activity_canResumeExpedition = canResumeExpedition;
			_currentGameSave.ps5Activity_availableShipLogCards.Clear();
			_currentGameSave.ps5Activity_availableShipLogCards.AddRange(shipLogCards);
		}
	}
}
