using UnityEngine;

public abstract class SingleInteractionVolume : InteractVolume
{
	public delegate void PressInteractEvent();

	public delegate void ReleaseInteractEvent();

	public delegate void GainFocusEvent();

	public delegate void LoseFocusEvent();

	[SerializeField]
	protected UITextType _textID;

	protected ScreenPrompt _screenPrompt;

	protected ScreenPrompt _noCommandIconPrompt;

	private bool _wasFocused;

	private bool _isInteractPressed;

	private bool _hasInteracted;

	private bool _usingPromptWithCommand = true;

	private float _resetInteractionTime;

	public event PressInteractEvent OnPressInteract;

	public event ReleaseInteractEvent OnReleaseInteract;

	public event GainFocusEvent OnGainFocus;

	public event LoseFocusEvent OnLoseFocus;

	protected virtual void Awake()
	{
		if (_textID != 0)
		{
			_screenPrompt = new ScreenPrompt(InputLibrary.interact, "<CMD> " + UITextLibrary.GetString(_textID));
			_noCommandIconPrompt = new ScreenPrompt(UITextLibrary.GetString(_textID));
		}
		else
		{
			_screenPrompt = new ScreenPrompt(InputLibrary.interact, "<CMD>");
			_noCommandIconPrompt = new ScreenPrompt("");
		}
	}

	public void ChangePrompt(string promptText)
	{
		_screenPrompt.SetText("<CMD> " + promptText);
		_noCommandIconPrompt.SetText(promptText);
		ResetInteraction();
	}

	public void ChangePrompt(UITextType promptID)
	{
		_screenPrompt.SetText("<CMD> " + UITextLibrary.GetString(promptID));
		_noCommandIconPrompt.SetText(UITextLibrary.GetString(promptID));
		ResetInteraction();
	}

	public void SetPromptText(UITextType promptID)
	{
		_screenPrompt.SetText("<CMD> " + UITextLibrary.GetString(promptID));
		_noCommandIconPrompt.SetText(UITextLibrary.GetString(promptID));
	}

	public void SetPromptText(UITextType actionPromptID, UITextType promptID)
	{
		_screenPrompt.SetText("<CMD>" + UITextLibrary.GetString(actionPromptID) + "   " + UITextLibrary.GetString(promptID));
		_noCommandIconPrompt.SetText(UITextLibrary.GetString(actionPromptID) + "   " + UITextLibrary.GetString(promptID));
	}

	public void SetPromptText(UITextType promptID, string _characterName)
	{
		_screenPrompt.SetText("<CMD> " + UITextLibrary.GetString(promptID) + " " + _characterName);
		_noCommandIconPrompt.SetText(UITextLibrary.GetString(promptID) + " " + _characterName);
	}

	public override void DisableInteraction()
	{
		LoseFocus();
		UpdatePromptVisibility();
	}

	public override void ResetInteraction()
	{
		_hasInteracted = false;
		_isInteractPressed = false;
		_resetInteractionTime = Time.time;
	}

	public override void LoseFocus()
	{
		_focused = (_wasFocused = false);
		ResetInteraction();
		UpdatePromptVisibility();
		Locator.GetPromptManager().RemoveScreenPrompt(_screenPrompt);
		Locator.GetPromptManager().RemoveScreenPrompt(_noCommandIconPrompt);
		if (this.OnLoseFocus != null)
		{
			this.OnLoseFocus();
		}
	}

	public override void UpdateInteractVolume()
	{
		if (_focused && !_wasFocused)
		{
			GainFocus();
		}
		else if (!_focused && _wasFocused)
		{
			LoseFocus();
		}
		if (_focused)
		{
			UpdateInput();
		}
		UpdatePromptVisibility();
		_wasFocused = _focused;
	}

	protected override void GainFocus()
	{
		Locator.GetPromptManager().AddScreenPrompt(_screenPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_noCommandIconPrompt, PromptPosition.Center);
		if (this.OnGainFocus != null)
		{
			this.OnGainFocus();
		}
	}

	protected virtual void UpdateInput()
	{
		bool flag = OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit);
		if (!_hasInteracted && OWInput.IsNewlyPressed(InputLibrary.interact) && Time.time > _resetInteractionTime && flag)
		{
			_isInteractPressed = true;
			_hasInteracted = true;
			if (this.OnPressInteract != null)
			{
				this.OnPressInteract();
			}
		}
		if (_isInteractPressed && (OWInput.IsNewlyReleased(InputLibrary.interact) || !flag))
		{
			_isInteractPressed = false;
			if (this.OnReleaseInteract != null)
			{
				this.OnReleaseInteract();
			}
		}
	}

	public virtual void SetKeyCommandVisible(bool value)
	{
		_usingPromptWithCommand = value;
		UpdatePromptVisibility();
	}

	protected virtual void UpdatePromptVisibility()
	{
		bool flag = _focused && !_hasInteracted && _playerCam.enabled && OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit);
		ScreenPrompt screenPrompt;
		ScreenPrompt screenPrompt2;
		if (_usingPromptWithCommand)
		{
			screenPrompt = _screenPrompt;
			screenPrompt2 = _noCommandIconPrompt;
		}
		else
		{
			screenPrompt = _noCommandIconPrompt;
			screenPrompt2 = _screenPrompt;
		}
		if (screenPrompt2.IsVisible())
		{
			screenPrompt2.SetVisibility(isVisible: false);
		}
		if (flag != screenPrompt.IsVisible())
		{
			screenPrompt.SetVisibility(flag);
		}
	}
}
