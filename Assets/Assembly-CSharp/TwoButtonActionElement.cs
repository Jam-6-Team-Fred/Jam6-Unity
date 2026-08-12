using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TwoButtonActionElement : MenuOption, IEventSystemHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IMoveHandler
{
	public delegate void ActionElementSubmitEvent(TwoButtonActionElement self);

	private enum ElementSelectables
	{
		UNDEFINED = -1,
		BUTTON_ONE = 1,
		BUTTON_TWO = 0
	}

	[SerializeField]
	private Button _buttonOne;

	[SerializeField]
	private Button _buttonTwo;

	[Space(10f)]
	[SerializeField]
	private ElementSelectables _selectButtonOnElementSelect = ElementSelectables.BUTTON_ONE;

	[Space(10f)]
	[SerializeField]
	private SubmitAction _submitActionOne;

	[SerializeField]
	private SubmitAction _submitActionTwo;

	[Space(10f)]
	[SerializeField]
	private Selectable[] _colorDependentSelectables;

	[SerializeField]
	private Graphic[] _colorDependentGraphics;

	private Button _navigableButton;

	private int _selection;

	private UIStyleApplier _buttonOneStyleApplier;

	private UIStyleApplier _buttonTwoStyleApplier;

	private InputEventListener _buttonOneEventListener;

	private InputEventListener _buttonTwoEventListener;

	public event ActionElementSubmitEvent OnActionElementSubmit;

	private void Awake()
	{
		_navigableButton = this.GetRequiredComponent<Button>();
		if (_buttonOne != null)
		{
			_buttonOneStyleApplier = _buttonOne.GetComponent<UIStyleApplier>();
			if (_buttonOneStyleApplier != null)
			{
				_buttonOneStyleApplier.SetAutoInputStateChangesEnabled(value: false);
				_buttonOneEventListener = _buttonOne.GetComponent<InputEventListener>();
				if (_buttonOneEventListener != null)
				{
					_buttonOneEventListener.OnPointerEnterEvent += OnPointerEnterActionButton;
					_buttonOneEventListener.OnPointerExitEvent += OnPointerExitActionButton;
					_buttonOneEventListener.OnSelectEvent += OnSelectActionButton;
				}
			}
		}
		if (_buttonTwo != null)
		{
			_buttonTwoStyleApplier = _buttonTwo.GetComponent<UIStyleApplier>();
			if (_buttonTwoStyleApplier != null)
			{
				_buttonTwoStyleApplier.SetAutoInputStateChangesEnabled(value: false);
				_buttonTwoEventListener = _buttonTwo.GetComponent<InputEventListener>();
				if (_buttonTwoEventListener != null)
				{
					_buttonTwoEventListener.OnPointerEnterEvent += OnPointerEnterActionButton;
					_buttonTwoEventListener.OnPointerExitEvent += OnPointerExitActionButton;
					_buttonTwoEventListener.OnSelectEvent += OnSelectActionButton;
				}
			}
		}
		_submitActionOne.OnSubmitAction += OnAnyActionSubmit;
		_submitActionTwo.OnSubmitAction += OnAnyActionSubmit;
	}

	public SubmitAction GetSubmitActionOne()
	{
		return _submitActionOne;
	}

	public SubmitAction GetSubmitActionTwo()
	{
		return _submitActionTwo;
	}

	public Button GetButtonOne()
	{
		return _buttonOne;
	}

	public Button GetButtonTwo()
	{
		return _buttonTwo;
	}

	public string GetLabelText()
	{
		return _label.text;
	}

	public void SetLabelText(string label)
	{
		_label.text = label;
	}

	private void InitializeSelection()
	{
		if (_selectButtonOnElementSelect == ElementSelectables.UNDEFINED)
		{
			Debug.LogError("Cannot initialize with Undefined option", this);
		}
		base.Initialize();
		_selection = (int)_selectButtonOnElementSelect;
		Button button;
		Button button2;
		if (_selectButtonOnElementSelect == ElementSelectables.BUTTON_ONE)
		{
			button = _buttonOne;
			button2 = _buttonTwo;
		}
		else
		{
			button = _buttonTwo;
			button2 = _buttonOne;
		}
		UIStyleApplier component = button.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.HIGHLIGHTED);
		}
		component = button2.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		InitializeSelection();
	}

	private void VirtualSelect(AxisEventData eventData)
	{
		Navigation navigation = _navigableButton.navigation;
		bool flag = false;
		switch (eventData.moveDir)
		{
		case MoveDirection.Up:
			if (navigation.selectOnUp != null)
			{
				flag = true;
			}
			break;
		case MoveDirection.Down:
			if (navigation.selectOnDown != null)
			{
				flag = true;
			}
			break;
		case MoveDirection.Left:
			if (navigation.selectOnLeft != null)
			{
				flag = true;
			}
			break;
		case MoveDirection.Right:
			if (navigation.selectOnRight != null)
			{
				flag = true;
			}
			break;
		}
		if (flag)
		{
			return;
		}
		ElementSelectables elementSelectables = ElementSelectables.UNDEFINED;
		ElementSelectables elementSelectables2 = ElementSelectables.UNDEFINED;
		Navigation navigation2;
		Button button;
		if (_selection == 1)
		{
			button = _buttonTwo;
			elementSelectables = ElementSelectables.BUTTON_TWO;
			navigation2 = _buttonOne.navigation;
		}
		else
		{
			button = _buttonOne;
			elementSelectables = ElementSelectables.BUTTON_ONE;
			navigation2 = _buttonTwo.navigation;
		}
		switch (eventData.moveDir)
		{
		case MoveDirection.Up:
			if (navigation2.selectOnUp == button)
			{
				elementSelectables2 = elementSelectables;
			}
			break;
		case MoveDirection.Down:
			if (navigation2.selectOnDown == button)
			{
				elementSelectables2 = elementSelectables;
			}
			break;
		case MoveDirection.Left:
			if (navigation2.selectOnLeft == button)
			{
				elementSelectables2 = elementSelectables;
			}
			break;
		case MoveDirection.Right:
			if (navigation2.selectOnRight == button)
			{
				elementSelectables2 = elementSelectables;
			}
			break;
		}
		Button button2;
		if (elementSelectables2 == ElementSelectables.BUTTON_ONE)
		{
			button2 = _buttonOne;
			button = _buttonTwo;
		}
		else
		{
			button2 = _buttonTwo;
			button = _buttonOne;
		}
		UIStyleApplier component = button2.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.HIGHLIGHTED);
		}
		component = button.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL);
		}
		_selection = (int)elementSelectables2;
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		UIStyleApplier component = _buttonOne.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL);
		}
		component = _buttonTwo.GetComponent<UIStyleApplier>();
		if (component != null)
		{
			component.ChangeState(UIElementState.NORMAL);
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		VirtualSelect(eventData);
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (_selection == 1)
		{
			if (_submitActionOne != null)
			{
				_submitActionOne.Submit();
			}
		}
		else if (_submitActionTwo != null)
		{
			_submitActionTwo.Submit();
		}
	}

	private void OnAnyActionSubmit()
	{
		if (this.OnActionElementSubmit != null)
		{
			this.OnActionElementSubmit(this);
		}
	}

	protected virtual void OnPointerExitActionButton(PointerEventData eventData, Selectable selectable)
	{
		if (Locator.GetEventSystem().currentSelectedGameObject == _navigableButton.gameObject)
		{
			if (_selection == 1)
			{
				_buttonOneStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
				_buttonTwoStyleApplier.ChangeState(UIElementState.NORMAL);
			}
			else
			{
				_buttonOneStyleApplier.ChangeState(UIElementState.NORMAL);
				_buttonTwoStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
			}
		}
		else
		{
			_buttonOneStyleApplier.ChangeState(UIElementState.NORMAL);
			_buttonTwoStyleApplier.ChangeState(UIElementState.NORMAL);
		}
	}

	protected virtual void OnPointerEnterActionButton(PointerEventData eventData, Selectable selectable)
	{
		if (Locator.GetEventSystem().currentSelectedGameObject == _navigableButton.gameObject)
		{
			if (selectable.gameObject == _buttonOne.gameObject && _selection == 0)
			{
				_buttonOneStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
			}
			else if (selectable.gameObject == _buttonTwo.gameObject && _selection == 1)
			{
				_buttonTwoStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
			}
		}
		else if (selectable.gameObject == _buttonOne.gameObject)
		{
			_buttonOneStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
			_buttonTwoStyleApplier.ChangeState(UIElementState.NORMAL);
		}
		else if (selectable.gameObject == _buttonTwo.gameObject)
		{
			_buttonOneStyleApplier.ChangeState(UIElementState.NORMAL);
			_buttonTwoStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
		}
	}

	private void OnSelectActionButton(BaseEventData eventData, Selectable selectable)
	{
		if (!Locator.GetMenuInputModule().IsPendingSelection())
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(_navigableButton);
		}
	}
}
