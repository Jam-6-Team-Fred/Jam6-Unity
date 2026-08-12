using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class TitleButtonPrompt : MonoBehaviour
{
	public delegate void TitleButtonPromptCloseEvent();

	[SerializeField]
	private Graphic[] _buttonPromptGraphics;

	[SerializeField]
	private Image _buttonImage;

	[SerializeField]
	private Sprite _xboxOneButtonA;

	[SerializeField]
	private Sprite _playstationButtonCross;

	[SerializeField]
	private Sprite _playstationButtonCircle;

	[SerializeField]
	private Sprite _ps5ButtonCross;

	[SerializeField]
	private Animator _animation;

	[SerializeField]
	private TitleCodeInputManager _titleCodeInputManager;

	private bool _initialized;

	private bool _completed;

	private bool _listeningToInputs;

	public event TitleButtonPromptCloseEvent OnTitleButtonPromptClose;

	private void Awake()
	{
		if (!_initialized)
		{
			DisableButtonPrompt();
		}
	}

	public void OnUnpairedDeviceInput(InputControl control, InputEventPtr eventPtr)
	{
	}

	public bool ShouldEnable()
	{
		return false;
	}

	public bool IsCompleted()
	{
		return _completed;
	}

	public void AutoLoginPS5()
	{
		_buttonImage.sprite = _ps5ButtonCross;
		_initialized = true;
		_completed = false;
		base.enabled = true;
	}

	public void EnableButtonPrompt()
	{
		_initialized = true;
		_completed = false;
		_animation.SetTrigger("Entry");
		for (int i = 0; i < _buttonPromptGraphics.Length; i++)
		{
			_buttonPromptGraphics[i].enabled = true;
		}
		if (!_listeningToInputs)
		{
			OWInput.SharedInputManager.EnableListeningToUnpairedDevices(enable: true);
			OWInput.SharedInputManager.OnUnpairedDeviceInput += OnUnpairedDeviceInput;
			InputLibrary.menuConfirm.OnPerformed += OnMenuConfirmPerformed;
			_listeningToInputs = true;
		}
		base.enabled = true;
	}

	public void DisableButtonPrompt()
	{
		_animation.SetTrigger("Idle");
		base.enabled = false;
		for (int i = 0; i < _buttonPromptGraphics.Length; i++)
		{
			_buttonPromptGraphics[i].enabled = false;
		}
	}

	private void LateUpdate()
	{
	}

	private void UpdatePS4Input()
	{
	}

	private void UpdatePS5Input()
	{
	}

	private void OnMenuConfirmPerformed()
	{
		InputLibrary.menuConfirm.GetActiveDevice();
	}

	public void OnPS4DataLoaded(bool success)
	{
	}

	public void OnPS5DataLoaded(bool success)
	{
	}

	public void OnPS4DataSaved(bool success)
	{
	}

	public void OnPS5DataSaved(bool success)
	{
	}

	public void OnSaveDataSearchResult(bool foundSaveData)
	{
	}

	public void OnPS5PS4SaveDataSearchResult(bool foundSaveData)
	{
	}

	public void SetCompleted()
	{
		Debug.Log("TitleButtonPrmpt.SetCompleted");
		_initialized = false;
		_completed = true;
		if (this.OnTitleButtonPromptClose != null)
		{
			this.OnTitleButtonPromptClose();
		}
	}

	public void XB4_StopListeningToInputsAndSetCompleted()
	{
		if (_listeningToInputs)
		{
			OWInput.SharedInputManager.EnableListeningToUnpairedDevices(enable: false);
			OWInput.SharedInputManager.OnUnpairedDeviceInput -= OnUnpairedDeviceInput;
			InputLibrary.menuConfirm.OnPerformed -= OnMenuConfirmPerformed;
			_listeningToInputs = false;
			SetCompleted();
		}
	}
}
