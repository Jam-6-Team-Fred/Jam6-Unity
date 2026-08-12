using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Obsolete("Obsolete InputCommand Type", true)]
public class RebindableAxisInputCommands : AbstractInputCommands<RebindableInputActionPair>
{
	public RebindableAxisInputCommands(InputConsts.InputCommandType commandType, RebindableID id, InputAction primary, InputAction secondary)
	{
		RebindableInputActionPair newAction = new RebindableInputActionPair(id, primary, secondary);
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
		if (_action.PrimaryAction.activeControl != null)
		{
			return _action.PrimaryAction.activeControl.device;
		}
		return null;
	}
}
