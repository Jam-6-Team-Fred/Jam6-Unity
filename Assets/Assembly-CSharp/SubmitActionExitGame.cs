using UnityEngine;

public class SubmitActionExitGame : SubmitAction
{
	public override void Submit()
	{
		base.Submit();
		Application.Quit();
	}
}
