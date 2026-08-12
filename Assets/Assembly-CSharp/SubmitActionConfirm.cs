using UnityEngine;

public class SubmitActionConfirm : SubmitAction
{
	public delegate void SetupPopupEvent(SubmitActionConfirm sender, PopupMenu popupToOpen);

	public delegate void ActionCancelEvent();

	[SerializeField]
	protected PopupMenu _confirmPopup;

	protected bool _confirmPopupEnabled = true;

	protected bool _listenersAttached;

	protected ScreenPrompt _confirmActionPrompt;

	protected ScreenPrompt _cancelActionPrompt;

	public event SetupPopupEvent OnPostSetupPopup;

	public event ActionCancelEvent OnCancelAction;

	public virtual void EnableConfirm(bool value)
	{
		_confirmPopupEnabled = value;
	}

	public virtual PopupMenu GetPopupMenu()
	{
		return _confirmPopup;
	}

	public override void Submit()
	{
		if (_confirmPopupEnabled)
		{
			if (_confirmPopup.EventsHaveListeners())
			{
				Debug.LogWarning("Confirm Popup event listeners are not null! This may execute unwanted code");
			}
			SetUpPopupMenu();
			_confirmPopup.EnableMenu(value: true);
		}
		else
		{
			ConfirmSubmit();
		}
	}

	protected virtual void OnPopupForceClosed()
	{
		CleanupPopup();
		if (this.OnCancelAction != null)
		{
			this.OnCancelAction();
		}
	}

	protected virtual void ConfirmSubmit()
	{
		CleanupPopup();
		base.Submit();
	}

	protected virtual void CancelSubmit()
	{
		CleanupPopup();
		if (this.OnCancelAction != null)
		{
			this.OnCancelAction();
		}
	}

	protected virtual void CleanupPopup()
	{
		if (_listenersAttached)
		{
			_listenersAttached = false;
			_confirmPopup.OnPopupCancel -= CancelSubmit;
			_confirmPopup.OnPopupConfirm -= ConfirmSubmit;
			_confirmPopup.OnForceClosed -= OnPopupForceClosed;
			_confirmPopup.ResetPopup();
		}
	}

	protected virtual void SetUpPopupMenu()
	{
		_confirmPopup.OnPopupCancel += CancelSubmit;
		_confirmPopup.OnPopupConfirm += ConfirmSubmit;
		_confirmPopup.OnForceClosed += OnPopupForceClosed;
		_listenersAttached = true;
		if (this.OnPostSetupPopup != null)
		{
			this.OnPostSetupPopup(this, _confirmPopup);
		}
	}
}
