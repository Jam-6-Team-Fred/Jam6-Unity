using Unity.Profiling;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class OWAxisProcessor : InputProcessor<float>
{
	private static ProfilerMarker perfProcess = new ProfilerMarker("OWAxisProcessor.Process");

	private static ProfilerMarker perfGamepadProcessing = new ProfilerMarker("OWAxisProcessor.TryApplyGamepadProcessing");

	public override float Process(float value, InputControl control)
	{
		if (control == null || control.device == null)
		{
			return value;
		}
		InputDevice device = control.device;
		TryApplyProcessing(device, control, ref value);
		return value;
	}

	private bool TryApplyProcessing(InputDevice device, InputControl control, ref float value)
	{
		if (device is Mouse && !(control is ButtonControl))
		{
			return false;
		}
		if (!(control is AxisControl))
		{
			return false;
		}
		if (InputActionUtil.ControlPathIsGamepadTrigger(control))
		{
			value = OWInputProcessorUtil.ApplyOWAxisDeadzone(value, 0.1f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
		}
		else
		{
			value = OWInputProcessorUtil.ApplyOWAxisDeadzone(value, 0.2f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
		}
		return true;
	}
}
