using UnityEngine;
using UnityEngine.InputSystem;

public class InputAxisCommands : AbstractInputCommands<IAxisInputAction>
{
	public InputAxisCommands(InputConsts.InputCommandType commandType, IAxisInputAction axisAction)
	{
		SetAction(ref _action, in axisAction);
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
		if (base.Action.ActiveControl != null)
		{
			return base.Action.ActiveControl.device;
		}
		return null;
	}
}
