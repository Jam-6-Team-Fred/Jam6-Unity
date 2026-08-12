using System.Collections.Generic;
using UnityEngine;

public abstract class MultipleInteractionVolume : InteractVolume
{
	public class Interaction
	{
		public bool interactionEnabled;

		public IInputCommands inputCommand;

		public InputMode inputMode;

		public UITextType promptText;

		public bool displayPromptCommandIcon;

		public ScreenPrompt cachedScreenPrompt;

		public ScreenPrompt cachedNoIconScreenPrompt;

		public Interaction()
		{
			interactionEnabled = false;
			inputCommand = null;
			inputMode = InputMode.None;
			promptText = UITextType.None;
			displayPromptCommandIcon = true;
			cachedScreenPrompt = null;
		}

		public void BuildScreenPrompts()
		{
			if (promptText != 0)
			{
				cachedScreenPrompt = new ScreenPrompt(inputCommand, "<CMD> " + UITextLibrary.GetString(promptText));
				cachedNoIconScreenPrompt = new ScreenPrompt(UITextLibrary.GetString(promptText));
			}
			else
			{
				cachedScreenPrompt = new ScreenPrompt(inputCommand, "<CMD>");
				cachedNoIconScreenPrompt = new ScreenPrompt("");
			}
		}

		public void SetPromptText(UITextType promptID)
		{
			cachedScreenPrompt.SetText("<CMD> " + UITextLibrary.GetString(promptID));
			cachedNoIconScreenPrompt.SetText(UITextLibrary.GetString(promptID));
		}

		public void SetPromptText(UITextType actionPromptID, UITextType promptID)
		{
			cachedScreenPrompt.SetText("<CMD>" + UITextLibrary.GetString(actionPromptID) + "   " + UITextLibrary.GetString(promptID));
			cachedNoIconScreenPrompt.SetText(UITextLibrary.GetString(actionPromptID) + "   " + UITextLibrary.GetString(promptID));
		}

		public void SetPromptText(UITextType promptID, string _characterName)
		{
			cachedScreenPrompt.SetText("<CMD> " + UITextLibrary.GetString(promptID) + " " + _characterName);
			cachedNoIconScreenPrompt.SetText(UITextLibrary.GetString(promptID) + " " + _characterName);
		}
	}

	public delegate void PressInteractEvent(IInputCommands inputCommand);

	public delegate void ReleaseInteractEvent(IInputCommands inputCommand);

	public delegate void GainFocusEvent();

	public delegate void LoseFocusEvent();

	private bool _hasInteracted;

	private bool _isInteractPressed;

	private float _resetInteractionTime;

	private bool _wasFocused;

	private List<Interaction> _listInteractions;

	public event PressInteractEvent OnPressInteract;

	public event ReleaseInteractEvent OnReleaseInteract;

	public event GainFocusEvent OnGainFocus;

	public event LoseFocusEvent OnLoseFocus;

	protected virtual void Awake()
	{
		_listInteractions = new List<Interaction>();
	}

	public int AddInteraction(IInputCommands inputCommand, InputMode mode, UITextType promptText, bool startEnabled, bool displayCommandIcon)
	{
		Interaction interaction = new Interaction();
		interaction.inputCommand = inputCommand;
		interaction.inputMode = mode;
		interaction.promptText = promptText;
		interaction.interactionEnabled = startEnabled;
		interaction.displayPromptCommandIcon = displayCommandIcon;
		interaction.BuildScreenPrompts();
		_listInteractions.Add(interaction);
		return _listInteractions.Count - 1;
	}

	public virtual void RemoveInteraction(int index)
	{
		if (index < _listInteractions.Count)
		{
			_listInteractions[index] = null;
		}
	}

	public virtual Interaction GetInteractionAt(int index)
	{
		if (index < _listInteractions.Count)
		{
			return _listInteractions[index];
		}
		return null;
	}

	public void ChangePrompt(UITextType promptID, int interactionIndex)
	{
		_listInteractions[interactionIndex].SetPromptText(promptID);
		ResetInteraction();
	}

	public void SetPromptText(UITextType promptID, int interactionIndex)
	{
		_listInteractions[interactionIndex].SetPromptText(promptID);
	}

	public void SetPromptText(UITextType actionPromptID, UITextType promptID, int interactionIndex)
	{
		_listInteractions[interactionIndex].SetPromptText(actionPromptID, promptID);
	}

	public void SetPromptText(UITextType promptID, string characterName, int interactionIndex)
	{
		_listInteractions[interactionIndex].SetPromptText(promptID, characterName);
	}

	public virtual void SetKeyCommandVisible(bool value, int interactionIndex)
	{
		_listInteractions[interactionIndex].displayPromptCommandIcon = value;
		UpdatePromptVisibility();
	}

	public virtual void EnableSingleInteraction(bool value, int interactionIndex)
	{
		_listInteractions[interactionIndex].interactionEnabled = value;
		UpdatePromptVisibility();
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
		for (int i = 0; i < _listInteractions.Count; i++)
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_listInteractions[i].cachedScreenPrompt);
			Locator.GetPromptManager().RemoveScreenPrompt(_listInteractions[i].cachedNoIconScreenPrompt);
		}
		if (this.OnLoseFocus != null)
		{
			this.OnLoseFocus();
		}
	}

	protected override void GainFocus()
	{
		for (int i = 0; i < _listInteractions.Count; i++)
		{
			Locator.GetPromptManager().AddScreenPrompt(_listInteractions[i].cachedScreenPrompt, PromptPosition.Center);
			Locator.GetPromptManager().AddScreenPrompt(_listInteractions[i].cachedNoIconScreenPrompt, PromptPosition.Center);
		}
		if (this.OnGainFocus != null)
		{
			this.OnGainFocus();
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

	protected virtual void UpdateInput()
	{
		for (int i = 0; i < _listInteractions.Count; i++)
		{
			Interaction interaction = _listInteractions[i];
			if (interaction == null)
			{
				continue;
			}
			bool flag = OWInput.IsInputMode(interaction.inputMode) && interaction.interactionEnabled;
			if (!_hasInteracted && OWInput.IsNewlyPressed(interaction.inputCommand) && Time.time > _resetInteractionTime && flag)
			{
				_isInteractPressed = true;
				_hasInteracted = true;
				if (this.OnPressInteract != null)
				{
					this.OnPressInteract(interaction.inputCommand);
				}
			}
			if (_isInteractPressed && (OWInput.IsNewlyReleased(interaction.inputCommand) || !flag))
			{
				_isInteractPressed = false;
				if (this.OnReleaseInteract != null)
				{
					this.OnReleaseInteract(interaction.inputCommand);
				}
			}
		}
	}

	protected virtual void UpdatePromptVisibility()
	{
		for (int i = 0; i < _listInteractions.Count; i++)
		{
			Interaction interaction = _listInteractions[i];
			if (interaction != null)
			{
				bool flag = _focused && !_hasInteracted && _playerCam.enabled && OWInput.IsInputMode(interaction.inputMode) && interaction.interactionEnabled;
				ScreenPrompt screenPrompt;
				ScreenPrompt screenPrompt2;
				if (interaction.displayPromptCommandIcon)
				{
					screenPrompt = interaction.cachedScreenPrompt;
					screenPrompt2 = interaction.cachedNoIconScreenPrompt;
				}
				else
				{
					screenPrompt = interaction.cachedNoIconScreenPrompt;
					screenPrompt2 = interaction.cachedScreenPrompt;
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
	}
}
