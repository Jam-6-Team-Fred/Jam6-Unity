using UnityEngine;

public class CursorManager : MonoBehaviour, IPermanentManagerWorker
{
	private bool _isPaused;

	private bool _hasFocus;

	public void InitializeOnAwake()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		RefreshCursorState();
	}

	private void RefreshCursorState()
	{
		bool flag = false;
		CursorLockMode cursorLockMode = CursorLockMode.Locked;
		if (_isPaused || !_hasFocus || (OWInput.IsInputMode(InputMode.Menu | InputMode.Rebinding | InputMode.KeyboardInput) && !OWInput.IsChangePending() && !OWInput.UsingGamepad()))
		{
			flag = true;
			cursorLockMode = CursorLockMode.None;
		}
		if (Cursor.visible != flag)
		{
			Cursor.visible = flag;
		}
		if (Cursor.lockState != cursorLockMode)
		{
			Cursor.lockState = cursorLockMode;
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		_isPaused = pauseStatus;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		_hasFocus = hasFocus;
	}
}
