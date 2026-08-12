using UnityEngine;
using UnityEngine.InputSystem;

public class RebindableInputActionPair : InputActionPair, IInputAction, IRebindableInputAction
{
	public RebindableID RebindableID { get; protected set; }

	public RebindableInputActionPair(RebindableID rebindableID, InputAction primaryAction, InputAction secondaryAction)
	{
		RebindableID = rebindableID;
		SetAction(primaryAction, secondaryAction);
	}

	public void UpdateFromAction(InputAction primary, InputAction secondary)
	{
		if (primary == null || secondary == null)
		{
			Debug.LogError("Failed to set InputActions for InputActionPair.");
			return;
		}
		Enable(enable: false);
		if (primary.bindings.Count == base.PrimaryAction.bindings.Count)
		{
			for (int i = 0; i < base.PrimaryAction.bindings.Count; i++)
			{
				if (base.PrimaryAction.bindings[i].path != primary.bindings[i].path)
				{
					base.PrimaryAction.ChangeBinding(i).WithPath(primary.bindings[i].path);
				}
			}
		}
		else
		{
			Debug.LogError("uh oh");
		}
		if (secondary.bindings.Count == base.SecondaryAction.bindings.Count)
		{
			for (int j = 0; j < base.SecondaryAction.bindings.Count; j++)
			{
				if (base.SecondaryAction.bindings[j].path != secondary.bindings[j].path)
				{
					base.SecondaryAction.ChangeBinding(j).WithPath(secondary.bindings[j].path);
				}
			}
		}
		else
		{
			Debug.LogError("uh oh");
		}
		PostInitializeActions();
		Enable(enable: true);
	}
}
