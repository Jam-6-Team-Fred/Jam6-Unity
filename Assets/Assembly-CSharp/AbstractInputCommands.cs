using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AbstractInputCommands<T> : AbstractCommands, IInputCommands, ISingleInputCommand where T : class, IInputAction
{
	protected T _action;

	public IInputAction Action => _action;

	public override bool IsRebindable => _action is IRebindableInputAction;

	public bool TryCastAction<TA>(out TA castAction) where TA : IInputAction
	{
		T action;
		object obj;
		if ((action = _action) is TA)
		{
			TA val = (TA)(object)action;
			obj = val;
		}
		else
		{
			obj = default(TA);
		}
		castAction = (TA)obj;
		return castAction != null;
	}

	public override bool HasSameBinding(IInputCommands compare, bool usingGamepad)
	{
		InputActionUtil.ExtractInputActions(compare, out var primary, out var secondary);
		bool flag = InputActionUtil.UsingSameBinding(Action, primary, usingGamepad);
		if (secondary != null)
		{
			flag |= InputActionUtil.UsingSameBinding(Action, secondary, usingGamepad);
		}
		return flag;
	}

	public override List<Texture2D> GetUITextures(bool gamepad, bool forceRefresh = false)
	{
		if (textureList.Count > 0 && isGamepadTextures == gamepad && !forceRefresh)
		{
			return textureList;
		}
		textureList.Clear();
		isGamepadTextures = gamepad;
		if (gamepad && !OWInput.UsingGamepad())
		{
			return textureList;
		}
		textureList = Action.GetUITextures(gamepad, combineImagesWhenPossible: true);
		return textureList;
	}

	protected override void UpdateFromAction()
	{
		base.AxisID = Action.AxisID;
		if (Action.HasActiveMouseInput)
		{
			base.IsActiveThisFrame = Action.Phase == InputActionPhase.Performed;
			return;
		}
		float num = ((Action.ValueType == InputConsts.InputValueType.DOUBLE_AXIS) ? float.Epsilon : base.PressedThreshold);
		base.IsActiveThisFrame = AxisValue.magnitude > num;
	}

	public override void EnableAllActions(bool enable)
	{
		if (Action != null)
		{
			Action.Enable(enable);
		}
	}
}
