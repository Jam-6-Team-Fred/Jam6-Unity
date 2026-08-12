using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
	[SerializeField]
	private Menu _pauseMenu;

	[SerializeField]
	private GameObject _skipToNextLoopButton;

	[SerializeField]
	private SubmitAction _endCurrentLoopAction;

	[SerializeField]
	private SubmitAction _exitToMainMenuAction;

	private bool _isOpen;

	private bool _waitingToApplySkipLoopStyle;

	private bool _isPaused;

	private void Start()
	{
		_pauseMenu.OnActivateMenu += OnActivateMenu;
		_exitToMainMenuAction.OnSubmitAction += OnExitToMainMenu;
		_endCurrentLoopAction.OnSubmitAction += OnSkipToNextTimeLoop;
		_skipToNextLoopButton.SetActive(PlayerData.GetPersistentCondition("KNOWS_MEDITATION") && TimeLoop.IsTimeLoopEnabled());
		Locator.GetSceneMenuManager().RegisterPauseMenu(this);
		Locator.GetPauseCommandListener().RemovePauseCommandLock();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_isPaused)
		{
			_isPaused = false;
			OWTime.Unpause(OWTime.PauseType.Menu);
		}
	}

	private void Update()
	{
		if (_waitingToApplySkipLoopStyle)
		{
			bool flag = PlayerState.IsSleepingAtCampfire() || PlayerState.IsGrabbedByGhost();
			_skipToNextLoopButton.GetComponent<UIStyleApplier>().ChangeState(flag ? UIElementState.DISABLED : UIElementState.NORMAL);
			_waitingToApplySkipLoopStyle = false;
		}
		if (OWInput.IsNewlyPressed(InputLibrary.pause) && _isOpen && MenuStackManager.SharedInstance.GetMenuCount() == 1)
		{
			_pauseMenu.EnableMenu(value: false);
		}
	}

	public bool IsOpen()
	{
		return _isOpen;
	}

	public bool TryOpenPauseMenu()
	{
		bool flag = true;
		if (OWInput.GetInputMode() == InputMode.Dialogue || OWInput.GetInputMode() == InputMode.ShipComputer)
		{
			flag = false;
		}
		if (LoadManager.IsBusy())
		{
			flag = false;
		}
		if (flag && _pauseMenu != null)
		{
			Locator.GetPauseCommandListener().AddPauseCommandLock();
			_pauseMenu.OnDeactivateMenu += OnDeactivatePauseMenu;
			OWTime.Pause(OWTime.PauseType.Menu);
			_isPaused = true;
			OWInput.ChangeInputMode(InputMode.Menu);
			_pauseMenu.EnableMenu(value: true);
			_isOpen = true;
			base.enabled = true;
		}
		return flag;
	}

	private void OnActivateMenu()
	{
		if (_skipToNextLoopButton.activeSelf)
		{
			bool flag = !PlayerState.IsSleepingAtCampfire() && !PlayerState.IsGrabbedByGhost();
			_endCurrentLoopAction.enabled = flag;
			_skipToNextLoopButton.GetComponent<Selectable>().interactable = flag;
			_skipToNextLoopButton.GetComponent<UIStyleApplier>().SetAutoInputStateChangesEnabled(flag);
			_waitingToApplySkipLoopStyle = true;
		}
	}

	private void OnDeactivatePauseMenu()
	{
		if (MenuStackManager.SharedInstance.GetMenuCount() == 0)
		{
			Locator.GetPauseCommandListener().RemovePauseCommandLock();
			_pauseMenu.OnDeactivateMenu -= OnDeactivatePauseMenu;
			OWTime.Unpause(OWTime.PauseType.Menu);
			_isPaused = false;
			OWInput.RestorePreviousInputs();
			_isOpen = false;
			base.enabled = false;
		}
	}

	private void OnSkipToNextTimeLoop()
	{
		_pauseMenu.EnableMenu(value: false);
		OWTime.Unpause(OWTime.PauseType.Menu);
		_isPaused = false;
		OWInput.RestorePreviousInputs();
	}

	private void OnExitToMainMenu()
	{
		_pauseMenu.EnableMenu(value: false);
		OWTime.Unpause(OWTime.PauseType.Menu);
		_isPaused = false;
		OWInput.RestorePreviousInputs();
	}
}
