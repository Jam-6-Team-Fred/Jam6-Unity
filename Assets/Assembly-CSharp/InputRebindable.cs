using System;

[Serializable]
[Obsolete("Use InputCommands/InputAction instead", false)]
public class InputRebindable
{
	public enum InputType
	{
		UNDEFINED = 0,
		BUTTON = 1,
		BINARY_AXIS = 2,
		ANALOG_AXIS = 3,
		ANALOG_AXIS_ZERO_TO_ONE = 4
	}

	public enum AxisDirection
	{
		UNDEFINED = 0,
		HORIZONTAL = 1,
		VERTICAL = 2
	}

	public enum BindingCategory
	{
		UNDEFINED = 0,
		GAMEPAD = 1,
		KEYBD_MOUSE = 2
	}

	private string _name;

	private UITextType _uiTextType;

	private InputType _inputType;

	private AxisDirection _axisDirection;

	private InputBinding _primaryInputs;

	private InputBinding _secondaryInputs;

	private RebindableID _rebindableId;

	public InputRebindable(RebindableID id, InputType inputType, AxisDirection direction = AxisDirection.UNDEFINED)
	{
		_name = "";
		_uiTextType = UITextType.None;
		_inputType = inputType;
		_axisDirection = direction;
		_primaryInputs = null;
		_secondaryInputs = null;
		_rebindableId = id;
	}

	public InputRebindable(UITextType labelType, InputType inputType, AxisDirection direction = AxisDirection.UNDEFINED)
	{
		_name = "";
		_uiTextType = labelType;
		_inputType = inputType;
		_axisDirection = direction;
		_primaryInputs = null;
		_secondaryInputs = null;
	}

	public bool ValidateBinding(InputBinding testBinding)
	{
		return true;
	}

	public void SetBindings(InputBinding primaryBinding, InputBinding secondaryBinding)
	{
		_primaryInputs = primaryBinding;
		_secondaryInputs = secondaryBinding;
	}

	public void SetRebindableID(RebindableID id)
	{
		_rebindableId = id;
	}

	public RebindableID GetRebindableID()
	{
		return _rebindableId;
	}

	public void SetUITextType(UITextType textType)
	{
		_uiTextType = textType;
	}

	public UITextType GetUITextType()
	{
		return _uiTextType;
	}

	public string GetLabel()
	{
		if (_uiTextType == UITextType.None)
		{
			_uiTextType = RebindableLookup.SharedInstance.LookupRebindableMenuString(_rebindableId);
		}
		if (_uiTextType != 0 && TextTranslation.Get() != null)
		{
			return UITextLibrary.GetString(_uiTextType);
		}
		return _name;
	}

	public InputType GetInputType()
	{
		return _inputType;
	}

	public AxisDirection GetAxisDirection()
	{
		return _axisDirection;
	}

	public InputBinding GetGamepadBinding()
	{
		return _primaryInputs;
	}

	public InputBinding GetKeyboardMouseBinding()
	{
		return _secondaryInputs;
	}

	public void CopySettingsToRebindable(InputRebindable targetRebindable)
	{
		targetRebindable.SetBindings(_primaryInputs, _secondaryInputs);
	}

	public override bool Equals(object obj)
	{
		if (base.Equals(obj))
		{
			return true;
		}
		if (!(obj is InputRebindable inputRebindable))
		{
			return false;
		}
		if (_name == inputRebindable._name && _inputType == inputRebindable._inputType && _axisDirection == inputRebindable._axisDirection && _rebindableId == inputRebindable._rebindableId && _primaryInputs.Equals(inputRebindable._primaryInputs))
		{
			return _secondaryInputs.Equals(inputRebindable._secondaryInputs);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _name.GetHashCode() ^ (_inputType.GetHashCode() << 1) ^ (_axisDirection.GetHashCode() << 2) ^ (_rebindableId.GetHashCode() << 3) ^ (_primaryInputs.GetHashCode() << 4) ^ (_secondaryInputs.GetHashCode() << 5);
	}

	public InputRebindable Clone()
	{
		InputRebindable inputRebindable = new InputRebindable(_rebindableId, _inputType, _axisDirection);
		inputRebindable._name = _name;
		inputRebindable._uiTextType = _uiTextType;
		inputRebindable.SetBindings(_primaryInputs, _secondaryInputs);
		return inputRebindable;
	}
}
