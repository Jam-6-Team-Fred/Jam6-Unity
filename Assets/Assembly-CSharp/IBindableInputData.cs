using UnityEngine;

public interface IBindableInputData
{
	KeyCode PrimaryKey { get; }

	KeyCode SecondaryKey { get; }

	JoystickButton PrimaryButton { get; }

	JoystickButton SecondaryButton { get; }
}
