using UnityEngine.InputSystem;

public interface IInputActionPair : IInputAction<float>, IInputAction, IAxisInputAction
{
	InputAction PrimaryAction { get; }

	InputAction SecondaryAction { get; }
}
