using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class OWDoubleAxisProcessor : InputProcessor<Vector2>
{
	private static ProfilerMarker perfProcess = new ProfilerMarker("OWAxisProcessor.Process");

	private static ProfilerMarker perfGamepadProcessing = new ProfilerMarker("OWAxisProcessor.TryApplyGamepadProcessing");

	private static ProfilerMarker perfMouseProcessing = new ProfilerMarker("OWAxisProcessor.TryApplyMouseProcessing");

	private AxisSmoothingBuffer _horizontalSmoothingBuffer;

	private AxisSmoothingBuffer _verticalSmoothingBuffer;

	public override Vector2 Process(Vector2 value, InputControl control)
	{
		if (control == null || control.device == null)
		{
			return value;
		}
		InputDevice device = control.device;
		if (!TryApplyProcessing(device, control, ref value))
		{
			Debug.LogWarning("OWAxisProcessor Code Path was not run for Control " + control.displayName);
		}
		return value;
	}

	private bool TryApplyProcessing(InputDevice device, InputControl control, ref Vector2 value)
	{
		if (TryApplyGamepadProcessing(device, control, ref value))
		{
			return true;
		}
		return TryApplyMouseProcessing(device, control, ref value);
	}

	private bool TryApplyGamepadProcessing(InputDevice device, InputControl control, ref Vector2 value)
	{
		if (!(device is Gamepad))
		{
			return false;
		}
		if (!(control is StickControl))
		{
			return false;
		}
		value = OWInputProcessorUtil.ApplyOWDoubleAxisDeadzones(value, 0.2f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
		return true;
	}

	private bool TryApplyMouseProcessing(InputDevice device, InputControl control, ref Vector2 value)
	{
		if (!(device is Mouse))
		{
			return false;
		}
		if (_horizontalSmoothingBuffer == null)
		{
			_horizontalSmoothingBuffer = new AxisSmoothingBuffer();
		}
		if (_verticalSmoothingBuffer == null)
		{
			_verticalSmoothingBuffer = new AxisSmoothingBuffer();
		}
		_horizontalSmoothingBuffer.Update(value.x);
		_verticalSmoothingBuffer.Update(value.y);
		value.x = _horizontalSmoothingBuffer.GetAverage();
		value.y = _verticalSmoothingBuffer.GetAverage();
		value *= 0.05f;
		return true;
	}
}
