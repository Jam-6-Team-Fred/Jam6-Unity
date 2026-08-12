using UnityEngine;

public static class MouseAxis
{
	public const string MOUSE_X_STRING_ID = "Mouse_X";

	public const string MOUSE_Y_STRING_ID = "Mouse_Y";

	public const string MOUSE_WHEEL_STRING_ID = "Mouse_ScrollWheel";

	public static readonly SingleAxis mouseX = new SingleAxis(AxisIdentifier.KEYBD_MOUSEX, "Mouse_X");

	public static readonly SingleAxis mouseY = new SingleAxis(AxisIdentifier.KEYBD_MOUSEY, "Mouse_Y");

	public static readonly SingleAxis mouseWheel = new SingleAxis(AxisIdentifier.KEYBD_MOUSEWHEEL, "Mouse_ScrollWheel");

	public static readonly DoubleAxis mouse = new DoubleAxis(AxisIdentifier.KEYBD_MOUSE, mouseX, mouseY, useSingleAxisDeadZones: true);

	public static readonly SingleAxis[] mouseAxisList = new SingleAxis[3] { mouseX, mouseY, mouseWheel };

	public static readonly string[] mouseAxisNames = new string[3] { "Mouse_X", "Mouse_Y", "Mouse_ScrollWheel" };

	public static void UpdateAxes()
	{
		mouseX.UpdateAxis();
		mouseY.UpdateAxis();
		mouseWheel.UpdateAxis();
		mouse.UpdateAxis();
	}

	public static string GetAnyRawAxisInput(float minimumInput, out float value)
	{
		value = 0f;
		for (int i = 0; i < mouseAxisNames.Length; i++)
		{
			string text = mouseAxisNames[i];
			value = Input.GetAxis(text);
			if (Mathf.Abs(value) >= minimumInput)
			{
				return text;
			}
		}
		return "";
	}
}
