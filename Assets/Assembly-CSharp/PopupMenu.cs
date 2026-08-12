using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PopupMenu : Menu
{
	public delegate void PopupConfirmEvent();

	public delegate bool PopupValidateEvent();

	public delegate void PopupCancelEvent();

	[SerializeField]
	protected Text _labelText;

	[SerializeField]
	protected SubmitAction _cancelAction;

	[SerializeField]
	protected SubmitAction _okAction;

	[SerializeField]
	protected ButtonWithHotkeyImageElement _cancelButton;

	[SerializeField]
	protected ButtonWithHotkeyImageElement _confirmButton;

	[Header("Root Canvas, cannot be the same as the Canvas on PopupMenu")]
	[SerializeField]
	protected Canvas _rootCanvas;

	protected Canvas _popupCanvas;

	protected GameObject _blocker;

	protected bool _closeMenuOnOk = true;

	protected IInputCommands _okCommand;

	protected IInputCommands _cancelCommand;

	protected bool _usingGamepad;

	public event PopupConfirmEvent OnPopupConfirm;

	public event PopupValidateEvent OnPopupValidate;

	public event PopupCancelEvent OnPopupCancel;

	public override Selectable GetSelectOnActivate()
	{
		_usingGamepad = OWInput.UsingGamepad();
		if (_usingGamepad)
		{
			return null;
		}
		return _selectOnActivate;
	}

	public virtual void SetUpPopup(string message, IInputCommands okCommand, IInputCommands cancelCommand, ScreenPrompt okPrompt, ScreenPrompt cancelPrompt, bool closeMenuOnOk = true, bool setCancelButtonActive = true)
	{
		_labelText.text = message;
		SetUpPopupCommands(okCommand, cancelCommand, okPrompt, cancelPrompt);
		if (_cancelAction != null)
		{
			_cancelAction.gameObject.SetActive(setCancelButtonActive);
			if (setCancelButtonActive)
			{
				_selectOnActivate = _cancelAction.GetRequiredComponent<Selectable>();
			}
			else
			{
				_selectOnActivate = _okAction.GetRequiredComponent<Selectable>();
			}
		}
		else
		{
			Debug.LogWarning("PopupMenu.SetUpPopup Cancel button not set!");
		}
	}

	public virtual void SetUpPopupCommands(IInputCommands okCommand, IInputCommands cancelCommand, ScreenPrompt okPrompt, ScreenPrompt cancelPrompt)
	{
		_okCommand = okCommand;
		_cancelCommand = cancelCommand;
		_confirmButton.SetPrompt(okPrompt);
		_cancelButton.SetPrompt(cancelPrompt);
	}

	public virtual void ResetPopup()
	{
		_labelText.text = "";
		_okCommand = null;
		_cancelCommand = null;
		_cancelButton.SetPrompt(null);
		_confirmButton.SetPrompt(null);
		_selectOnActivate = null;
	}

	public virtual void CloseMenuOnOk(bool value)
	{
		_closeMenuOnOk = value;
	}

	public virtual bool EventsHaveListeners()
	{
		if (this.OnPopupCancel == null && this.OnPopupConfirm == null)
		{
			return false;
		}
		return true;
	}

	protected override void InitializeMenu()
	{
		base.InitializeMenu();
		if (_cancelAction != null)
		{
			_cancelAction.OnSubmitAction += InvokeCancel;
		}
		_okAction.OnSubmitAction += InvokeOk;
		_popupCanvas = base.gameObject.GetAddComponent<Canvas>();
		_popupCanvas.overrideSorting = true;
		_popupCanvas.sortingOrder = 30000;
		base.gameObject.GetAddComponent<GraphicRaycaster>();
		base.gameObject.GetAddComponent<CanvasGroup>();
	}

	protected virtual void Update()
	{
		if (_cancelCommand != null && OWInput.IsNewlyPressed(_cancelCommand))
		{
			InvokeCancel();
		}
		else if (_okCommand != null && OWInput.IsNewlyPressed(_okCommand))
		{
			InvokeOk();
		}
		else if (_usingGamepad != OWInput.UsingGamepad())
		{
			_usingGamepad = OWInput.UsingGamepad();
			if (_usingGamepad)
			{
				Locator.GetMenuInputModule().SelectOnNextUpdate(null);
			}
			else
			{
				Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnActivate);
			}
		}
	}

	public override void EnableMenu(bool value)
	{
		if (value == _enabledMenu)
		{
			return;
		}
		_enabledMenu = value;
		if (_enabledMenu && !_initialized)
		{
			InitializeMenu();
		}
		if (_addToMenuStackManager)
		{
			if (_enabledMenu)
			{
				MenuStackManager.SharedInstance.Push(this, keepPreviousMenuVisible: true);
			}
			else if (MenuStackManager.SharedInstance.Peek() == this)
			{
				MenuStackManager.SharedInstance.Pop(playCloseAudio: false);
			}
			else
			{
				Debug.LogError("Cannot disable Menu unless it is on the top the MenuLayerManager stack. Current menu on top: " + MenuStackManager.SharedInstance.Peek().gameObject.name);
			}
		}
		else if (_enabledMenu)
		{
			Activate();
			if (_selectOnActivate != null)
			{
				SelectableAudioPlayer component = _selectOnActivate.GetComponent<SelectableAudioPlayer>();
				if (component != null)
				{
					component.SilenceNextSelectEvent();
				}
				Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnActivate);
			}
		}
		else
		{
			Deactivate();
		}
	}

	public override void Activate()
	{
		base.Activate();
		if (_rootCanvas != null)
		{
			_blocker = CreateBlocker(_rootCanvas);
		}
	}

	public override void Deactivate(bool keepPreviousMenuVisible = false)
	{
		if (_rootCanvas != null)
		{
			DestroyBlocker(_blocker);
		}
		UIStyleApplier component = _cancelAction.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, force: true);
		}
		component = _okAction.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, force: true);
		}
		base.Deactivate(keepPreviousMenuVisible);
	}

	public override void OnCancelEvent(GameObject selectedObj, BaseEventData eventData)
	{
		base.OnCancelEvent(selectedObj, eventData);
		if (this.OnPopupCancel != null)
		{
			this.OnPopupCancel();
		}
	}

	protected virtual void InvokeCancel()
	{
		EnableMenu(value: false);
		if (this.OnPopupCancel != null)
		{
			this.OnPopupCancel();
		}
	}

	protected virtual bool Validate()
	{
		bool flag = true;
		if (this.OnPopupValidate != null)
		{
			Delegate[] invocationList = this.OnPopupValidate.GetInvocationList();
			bool flag2 = true;
			for (int i = 0; i < invocationList.Length; i++)
			{
				flag2 = (bool)invocationList[i].DynamicInvoke();
				flag = flag && flag2;
			}
		}
		return flag;
	}

	protected virtual void InvokeOk()
	{
		if (!Validate())
		{
			Debug.Log("PopupMenu.InvokeOk Got Invalid Name, rejecting Confirm");
			return;
		}
		if (_closeMenuOnOk)
		{
			EnableMenu(value: false);
		}
		if (this.OnPopupConfirm != null)
		{
			this.OnPopupConfirm();
		}
	}

	protected virtual GameObject CreateBlocker(Canvas rootCanvas)
	{
		GameObject obj = new GameObject("Blocker");
		RectTransform rectTransform = obj.AddComponent<RectTransform>();
		rectTransform.SetParent(rootCanvas.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector3.zero;
		rectTransform.anchorMax = Vector3.one;
		rectTransform.sizeDelta = Vector2.zero;
		Canvas canvas = obj.AddComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingLayerID = _popupCanvas.sortingLayerID;
		canvas.sortingOrder = _popupCanvas.sortingOrder - 1;
		obj.AddComponent<GraphicRaycaster>();
		Image image = obj.AddComponent<Image>();
		if (Locator.GetUIStyleManager() != null)
		{
			image.color = Locator.GetUIStyleManager().GetPopupBlockerColor();
			return obj;
		}
		image.color = Color.clear;
		return obj;
	}

	protected virtual void DestroyBlocker(GameObject blocker)
	{
		UnityEngine.Object.Destroy(blocker);
	}
}
