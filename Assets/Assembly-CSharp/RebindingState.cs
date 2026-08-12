using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class RebindingState
{
	private InputConsts.InputValueType _rebindableType;

	private RebindableAxisInputAction _rebindableActionSingle;

	private RebindableInputActionPair _rebindableActionPair;

	private InputAction _positiveAction;

	private InputAction _negativeAction;

	private InputAction _positiveActionOrig;

	private InputAction _negativeActionOrig;

	private bool _firstBindingUsingGamepad;

	private bool _isXAxisRebindable;

	private string _controlScheme;

	private float _rebindingStartTime;

	private string[] _rebindableParts;

	private string[] _originalBindings;

	private SettingsMenuView _settingsView;

	private bool _validState;

	public Action<InputConsts.InputCommandType, string, string, int, bool> OnBindingChanged;

	public Action<string, string, int, bool> OnBindingApplied;

	public Action OnFinishedRebinding;

	public Action OnCancelledRebinding;

	private int _actionsRebound;

	private Dictionary<InputConsts.InputValueType, List<InputControl>> _inputCandidates = new Dictionary<InputConsts.InputValueType, List<InputControl>>
	{
		{
			InputConsts.InputValueType.BUTTON,
			new List<InputControl>()
		},
		{
			InputConsts.InputValueType.SINGLE_AXIS,
			new List<InputControl>()
		}
	};

	private Dictionary<InputControl, AxisSmoothingBuffer> _mouseAxisCandidates = new Dictionary<InputControl, AxisSmoothingBuffer>();

	private const float c_minActuationValue = 0.6f;

	private static List<string> _excludedPaths = new List<string>
	{
		"<Pointer>/position", "<Touchscreen>/touch*/position", "<Touchscreen>/touch*/delta", "<Mouse>/clickCount", "<Mouse>/press", "<Mouse>/pressure", "<Mouse>/delta/up", "<Mouse>/delta/down", "<Mouse>/delta/left", "<Mouse>/delta/right",
		"<Keyboard>/anyKey", "<Gamepad>/rightStick/x", "<Gamepad>/rightStick/y", "<Gamepad>/leftStick/x", "<Gamepad>/leftStick/y", "<Gamepad>/acceleration/x", "<Gamepad>/acceleration/y", "<Gamepad>/acceleration/z", "<Gamepad>/orientation/x", "<Gamepad>/orientation/y",
		"<Gamepad>/orientation/z", "<Gamepad>/orientation/w", "<Gamepad>/angularVelocity/x", "<Gamepad>/angularVelocity/y", "<Gamepad>/angularVelocity/z", "<Gamepad>/touch0/position/x", "<Gamepad>/touch0/position/y", "<Gamepad>/touch1/position/x", "<Gamepad>/touch1/position/y", "<Gamepad>/gyro/x",
		"<Gamepad>/gyro/y", "<Gamepad>/gyro/z", "<Gamepad>/accel/x", "<Gamepad>/accel/y", "<Gamepad>/accel/z", "<Gamepad>/batteryCharging", "<Gamepad>/batteryFullyCharged", "<Gamepad>/batteryLevel", "<Gamepad>/touchpadButton"
	};

	private const string _mouseAxisPathX = "<Mouse>/delta/x";

	private const string _mouseAxisPathY = "<Mouse>/delta/y";

	private const string _mouseLeftClickPath = "<Mouse>/leftButton";

	private static Dictionary<string, string> _inputPairs = new Dictionary<string, string>
	{
		{ "<Gamepad>/leftStick/up", "<Gamepad>/leftStick/down" },
		{ "<Gamepad>/leftStick/down", "<Gamepad>/leftStick/up" },
		{ "<Gamepad>/leftStick/left", "<Gamepad>/leftStick/right" },
		{ "<Gamepad>/leftStick/right", "<Gamepad>/leftStick/left" },
		{ "<Gamepad>/rightStick/up", "<Gamepad>/rightStick/down" },
		{ "<Gamepad>/rightStick/down", "<Gamepad>/rightStick/up" },
		{ "<Gamepad>/rightStick/left", "<Gamepad>/rightStick/right" },
		{ "<Gamepad>/rightStick/right", "<Gamepad>/rightStick/left" },
		{ "<Mouse>/delta/x", "<Mouse>/delta/x" },
		{ "<Mouse>/delta/y", "<Mouse>/delta/y" }
	};

	public bool IsValid => _validState;

	public RebindingState(IRebindableInputAction rebindableInputAction, SettingsMenuView settingsMenuView)
	{
		_settingsView = settingsMenuView;
		_rebindableActionSingle = rebindableInputAction as RebindableAxisInputAction;
		_rebindableActionPair = rebindableInputAction as RebindableInputActionPair;
		if (_rebindableActionSingle != null)
		{
			_positiveAction = _rebindableActionSingle.Action;
			_negativeAction = null;
			_positiveActionOrig = _positiveAction.Clone();
			_negativeActionOrig = null;
			_rebindableType = _rebindableActionSingle.ValueType;
		}
		else if (_rebindableActionPair != null)
		{
			_isXAxisRebindable = RebindableLookup.IsXAxisRebindable(_rebindableActionPair.RebindableID);
			_positiveAction = _rebindableActionPair.PrimaryAction;
			_negativeAction = _rebindableActionPair.SecondaryAction;
			_positiveActionOrig = _positiveAction.Clone();
			_negativeActionOrig = _negativeAction.Clone();
			_rebindableType = _rebindableActionPair.ValueType;
		}
		if (_rebindableActionSingle == null && _rebindableActionPair == null)
		{
			_validState = false;
			return;
		}
		rebindableInputAction.Enable(enable: false);
		_validState = true;
		_settingsView.RegisterRebindingState(this);
		_rebindingStartTime = Time.realtimeSinceStartup;
		InputSystem.onAfterUpdate += OnInputEvent;
	}

	~RebindingState()
	{
		if (_validState)
		{
			InputSystem.onAfterUpdate -= OnInputEvent;
		}
	}

	public IInputAction GetCurrentlyBindingAction()
	{
		if (_rebindableActionSingle != null)
		{
			return _rebindableActionSingle;
		}
		if (_rebindableActionPair != null)
		{
			return _rebindableActionPair;
		}
		return null;
	}

	private InputConsts.InputValueType GetInputValueType(InputControl control)
	{
		if (control is ButtonControl)
		{
			return InputConsts.InputValueType.BUTTON;
		}
		if (control.layout == "Axis")
		{
			return InputConsts.InputValueType.SINGLE_AXIS;
		}
		if (control.layout == "Vector2")
		{
			return InputConsts.InputValueType.DOUBLE_AXIS;
		}
		return InputConsts.InputValueType.NONE;
	}

	public void CancelRebinding()
	{
		if (_rebindableActionSingle != null)
		{
			_rebindableActionSingle.UpdateFromAction(_positiveActionOrig);
			_rebindableActionSingle.Enable(enable: true);
		}
		else if (_rebindableActionPair != null)
		{
			_rebindableActionPair.UpdateFromAction(_positiveActionOrig, _negativeActionOrig);
			_rebindableActionPair.Enable(enable: true);
		}
		OnCancelledRebinding?.Invoke();
		InputSystem.onAfterUpdate -= OnInputEvent;
		_validState = false;
		_settingsView.UnregisterRebindingState(this);
		Debug.Log("Input Done, Rebinding No longer Valid");
	}

	private void ApplyBinding(InputDevice device, InputControl inputControl, bool usingGamepad)
	{
		ApplyBinding(device, inputControl.path, usingGamepad);
	}

	private void ApplyBinding(InputDevice device, string controlPath, bool usingGamepad)
	{
		string text = "";
		if (device is Gamepad)
		{
			text = "<Gamepad>";
		}
		else if (device is Mouse)
		{
			text = "<Mouse>";
		}
		else if (device is Keyboard)
		{
			text = "<Keyboard>";
		}
		InputAction inputAction;
		InputAction inputAction2;
		if (_actionsRebound == 0)
		{
			if (_isXAxisRebindable)
			{
				inputAction = _negativeAction;
				inputAction2 = _positiveAction;
			}
			else
			{
				inputAction = _positiveAction;
				inputAction2 = _negativeAction;
			}
		}
		else if (_isXAxisRebindable)
		{
			inputAction = _positiveAction;
			inputAction2 = _negativeAction;
		}
		else
		{
			inputAction = _negativeAction;
			inputAction2 = _positiveAction;
		}
		int num = -1;
		for (int i = 0; i < inputAction.bindings.Count; i++)
		{
			if (inputAction.bindings[i].path.StartsWith("<Gamepad>") && usingGamepad)
			{
				num = i;
				break;
			}
			if ((inputAction.bindings[i].path.StartsWith("<Mouse>") || inputAction.bindings[i].path.StartsWith("<Keyboard>")) && !usingGamepad)
			{
				num = i;
				break;
			}
		}
		int startIndex = controlPath.IndexOf('/', 1);
		string text2 = text + controlPath.Substring(startIndex);
		string effectivePath = inputAction.bindings[num].effectivePath;
		inputAction.ChangeBinding(num).WithPath(text2);
		if (_rebindableActionSingle != null)
		{
			RebindingUtil.ResolveConflicts(ref _rebindableActionSingle, effectivePath, text2, num, usingGamepad);
		}
		_actionsRebound++;
		bool flag = false;
		if (inputAction2 == null && _actionsRebound == 1)
		{
			flag = true;
			_rebindableActionSingle.SetAction(inputAction);
		}
		else if (inputAction2 != null && _actionsRebound == 2)
		{
			flag = true;
			_rebindableActionPair.SetAction(_positiveAction, _negativeAction);
		}
		OnBindingApplied?.Invoke(effectivePath, text2, num, usingGamepad);
		if (flag)
		{
			OnFinishedRebinding?.Invoke();
			InputSystem.onAfterUpdate -= OnInputEvent;
			_validState = false;
			_settingsView.UnregisterRebindingState(this);
			Debug.Log("Input Done, Rebinding No longer Valid");
		}
		else if (inputAction2 != null && _inputPairs.ContainsKey(text2))
		{
			ApplyBinding(device, _inputPairs[text2], usingGamepad);
		}
		else
		{
			_rebindingStartTime = Time.realtimeSinceStartup;
			_firstBindingUsingGamepad = usingGamepad;
		}
	}

	public void ApplyGamepadBindingNoInputChecks(InputControl control)
	{
		for (int i = 0; i < InputSystem.devices.Count; i++)
		{
			InputDevice inputDevice = InputSystem.devices[i];
			if (inputDevice is Gamepad)
			{
				while (_validState)
				{
					ApplyBinding(inputDevice, control, usingGamepad: true);
				}
			}
		}
	}

	private void OnInputEvent()
	{
		if (!_validState || Time.realtimeSinceStartup - _rebindingStartTime < 0.2f)
		{
			return;
		}
		Mouse mouse = null;
		for (int i = 0; i < InputSystem.devices.Count; i++)
		{
			InputDevice inputDevice = InputSystem.devices[i];
			if (inputDevice is Mouse)
			{
				mouse = inputDevice as Mouse;
			}
			InputControl inputControl = null;
			InputControl inputControl2 = null;
			for (int j = 0; j < inputDevice.allControls.Count; j++)
			{
				InputControl inputControl3 = inputDevice.allControls[j];
				bool flag = false;
				for (int k = 0; k < _excludedPaths.Count; k++)
				{
					if (InputControlPath.MatchesPrefix(_excludedPaths[k], inputControl3))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				if (InputControlPath.MatchesPrefix("<Mouse>/delta/x", inputControl3))
				{
					inputControl = inputControl3;
					continue;
				}
				if (InputControlPath.MatchesPrefix("<Mouse>/delta/y", inputControl3))
				{
					inputControl2 = inputControl3;
					continue;
				}
				bool flag2 = false;
				InputConsts.InputValueType inputValueType = GetInputValueType(inputControl3);
				switch (inputValueType)
				{
				case InputConsts.InputValueType.BUTTON:
					flag2 = inputControl3 is ButtonControl buttonControl && buttonControl.wasReleasedThisFrame;
					if (flag2)
					{
						if (_rebindableType == InputConsts.InputValueType.BUTTON || _actionsRebound == 1)
						{
							flag2 = !(inputControl3.parent is StickControl);
						}
						if (flag2)
						{
							Debug.Log("Add button control: " + inputControl3.displayName);
						}
					}
					if (InputControlPath.MatchesPrefix("<Mouse>/leftButton", inputControl3) && _settingsView.IsPointerOverCancelButton())
					{
						flag2 = false;
					}
					break;
				case InputConsts.InputValueType.SINGLE_AXIS:
					flag2 = _rebindableType != InputConsts.InputValueType.BUTTON && _actionsRebound != 1 && inputControl3.IsActuated(0.6f);
					break;
				}
				bool flag3 = inputControl3.device is Gamepad;
				if (_actionsRebound == 1 && _negativeAction != null && flag3 != _firstBindingUsingGamepad)
				{
					flag2 = false;
				}
				if (flag2)
				{
					_inputCandidates[inputValueType].Add(inputControl3);
				}
			}
			if (mouse == null || inputControl == null || inputControl2 == null)
			{
				continue;
			}
			bool flag4 = true;
			InputConsts.InputValueType key = InputConsts.InputValueType.SINGLE_AXIS;
			if (_rebindableType == InputConsts.InputValueType.BUTTON || _actionsRebound == 1)
			{
				flag4 = false;
			}
			if (!flag4)
			{
				continue;
			}
			Vector2 vector = mouse.delta.ReadValue();
			float num = (float)PlayerData.GetGraphicSettings().displayResWidth * 0.1f;
			num *= num;
			if (vector.sqrMagnitude > num)
			{
				if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
				{
					_inputCandidates[key].Add(inputControl);
				}
				else
				{
					_inputCandidates[key].Add(inputControl2);
				}
			}
		}
	}

	public bool HasInputEvents()
	{
		if (_inputCandidates[InputConsts.InputValueType.BUTTON].Count <= 0)
		{
			return _inputCandidates[InputConsts.InputValueType.SINGLE_AXIS].Count > 0;
		}
		return true;
	}

	public void ProcessInputCandidates()
	{
		foreach (KeyValuePair<InputConsts.InputValueType, List<InputControl>> inputCandidate in _inputCandidates)
		{
			List<InputControl> value = inputCandidate.Value;
			if (value.Count > 0)
			{
				InputControl inputControl = value[0];
				bool usingGamepad = inputControl.device is Gamepad;
				ApplyBinding(inputControl.device, inputControl, usingGamepad);
				break;
			}
		}
		_inputCandidates[InputConsts.InputValueType.BUTTON].Clear();
		_inputCandidates[InputConsts.InputValueType.SINGLE_AXIS].Clear();
	}
}
