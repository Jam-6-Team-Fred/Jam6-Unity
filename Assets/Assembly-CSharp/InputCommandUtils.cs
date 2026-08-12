using UnityEngine;
using UnityEngine.InputSystem;

public static class InputCommandUtils
{
	public static bool TryCreateInputCommands(InputCommandDefinitions.InputCommandData data, InputActionAsset asset, out IInputCommands command)
	{
		command = null;
		if (!data.IsValid)
		{
			Debug.LogError("Invalid InputCommandData");
			return false;
		}
		if (data.DataType == CommandDataType.Undefined)
		{
			return false;
		}
		if (data.DataType == CommandDataType.Basic && TryCreateBasicAction(data.DataType, data.Primary, asset, out var action))
		{
			command = new InputCommands(data.CommandType, action);
			return true;
		}
		if (!TryCreateAxisAction(data.DataType, data.Primary, asset, out var action2))
		{
			Debug.LogError("Invalid primary axis ActionData for InputCommandType " + data.CommandType);
			return false;
		}
		if (data.IsComposite)
		{
			if (!TryCreateAxisAction(data.DataType, data.Secondary, asset, out var action3))
			{
				Debug.LogError("Invalid secondary axis ActionData for InputCommandType " + data.CommandType);
				return false;
			}
			if (!(action2 is IInputActionPair primaryPair) || !(action3 is IInputActionPair secondaryPair))
			{
				Debug.LogError("Composite bindings not InputActionPairs");
				return false;
			}
			command = new CompositeInputCommands(data.CommandType, primaryPair, secondaryPair);
			return true;
		}
		if (!data.IsSingle)
		{
			Debug.LogError("Single Action InputCommand with type " + data.CommandType.ToString() + " unhandled");
			return false;
		}
		command = new InputAxisCommands(data.CommandType, action2);
		return true;
	}

	public static bool TryCreateMissingInputCommand(InputCommandDefinitions.InputCommandData data, InputActionAsset asset, InputActionAsset defaultAsset, out IInputCommands outputCommand)
	{
		outputCommand = null;
		InputConsts.InputCommandType commandType = data.CommandType;
		if ((uint)(commandType - 131) <= 3u)
		{
			IInputCommands command;
			bool num = TryCreateInputCommands(data, asset, out command);
			IInputCommands command2;
			bool flag = TryCreateInputCommands(data, defaultAsset, out command2);
			if (!num && flag)
			{
				Debug.Log("Adding missing InputCommand to user save: " + data.CommandType);
				outputCommand = command2;
				return true;
			}
		}
		return false;
	}

	private static bool TryCreateBasicAction(CommandDataType type, InputCommandDefinitions.InputActionData actionData, InputActionAsset asset, out IVectorInputAction action)
	{
		action = null;
		if (type != CommandDataType.Basic)
		{
			return false;
		}
		if (!actionData.IsSingle)
		{
			return false;
		}
		InputAction inputAction = asset.FindAction(actionData.ActionName1);
		if (inputAction == null)
		{
			return false;
		}
		InputCommandManager.SanitizeActionForPlatform(inputAction);
		bool isRebindable = actionData.IsRebindable;
		if (InputCommandManager.RebindableInputActionsMap.ContainsKey(actionData.ID))
		{
			action = InputCommandManager.RebindableInputActionsMap[actionData.ID] as RebindableInputAction;
			if (action == null)
			{
				Debug.LogError("IRebindableInputAction RebindableInputActionsMap mismatch when loading " + actionData.ID);
			}
		}
		else if (isRebindable)
		{
			action = new RebindableInputAction(actionData.ID, inputAction);
			InputCommandManager.RebindableInputActionsMap.Add(actionData.ID, (RebindableInputAction)action);
		}
		else
		{
			action = new BasicInputAction(inputAction);
		}
		return action != null;
	}

	private static bool TryCreateAxisAction(CommandDataType type, InputCommandDefinitions.InputActionData actionData, InputActionAsset asset, out IAxisInputAction action)
	{
		action = null;
		if (type != CommandDataType.Axis && type != CommandDataType.Composite)
		{
			return false;
		}
		InputAction inputAction = asset.FindAction(actionData.ActionName1);
		if (inputAction == null)
		{
			return false;
		}
		InputCommandManager.SanitizeActionForPlatform(inputAction);
		bool isRebindable = actionData.IsRebindable;
		if (actionData.IsPair)
		{
			InputAction inputAction2 = asset.FindAction(actionData.ActionName2);
			if (inputAction2 == null)
			{
				return false;
			}
			InputCommandManager.SanitizeActionForPlatform(inputAction2);
			if (InputCommandManager.RebindableInputActionsMap.ContainsKey(actionData.ID))
			{
				action = InputCommandManager.RebindableInputActionsMap[actionData.ID] as RebindableInputActionPair;
				if (action == null)
				{
					Debug.LogError("IRebindableInputAction RebindableInputActionsMap mismatch when loading " + actionData.ID);
				}
			}
			else if (isRebindable)
			{
				action = new RebindableInputActionPair(actionData.ID, inputAction, inputAction2);
				InputCommandManager.RebindableInputActionsMap.Add(actionData.ID, (RebindableInputActionPair)action);
			}
			else
			{
				action = new InputActionPair(inputAction, inputAction2);
			}
			return action != null;
		}
		if (InputCommandManager.RebindableInputActionsMap.ContainsKey(actionData.ID))
		{
			action = InputCommandManager.RebindableInputActionsMap[actionData.ID] as RebindableAxisInputAction;
			if (action == null)
			{
				Debug.LogError("IRebindableInputAction RebindableInputActionsMap mismatch when loading " + actionData.ID);
			}
		}
		else if (isRebindable)
		{
			action = new RebindableAxisInputAction(actionData.ID, inputAction);
			InputCommandManager.RebindableInputActionsMap.Add(actionData.ID, (RebindableAxisInputAction)action);
		}
		else
		{
			action = new AxisInputAction(inputAction);
		}
		return action != null;
	}
}
