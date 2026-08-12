using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;

public class ExtendedPointerEventData : PointerEventData
{
	public InputControl control { get; set; }

	public InputDevice device { get; set; }

	public int touchId { get; set; }

	public UIPointerType pointerType { get; set; }

	public int uiToolkitPointerId { get; set; }

	public Vector3 trackedDevicePosition { get; set; }

	public Quaternion trackedDeviceOrientation { get; set; }

	public ExtendedPointerEventData(EventSystem eventSystem)
		: base(eventSystem)
	{
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.ToString());
		stringBuilder.AppendLine("button: " + base.button);
		stringBuilder.AppendLine("clickTime: " + base.clickTime);
		stringBuilder.AppendLine("clickCount: " + base.clickCount);
		stringBuilder.AppendLine("device: " + device);
		stringBuilder.AppendLine("pointerType: " + pointerType);
		stringBuilder.AppendLine("touchId: " + touchId);
		stringBuilder.AppendLine("pressPosition: " + base.pressPosition);
		stringBuilder.AppendLine("trackedDevicePosition: " + trackedDevicePosition);
		stringBuilder.AppendLine("trackedDeviceOrientation: " + trackedDeviceOrientation);
		return stringBuilder.ToString();
	}

	internal static int MakePointerIdForTouch(int deviceId, int touchId)
	{
		return (deviceId << 24) + touchId;
	}

	internal static int TouchIdFromPointerId(int pointerId)
	{
		return pointerId & 0xFF;
	}

	public void ReadDeviceState()
	{
		if (control.parent is Pen pen)
		{
			uiToolkitPointerId = GetPenPointerId(pen);
		}
		else if (control.parent is TouchControl touchControl)
		{
			uiToolkitPointerId = GetTouchPointerId(touchControl);
		}
		else if (control.parent is Touchscreen touchscreen)
		{
			uiToolkitPointerId = GetTouchPointerId(touchscreen.primaryTouch);
		}
		else
		{
			uiToolkitPointerId = PointerId.mousePointerId;
		}
	}

	private static int GetPenPointerId(Pen pen)
	{
		int num = 0;
		foreach (InputDevice device in InputSystem.devices)
		{
			if (device is Pen pen2)
			{
				if (pen == pen2)
				{
					return PointerId.penPointerIdBase + Mathf.Min(num, PointerId.penPointerCount - 1);
				}
				num++;
			}
		}
		return PointerId.penPointerIdBase;
	}

	private static int GetTouchPointerId(TouchControl touchControl)
	{
		int value = ((Touchscreen)touchControl.device).touches.IndexOfReference(touchControl);
		return PointerId.touchPointerIdBase + Mathf.Clamp(value, 0, PointerId.touchPointerCount - 1);
	}
}
