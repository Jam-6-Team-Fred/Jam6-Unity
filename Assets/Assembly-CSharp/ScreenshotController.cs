using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotController : MonoBehaviour
{
	private void Update()
	{
		if (!InputLibrary.rollMode.IsPressed() && InputLibrary.takeScreenshot.IsNewlyPressed() && Keyboard.current != null)
		{
			if (Keyboard.current.digit2Key.isPressed || Keyboard.current.numpad2Key.isPressed)
			{
				OWUtilities.TakeScreenshot(2);
			}
			else if (Keyboard.current.digit3Key.isPressed || Keyboard.current.numpad3Key.isPressed)
			{
				OWUtilities.TakeScreenshot(3);
			}
			else if (Keyboard.current.digit4Key.isPressed || Keyboard.current.numpad4Key.isPressed)
			{
				OWUtilities.TakeScreenshot(4);
			}
			else if (Keyboard.current.digit5Key.isPressed || Keyboard.current.numpad5Key.isPressed)
			{
				OWUtilities.TakeScreenshot(5);
			}
			else if (Keyboard.current.digit6Key.isPressed || Keyboard.current.numpad6Key.isPressed)
			{
				OWUtilities.TakeScreenshot(6);
			}
			else if (Keyboard.current.digit7Key.isPressed || Keyboard.current.numpad7Key.isPressed)
			{
				OWUtilities.TakeScreenshot(7);
			}
			else if (Keyboard.current.digit8Key.isPressed || Keyboard.current.numpad8Key.isPressed)
			{
				OWUtilities.TakeScreenshot(8);
			}
			else
			{
				OWUtilities.TakeScreenshot();
			}
		}
	}
}
