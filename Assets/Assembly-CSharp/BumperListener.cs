using UnityEngine;
using UnityEngine.UI;

public class BumperListener : Selectable
{
	private void Update()
	{
		if (base.currentSelectionState == SelectionState.Highlighted)
		{
			if (Input.GetKeyDown(KeyCode.Joystick1Button5))
			{
				base.navigation.selectOnRight.Select();
			}
			else if (Input.GetKeyDown(KeyCode.Joystick1Button4))
			{
				base.navigation.selectOnLeft.Select();
			}
		}
	}
}
