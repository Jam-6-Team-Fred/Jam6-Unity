using UnityEngine;

public class DoubleAxis : InputAxis
{
	private SingleAxis _axisX;

	private SingleAxis _axisY;

	private Vector2 _value;

	private bool _useSingleAxisDeadZones;

	public DoubleAxis(AxisIdentifier identifier, SingleAxis axisX, SingleAxis axisY, bool useSingleAxisDeadZones)
	{
		_value = Vector2.zero;
		_axisID = identifier;
		_axisX = axisX;
		_axisY = axisY;
		_useSingleAxisDeadZones = useSingleAxisDeadZones;
		_isMouse = identifier == AxisIdentifier.KEYBD_MOUSE;
	}

	public DoubleAxis(AxisIdentifier identifier, SingleAxis axisX, SingleAxis axisY, float innerDeadZone, float outerDeadZone)
		: this(identifier, axisX, axisY, useSingleAxisDeadZones: false)
	{
		_innerDeadZone = (_origInnerDeadZone = innerDeadZone);
		_outerDeadZone = (_origOuterDeadZone = outerDeadZone);
	}

	public override void UpdateAxis()
	{
		Vector2 zero = Vector2.zero;
		if (_useSingleAxisDeadZones || _isMouse)
		{
			zero = new Vector2(_axisX.GetValue(), _axisY.GetValue());
		}
		else
		{
			zero = new Vector2(_axisX.GetRawValue(), _axisY.GetRawValue());
			zero = zero.normalized * Mathf.InverseLerp(_innerDeadZone, 1f - _outerDeadZone, zero.magnitude);
		}
		_value = zero;
	}

	public Vector2 GetValue()
	{
		return _value;
	}
}
