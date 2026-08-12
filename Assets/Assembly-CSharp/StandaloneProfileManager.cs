using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;

public class StandaloneProfileManager : IProfileManager
{
	[Serializable]
	public class ProfileData
	{
		public string profileName;

		public DateTime lastModifiedTime;

		public bool brokenSaveData;

		public bool brokenSettingsData;

		public bool brokenGfxSettingsData;

		public bool brokenRebindingData;

		private GameSave _gameSave;

		private SettingsSave _settingsSave;

		private GraphicSettings _graphicsSettings;

		private string _inputJSON;

		[JsonIgnore]
		public GameSave gameSave
		{
			get
			{
				return _gameSave;
			}
			set
			{
				_gameSave = value;
			}
		}

		[JsonIgnore]
		public SettingsSave settingsSave
		{
			get
			{
				return _settingsSave;
			}
			set
			{
				_settingsSave = value;
			}
		}

		[JsonIgnore]
		public GraphicSettings graphicsSettings
		{
			get
			{
				return _graphicsSettings;
			}
			set
			{
				_graphicsSettings = value;
			}
		}

		[JsonIgnore]
		public string inputJSON
		{
			get
			{
				return _inputJSON;
			}
			set
			{
				_inputJSON = value;
			}
		}

		[OnDeserializing]
		private void SetDefaultValuesOnDeserializing(StreamingContext context)
		{
			brokenSaveData = false;
			brokenSettingsData = false;
			brokenGfxSettingsData = false;
			brokenRebindingData = false;
		}

		[OnDeserialized]
		private void SetDefaultValuesOnDeserialized(StreamingContext context)
		{
			brokenSaveData = false;
			brokenSettingsData = false;
			brokenGfxSettingsData = false;
			brokenRebindingData = false;
		}
	}

	public delegate void NoProfilesExistEvent();

	public delegate void BrokenDataExistsEvent();

	public delegate void BackupDataRestoredEvent();

	public delegate void UpdatePlayerProfilesEvent();

	private List<ProfileData> _profiles;

	private static StandaloneProfileManager s_instance;

	private const string _saveDirectory = "/SteamSaves";

	private const string _backupDirectory = "/Backup";

	private const string _tempDirectory = "/Temp";

	private const string _gameSaveFilename = "data.owsave";

	private const string _gameSettingsFilename = "player.owsett";

	private const string _gfxSettingsFilename = "graphics.owsett";

	private const string _legacyInputBindingSettingsFilename = "input.owsett";

	private const string _inputActionsSettingsFilename = "input_new.owsett";

	private const int _profileNameCharLimit = 16;

	private string _profilesPath;

	private string _profileTempPath;

	private string _profileBackupPath;

	private ProfileData _currentProfile;

	private int _fileOpsBusyLocks;

	private GameSave _pendingGameSave;

	private SettingsSave _pendingSettingsSave;

	private GraphicSettings _pendingGfxSettingsSave;

	private string _pendingInputJSONSave;

	private BinaryFormatter _binaryFormatter;

	private JsonSerializer _jsonSerializer;

	public static StandaloneProfileManager SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new StandaloneProfileManager();
			}
			return s_instance;
		}
	}

	public GameSave currentProfileGameSave => _currentProfile?.gameSave;

	public SettingsSave currentProfileGameSettings => _currentProfile?.settingsSave;

	public GraphicSettings currentProfileGraphicsSettings => _currentProfile?.graphicsSettings;

	public string currentProfileInputJSON => _currentProfile?.inputJSON;

	public ProfileData currentProfile => _currentProfile;

	public ProfileData mostRecentProfile => Enumerable.FirstOrDefault(Enumerable.OrderByDescending(_profiles, (ProfileData profile) => profile.lastModifiedTime));

	public int profileNameCharacterLimit => 16;

	public List<ProfileData> profiles => _profiles;

	public int numberOfProfiles => _profiles.Count;

	public bool isInitialized => currentProfileGameSave != null;

	public bool isBusyWithFileOps => _fileOpsBusyLocks > 0;

	public bool hasPendingSaveOperation
	{
		get
		{
			if (_pendingGameSave == null && _pendingSettingsSave == null && _pendingGfxSettingsSave == null)
			{
				return _pendingInputJSONSave != "";
			}
			return true;
		}
	}

	public int profileCharacterLimit => 16;

	public event NoProfilesExistEvent OnNoProfilesExist;

	public event BrokenDataExistsEvent OnBrokenDataExists;

	public event BackupDataRestoredEvent OnBackupDataRestored;

	public event UpdatePlayerProfilesEvent OnUpdatePlayerProfiles;

	public event ProfileSignInCompleteEvent OnProfileSignInComplete;

	public event ProfileReadDoneEvent OnProfileReadDone;

	public event ProfileDataSavedEvent OnProfileDataSaved;

	public event ProfileSignOutCompleteEvent OnProfileSignOutComplete;

	public event ProfileSignInStartEvent OnProfileSignInStart;

	public event ProfileSignOutStartEvent OnProfileSignOutStart;

	public event ControllerDisconnectedEvent OnControllerDisconnected;

	public event ControllerReconnectedEvent OnControllerReconnected;

	public void PreInitialize()
	{
		_fileOpsBusyLocks = 0;
		_pendingGameSave = null;
		_pendingSettingsSave = null;
		_pendingGfxSettingsSave = null;
		_pendingInputJSONSave = "";
	}

	public void Initialize()
	{
		_profilesPath = Application.persistentDataPath + "/SteamSaves";
		_profileBackupPath = Application.persistentDataPath + "/Backup";
		_profileTempPath = Application.persistentDataPath + "/Temp";
		_profiles = new List<ProfileData>();
		VersionDeserializationBinder versionDeserializationBinder = new VersionDeserializationBinder();
		_jsonSerializer = new JsonSerializer();
		_jsonSerializer.SerializationBinder = versionDeserializationBinder;
		_binaryFormatter = new BinaryFormatter();
		_binaryFormatter.Binder = versionDeserializationBinder;
		Achievements.Init();
		InitializeProfileData();
	}

	public void InitializeForEditor()
	{
		_profilesPath = Application.persistentDataPath + "/SteamSaves";
		_profileBackupPath = Application.persistentDataPath + "/Backup";
		_profileTempPath = Application.persistentDataPath + "/Temp";
		_profiles = new List<ProfileData>();
		VersionDeserializationBinder versionDeserializationBinder = new VersionDeserializationBinder();
		_jsonSerializer = new JsonSerializer();
		_jsonSerializer.SerializationBinder = versionDeserializationBinder;
		_binaryFormatter = new BinaryFormatter();
		_binaryFormatter.Binder = versionDeserializationBinder;
		MarkBusyWithFileOps(isBusy: true);
		_profiles.Clear();
		LoadProfiles();
		LoadSaveFilesFromProfiles();
		bool flag = false;
		for (int i = 0; i < _profiles.Count; i++)
		{
			if (_profiles[i].profileName == "Debug")
			{
				_currentProfile = _profiles[i];
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			TryCreateProfile("Debug");
		}
		MarkBusyWithFileOps(isBusy: false);
		PlayerData.Init(currentProfileGameSave, currentProfileGameSettings, currentProfileGraphicsSettings, currentProfileInputJSON);
	}

	private void MarkBusyWithFileOps(bool isBusy)
	{
		if (isBusy)
		{
			_fileOpsBusyLocks++;
		}
		else if (_fileOpsBusyLocks <= 0)
		{
			Debug.LogWarning("No File I/O lock to remove!");
		}
		else
		{
			_fileOpsBusyLocks--;
		}
	}

	public void PerformPendingSaveOperation()
	{
		if (!isBusyWithFileOps && !LoadManager.IsBusy())
		{
			TrySaveProfile(_currentProfile, _pendingGameSave, _pendingSettingsSave, _pendingGfxSettingsSave, _pendingInputJSONSave);
			_pendingGameSave = null;
			_pendingSettingsSave = null;
			_pendingGfxSettingsSave = null;
			_pendingInputJSONSave = "";
		}
	}

	public void SaveGame(GameSave gameSave, SettingsSave settSave, GraphicSettings graphicSettings, string inputBindings)
	{
		if (isBusyWithFileOps || LoadManager.IsBusy())
		{
			_pendingGameSave = gameSave;
			_pendingSettingsSave = settSave;
			_pendingGfxSettingsSave = graphicSettings;
			_pendingInputJSONSave = inputBindings;
		}
		else
		{
			TrySaveProfile(_currentProfile, gameSave, settSave, graphicSettings, inputBindings);
		}
	}

	private void InitializeProfileData()
	{
		LoadProfiles();
		_currentProfile = mostRecentProfile;
		if (_currentProfile == null)
		{
			this.OnNoProfilesExist?.Invoke();
		}
		else
		{
			LoadSaveFilesFromProfiles();
		}
	}

	private void LoadSaveFilesFromProfiles()
	{
		MarkBusyWithFileOps(isBusy: true);
		foreach (ProfileData profile in _profiles)
		{
			string path = _profilesPath + "/" + profile.profileName;
			GameSave saveData = null;
			SettingsSave saveData2 = null;
			GraphicSettings saveData3 = null;
			string inputJSON = "";
			if (Directory.Exists(path))
			{
				Stream stream = null;
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				profile.brokenSaveData = TryLoadSaveData<GameSave>(profile, ref stream, "data.owsave", directoryInfo, out saveData);
				profile.brokenSettingsData = TryLoadSaveData<SettingsSave>(profile, ref stream, "player.owsett", directoryInfo, out saveData2);
				profile.brokenGfxSettingsData = TryLoadSaveData<GraphicSettings>(profile, ref stream, "graphics.owsett", directoryInfo, out saveData3);
				profile.brokenRebindingData = TryLoadInputBindingsSave(profile, ref stream, directoryInfo, out inputJSON);
			}
			string text = _profileBackupPath + "/" + profile.profileName;
			string path2 = text + "/data.owsave";
			string path3 = text + "/player.owsett";
			string path4 = text + "/graphics.owsett";
			string path5 = text + "/input_new.owsett";
			if (saveData == null)
			{
				profile.brokenSaveData = File.Exists(path2);
				saveData = new GameSave();
				Debug.LogError("Could not find game save for " + profile.profileName);
			}
			if (saveData2 == null)
			{
				profile.brokenSettingsData = File.Exists(path3);
				saveData2 = new SettingsSave();
				Debug.LogError("Could not find game settings for " + profile.profileName);
			}
			if (saveData3 == null)
			{
				profile.brokenGfxSettingsData = File.Exists(path4);
				saveData3 = new GraphicSettings(init: true);
				Debug.LogError("Could not find graphics settings for " + profile.profileName);
			}
			if (string.IsNullOrEmpty(inputJSON))
			{
				profile.brokenRebindingData = File.Exists(path5);
				inputJSON = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
				Debug.LogError("Could not find input action settings for " + profile.profileName);
			}
			profile.gameSave = saveData;
			profile.settingsSave = saveData2;
			profile.graphicsSettings = saveData3;
			profile.inputJSON = inputJSON;
		}
		MarkBusyWithFileOps(isBusy: false);
		if (CurrentProfileHasBrokenData())
		{
			this.OnBrokenDataExists?.Invoke();
		}
		this.OnProfileReadDone?.Invoke();
	}

	private bool TryLoadSaveData<T>(ProfileData profileData, ref Stream stream, string fileName, DirectoryInfo directoryInfo, out T saveData)
	{
		saveData = default(T);
		bool flag = true;
		FileInfo[] files = directoryInfo.GetFiles(fileName);
		if (files.Length != 0)
		{
			stream = null;
			if (TryOpenFile(files[0].FullName, ref stream))
			{
				JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(stream));
				flag = !TryDeserializeJson<T>(jsonTextReader, out saveData);
				if (flag)
				{
					stream.Position = 0L;
					flag = !TryDeserializeBinary<T>(stream, out saveData);
				}
				jsonTextReader.Close();
			}
		}
		return flag;
	}

	private bool TryLoadInputBindingsSave(ProfileData profileData, ref Stream stream, DirectoryInfo directoryInfo, out string inputJSON)
	{
		inputJSON = null;
		bool result = true;
		FileInfo[] files = directoryInfo.GetFiles("input_new.owsett");
		if (files.Length != 0)
		{
			stream = null;
			if (TryOpenFile(files[0].FullName, ref stream))
			{
				result = !TryDeserializeJsonAsInputActionsData(stream, out inputJSON);
			}
			stream?.Close();
		}
		return result;
	}

	private bool TryOpenFile(string fullPath, ref Stream dataStream)
	{
		try
		{
			dataStream = File.Open(fullPath, FileMode.Open);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("[" + ex.Message + "] Failed loading opening file " + fullPath);
			return false;
		}
	}

	private bool TryDeserializeBinary<T>(Stream dataStream, out T saveData)
	{
		try
		{
			saveData = default(T);
			saveData = (T)_binaryFormatter.Deserialize(dataStream);
			Debug.Log("Successfully read " + typeof(T).Name + " save data as binary");
			return true;
		}
		catch (Exception ex)
		{
			saveData = default(T);
			Debug.LogError("[" + ex.Message + "] Deserialization error for binary " + typeof(T).Name + " save data");
			return false;
		}
	}

	private bool TryDeserializeJson<T>(JsonTextReader jsonReader, out T rebindingData)
	{
		try
		{
			rebindingData = _jsonSerializer.Deserialize<T>(jsonReader);
			return true;
		}
		catch (Exception)
		{
			rebindingData = default(T);
			Debug.LogWarning("Could not read " + typeof(T).Name + " save data as JSON, it might be in binary so giving that a try.");
			return false;
		}
	}

	private bool TryDeserializeJsonAsInputActionsData(Stream dataStream, out string inputJSON)
	{
		try
		{
			using (StreamReader streamReader = new StreamReader(dataStream))
			{
				string text = streamReader.ReadToEnd();
				inputJSON = text;
				Debug.Log("Successfully read Input Bindings save data as JSON");
				return true;
			}
		}
		catch (Exception ex)
		{
			inputJSON = null;
			Debug.LogError("[" + ex.Message + "] Deserialization error for Input Actions Save");
			return false;
		}
	}

	public bool CurrentProfileHasBrokenData()
	{
		if (_currentProfile == null)
		{
			Debug.LogError("StandaloneProfileManager.CurrentProfileHasBrokenData We should never get here outside of the Unity Editor");
			return false;
		}
		if (!_currentProfile.brokenSaveData && !_currentProfile.brokenSettingsData && !_currentProfile.brokenGfxSettingsData)
		{
			return _currentProfile.brokenRebindingData;
		}
		return true;
	}

	public bool BackupExistsForBrokenData()
	{
		string text = _profileBackupPath + "/" + _currentProfile.profileName;
		string path = text + "/data.owsave";
		string path2 = text + "/player.owsett";
		string path3 = text + "/graphics.owsett";
		string path4 = text + "/input_new.owsett";
		if (_currentProfile.brokenSaveData && File.Exists(path))
		{
			return true;
		}
		if (_currentProfile.brokenSettingsData && File.Exists(path2))
		{
			return true;
		}
		if (_currentProfile.brokenGfxSettingsData && File.Exists(path3))
		{
			return true;
		}
		if (_currentProfile.brokenRebindingData && File.Exists(path4))
		{
			return true;
		}
		return false;
	}

	private void LoadProfiles()
	{
		MarkBusyWithFileOps(isBusy: true);
		_profiles.Clear();
		if (Directory.Exists(_profilesPath))
		{
			ProfileData profileData = null;
			Stream stream = null;
			FileInfo[] files = new DirectoryInfo(_profilesPath).GetFiles("*.owprofile");
			foreach (FileInfo fileInfo in files)
			{
				try
				{
					stream = null;
					stream = File.Open(fileInfo.FullName, FileMode.Open);
					JsonTextReader jsonTextReader = new JsonTextReader(new StreamReader(stream));
					try
					{
						profileData = _jsonSerializer.Deserialize<ProfileData>(jsonTextReader);
					}
					catch
					{
						stream.Position = 0L;
						profileData = (ProfileData)_binaryFormatter.Deserialize(stream);
					}
					finally
					{
						jsonTextReader.Close();
					}
					if (profileData == null)
					{
						Debug.LogError("Profile at " + fileInfo.FullName + " null. Skipping.");
					}
					else
					{
						_profiles.Add(profileData);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("[" + ex.Message + "] Failed loading profile at " + fileInfo.Name);
					stream?.Close();
				}
			}
		}
		MarkBusyWithFileOps(isBusy: false);
	}

	public void RestoreCurrentProfileBackup()
	{
		MarkBusyWithFileOps(isBusy: true);
		string text = _profilesPath + "/" + _currentProfile.profileName;
		string fullPath2 = text + "/data.owsave";
		string fullPath3 = text + "/player.owsett";
		string fullPath4 = text + "/graphics.owsett";
		string destFileName = text + "/input_new.owsett";
		string text2 = _profileBackupPath + "/" + _currentProfile.profileName;
		string text3 = text2 + "/data.owsave";
		string text4 = text2 + "/player.owsett";
		string text5 = text2 + "/graphics.owsett";
		string text6 = text2 + "/input_new.owsett";
		Stream stream = null;
		try
		{
			if (!Directory.Exists(_profilesPath))
			{
				Directory.CreateDirectory(_profilesPath);
			}
			if (!Directory.Exists(_profileTempPath))
			{
				Directory.CreateDirectory(_profileTempPath);
			}
			if (!Directory.Exists(_profileBackupPath))
			{
				Directory.CreateDirectory(_profileBackupPath);
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text2))
			{
				Directory.CreateDirectory(text2);
			}
			DirectoryInfo di = new DirectoryInfo(text2);
			if (_currentProfile.brokenSaveData && File.Exists(text3))
			{
				_currentProfile.gameSave = LoadAndCopyBackupSave<GameSave>("data.owsave", text3, fullPath2);
			}
			if (_currentProfile.brokenSettingsData && File.Exists(text4))
			{
				_currentProfile.settingsSave = LoadAndCopyBackupSave<SettingsSave>("player.owsett", text4, fullPath3);
			}
			if (_currentProfile.brokenGfxSettingsData && File.Exists(text5))
			{
				_currentProfile.graphicsSettings = LoadAndCopyBackupSave<GraphicSettings>("graphics.owsett", text5, fullPath4);
			}
			if (_currentProfile.brokenRebindingData && File.Exists(text6))
			{
				string inputJSON = "";
				TryLoadInputBindingsSave(_currentProfile, ref stream, di, out inputJSON);
				if (inputJSON != "")
				{
					_currentProfile.inputJSON = inputJSON;
					File.Copy(text6, destFileName, overwrite: true);
				}
				else
				{
					Debug.LogError("Could not load backup input bindings save.");
				}
				stream?.Close();
				stream = null;
			}
			if (this.OnBackupDataRestored != null)
			{
				this.OnBackupDataRestored();
			}
			T LoadAndCopyBackupSave<T>(string fileName, string backupPath, string fullPath) where T : class
			{
				TryLoadSaveData<T>(_currentProfile, ref stream, fileName, di, out var saveData);
				if (saveData != null)
				{
					File.Copy(backupPath, fullPath, overwrite: true);
				}
				else
				{
					Debug.LogError("Could not load backup " + typeof(T).Name + " save.");
				}
				stream?.Close();
				stream = null;
				return saveData;
			}
		}
		catch (Exception ex)
		{
			stream?.Close();
			Debug.LogError("Exception during backup restore: " + ex.Message);
			MarkBusyWithFileOps(isBusy: false);
		}
		MarkBusyWithFileOps(isBusy: false);
	}

	private bool TrySaveProfile(ProfileData pd, GameSave gameSave, SettingsSave settingsSave, GraphicSettings graphicsSettings, string inputJson)
	{
		MarkBusyWithFileOps(isBusy: true);
		string text = _profilesPath + "/" + pd.profileName;
		string text2 = _profilesPath + "/" + pd.profileName + ".owprofile";
		string text3 = text + "/data.owsave";
		string text4 = text + "/player.owsett";
		string text5 = text + "/graphics.owsett";
		string text6 = text + "/input_new.owsett";
		string text7 = _profileTempPath + "/GameData";
		string text8 = _profileTempPath + "/CurrentProfile.owprofile";
		string text9 = text7 + "/data.owsave";
		string text10 = text7 + "/player.owsett";
		string text11 = text7 + "/graphics.owsett";
		string text12 = text7 + "/input_new.owsett";
		string text13 = _profileBackupPath + "/" + pd.profileName;
		string destFileName = text13 + "/data.owsave";
		string destFileName2 = text13 + "/player.owsett";
		string destFileName3 = text13 + "/graphics.owsett";
		string destFileName4 = text13 + "/input_new.owsett";
		Stream stream = null;
		try
		{
			if (!Directory.Exists(_profilesPath))
			{
				Directory.CreateDirectory(_profilesPath);
			}
			if (!Directory.Exists(_profileTempPath))
			{
				Directory.CreateDirectory(_profileTempPath);
			}
			if (!Directory.Exists(_profileBackupPath))
			{
				Directory.CreateDirectory(_profileBackupPath);
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!Directory.Exists(text7))
			{
				Directory.CreateDirectory(text7);
			}
			if (!Directory.Exists(text13))
			{
				Directory.CreateDirectory(text13);
			}
			SaveData<ProfileData>(text8, pd);
			if (gameSave != null)
			{
				pd.gameSave = SaveData<GameSave>(text9, gameSave);
			}
			if (settingsSave != null)
			{
				pd.settingsSave = SaveData<SettingsSave>(text10, settingsSave);
			}
			if (graphicsSettings != null)
			{
				pd.graphicsSettings = SaveData<GraphicSettings>(text11, graphicsSettings);
			}
			if (inputJson != null)
			{
				File.WriteAllText(text12, inputJson);
				pd.inputJSON = inputJson;
			}
			if (File.Exists(text3))
			{
				File.Copy(text3, destFileName, overwrite: true);
			}
			if (File.Exists(text4))
			{
				File.Copy(text4, destFileName2, overwrite: true);
			}
			if (File.Exists(text5))
			{
				File.Copy(text5, destFileName3, overwrite: true);
			}
			if (File.Exists(text6))
			{
				File.Copy(text6, destFileName4, overwrite: true);
			}
			File.Delete(text2);
			File.Move(text8, text2);
			if (gameSave != null)
			{
				File.Delete(text3);
				File.Move(text9, text3);
			}
			if (settingsSave != null)
			{
				File.Delete(text4);
				File.Move(text10, text4);
			}
			if (graphicsSettings != null)
			{
				File.Delete(text5);
				File.Move(text11, text5);
			}
			if (inputJson != null)
			{
				File.Delete(text6);
				File.Move(text12, text6);
			}
			Debug.Log("Wrote save data to file for " + pd.profileName);
			if (this.OnProfileDataSaved != null)
			{
				this.OnProfileDataSaved(success: true);
			}
		}
		catch (Exception ex)
		{
			if (stream != null)
			{
				stream.Close();
			}
			if (this.OnProfileDataSaved != null)
			{
				this.OnProfileDataSaved(success: false);
			}
			Debug.LogError("[" + ex.Message + "] Error saving file for " + pd.profileName);
			MarkBusyWithFileOps(isBusy: false);
			return false;
		}
		MarkBusyWithFileOps(isBusy: false);
		return true;
		T SaveData<T>(string filePath, T data)
		{
			stream = File.Open(filePath, FileMode.Create);
			using (JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream)))
			{
				_jsonSerializer.Serialize(jsonWriter, data);
			}
			stream = null;
			return data;
		}
	}

	public bool IsValidCharacterForProfileName(char inputChar)
	{
		if (char.IsWhiteSpace(inputChar))
		{
			return false;
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		for (int i = 0; i < invalidFileNameChars.Length; i++)
		{
			if (invalidFileNameChars[i] == inputChar)
			{
				return false;
			}
		}
		if (inputChar == '.')
		{
			return false;
		}
		return true;
	}

	public bool ValidateProfileName(string profileName)
	{
		bool result = true;
		if (profileName == "")
		{
			result = false;
		}
		else if (profileName.Length > 16)
		{
			result = false;
		}
		else if (_profiles.Count > 0)
		{
			for (int i = 0; i < _profiles.Count; i++)
			{
				if (_profiles[i].profileName == profileName)
				{
					result = false;
				}
			}
		}
		return result;
	}

	public bool TryCreateProfile(string profileName)
	{
		bool flag = ValidateProfileName(profileName);
		if (flag)
		{
			bool flag2 = _profiles.Count == 0;
			ProfileData profileData = new ProfileData();
			profileData.profileName = profileName;
			profileData.lastModifiedTime = DateTime.UtcNow;
			GameSave gameSave = new GameSave();
			SettingsSave settingsSave = new SettingsSave();
			GraphicSettings graphicSettings = currentProfileGraphicsSettings;
			if (graphicSettings == null)
			{
				graphicSettings = new GraphicSettings(init: true);
			}
			string text = ((InputManager)OWInput.SharedInputManager).commandManager.DefaultInputActions.ToJson();
			_profiles.Add(profileData);
			profileData.gameSave = gameSave;
			profileData.settingsSave = settingsSave;
			profileData.graphicsSettings = graphicSettings;
			profileData.inputJSON = text;
			flag = TrySaveProfile(profileData, gameSave, settingsSave, graphicSettings, text);
			if (flag)
			{
				if (_currentProfile != null && _currentProfile.profileName != string.Empty && this.OnProfileSignOutComplete != null)
				{
					this.OnProfileSignOutComplete();
				}
				_currentProfile = profileData;
				if (flag2)
				{
					if (this.OnProfileSignInComplete != null)
					{
						this.OnProfileSignInComplete(ProfileManagerSignInResult.COMPLETE);
					}
					if (this.OnProfileReadDone != null)
					{
						this.OnProfileReadDone();
					}
				}
				else
				{
					if (this.OnProfileSignInComplete != null)
					{
						this.OnProfileSignInComplete(ProfileManagerSignInResult.COMPLETE);
					}
					if (this.OnProfileReadDone != null)
					{
						this.OnProfileReadDone();
					}
					if (this.OnUpdatePlayerProfiles != null)
					{
						this.OnUpdatePlayerProfiles();
					}
				}
			}
			else
			{
				DeleteProfile(profileName);
			}
		}
		return flag;
	}

	public bool SwitchProfile(string profileName)
	{
		LoadSaveFilesFromProfiles();
		bool flag = false;
		for (int i = 0; i < _profiles.Count; i++)
		{
			if (profileName == _profiles[i].profileName)
			{
				if (_currentProfile != null && _currentProfile.profileName != string.Empty && this.OnProfileSignOutComplete != null)
				{
					this.OnProfileSignOutComplete();
				}
				_currentProfile = _profiles[i];
				flag = true;
				break;
			}
		}
		if (flag)
		{
			_currentProfile.lastModifiedTime = DateTime.UtcNow;
			TrySaveProfile(_currentProfile, null, null, null, null);
			if (this.OnProfileSignInComplete != null)
			{
				this.OnProfileSignInComplete(ProfileManagerSignInResult.COMPLETE);
			}
			if (CurrentProfileHasBrokenData() && this.OnBrokenDataExists != null)
			{
				this.OnBrokenDataExists();
				return false;
			}
			if (this.OnProfileReadDone != null)
			{
				this.OnProfileReadDone();
			}
		}
		return true;
	}

	public void DeleteProfile(string profileName)
	{
		Debug.Log("DeleteProfile");
		bool flag = false;
		ProfileData profileData = new ProfileData();
		profileData.profileName = string.Empty;
		for (int i = 0; i < _profiles.Count; i++)
		{
			if (profileName == _profiles[i].profileName)
			{
				profileData = _profiles[i];
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		MarkBusyWithFileOps(isBusy: true);
		string text = _profilesPath + "/" + profileData.profileName + ".owprofile";
		string text2 = _profilesPath + "/" + profileData.profileName;
		string text3 = text2 + "/data.owsave";
		string text4 = text2 + "/player.owsett";
		string text5 = text2 + "/graphics.owsett";
		string text6 = text2 + "/input.owsett";
		string text7 = text2 + "/input_new.owsett";
		string text8 = _profileBackupPath + "/" + profileData.profileName;
		string text9 = text8 + "/data.owsave";
		string text10 = text8 + "/player.owsett";
		string text11 = text8 + "/graphics.owsett";
		string text12 = text8 + "/input.owsett";
		string text13 = text8 + "/input_new.owsett";
		Stream stream = null;
		try
		{
			if (File.Exists(text))
			{
				File.Delete(text);
				Debug.Log("Delete " + text);
			}
			if (File.Exists(text3))
			{
				File.Delete(text3);
				Debug.Log("Delete " + text3);
			}
			if (File.Exists(text4))
			{
				File.Delete(text4);
				Debug.Log("Delete " + text4);
			}
			if (File.Exists(text5))
			{
				File.Delete(text5);
				Debug.Log("Delete " + text5);
			}
			if (File.Exists(text6))
			{
				File.Delete(text6);
				Debug.Log("Delete " + text6);
			}
			if (File.Exists(text7))
			{
				File.Delete(text7);
				Debug.Log("Delete " + text7);
			}
			if (File.Exists(text9))
			{
				File.Delete(text9);
				Debug.Log("Delete " + text9);
			}
			if (File.Exists(text10))
			{
				File.Delete(text10);
				Debug.Log("Delete " + text10);
			}
			if (File.Exists(text11))
			{
				File.Delete(text11);
				Debug.Log("Delete " + text11);
			}
			if (File.Exists(text12))
			{
				File.Delete(text12);
				Debug.Log("Delete " + text12);
			}
			if (File.Exists(text13))
			{
				File.Delete(text13);
				Debug.Log("Delete " + text13);
			}
			_profiles.Remove(profileData);
			string[] files = Directory.GetFiles(text2);
			string[] directories = Directory.GetDirectories(text2);
			if (files.Length == 0 && directories.Length == 0)
			{
				Directory.Delete(text2);
			}
			else
			{
				Debug.LogWarning(" Directory not empty. Cannot delete. ");
			}
			if (Directory.Exists(text8))
			{
				files = Directory.GetFiles(text8);
				directories = Directory.GetDirectories(text8);
				if (files.Length == 0 && directories.Length == 0)
				{
					Directory.Delete(text8);
				}
				else
				{
					Debug.LogWarning("Backup Directory not empty. Cannot delete.");
				}
			}
			if (this.OnUpdatePlayerProfiles != null)
			{
				this.OnUpdatePlayerProfiles();
			}
		}
		catch (Exception ex)
		{
			stream?.Close();
			Debug.LogError("[" + ex.Message + "] Failed to delete all profile data");
			MarkBusyWithFileOps(isBusy: false);
		}
		MarkBusyWithFileOps(isBusy: false);
	}
}
