public interface IProfileManager
{
	bool isInitialized { get; }

	bool isBusyWithFileOps { get; }

	bool hasPendingSaveOperation { get; }

	GameSave currentProfileGameSave { get; }

	SettingsSave currentProfileGameSettings { get; }

	GraphicSettings currentProfileGraphicsSettings { get; }

	string currentProfileInputJSON { get; }

	event ProfileDataSavedEvent OnProfileDataSaved;

	event ProfileSignInStartEvent OnProfileSignInStart;

	event ProfileSignInCompleteEvent OnProfileSignInComplete;

	event ProfileReadDoneEvent OnProfileReadDone;

	event ProfileSignOutStartEvent OnProfileSignOutStart;

	event ProfileSignOutCompleteEvent OnProfileSignOutComplete;

	void PreInitialize();

	void Initialize();

	void InitializeForEditor();

	void PerformPendingSaveOperation();

	void SaveGame(GameSave gameSave, SettingsSave settSave, GraphicSettings gfxSettings, string jsonInputBindings);
}
