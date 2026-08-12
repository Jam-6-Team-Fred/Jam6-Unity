public interface ISingleInputCommand : IInputCommands
{
	IInputAction Action { get; }

	bool TryCastAction<TA>(out TA castAction) where TA : IInputAction;
}
