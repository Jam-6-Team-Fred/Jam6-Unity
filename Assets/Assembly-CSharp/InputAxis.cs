public abstract class InputAxis
{
	protected AxisIdentifier _axisID;

	protected float _origInnerDeadZone;

	protected float _innerDeadZone;

	protected float _origOuterDeadZone;

	protected float _outerDeadZone;

	protected bool _isMouse;

	protected bool _isMouseWheel;

	public AxisIdentifier GetID()
	{
		return _axisID;
	}

	public void SetInnerDeadZoneMultiplier(float multiplier)
	{
		_innerDeadZone = _origInnerDeadZone * multiplier;
	}

	public void SetOuterDeadZoneMultiplier(float multiplier)
	{
		_outerDeadZone = _origOuterDeadZone * multiplier;
	}

	public abstract void UpdateAxis();
}
