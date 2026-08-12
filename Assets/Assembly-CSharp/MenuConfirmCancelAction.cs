using UnityEngine;
using UnityEngine.EventSystems;

public class MenuConfirmCancelAction : MenuCancelAction
{
	[SerializeField]
	protected PopupMenu _confirmPopup;

	[SerializeField]
	protected UITextType _confirmMessage;

	private bool _addedListeners;

	private bool _confirmPopupEnabled = true;

	private bool _closeMenuOnCancel;

	protected ScreenPrompt _confirmCancelPrompt;

	protected ScreenPrompt _cancelCancelPrompt;

	public void EnableConfirmPopup(bool enable)
	{
		_confirmPopupEnabled = enable;
	}

	public bool IsConfirmPopupEnabled()
	{
		return _confirmPopupEnabled;
	}

	public void SetCloseMenuOnCancel(bool closeMenuOnCancel)
	{
		_closeMenuOnCancel = closeMenuOnCancel;
	}

	public virtual PopupMenu GetPopupMenu()
	{
		return _confirmPopup;
	}

	public override void MenuCancel(GameObject selectedObject, BaseEventData eventData)
	{
		if (eventData.used)
		{
			return;
		}
		RaiseMenuCancelEvent(selectedObject, eventData);
		if (_menu == null)
		{
			_menu = this.GetRequiredComponent<Menu>();
		}
		if (_confirmPopupEnabled)
		{
			if (_confirmPopup.EventsHaveListeners())
			{
				Debug.LogWarning("Confirm Popup event listeners are not null! This may execute unwanted code");
			}
			_confirmPopup.OnPopupCancel += CancelSubmit;
			_confirmPopup.OnPopupConfirm += ConfirmSubmit;
			_addedListeners = true;
			if (_confirmCancelPrompt == null)
			{
				_confirmCancelPrompt = new ScreenPrompt(InputLibrary.menuConfirm, UITextLibrary.GetString(UITextType.MenuConfirm));
			}
			if (_cancelCancelPrompt == null)
			{
				_cancelCancelPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.MenuCancel));
			}
			_confirmPopup.SetUpPopup(UITextLibrary.GetString(_confirmMessage), InputLibrary.menuConfirm, InputLibrary.cancel, _confirmCancelPrompt, _cancelCancelPrompt);
			_confirmPopup.EnableMenu(value: true);
		}
		else
		{
			CloseMenu();
		}
	}

	protected virtual void ConfirmSubmit()
	{
		CleanupListeners();
		CloseMenu();
	}

	protected virtual void CancelSubmit()
	{
		CleanupListeners();
		if (_closeMenuOnCancel)
		{
			CloseMenu();
		}
	}

	protected virtual void CleanupListeners()
	{
		if (_addedListeners)
		{
			_confirmPopup.OnPopupCancel -= CancelSubmit;
			_confirmPopup.OnPopupConfirm -= ConfirmSubmit;
			_addedListeners = false;
		}
	}
}
