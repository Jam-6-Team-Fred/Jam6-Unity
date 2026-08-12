public class SubmitActionSkipToNextLoop : SubmitActionConfirm
{
	protected override void SetUpPopupMenu()
	{
		string @string = UITextLibrary.GetString(UITextType.PauseMeditate);
		if (_confirmActionPrompt == null)
		{
			_confirmActionPrompt = new ScreenPrompt(InputLibrary.confirm, UITextLibrary.GetString(UITextType.MenuConfirm));
		}
		if (_cancelActionPrompt == null)
		{
			_cancelActionPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuCancel));
		}
		_confirmPopup.SetUpPopup(@string, InputLibrary.confirm, InputLibrary.cancel, _confirmActionPrompt, _cancelActionPrompt);
		base.SetUpPopupMenu();
	}

	protected override void ConfirmSubmit()
	{
		base.ConfirmSubmit();
		AdvanceToNewTimeLoop();
	}

	public void AdvanceToNewTimeLoop()
	{
		Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
	}
}
