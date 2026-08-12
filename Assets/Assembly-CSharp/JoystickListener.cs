using UnityEngine;

public class JoystickListener : MonoBehaviour
{
	private int _joystickNumber;

	public int JoystickNumber
	{
		get
		{
			return _joystickNumber;
		}
		set
		{
			_joystickNumber = value;
		}
	}

	public void OnMenuConfirmPerformed(IInputCommands confirmCommand)
	{
	}
}
