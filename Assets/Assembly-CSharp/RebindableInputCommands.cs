using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Obsolete("Obsolete InputCommand Type", false)]
public class RebindableInputCommands : AbstractInputCommands<RebindableAxisInputAction>
{
	public RebindableInputCommands(InputConsts.InputCommandType commandType, RebindableID id, InputAction inputAction)
	{
		RebindableAxisInputAction newAction = new RebindableAxisInputAction(id, inputAction);
		SetAction(ref _action, in newAction);
		base.CommandType = commandType;
	}

	protected override void UpdateFromAction()
	{
		base.Action.Update();
		AxisValue = new Vector2(_action.Value, 0f);
		base.UpdateFromAction();
	}

	public override InputControl GetActiveDevice()
	{
		if (_action.Action.activeControl != null)
		{
			return _action.Action.activeControl.device;
		}
		return null;
	}
}
