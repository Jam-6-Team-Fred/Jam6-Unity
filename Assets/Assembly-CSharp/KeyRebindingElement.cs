using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class KeyRebindingElement : MenuOption, IEventSystemHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IMoveHandler, ICancelHandler
{
	protected enum RebindingElementState
	{
		STANDBY = 0,
		SELECTED = 1,
		REBINDING = 2
	}

	[SerializeField]
	protected RebindableID _rebindID;

	[Space(10f)]
	[SerializeField]
	protected Button _controlButton;

	[SerializeField]
	protected SubmitAction _controlSubmitAction;

	[Space(10f)]
	[SerializeField]
	protected Button _labelButton;

	[Space(10f)]
	[SerializeField]
	protected float _referenceButtonImageHeight;

	[Space(10f)]
	[SerializeField]
	protected GameObject _gamepadBindingImage1Obj;

	[SerializeField]
	protected GameObject _gamepadBindingImage2Obj;

	[SerializeField]
	protected Image _gamepadBindingImage1;

	[SerializeField]
	protected Image _gamepadBindingImage2;

	[Space(10f)]
	[SerializeField]
	protected GameObject _keyboardMouseBindingBlockObj;

	[SerializeField]
	protected GameObject _keyboardMouseBindingImage1Obj;

	[SerializeField]
	protected GameObject _keyboardMouseBindingImage2Obj;

	[SerializeField]
	protected Image _keyboardMouseBindingImage1;

	[SerializeField]
	protected Image _keyboardMouseBindingImage2;

	protected LayoutElement _gamepadBindingImage1Layout;

	protected LayoutElement _gamepadBindingImage2Layout;

	protected LayoutElement _keyboardMouseBindingImage1Layout;

	protected LayoutElement _keyboardMouseBindingImage2Layout;

	protected Texture2D[] _gamepadBindingTextures;

	protected Texture2D[] _keyboardMouseBindingTextures;

	protected Selectable _rebindingElementSelectable;

	protected UIStyleApplier _controlButtonStyleApplier;

	protected InputEventListener _controlButtonEventListener;

	protected SettingsMenuModel _settingsModel;

	protected IRebindableInputAction _rebindingInputCommand;

	protected IInputCommands _cancelRebind1;

	protected IInputCommands _cancelRebind2;

	protected RebindingState _rebindState;

	protected RebindingElementState _currentState;

	protected void OnValidate()
	{
		_settingId = SettingsID.REBINDABLE_OPTION;
	}

	public virtual void Initialize(SettingsMenuModel settingsMenuModel)
	{
		base.Initialize();
		_rebindingElementSelectable = this.GetRequiredComponent<Selectable>();
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		_keyboardMouseBindingTextures = new Texture2D[2];
		_settingsModel = settingsMenuModel;
		_gamepadBindingTextures = new Texture2D[2];
		_gamepadBindingImage1Layout = _gamepadBindingImage1Obj.GetRequiredComponent<LayoutElement>();
		_gamepadBindingImage2Layout = _gamepadBindingImage2Obj.GetRequiredComponent<LayoutElement>();
		_keyboardMouseBindingImage1Layout = _keyboardMouseBindingImage1Obj.GetRequiredComponent<LayoutElement>();
		_keyboardMouseBindingImage2Layout = _keyboardMouseBindingImage2Obj.GetRequiredComponent<LayoutElement>();
		IInputCommands value = null;
		InputCommandDefinitions.TryGetInputCommandData(_rebindID, out var data);
		InputConsts.InputCommandType commandType = data.CommandType;
		InputCommandManager.MappedInputActions.TryGetValue(commandType, out value);
		if (value == null || !value.IsRebindable)
		{
			Debug.LogError("Command is non-bindable!", base.gameObject);
			return;
		}
		if (value is InputCommands inputCommands && inputCommands.Action is IRebindableInputAction rebindableAction)
		{
			SetRebindableAction(rebindableAction);
		}
		else if (value is InputAxisCommands inputAxisCommands && inputAxisCommands.Action is IRebindableInputAction rebindableAction2)
		{
			SetRebindableAction(rebindableAction2);
		}
		else if (value is CompositeInputCommands compositeInputCommands)
		{
			RebindableInputActionPair actionForRebindID = compositeInputCommands.GetActionForRebindID(_rebindID);
			SetRebindableAction(actionForRebindID);
		}
		_cancelRebind1 = InputLibrary.cancelRebinding1;
		_cancelRebind2 = InputLibrary.cancelRebinding2;
		if (_labelButton != null)
		{
			_labelButton.onClick.AddListener(OnPointerUpInLabelButton);
		}
		_controlButtonStyleApplier = _controlButton.GetComponent<UIStyleApplier>();
		_controlButtonStyleApplier.SetAutoInputStateChangesEnabled(value: false);
		_controlButtonStyleApplier.ChangeState(UIElementState.NORMAL);
		_controlButtonEventListener = _controlButton.gameObject.GetAddComponent<InputEventListener>();
		_controlButtonEventListener.OnPointerEnterEvent += OnPointerEnterKeyBinderButton;
		_controlButtonEventListener.OnPointerExitEvent += OnPointerExitKeyBinderButton;
		_controlButtonEventListener.OnPointerUpEvent += OnPointerUpInKeyBinderButton;
		UpdateDisplay();
	}

	protected virtual void EnterRebindingState()
	{
		if (_currentState == RebindingElementState.SELECTED)
		{
			OWInput.ChangeInputMode(InputMode.Rebinding);
			Debug.Log("ENTER REBINDING MODE: " + _rebindID, this);
			_currentState = RebindingElementState.REBINDING;
			UpdateRebindingButtonColors();
			if (_settingsModel.InitializeRebindingAction(_rebindingInputCommand, out var rebindingState))
			{
				_rebindState = rebindingState;
				RebindingState rebindState = _rebindState;
				rebindState.OnFinishedRebinding = (Action)Delegate.Combine(rebindState.OnFinishedRebinding, new Action(OnFinishedRebinding));
				RebindingState rebindState2 = _rebindState;
				rebindState2.OnCancelledRebinding = (Action)Delegate.Combine(rebindState2.OnCancelledRebinding, new Action(OnCancelledRebinding));
			}
			else
			{
				ExitRebindingState();
			}
		}
	}

	protected void ExitRebindingState()
	{
		_rebindState = null;
		if (_currentState == RebindingElementState.REBINDING)
		{
			_currentState = RebindingElementState.SELECTED;
			Locator.GetMenuInputModule().SelectOnNextUpdate(_rebindingElementSelectable);
			UpdateRebindingButtonColors();
			OWInput.RestorePreviousInputs();
		}
	}

	protected virtual void OnFinishedRebinding()
	{
		Debug.Log("Rebind COMPLETE", this);
		ExitRebindingState();
	}

	protected void OnCancelledRebinding()
	{
		Debug.Log("Rebind CANCELED", this);
		ExitRebindingState();
		UpdateDisplay();
	}

	public RebindableID GetRebindableID()
	{
		return _rebindID;
	}

	protected void OnRebindingMenuClose()
	{
		_currentState = RebindingElementState.STANDBY;
		_controlButton.GetComponent<UIStyleApplier>().ChangeState(UIElementState.NORMAL);
	}

	protected void OnDestroy()
	{
		if (_labelButton != null)
		{
			_labelButton.onClick.RemoveListener(OnPointerUpInLabelButton);
		}
	}

	public void SetRebindableAction(IRebindableInputAction rebindableAction)
	{
		_rebindingInputCommand = rebindableAction;
		UpdateDisplay();
	}

	public bool IsInRebindingMode()
	{
		return _currentState == RebindingElementState.REBINDING;
	}

	public virtual void UpdateDisplay(bool forceRefresh = false)
	{
		if (_rebindingInputCommand != null)
		{
			UpdateTextures(usingGamepad: true, _gamepadBindingImage1, _gamepadBindingImage2, _gamepadBindingImage1Layout, _gamepadBindingImage2Layout, forceRefresh);
			UpdateTextures(usingGamepad: false, _keyboardMouseBindingImage1, _keyboardMouseBindingImage2, _keyboardMouseBindingImage1Layout, _keyboardMouseBindingImage2Layout, forceRefresh);
		}
	}

	protected virtual void UpdateTextures(bool usingGamepad, Image image1, Image image2, LayoutElement layout1, LayoutElement layout2, bool forceRefresh)
	{
		List<Texture2D> list = ((_rebindState == null) ? _rebindingInputCommand.GetUITextures(usingGamepad, forceRefresh) : _rebindState.GetCurrentlyBindingAction().GetUITextures(usingGamepad, forceRefresh));
		if (list.Count > 0)
		{
			image1.sprite = Sprite.Create(list[0], new Rect(0f, 0f, list[0].width, list[0].height), new Vector2(0.5f, 0.5f), list[0].width);
			image1.enabled = true;
			layout1.minHeight = ButtonPromptLibrary.AdjustButtonImageSize(list[0], _referenceButtonImageHeight, reduceAxisImages: true).y;
		}
		else
		{
			image1.enabled = false;
		}
		GameObject gameObject = (usingGamepad ? _gamepadBindingImage2Obj : _keyboardMouseBindingImage2Obj);
		if (list.Count > 1)
		{
			if (list[0] == list[1])
			{
				gameObject.SetActive(value: false);
				return;
			}
			gameObject.SetActive(value: true);
			image2.sprite = Sprite.Create(list[1], new Rect(0f, 0f, list[1].width, list[1].height), new Vector2(0.5f, 0.5f), list[1].width);
			layout2.minHeight = ButtonPromptLibrary.AdjustButtonImageSize(list[1], _referenceButtonImageHeight, reduceAxisImages: true).y;
			if (RebindableLookup.IsXAxisRebindable(_rebindID))
			{
				Sprite sprite = image1.sprite;
				image1.sprite = image2.sprite;
				image2.sprite = sprite;
			}
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (_currentState == RebindingElementState.STANDBY)
		{
			_currentState = RebindingElementState.SELECTED;
		}
		UpdateRebindingButtonColors();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (_currentState == RebindingElementState.SELECTED)
		{
			if (eventData is PointerEventData pointerEventData && pointerEventData.pointerPressRaycast.gameObject.transform.IsChildOf(base.transform))
			{
				_selectable.Select();
				return;
			}
			_currentState = RebindingElementState.STANDBY;
			_controlButton.GetComponent<UIStyleApplier>().ChangeState(UIElementState.NORMAL);
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		if (_currentState > RebindingElementState.SELECTED)
		{
			eventData.Use();
		}
	}

	public void OnCancel(BaseEventData eventData)
	{
		_controlButtonStyleApplier.ChangeState(UIElementState.NORMAL);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		eventData.Use();
		if (_currentState == RebindingElementState.SELECTED)
		{
			_controlButtonStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
			EnterRebindingState();
		}
	}

	protected virtual void OnPointerUpInLabelButton()
	{
		if (_currentState == RebindingElementState.STANDBY)
		{
			_rebindingElementSelectable.Select();
		}
		else if (_currentState == RebindingElementState.SELECTED)
		{
			_controlButtonStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
			EnterRebindingState();
			Locator.GetMenuAudioController().PlayOptionToggle();
		}
	}

	protected virtual void OnPointerUpInKeyBinderButton(PointerEventData eventData, Selectable selectable)
	{
		Debug.Log("Pointer Up!");
		if (_currentState == RebindingElementState.STANDBY)
		{
			eventData.Use();
			_rebindingElementSelectable.Select();
		}
		else if (_currentState == RebindingElementState.SELECTED)
		{
			eventData.Use();
			Locator.GetMenuAudioController().PlayOptionToggle();
			_rebindingElementSelectable.Select();
			EnterRebindingState();
		}
	}

	protected virtual void OnPointerExitKeyBinderButton(PointerEventData eventData, Selectable selectable)
	{
		UpdateRebindingButtonColors();
	}

	protected virtual void OnPointerEnterKeyBinderButton(PointerEventData eventData, Selectable selectable)
	{
		_controlButtonStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
	}

	protected void UpdateRebindingButtonColors()
	{
		UIElementState state;
		switch (_currentState)
		{
		case RebindingElementState.REBINDING:
			state = UIElementState.INTERMEDIATELY_HIGHLIGHTED;
			break;
		case RebindingElementState.SELECTED:
			state = UIElementState.HIGHLIGHTED;
			break;
		default:
			state = UIElementState.NORMAL;
			break;
		}
		_controlButtonStyleApplier.ChangeState(state);
	}
}
