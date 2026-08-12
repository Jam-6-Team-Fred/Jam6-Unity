using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReadInputManager
{
	public static StringBuilder _sb;

	public static StringBuilder _sb2;

	public static void BuildInputCommandOutput(StringBuilder sb, string label, IInputCommands command)
	{
		sb.AppendLine();
		sb.Append(label);
		sb.Append(": ");
		if (command.ValueType == InputConsts.InputValueType.DOUBLE_AXIS)
		{
			sb.Append(command.GetAxisValue().ToString("F2"));
		}
		else
		{
			sb.Append(command.GetValue().ToString("F2"));
		}
	}

	public static string ReadCommandInputs(bool verbose)
	{
		if (_sb == null)
		{
			_sb = new StringBuilder();
		}
		else
		{
			_sb.Length = 0;
		}
		if (verbose)
		{
			_sb.Append("SHIP");
			BuildInputCommandOutput(_sb, "landingCamera", InputLibrary.landingCamera);
			BuildInputCommandOutput(_sb, "autopilot", InputLibrary.autopilot);
			BuildInputCommandOutput(_sb, "freeLook", InputLibrary.freeLook);
			_sb.AppendLine();
			_sb.Append("REFERENCE FRAMES");
			BuildInputCommandOutput(_sb, "lockOn", InputLibrary.lockOn);
			BuildInputCommandOutput(_sb, "matchVelocity", InputLibrary.matchVelocity);
			_sb.AppendLine();
			_sb.Append("TOOLS");
			BuildInputCommandOutput(_sb, "primaryAction", InputLibrary.toolActionPrimary);
			BuildInputCommandOutput(_sb, "secondaryAction", InputLibrary.toolActionSecondary);
			_sb.AppendLine();
			BuildInputCommandOutput(_sb, "toolUp", InputLibrary.toolOptionUp);
			BuildInputCommandOutput(_sb, "toolDown", InputLibrary.toolOptionDown);
			BuildInputCommandOutput(_sb, "toolLeft", InputLibrary.toolOptionLeft);
			BuildInputCommandOutput(_sb, "toolRight", InputLibrary.toolOptionRight);
			_sb.AppendLine();
			BuildInputCommandOutput(_sb, "signalscopeEquip", InputLibrary.signalscope);
			BuildInputCommandOutput(_sb, "flashlight", InputLibrary.flashlight);
			BuildInputCommandOutput(_sb, "extendStick", InputLibrary.extendStick);
			_sb.AppendLine();
			_sb.Append("MENU");
			BuildInputCommandOutput(_sb, "select", InputLibrary.select);
			BuildInputCommandOutput(_sb, "menuConfirm", InputLibrary.menuConfirm);
			BuildInputCommandOutput(_sb, "enter", InputLibrary.enter);
			BuildInputCommandOutput(_sb, "enter2", InputLibrary.enter2);
			BuildInputCommandOutput(_sb, "cancel", InputLibrary.cancel);
			BuildInputCommandOutput(_sb, "escape", InputLibrary.escape);
			BuildInputCommandOutput(_sb, "setDefaults", InputLibrary.setDefaults);
			BuildInputCommandOutput(_sb, "up", InputLibrary.up);
			BuildInputCommandOutput(_sb, "down", InputLibrary.down);
			BuildInputCommandOutput(_sb, "right", InputLibrary.right);
			BuildInputCommandOutput(_sb, "left", InputLibrary.left);
			BuildInputCommandOutput(_sb, "up2", InputLibrary.up2);
			BuildInputCommandOutput(_sb, "down2", InputLibrary.down2);
			BuildInputCommandOutput(_sb, "right2", InputLibrary.right2);
			BuildInputCommandOutput(_sb, "left2", InputLibrary.left2);
			BuildInputCommandOutput(_sb, "tab", InputLibrary.tab);
			BuildInputCommandOutput(_sb, "tabL", InputLibrary.tabL);
			BuildInputCommandOutput(_sb, "tabR", InputLibrary.tabR);
			BuildInputCommandOutput(_sb, "tabL2", InputLibrary.tabL2);
			BuildInputCommandOutput(_sb, "tabR2", InputLibrary.tabR2);
			BuildInputCommandOutput(_sb, "shiftL", InputLibrary.shiftL);
			BuildInputCommandOutput(_sb, "shiftR", InputLibrary.shiftR);
			BuildInputCommandOutput(_sb, "pause", InputLibrary.pause);
			BuildInputCommandOutput(_sb, "faceUp", InputLibrary.faceUp);
			BuildInputCommandOutput(_sb, "faceDown", InputLibrary.faceDown);
			BuildInputCommandOutput(_sb, "faceRight", InputLibrary.faceRight);
			BuildInputCommandOutput(_sb, "faceLeft", InputLibrary.faceLeft);
			_sb.AppendLine();
			_sb.Append("SHIP LOG");
			BuildInputCommandOutput(_sb, "swapShipLogMode", InputLibrary.swapShipLogMode);
			BuildInputCommandOutput(_sb, "markEntryOnHUD", InputLibrary.markEntryOnHUD);
			BuildInputCommandOutput(_sb, "scrollLogText", InputLibrary.scrollLogText);
			_sb.AppendLine();
			_sb.Append("MAP");
			BuildInputCommandOutput(_sb, "map", InputLibrary.map);
			BuildInputCommandOutput(_sb, "mapZoomIn", InputLibrary.mapZoomIn);
			BuildInputCommandOutput(_sb, "mapZoomOut", InputLibrary.mapZoomOut);
		}
		else
		{
			_sb.Append("MOVEMENT");
			BuildInputCommandOutput(_sb, "moveXZ", InputLibrary.moveXZ);
			BuildInputCommandOutput(_sb, "look", InputLibrary.look);
			BuildInputCommandOutput(_sb, "jump", InputLibrary.jump);
			BuildInputCommandOutput(_sb, "interact", InputLibrary.interact);
			_sb.AppendLine();
			_sb.Append("FLIGHT");
			BuildInputCommandOutput(_sb, "rollMode", InputLibrary.rollMode);
			BuildInputCommandOutput(_sb, "boost", InputLibrary.boost);
			BuildInputCommandOutput(_sb, "thrustX", InputLibrary.thrustX);
			BuildInputCommandOutput(_sb, "thrustZ", InputLibrary.thrustZ);
			BuildInputCommandOutput(_sb, "thrustUp", InputLibrary.thrustUp);
			BuildInputCommandOutput(_sb, "thrustDown", InputLibrary.thrustDown);
			BuildInputCommandOutput(_sb, "yaw", InputLibrary.yaw);
			BuildInputCommandOutput(_sb, "pitch", InputLibrary.pitch);
		}
		return _sb.ToString();
	}

	public static string ReadRawInputManagerButtons()
	{
		if (_sb == null)
		{
			_sb = new StringBuilder();
		}
		else
		{
			_sb.Length = 0;
		}
		KeyCode keyCode = KeyCode.JoystickButton0;
		int num = (int)keyCode;
		for (int i = 0; i < 20; i++)
		{
			_sb.AppendLine();
			_sb.Append("Button ");
			_sb.Append(i);
			_sb.Append(": ");
			keyCode = (KeyCode)(num + i);
			_sb.Append(Input.GetKey(keyCode) ? "PRESSED" : "");
		}
		return _sb.ToString();
	}

	public static string ReadInputAxes()
	{
		if (_sb == null)
		{
			_sb = new StringBuilder();
		}
		else
		{
			_sb.Length = 0;
		}
		Gamepad current = Gamepad.current;
		if (current != null)
		{
			_sb.AppendLine("Left Joystick: " + current.leftStick.ReadValue().ToString());
			_sb.AppendLine("Right Joystick: " + current.rightStick.ReadValue().ToString());
		}
		else
		{
			_sb.AppendLine("Active Joystick: NONE");
		}
		return _sb.ToString();
	}
}
