using System;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupInputMenu : PopupMenu
{
	public delegate bool InputPopupTextChangedEvent();

	public delegate bool InputPopupValidateCharEvent(char c);

	[Space(10f)]
	[SerializeField]
	private InputField _inputField;

	[SerializeField]
	private InputEventListener _inputFieldEventListener;

	protected Transform _caretTransform;

	protected bool _virtualKeyboardOpen;

	public event InputPopupTextChangedEvent OnInputPopupTextChanged;

	public event InputPopupValidateCharEvent OnInputPopupValidateChar;

	protected override void Awake()
	{
		base.Awake();
		_inputField.DeactivateInputField();
	}

	public override Selectable GetSelectOnActivate()
	{
		return _selectOnActivate;
	}

	public override void SetUpPopup(string message, IInputCommands okCommand, IInputCommands cancelCommand, ScreenPrompt okPrompt, ScreenPrompt cancelPrompt, bool closeMenuOnOk = true, bool setCancelButtonActive = true)
	{
		base.SetUpPopup(message, okCommand, cancelCommand, okPrompt, cancelPrompt, closeMenuOnOk, setCancelButtonActive);
		_selectOnActivate = _inputField;
	}

	public override void Activate()
	{
		base.Activate();
		ClearInputFieldText();
		_inputField.ActivateInputField();
		_inputFieldEventListener.OnSelectEvent += OnInputFieldSelect;
		_inputFieldEventListener.OnPointerUpEvent += OnPointerUpInInputField;
		SteamManager.Instance.OnGamepadTextInputDismissed += OnSteamVirtualKeyboardDismissed;
		if (SteamManager.Initialized)
		{
			SteamUserStats.RequestCurrentStats();
		}
		_inputField.onValueChanged.AddListener(delegate
		{
			OnTextFieldChanged();
		});
		InputField inputField = _inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(inputField.onValidateInput, new InputField.OnValidateInput(OnValidateInput));
		Locator.GetMenuInputModule().OnInputModuleSubmit += OnMenuInputModuleSubmit;
		Locator.GetMenuInputModule().OnInputModuleCancel += OnMenuInputModuleCancel;
		Transform transform = _inputField.transform.Find(_inputField.transform.name + " Input Caret");
		if (transform != null)
		{
			_caretTransform = transform;
			transform.SetAsLastSibling();
		}
	}

	protected void OnInputFieldSelect(BaseEventData eventData, Selectable selectable)
	{
		if (!(selectable != _inputField))
		{
			_virtualKeyboardOpen = TryOpenVirtualKeyboard();
		}
	}

	protected void OnPointerUpInInputField(PointerEventData eventData, Selectable selectable)
	{
		_virtualKeyboardOpen = TryOpenVirtualKeyboard();
	}

	protected bool TryOpenVirtualKeyboard()
	{
		_inputField.ActivateInputField();
		if (SteamUtils.IsSteamRunningOnSteamDeck())
		{
			return SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, 0, 0, 1280, 370);
		}
		return false;
	}

	protected void OnSteamVirtualKeyboardDismissed(bool bSubmitted, uint unSubmittedText)
	{
		_virtualKeyboardOpen = false;
	}

	protected void OnMenuInputModuleCancel(GameObject selectedObj, BaseEventData eventData)
	{
		InvokeCancel();
	}

	protected void OnMenuInputModuleSubmit(GameObject selectedObj, BaseEventData eventData)
	{
		InvokeOk();
	}

	protected override void InvokeOk()
	{
		base.InvokeOk();
		if (_enabledMenu && _inputField.text == "")
		{
			if (Locator.GetEventSystem().currentSelectedGameObject != _inputField.gameObject)
			{
				_inputField.Select();
			}
			else
			{
				_virtualKeyboardOpen = TryOpenVirtualKeyboard();
			}
		}
	}

	protected override void Update()
	{
		if (_caretTransform == null)
		{
			Transform transform = _inputField.transform.Find(_inputField.transform.name + " Input Caret");
			if (transform != null)
			{
				_caretTransform = transform;
				transform.SetAsLastSibling();
			}
		}
		if (!OWInput.IsNewlyPressed(InputLibrary.menuConfirm))
		{
			return;
		}
		if (Locator.GetEventSystem().currentSelectedGameObject == _inputField.gameObject)
		{
			_virtualKeyboardOpen = TryOpenVirtualKeyboard();
			return;
		}
		EventSystem eventSystem = Locator.GetEventSystem();
		if (eventSystem.currentSelectedGameObject != _confirmButton.gameObject && eventSystem.currentSelectedGameObject != _cancelButton.gameObject)
		{
			_inputField.Select();
		}
	}

	public override void Deactivate(bool keepPreviousMenuVisible = false)
	{
		base.Deactivate(keepPreviousMenuVisible);
		_inputField.DeactivateInputField();
		_inputField.onValueChanged.RemoveListener(delegate
		{
			OnTextFieldChanged();
		});
		InputField inputField = _inputField;
		inputField.onValidateInput = (InputField.OnValidateInput)Delegate.Remove(inputField.onValidateInput, new InputField.OnValidateInput(OnValidateInput));
		Locator.GetMenuInputModule().OnInputModuleSubmit -= OnMenuInputModuleSubmit;
		Locator.GetMenuInputModule().OnInputModuleCancel -= OnMenuInputModuleCancel;
		_inputFieldEventListener.OnSelectEvent -= OnInputFieldSelect;
		_inputFieldEventListener.OnPointerUpEvent -= OnPointerUpInInputField;
		SteamManager.Instance.OnGamepadTextInputDismissed -= OnSteamVirtualKeyboardDismissed;
		_virtualKeyboardOpen = false;
	}

	private void OnTextFieldChanged()
	{
		if (this.OnInputPopupTextChanged != null)
		{
			this.OnInputPopupTextChanged();
		}
	}

	private char OnValidateInput(string input, int charIndex, char addedChar)
	{
		bool flag = true;
		if (this.OnInputPopupValidateChar != null)
		{
			Delegate[] invocationList = this.OnInputPopupValidateChar.GetInvocationList();
			bool flag2 = true;
			for (int i = 0; i < invocationList.Length; i++)
			{
				flag2 = (bool)invocationList[i].DynamicInvoke(addedChar);
				flag = flag && flag2;
			}
		}
		if (flag)
		{
			return addedChar;
		}
		return '\0';
	}

	public void ClearInputFieldText()
	{
		_inputField.text = "";
	}

	public virtual string GetInputText()
	{
		return _inputField.text;
	}

	public virtual InputField GetInputField()
	{
		return _inputField;
	}

	public virtual void SetInputFieldPlaceholderText(string text)
	{
		Text component = _inputField.placeholder.GetComponent<Text>();
		if (component != null)
		{
			component.text = text;
		}
		else
		{
			Debug.LogWarning("Could not find InputField Placeholder Text Element");
		}
	}

	public override void SetUpPopupCommands(IInputCommands okCommand, IInputCommands cancelCommand, ScreenPrompt okPrompt, ScreenPrompt cancelPrompt)
	{
		_okCommand = okCommand;
		_cancelCommand = cancelCommand;
		_confirmButton.SetPrompt(okPrompt);
		_cancelButton.SetPrompt(cancelPrompt);
	}
}
