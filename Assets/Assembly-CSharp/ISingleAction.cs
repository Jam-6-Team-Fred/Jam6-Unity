using UnityEngine.InputSystem;

public interface ISingleAction : IInputAction
{
	InputAction Action { get; }
}
