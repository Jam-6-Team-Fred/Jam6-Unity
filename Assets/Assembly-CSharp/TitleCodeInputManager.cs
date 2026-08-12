using System;
using System.Collections.Generic;
using UnityEngine;

public class TitleCodeInputManager : MonoBehaviour
{
	private struct InputSequenceItem
	{
		public IInputCommands inputCommand;

		public bool pressed;
	}

	[Flags]
	public enum CommandSequenceIds
	{
		NONE = 0,
		KON_CODE = 1,
		INPUT_RESET = 2
	}

	private InputSequenceItem[] _konCodeSequence;

	private InputSequenceItem[] _inputResetSequence;

	private int _konCodeInputIndex;

	private int _debugInputModeIndex;

	private static List<IInputCommands> s_standardGamepadCommands;

	private bool _requestResetInputsToDefaults;

	private bool _gotValidJoystickInput;

	public bool ResetInputsToDefaults
	{
		get
		{
			return _requestResetInputsToDefaults;
		}
		set
		{
			_requestResetInputsToDefaults = value;
		}
	}

	private void Awake()
	{
		OnInputCommandsInitialized();
		((InputManager)OWInput.SharedInputManager).commandManager.OnInputCommandsInitialized += OnInputCommandsInitialized;
	}

	private void OnDestroy()
	{
		((InputManager)OWInput.SharedInputManager).commandManager.OnInputCommandsInitialized -= OnInputCommandsInitialized;
	}

	private void OnInputCommandsInitialized()
	{
		InitInputSequences();
		s_standardGamepadCommands = new List<IInputCommands>
		{
			InputLibrary.up2,
			InputLibrary.down2,
			InputLibrary.left2,
			InputLibrary.right2,
			InputLibrary.select,
			InputLibrary.pause,
			InputLibrary.faceUp,
			InputLibrary.faceDown,
			InputLibrary.faceLeft,
			InputLibrary.faceRight,
			InputLibrary.tabL,
			InputLibrary.tabR
		};
	}

	private void InitInputSequences()
	{
		InputSequenceItem inputSequenceItem = default(InputSequenceItem);
		inputSequenceItem.inputCommand = InputLibrary.up2;
		inputSequenceItem.pressed = false;
		InputSequenceItem inputSequenceItem2 = default(InputSequenceItem);
		inputSequenceItem2.inputCommand = InputLibrary.down2;
		inputSequenceItem2.pressed = false;
		InputSequenceItem inputSequenceItem3 = default(InputSequenceItem);
		inputSequenceItem3.inputCommand = InputLibrary.left2;
		inputSequenceItem3.pressed = false;
		InputSequenceItem inputSequenceItem4 = default(InputSequenceItem);
		inputSequenceItem4.inputCommand = InputLibrary.right2;
		inputSequenceItem4.pressed = false;
		InputSequenceItem inputSequenceItem5 = default(InputSequenceItem);
		InputSequenceItem inputSequenceItem6 = default(InputSequenceItem);
		inputSequenceItem5.inputCommand = InputLibrary.faceDown;
		inputSequenceItem6.inputCommand = InputLibrary.faceRight;
		inputSequenceItem5.pressed = false;
		inputSequenceItem6.pressed = false;
		InputSequenceItem inputSequenceItem7 = default(InputSequenceItem);
		inputSequenceItem7.inputCommand = InputLibrary.pause;
		inputSequenceItem7.pressed = false;
		_konCodeSequence = new InputSequenceItem[11];
		_konCodeSequence[0] = (_konCodeSequence[1] = inputSequenceItem);
		_konCodeSequence[2] = (_konCodeSequence[3] = inputSequenceItem2);
		_konCodeSequence[4] = (_konCodeSequence[6] = inputSequenceItem3);
		_konCodeSequence[5] = (_konCodeSequence[7] = inputSequenceItem4);
		_konCodeSequence[8] = inputSequenceItem5;
		_konCodeSequence[9] = inputSequenceItem6;
		_konCodeSequence[10] = inputSequenceItem7;
		_inputResetSequence = new InputSequenceItem[9];
		_inputResetSequence[0] = (_inputResetSequence[1] = inputSequenceItem);
		_inputResetSequence[2] = (_inputResetSequence[3] = inputSequenceItem2);
		_inputResetSequence[4] = (_inputResetSequence[5] = inputSequenceItem3);
		_inputResetSequence[6] = (_inputResetSequence[7] = inputSequenceItem4);
		_inputResetSequence[8] = inputSequenceItem7;
	}

	private void Update()
	{
		if (AnyJoystickButtonNewlyPressed())
		{
			CommandSequenceIds commandSequenceIds = CommandSequenceIds.NONE;
			if (_konCodeSequence[_konCodeInputIndex].inputCommand.IsNewlyPressed())
			{
				commandSequenceIds |= CommandSequenceIds.KON_CODE;
			}
			if (_inputResetSequence[_debugInputModeIndex].inputCommand.IsNewlyPressed())
			{
				commandSequenceIds |= CommandSequenceIds.INPUT_RESET;
			}
			if ((CommandSequenceIds.INPUT_RESET & commandSequenceIds) != 0)
			{
				_debugInputModeIndex++;
				if (_debugInputModeIndex >= _inputResetSequence.Length)
				{
					Debug.Log("GotResetInputsCode");
					_requestResetInputsToDefaults = true;
					_debugInputModeIndex = 0;
				}
			}
			if ((CommandSequenceIds.KON_CODE & commandSequenceIds) != 0)
			{
				_konCodeInputIndex++;
				if (_konCodeInputIndex >= _konCodeSequence.Length)
				{
					_konCodeInputIndex = 0;
				}
			}
			_gotValidJoystickInput = commandSequenceIds != CommandSequenceIds.NONE;
			if (_gotValidJoystickInput)
			{
				if ((CommandSequenceIds.INPUT_RESET & commandSequenceIds) == 0 && _debugInputModeIndex != 0)
				{
					_debugInputModeIndex = 0;
				}
				if ((CommandSequenceIds.KON_CODE & commandSequenceIds) == 0 && _konCodeInputIndex != 0)
				{
					_konCodeInputIndex = 0;
				}
			}
		}
		if (_gotValidJoystickInput && AnyJoystickButtonNewlyReleased())
		{
			_gotValidJoystickInput = false;
		}
	}

	public bool CodeInputInProgress()
	{
		if (_debugInputModeIndex == 0)
		{
			return _konCodeInputIndex != 0;
		}
		return true;
	}

	private bool AnyJoystickButtonNewlyPressed()
	{
		for (int i = 0; i < s_standardGamepadCommands.Count; i++)
		{
			if (s_standardGamepadCommands[i] == null)
			{
				return false;
			}
			if (s_standardGamepadCommands[i].IsNewlyPressed())
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyJoystickButtonNewlyReleased()
	{
		for (int i = 0; i < s_standardGamepadCommands.Count; i++)
		{
			if (s_standardGamepadCommands[i] == null)
			{
				return false;
			}
			if (s_standardGamepadCommands[i].IsNewlyReleased())
			{
				return true;
			}
		}
		return false;
	}
}
