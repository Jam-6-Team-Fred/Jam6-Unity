using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CompositeInputCommands : AbstractCompositeInputCommands<IInputActionPair>
{
	private bool _initialized;

	private AxisIdentifier _primaryAxisId;

	private AxisIdentifier _secondaryAxisId;

	public CompositeInputCommands(InputConsts.InputCommandType commandType, IInputActionPair primaryPair, IInputActionPair secondaryPair)
	{
		SetAction(ref _primaryAction, in primaryPair);
		SetAction(ref _secondaryAction, in secondaryPair);
		base.CommandType = commandType;
	}

	protected void InitializeAxisID()
	{
		if (_initialized)
		{
			if (_primaryAxisId == base.PrimaryAction.AxisID && _secondaryAxisId == base.SecondaryAction.AxisID)
			{
				return;
			}
			_initialized = false;
		}
		_primaryAxisId = base.PrimaryAction.AxisID;
		_secondaryAxisId = base.SecondaryAction.AxisID;
		if (base.PrimaryAction.AxisID == base.SecondaryAction.AxisID)
		{
			base.AxisID = base.PrimaryAction.AxisID;
		}
		else
		{
			InputTransitionUtil.TryGetParentAxisID(base.PrimaryAction.AxisID, out var parentAxis);
			InputTransitionUtil.TryGetParentAxisID(base.SecondaryAction.AxisID, out var parentAxis2);
			base.AxisID = ((parentAxis == parentAxis2) ? parentAxis : AxisIdentifier.NONE);
		}
		if (base.AxisID == AxisIdentifier.NONE)
		{
			InputActionUtil.TryGetSharedAxisID(_primaryAction.PrimaryAction, _primaryAction.SecondaryAction, _secondaryAction.PrimaryAction, _secondaryAction.SecondaryAction, OWInput.UsingGamepad(), out _axisID);
		}
		_initialized = true;
	}

	protected override void UpdateFromAction()
	{
		base.PrimaryAction.Update();
		base.SecondaryAction.Update();
		bool flag = base.PrimaryAction.HasActiveMouseInput || base.SecondaryAction.HasActiveMouseInput;
		if (TryGetSharedStickControl(out var stickControl))
		{
			Vector2 value = stickControl.ReadUnprocessedValue();
			if (base.PrimaryAction.AxisID == AxisIdentifier.CTRLR_LSTICKX || base.PrimaryAction.AxisID == AxisIdentifier.CTRLR_RSTICKX)
			{
				value = new Vector2(value.y, value.x);
			}
			AxisValue = OWInputProcessorUtil.ApplyOWDoubleAxisDeadzones(value, 0.2f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
		}
		else
		{
			float x = _secondaryAction.Value;
			float y = _primaryAction.Value;
			if (TryGetSharedAxisControl(_secondaryAction, out var axisControl))
			{
				x = axisControl.ReadUnprocessedValue();
			}
			if (TryGetSharedAxisControl(_primaryAction, out var axisControl2))
			{
				y = axisControl2.ReadUnprocessedValue();
			}
			AxisValue = new Vector2(x, y);
			if (!flag)
			{
				AxisValue = OWInputProcessorUtil.ApplyOWDoubleAxisDeadzones(AxisValue, 0.2f * OWInputProcessorUtil.InnerDeadZoneMultiplier, 0.05f * OWInputProcessorUtil.OuterDeadZoneMultiplier);
			}
		}
		InitializeAxisID();
		if (flag)
		{
			base.IsActiveThisFrame = base.PrimaryAction.Phase == InputActionPhase.Performed || base.SecondaryAction.Phase == InputActionPhase.Performed;
			return;
		}
		float num = ((base.SecondaryAction != null) ? float.Epsilon : base.PressedThreshold);
		base.IsActiveThisFrame = AxisValue.magnitude > num;
	}

	private bool TryGetSharedAxisControl(IInputActionPair actionPair, out AxisControl axisControl)
	{
		axisControl = null;
		if (!InputActionUtil.TryGetSharedControl(actionPair.PrimaryAction, actionPair.SecondaryAction, OWInput.UsingGamepad(), out var control))
		{
			return false;
		}
		axisControl = control as AxisControl;
		return axisControl != null;
	}

	private bool TryGetSharedStickControl(out StickControl stickControl)
	{
		stickControl = null;
		if (!InputActionUtil.TryGetSharedControl(_primaryAction.PrimaryAction, _primaryAction.SecondaryAction, _secondaryAction.PrimaryAction, _secondaryAction.SecondaryAction, OWInput.UsingGamepad(), out var control))
		{
			return false;
		}
		stickControl = control as StickControl;
		return stickControl != null;
	}

	public RebindableInputActionPair GetActionForRebindID(RebindableID id)
	{
		if (base.PrimaryAction is RebindableInputActionPair rebindableInputActionPair && id == rebindableInputActionPair.RebindableID)
		{
			return rebindableInputActionPair;
		}
		if (base.SecondaryAction is RebindableInputActionPair rebindableInputActionPair2 && id == rebindableInputActionPair2.RebindableID)
		{
			return rebindableInputActionPair2;
		}
		return null;
	}

	public override InputControl GetActiveDevice()
	{
		if (base.PrimaryAction.ActiveControl != null)
		{
			return base.PrimaryAction.ActiveControl.device;
		}
		return null;
	}
}
