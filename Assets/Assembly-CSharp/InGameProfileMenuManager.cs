using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameProfileMenuManager : MonoBehaviour, IPermanentManagerWorker
{
	private bool _initialized;

	private bool _refreshControllerStatesNextFrame;

	private IProfileManager _profileManager;

	[SerializeField]
	private Menu _onDisconnectPauseMenu;

	[SerializeField]
	private Text _messageText;

	[SerializeField]
	private SubmitActionCloseMenu _returnToGameSubmitAction;

	[SerializeField]
	private SubmitActionLoadScene _returnToTitleSubmitAction;

	[SerializeField]
	private JoystickListener _returnToGameJoystickListener;

	[SerializeField]
	private Button _returnToGameButton;

	[SerializeField]
	private Button _signInButton;

	[SerializeField]
	private Button _exitToMainMenu;

	private bool _resumeOnNextUpdate;

	private bool _suspendOnNextUpdate;

	private bool _checkStateOnNextUpdate;

	private Selectable _selectOnNextUpdate;

	private bool _sceneLoadStarted;

	private bool _hasPaused;

	private bool _addedPauseLock;

	[Conditional("VERBOSE_LOG")]
	public static void VerboseLog(object message)
	{
		UnityEngine.Debug.Log(message);
	}

	public void InitializeOnAwake()
	{
		if (!_initialized)
		{
			TextTranslation.Get().OnLanguageChanged += UpdateLanguage;
			UpdateLanguage();
			_profileManager = StandaloneProfileManager.SharedInstance;
			_profileManager.OnProfileSignInComplete += OnProfileSignInComplete;
			_profileManager.OnProfileSignOutComplete += OnProfileSignOutComplete;
			_profileManager.OnProfileReadDone += OnProfileReadDone;
			_returnToGameSubmitAction.OnSubmitAction += OnResumeGameBtnSubmit;
			_returnToTitleSubmitAction.OnSubmitAction += OnTitleSubmitAction;
			LoadManager.OnStartSceneLoad += OnStartSceneLoad;
			LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
			GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
			_initialized = true;
		}
	}

	protected virtual void OnDestroy()
	{
		TextTranslation.Get().OnLanguageChanged -= UpdateLanguage;
		LoadManager.OnStartSceneLoad -= OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad -= OnCompleteSceneLoad;
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
	}

	protected virtual void UpdateLanguage()
	{
	}

	private void OnPlayerResurrection()
	{
		_checkStateOnNextUpdate = true;
	}

	protected virtual void OnCompleteSceneLoad(OWScene originalScene, OWScene loadScene)
	{
		_sceneLoadStarted = false;
		_checkStateOnNextUpdate = true;
	}

	protected virtual void CheckStateOnSceneLoad()
	{
	}

	protected virtual void OnStartSceneLoad(OWScene originalScene, OWScene loadScene)
	{
		_sceneLoadStarted = true;
	}

	private void OnProfileSignInComplete(ProfileManagerSignInResult result)
	{
		OWScene currentScene = LoadManager.GetCurrentScene();
		if ((currentScene == OWScene.SolarSystem || currentScene == OWScene.EyeOfTheUniverse) && result == ProfileManagerSignInResult.COMPLETE)
		{
			RefreshMenuOptions();
		}
	}

	private void OnProfileReadDone()
	{
		OWScene currentScene = LoadManager.GetCurrentScene();
		if (currentScene != OWScene.SolarSystem)
		{
			_ = 3;
		}
	}

	private void OnProfileSignOutComplete()
	{
		if (_sceneLoadStarted)
		{
			return;
		}
		OWScene currentScene = LoadManager.GetCurrentScene();
		if (currentScene == OWScene.SolarSystem || currentScene == OWScene.EyeOfTheUniverse)
		{
			RefreshMenuOptions();
			if (!_hasPaused && IsGameInPausableState())
			{
				_suspendOnNextUpdate = true;
			}
		}
	}

	public void ForceControllerDisconnectEvent()
	{
		OnControllerDisconnected();
	}

	private void OnControllerDisconnected()
	{
		UnityEngine.Debug.Log("InGameProfileMenuManager.OnControllerDisconnected");
		if (!_sceneLoadStarted && IsGameInPausableState())
		{
			_suspendOnNextUpdate = true;
			RefreshMenuOptions();
		}
	}

	private void OnControllerReconnected()
	{
	}

	private void OnDevicesChanged(InputDevice device, InputDeviceChange change)
	{
		if (_hasPaused)
		{
			_refreshControllerStatesNextFrame = true;
		}
	}

	public void OnSystemSuspend()
	{
		if (!_sceneLoadStarted)
		{
			OWScene currentScene = LoadManager.GetCurrentScene();
			if (currentScene == OWScene.SolarSystem || currentScene == OWScene.EyeOfTheUniverse)
			{
				PauseGame();
			}
		}
	}

	public void OnSystemResume()
	{
		OWScene currentScene = LoadManager.GetCurrentScene();
		if (currentScene == OWScene.SolarSystem || currentScene == OWScene.EyeOfTheUniverse)
		{
			RefreshMenuOptions();
		}
		if (_hasPaused)
		{
			_refreshControllerStatesNextFrame = true;
		}
	}

	private void PauseGame()
	{
		if (!_hasPaused)
		{
			UnityEngine.Debug.Log("InGameProfileMenuManager.PauseGame");
			OWTime.Pause(OWTime.PauseType.System);
			OWInput.ChangeInputMode(InputMode.Menu);
			PauseCommandListener pauseCommandListener = Locator.GetPauseCommandListener();
			if (pauseCommandListener != null)
			{
				pauseCommandListener.AddPauseCommandLock();
				_addedPauseLock = true;
			}
			_onDisconnectPauseMenu.EnableMenu(value: true);
			_returnToGameButton.gameObject.SetActive(value: false);
			_hasPaused = true;
		}
	}

	private void ResumeGame()
	{
		UnityEngine.Debug.Log("InGameProfileMenuManager.ResumeGame");
		PauseCommandListener pauseCommandListener = Locator.GetPauseCommandListener();
		if (pauseCommandListener != null && _addedPauseLock)
		{
			pauseCommandListener.RemovePauseCommandLock();
			_addedPauseLock = false;
		}
		OWTime.Unpause(OWTime.PauseType.System);
		OWInput.RestorePreviousInputs();
		_hasPaused = false;
	}

	private bool HasGameControl()
	{
		return true;
	}

	private void SetNotificationMessage()
	{
		_messageText.text = "";
	}

	private bool IsGameInPausableState()
	{
		OWScene currentScene = LoadManager.GetCurrentScene();
		if (currentScene == OWScene.SolarSystem || currentScene == OWScene.EyeOfTheUniverse)
		{
			if (!Locator.GetDeathManager().IsPlayerDead())
			{
				return !Locator.GetDeathManager().IsPlayerDying();
			}
			return false;
		}
		return false;
	}

	private void RefreshMenuOptions()
	{
		SetNotificationMessage();
		bool flag = true;
		if (!IsGameInPausableState())
		{
			return;
		}
		Selectable selectable = null;
		_returnToGameButton.gameObject.SetActive(flag);
		if (flag)
		{
			_returnToGameSubmitAction.EnableMenuClose();
		}
		else
		{
			_returnToGameSubmitAction.DisableMenuClose();
		}
		if (flag)
		{
			SelectableAudioPlayer component = _returnToGameButton.GetComponent<SelectableAudioPlayer>();
			if (component != null)
			{
				component.SilenceNextSelectEvent();
			}
			selectable = _returnToGameButton.GetRequiredComponent<Button>();
		}
		if (selectable == null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		else
		{
			_selectOnNextUpdate = selectable;
		}
	}

	private void OnResumeGameBtnSubmit()
	{
		_resumeOnNextUpdate = true;
	}

	private void OnTitleSubmitAction()
	{
		EventSystem.current.SetSelectedGameObject(null);
		UIStyleApplier component = _returnToTitleSubmitAction.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, force: true);
		}
		_onDisconnectPauseMenu.EnableMenu(value: false);
		_resumeOnNextUpdate = true;
	}

	private void LateUpdate()
	{
		if (_checkStateOnNextUpdate)
		{
			CheckStateOnSceneLoad();
			_checkStateOnNextUpdate = false;
		}
		if (_refreshControllerStatesNextFrame)
		{
			_refreshControllerStatesNextFrame = false;
		}
		if (_suspendOnNextUpdate)
		{
			PauseGame();
			_suspendOnNextUpdate = false;
		}
		if (_resumeOnNextUpdate)
		{
			_resumeOnNextUpdate = false;
			ResumeGame();
		}
		if (_selectOnNextUpdate != null)
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnNextUpdate);
			_selectOnNextUpdate = null;
		}
	}
}
