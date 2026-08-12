public interface ICompositeInputCommands : IInputCommands
{
	IInputAction PrimaryAction { get; }

	IInputAction SecondaryAction { get; }

	bool TryCastActions<TA>(out TA castPrimary, out TA castSecondary) where TA : IInputAction;
}
