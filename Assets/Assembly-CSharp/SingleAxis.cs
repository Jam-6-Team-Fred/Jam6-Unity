using UnityEngine;

public class SingleAxis : InputAxis
{
	private string _axisName;

	private string _axisNameWithJoystick;

	private KeyCode _keyCodePos;

	private KeyCode _keyCodeNeg;

	private float _value;

	private float _rawValue;

	private int _joystickNum;

	private bool _remapTriggerAxis;

	private bool _hasTriggerBeenPressed;

	private float[] _mouseBuffer;

	private int _bufferIndex;

	public SingleAxis(AxisIdentifier identifier, string pcAxis, JoystickButton macButtonPos, JoystickButton macButtonNeg)
		: this(identifier, pcAxis)
	{
		_axisID = identifier;
	}

	public SingleAxis(AxisIdentifier identifier, string pcAxis, string macAxis, float innerDeadZone, float outerDeadZone)
		: this(identifier, pcAxis, innerDeadZone, outerDeadZone)
	{
		_axisID = identifier;
	}

	public SingleAxis(AxisIdentifier identifier, string axisName, float innerDeadZone = 0f, float outerDeadZone = 0f)
	{
		_value = 0f;
		_rawValue = 0f;
		_axisID = identifier;
		_axisName = axisName;
		_axisNameWithJoystick = _axisName;
		_innerDeadZone = (_origInnerDeadZone = innerDeadZone);
		_outerDeadZone = (_origOuterDeadZone = outerDeadZone);
		_isMouse = identifier == AxisIdentifier.KEYBD_MOUSEX || identifier == AxisIdentifier.KEYBD_MOUSEY;
		_isMouseWheel = identifier == AxisIdentifier.KEYBD_MOUSEWHEEL;
		if (_isMouse)
		{
			_mouseBuffer = new float[10];
			_bufferIndex = 0;
		}
	}

	public void SetAxisJoystick(int joystickNum)
	{
		_joystickNum = joystickNum;
		_axisNameWithJoystick = GetInputAxisName(_joystickNum);
	}

	public int GetJoystickNumber()
	{
		return _joystickNum;
	}

	public string GetInputAxisName(int joystickNum)
	{
		if (joystickNum == 0)
		{
			return _axisName;
		}
		return _axisName + "_" + joystickNum;
	}

	public override void UpdateAxis()
	{
		bool useDeadZone = true;
		_value = CalculateValue(_keyCodePos, _keyCodeNeg, _axisNameWithJoystick, useDeadZone);
	}

	private float CalculateValue(KeyCode posKey, KeyCode negKey, string axisName, bool useDeadZone)
	{
		float num = 0f;
		float num2 = 0f;
		if (posKey != 0)
		{
			if (Input.GetKey(posKey))
			{
				num2 += 1f;
			}
			if (Input.GetKey(negKey))
			{
				num2 -= 1f;
			}
			return _rawValue = num2;
		}
		num2 = Input.GetAxis(axisName);
		if (_isMouse)
		{
			_mouseBuffer[_bufferIndex] = num2;
			float num3 = 0f;
			float num4 = 1f;
			float num5 = 0.5f;
			float num6 = 0f;
			int num7 = _bufferIndex;
			do
			{
				num3 += _mouseBuffer[num7] * num4;
				num6 += num4;
				num4 *= num5;
				num7--;
				if (num7 < 0)
				{
					num7 = _mouseBuffer.Length - 1;
				}
			}
			while (num7 != _bufferIndex);
			_bufferIndex++;
			if (_bufferIndex > _mouseBuffer.Length - 1)
			{
				_bufferIndex = 0;
			}
			num = (_rawValue = num3 / num6);
		}
		else
		{
			num = (_rawValue = (_remapTriggerAxis ? RemapTriggerValue(num2) : num2));
			if (useDeadZone && !_isMouseWheel)
			{
				float value = Mathf.Abs(num2);
				num = Mathf.Sign(num2) * Mathf.InverseLerp(_innerDeadZone, 1f - _outerDeadZone, value);
			}
		}
		return num;
	}

	public float GetValue()
	{
		return _value;
	}

	public float GetRawValue()
	{
		return _rawValue;
	}

	public float RemapTriggerValue(float triggerValue)
	{
		if (!_hasTriggerBeenPressed)
		{
			_hasTriggerBeenPressed = triggerValue != 0f;
			return 0f;
		}
		return (triggerValue + 1f) * 0.5f;
	}
}
