using UnityEngine;

public class TabbedSubMenu : TabbedMenu
{
	protected override void Initialize()
	{
		base.Initialize();
		_tabBackwardCmd = InputLibrary.submenuLeft;
		_tabForwardCmd = InputLibrary.submenuRight;
	}

	protected virtual void Update()
	{
		if (OWInput.IsNewlyPressed(_tabBackwardCmd, InputMode.Menu))
		{
			_tabBackwardCmd.ConsumeInput();
			TabBackward();
		}
		else if (OWInput.IsNewlyPressed(_tabForwardCmd, InputMode.Menu))
		{
			_tabForwardCmd.ConsumeInput();
			TabForward();
		}
	}

	protected override void OnInputModuleTabEvent(GameObject selectedObj, TabEventData eventData)
	{
	}
}
