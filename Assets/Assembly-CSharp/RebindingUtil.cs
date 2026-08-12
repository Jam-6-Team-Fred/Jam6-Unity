using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public static class RebindingUtil
{
	private static RebindableID[] sharedConflictGroup1 = new RebindableID[3]
	{
		RebindableID.MENU_CONFIRM,
		RebindableID.MENU_CANCEL,
		RebindableID.SET_DEFAULTS
	};

	private static RebindableID[] sharedConflictGroup2 = new RebindableID[3]
	{
		RebindableID.INTERACT,
		RebindableID.INTERACT_SECONDARY,
		RebindableID.JUMP
	};

	private static RebindableID[] sharedConflictGroup3 = new RebindableID[2]
	{
		RebindableID.TOOL_PRIMARY,
		RebindableID.SIGNALSCOPE
	};

	private static RebindableID[] sharedConflictGroup4 = new RebindableID[2]
	{
		RebindableID.TOOL_PRIMARY,
		RebindableID.TOOL_SECONDARY
	};

	private static RebindableID[][] sharedConflictGroups = new RebindableID[4][] { sharedConflictGroup1, sharedConflictGroup2, sharedConflictGroup3, sharedConflictGroup4 };

	private static RebindableID[] cancelConflictGroup1 = new RebindableID[2]
	{
		RebindableID.MENU_CANCEL,
		RebindableID.FLIGHT_MATCHV
	};

	private static RebindableID[] cancelConflictGroup3 = new RebindableID[2]
	{
		RebindableID.MENU_CANCEL,
		RebindableID.JUMP
	};

	private static RebindableID[] cancelConflictGroup4 = new RebindableID[2]
	{
		RebindableID.MENU_CANCEL,
		RebindableID.FLIGHT_BOOST
	};

	private static RebindableID[][] cancelConflictGroups = new RebindableID[7][] { sharedConflictGroup1, sharedConflictGroup2, sharedConflictGroup3, sharedConflictGroup4, cancelConflictGroup1, cancelConflictGroup3, cancelConflictGroup4 };

	public static void ResolveConflicts(ref RebindableAxisInputAction action, string oldPath, string newPath, int bindingIndex, bool usingGamepad, bool consoleCancelConfirmCheck = false)
	{
		ResolveSelfConflicts(ref action, oldPath, newPath, bindingIndex);
		ResolveExternalConflicts(ref action, oldPath, newPath, usingGamepad, consoleCancelConfirmCheck);
	}

	private static void ResolveSelfConflicts(ref RebindableAxisInputAction action, string oldPath, string newPath, int bindingIndex)
	{
		for (int i = 0; i < action.Action.bindings.Count; i++)
		{
			if (i != bindingIndex && action.Action.bindings[i].effectivePath == newPath)
			{
				action.Action.ChangeBinding(i).WithPath(oldPath);
			}
		}
	}

	private static void ResolveExternalConflicts(ref RebindableAxisInputAction inputAction, string oldPath, string newPath, bool usingGamepad, bool consoleCancelConfirmCheck)
	{
		RebindableID[][] array = (consoleCancelConfirmCheck ? cancelConflictGroups : sharedConflictGroups);
		foreach (RebindableID[] array2 in array)
		{
			if (!Enumerable.Contains(array2, inputAction.RebindableID))
			{
				continue;
			}
			foreach (RebindableID rebindableID in array2)
			{
				if (rebindableID == inputAction.RebindableID)
				{
					continue;
				}
				InputConsts.InputCommandType key = RebindableLookup.SharedInstance.LookupInputCommandType(rebindableID);
				if (!InputCommandManager.MappedInputActions.TryGetValue(key, out var value))
				{
					continue;
				}
				if (!(value is ISingleInputCommand singleInputCommand))
				{
					Debug.LogError("Conflict resolution was never made to work with action pairs");
					continue;
				}
				if (!singleInputCommand.TryCastAction<RebindableAxisInputAction>(out var castAction))
				{
					Debug.LogError("Conflict resolution Action Type is invalid");
					continue;
				}
				string controlScheme = InputConsts.GetControlScheme(usingGamepad);
				int bindingIndex = castAction.Action.GetBindingIndex(UnityEngine.InputSystem.InputBinding.MaskByGroup(controlScheme));
				UnityEngine.InputSystem.InputBinding inputBinding = castAction.Action.bindings[bindingIndex];
				Debug.Log("Checking for conflict with " + inputAction.RebindableID.ToString() + "\nBetween " + inputBinding.effectivePath + " and " + newPath);
				if (bindingIndex != -1 && inputBinding.effectivePath == newPath)
				{
					Debug.Log("Conflict found for binding " + inputAction.RebindableID.ToString() + " and " + castAction.RebindableID);
					castAction.Action.Disable();
					castAction.Action.ChangeBinding(bindingIndex).WithPath(oldPath);
					castAction.Action.Enable();
					OWInput.NotifyBindingChanged();
				}
			}
		}
	}
}
