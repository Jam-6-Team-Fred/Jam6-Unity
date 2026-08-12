using System;

[Obsolete("Obsolete Input Command type", true)]
public class RebindableCompositeInputCommands : CompositeInputCommands
{
	public RebindableCompositeInputCommands(InputConsts.InputCommandType commandType, InputActionPair primaryPair, InputActionPair secondaryPair)
		: base(commandType, primaryPair, secondaryPair)
	{
	}
}
