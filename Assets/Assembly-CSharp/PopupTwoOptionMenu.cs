using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PopupTwoOptionMenu : Menu
{
	public delegate bool PopupValidateEvent();

	public delegate void PopupActionEvent();

	[SerializeField]
	protected SubmitAction _actionOne;

	[SerializeField]
	protected SubmitAction _actionTwo;

	[SerializeField]
	protected ButtonWithHotkeyImageElement _buttonOne;

	[SerializeField]
	protected ButtonWithHotkeyImageElement _buttonTwo;

	[Header("Root Canvas, cannot be the same as the Canvas on PopupMenu")]
	[SerializeField]
	protected Canvas _rootCanvas;

	protected Canvas _popupCanvas;

	protected GameObject _blocker;

	protected bool _closeMenuOnAction = true;

	protected IInputCommands _commandOne;

	protected IInputCommands _commandTwo;

	protected bool _usingGamepad;

	public event PopupValidateEvent OnPopupValidateActionOne;

	public event PopupValidateEvent OnPopupValidateActionTwo;

	public event PopupActionEvent OnPopupActionOne;

	public event PopupActionEvent OnPopupActionTwo;

	public override Selectable GetSelectOnActivate()
	{
		_usingGamepad = OWInput.UsingGamepad();
		if (_usingGamepad)
		{
			return null;
		}
		return _selectOnActivate;
	}

	public virtual void ResetPopup()
	{
		_commandOne = null;
		_commandTwo = null;
		_selectOnActivate = null;
	}

	public virtual void CloseMenuOnAction(bool value)
	{
		_closeMenuOnAction = value;
	}

	public virtual bool EventsHaveListeners()
	{
		if (this.OnPopupActionOne == null && this.OnPopupActionTwo == null)
		{
			return false;
		}
		return true;
	}

	public virtual void SetButtonPrompts(ScreenPrompt promptOne, IInputCommands commandOne, ScreenPrompt promptTwo, IInputCommands commandTwo)
	{
		_buttonOne.SetPrompt(promptOne);
		_buttonTwo.SetPrompt(promptTwo);
		_commandOne = commandOne;
		_commandTwo = commandTwo;
	}

	protected override void InitializeMenu()
	{
		base.InitializeMenu();
		if (_actionOne != null)
		{
			_actionOne.OnSubmitAction += InvokeActionOne;
		}
		if (_actionTwo != null)
		{
			_actionTwo.OnSubmitAction += InvokeActionTwo;
		}
		_popupCanvas = base.gameObject.GetAddComponent<Canvas>();
		_popupCanvas.overrideSorting = true;
		_popupCanvas.sortingOrder = 30000;
		base.gameObject.GetAddComponent<GraphicRaycaster>();
		base.gameObject.GetAddComponent<CanvasGroup>();
	}

	protected virtual void Update()
	{
		if (_enabledMenu && OWInput.UsingGamepad())
		{
			if (_commandOne != null && _actionOne != null && _commandOne.IsNewlyPressed())
			{
				_actionOne.Submit();
			}
			else if (_commandTwo != null && _actionTwo != null && _commandTwo.IsNewlyPressed())
			{
				_actionTwo.Submit();
			}
		}
	}

	protected void InvokeActionOne()
	{
		if (ValidateOne())
		{
			if (_closeMenuOnAction)
			{
				EnableMenu(value: false);
			}
			if (this.OnPopupActionOne != null)
			{
				this.OnPopupActionOne();
			}
		}
	}

	protected void InvokeActionTwo()
	{
		if (ValidateTwo())
		{
			if (_closeMenuOnAction)
			{
				EnableMenu(value: false);
			}
			if (this.OnPopupActionTwo != null)
			{
				this.OnPopupActionTwo();
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
				MenuStackManager.SharedInstance.Push(this);
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
		UIStyleApplier component = _actionOne.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, force: true);
		}
		component = _actionTwo.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL, force: true);
		}
		base.Deactivate(keepPreviousMenuVisible);
	}

	protected virtual bool ValidateOne()
	{
		bool flag = true;
		if (this.OnPopupValidateActionOne != null)
		{
			Delegate[] invocationList = this.OnPopupValidateActionOne.GetInvocationList();
			bool flag2 = true;
			for (int i = 0; i < invocationList.Length; i++)
			{
				flag2 = (bool)invocationList[i].DynamicInvoke();
				flag = flag && flag2;
			}
		}
		return flag;
	}

	protected virtual bool ValidateTwo()
	{
		bool flag = true;
		if (this.OnPopupValidateActionTwo != null)
		{
			Delegate[] invocationList = this.OnPopupValidateActionTwo.GetInvocationList();
			bool flag2 = true;
			for (int i = 0; i < invocationList.Length; i++)
			{
				flag2 = (bool)invocationList[i].DynamicInvoke();
				flag = flag && flag2;
			}
		}
		return flag;
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
