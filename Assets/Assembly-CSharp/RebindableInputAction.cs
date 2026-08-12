using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindableInputAction : BasicInputAction, IInputAction, IRebindableInputAction
{
	public RebindableID RebindableID { get; protected set; }

	public RebindableInputAction(RebindableID rebindableID, InputAction action)
	{
		throw new NotSupportedException("RebindableInputAction Type is Unsupported at this time.");
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
				base.Action.ChangeBinding(i).WithPath(action.bindings[i].path);
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
