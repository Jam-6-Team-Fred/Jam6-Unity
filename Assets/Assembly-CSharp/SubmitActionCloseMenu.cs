using UnityEngine;

public class SubmitActionCloseMenu : SubmitAction
{
	[SerializeField]
	private Menu _menuToClose;

	protected bool _allowMenuClose = true;

	protected void Awake()
	{
		_allowMenuClose = true;
	}

	public override void Submit()
	{
		base.Submit();
		if (_allowMenuClose)
		{
			_menuToClose.EnableMenu(value: false);
		}
	}

	public void DisableMenuClose()
	{
		_allowMenuClose = false;
	}

	public void EnableMenuClose()
	{
		_allowMenuClose = true;
	}
}
