using UnityEngine;

public class SubmitActionMenu : SubmitAction
{
	[SerializeField]
	private Menu _menuToOpen;

	public override void Submit()
	{
		base.Submit();
		_menuToOpen.EnableMenu(value: true);
	}
}
