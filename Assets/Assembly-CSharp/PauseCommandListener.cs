using UnityEngine;

public class PauseCommandListener : MonoBehaviour
{
	private PauseMenuManager _pauseMenu;

	private int _pauseCommandLockCount;

	private void Awake()
	{
		_pauseMenu = null;
		base.enabled = false;
		_pauseCommandLockCount = 1;
	}

	public void RemovePauseCommandLock()
	{
		_pauseCommandLockCount--;
		if (_pauseCommandLockCount == 0)
		{
			base.enabled = true;
		}
		if (_pauseCommandLockCount < 0)
		{
			Debug.LogError("Trying to remove PauseCommandLock that does not exist!");
		}
	}

	public void AddPauseCommandLock()
	{
		if (_pauseCommandLockCount == 0)
		{
			base.enabled = false;
		}
		_pauseCommandLockCount++;
	}

	private void Update()
	{
		if (!OWInput.IsNewlyPressed(InputLibrary.pause))
		{
			return;
		}
		if (_pauseMenu == null)
		{
			_pauseMenu = Locator.GetSceneMenuManager().pauseMenu;
			if (_pauseMenu == null)
			{
				Debug.LogError("Could not find PauseMenu");
				return;
			}
		}
		if (!_pauseMenu.IsOpen())
		{
			_pauseMenu.TryOpenPauseMenu();
		}
	}
}
