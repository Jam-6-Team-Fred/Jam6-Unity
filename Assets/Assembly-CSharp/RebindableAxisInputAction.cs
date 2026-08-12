using UnityEngine;
using UnityEngine.InputSystem;

public class RebindableAxisInputAction : AxisInputAction, IInputAction, IRebindableInputAction
{
	public RebindableID RebindableID { get; protected set; }

	public RebindableAxisInputAction(RebindableID rebindableID, InputAction action)
	{
		RebindableID = rebindableID;
		SetAction(action);
	}

	public void UpdateFromAction(InputAction action)
	{
		if (action == null)
		{
			Debug.LogError("Failed to set InputAction.");
			return;
		}
		Enable(enable: false);
		if (action.bindings.Count == base.Action.bindings.Count)
		{
			for (int i = 0; i < base.Action.bindings.Count; i++)
			{
				if (base.Action.bindings[i].path != action.bindings[i].path)
				{
					base.Action.ChangeBinding(i).WithPath(action.bindings[i].path);
				}
			}
		}
		else
		{
			Debug.LogError("uh oh");
		}
		InitializeAxisID();
		Enable(enable: true);
	}
}
