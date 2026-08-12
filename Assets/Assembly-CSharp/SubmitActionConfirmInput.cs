public class SubmitActionConfirmInput : SubmitActionConfirm
{
	protected PopupInputMenu _confirmInputPopup;

	protected virtual void Awake()
	{
		if (_confirmInputPopup == null)
		{
			_confirmInputPopup = _confirmPopup as PopupInputMenu;
		}
	}

	public virtual string GetInputString()
	{
		return _confirmInputPopup.GetInputText();
	}

	public PopupInputMenu GetInputPopup()
	{
		if (_confirmInputPopup == null)
		{
			_confirmInputPopup = _confirmPopup as PopupInputMenu;
		}
		return _confirmInputPopup;
	}
}
