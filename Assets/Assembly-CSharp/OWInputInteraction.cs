using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class OWInputInteraction : IInputInteraction
{
	private static ProfilerMarker perfProcess = new ProfilerMarker("OWInputInteraction.Process");

	private static ProfilerMarker perfButton = new ProfilerMarker("OWInputInteraction.TryApplyButtonInteraction");

	private static ProfilerMarker perfMouse = new ProfilerMarker("OWInputInteraction.TryApplyMouseInteraction");

	private static ProfilerMarker perfSingle = new ProfilerMarker("OWInputInteraction.TryApplySingleInteraction");

	private static ProfilerMarker perfV2 = new ProfilerMarker("OWInputInteraction.TryApplyVector2AxisInteraction");

	private InputConsts.InputType _activeInputType;

	private bool _isActive;

	public void Process(ref InputInteractionContext context)
	{
		InputControl control = context.control;
		InputDevice device = control.device;
		InputConsts.InputType deviceType = InputConsts.GetDeviceType(device);
		if ((_activeInputType == InputConsts.InputType.None || InputConsts.ContainsInputType(_activeInputType, deviceType)) && !ApplyInteraction(ref context, control, device as Mouse))
		{
			Debug.LogWarning("OWInputInteraction Code Path was not run on Action " + context.action.name);
		}
	}

	public void Reset()
	{
		_isActive = false;
	}

	private bool ApplyInteraction(ref InputInteractionContext ctx, InputControl control, Mouse device)
	{
		if (TryApplyButtonInteraction(ref ctx, control as ButtonControl))
		{
			return true;
		}
		if (TryApplyMouseInteraction(ref ctx, device))
		{
			return true;
		}
		if (TryApplyVector2Interaction(ref ctx, control as InputControl<Vector2>))
		{
			return true;
		}
		return TryApplySingleInteraction(ref ctx, control as InputControl<float>);
	}

	private bool TryApplyButtonInteraction(ref InputInteractionContext ctx, ButtonControl buttonControl)
	{
		if (buttonControl == null)
		{
			return false;
		}
		if (InputConsts.FindValueType(ctx.action) != InputConsts.InputValueType.BUTTON)
		{
			return false;
		}
		if (buttonControl.wasPressedThisFrame)
		{
			StartAndPerformAction(ref ctx);
		}
		else if (buttonControl.wasReleasedThisFrame)
		{
			CancelAction(ref ctx);
		}
		return true;
	}

	private bool TryApplyMouseInteraction(ref InputInteractionContext ctx, Mouse device)
	{
		if (device == null)
		{
			return false;
		}
		if (ctx.control is ButtonControl)
		{
			return false;
		}
		if (ctx.timerHasExpired)
		{
			_isActive = false;
			CancelAction(ref ctx);
		}
		else
		{
			if (!_isActive && device.wasUpdatedThisFrame)
			{
				_isActive = true;
				StartAndPerformAction(ref ctx);
			}
			ctx.SetTimeout(0.2f);
		}
		return true;
	}

	private bool TryApplyVector2Interaction(ref InputInteractionContext ctx, InputControl<Vector2> control)
	{
		if (control == null)
		{
			return false;
		}
		float magnitude = ctx.ReadValue<Vector2>().magnitude;
		UpdateActionPhase(ref ctx, magnitude);
		return true;
	}

	private bool TryApplySingleInteraction(ref InputInteractionContext ctx, InputControl<float> control)
	{
		if (control == null)
		{
			return false;
		}
		float value = Mathf.Abs(ctx.ReadValue<float>());
		UpdateActionPhase(ref ctx, value);
		return true;
	}

	private void UpdateActionPhase(ref InputInteractionContext ctx, float value)
	{
		bool flag = value >= float.Epsilon;
		switch (ctx.phase)
		{
		case InputActionPhase.Waiting:
		case InputActionPhase.Canceled:
			if (flag)
			{
				StartAndPerformAction(ref ctx);
			}
			break;
		case InputActionPhase.Started:
		case InputActionPhase.Performed:
			if (!flag)
			{
				CancelAction(ref ctx);
			}
			break;
		}
	}

	private void StartAndPerformAction(ref InputInteractionContext context)
	{
		if (context.phase != InputActionPhase.Performed && context.phase != InputActionPhase.Started)
		{
			context.Started();
			context.PerformedAndStayPerformed();
		}
	}

	private void CancelAction(ref InputInteractionContext context)
	{
		if (context.phase != InputActionPhase.Canceled && context.phase != InputActionPhase.Waiting)
		{
			context.Canceled();
		}
	}
}
