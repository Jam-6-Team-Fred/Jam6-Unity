using UnityEngine.InputSystem;

public class InputCommands : AbstractInputCommands<IVectorInputAction>
{
	public InputCommands(InputConsts.InputCommandType commandType, IVectorInputAction action)
	{
		SetAction(ref _action, in action);
		base.CommandType = commandType;
	}

	protected override void UpdateFromAction()
	{
		base.Action.Update();
		AxisValue = _action.Value;
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
