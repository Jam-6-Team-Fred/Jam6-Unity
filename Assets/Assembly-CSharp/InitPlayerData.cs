using UnityEngine;

public class InitPlayerData : MonoBehaviour
{
	[SerializeField]
	private bool _unityEditorOnly = true;

	[SerializeField]
	private bool _createNewDebugSaveFile;

	private void Awake()
	{
		if (LoadManager.GetPreviousScene() == OWScene.None && !_unityEditorOnly)
		{
			StandaloneProfileManager.SharedInstance.InitializeForEditor();
			if (_createNewDebugSaveFile)
			{
				PlayerData.Init(new GameSave(), StandaloneProfileManager.SharedInstance.currentProfileGameSettings, StandaloneProfileManager.SharedInstance.currentProfileGraphicsSettings, StandaloneProfileManager.SharedInstance.currentProfileInputJSON);
				PlayerData.SaveCurrentGame();
			}
		}
	}
}
