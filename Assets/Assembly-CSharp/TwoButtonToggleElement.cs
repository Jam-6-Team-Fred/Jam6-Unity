using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TwoButtonToggleElement : ToggleElement, IEventSystemHandler, ISelectHandler, IDeselectHandler, IMoveHandler
{
	public delegate void ValueChangedEvent();

	[SerializeField]
	private Button _buttonTrue;

	[SerializeField]
	private Button _buttonFalse;

	[Space(10f)]
	[SerializeField]
	private Selectable[] _colorDependentSelectables;

	[SerializeField]
	private Graphic[] _colorDependentGraphics;

	private Button _navigableButton;

	private UIStyleApplier _trueButtonStyleApplier;

	private UIStyleApplier _falseButtonStyleApplier;

	private InputEventListener _trueButtonEventListener;

	private InputEventListener _falseButtonEventListener;

	public event ValueChangedEvent OnValueChanged;

	private void Update()
	{
		if (_initOnNextFrame)
		{
			Initialize(_value);
		}
	}

	public override void Initialize(int value)
	{
		base.Initialize(value);
		if (!_initOnNextFrame)
		{
			_initOnNextFrame = true;
			return;
		}
		if (_navigableButton == null)
		{
			_navigableButton = this.GetRequiredComponent<Button>();
		}
		if (_buttonTrue != null)
		{
			_trueButtonStyleApplier = _buttonTrue.GetComponent<UIStyleApplier>();
			_trueButtonEventListener = _buttonTrue.gameObject.GetAddComponent<InputEventListener>();
			_trueButtonStyleApplier.SetAutoInputStateChangesEnabled(value: false);
			_trueButtonEventListener.OnPointerEnterEvent += OnPointerEnterToggleButton;
			_trueButtonEventListener.OnPointerExitEvent += OnPointerExitToggleButton;
			_trueButtonEventListener.OnPointerUpEvent += OnPointerUpInToggleButton;
		}
		if (_buttonFalse != null)
		{
			_falseButtonStyleApplier = _buttonFalse.GetComponent<UIStyleApplier>();
			_falseButtonEventListener = _buttonFalse.gameObject.GetAddComponent<InputEventListener>();
			_falseButtonStyleApplier.SetAutoInputStateChangesEnabled(value: false);
			_falseButtonEventListener.OnPointerEnterEvent += OnPointerEnterToggleButton;
			_falseButtonEventListener.OnPointerExitEvent += OnPointerExitToggleButton;
			_falseButtonEventListener.OnPointerUpEvent += OnPointerUpInToggleButton;
		}
		_navigationButtonStyleApplier = GetComponent<UIStyleApplier>();
		if (_navigationButtonStyleApplier != null && EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			_navigationButtonStyleApplier.ChangeState(UIElementState.NORMAL, force: true);
		}
		UpdateToggleColors();
		_initOnNextFrame = false;
	}

	protected override void OnPointerUpInToggleButton(PointerEventData eventData, Selectable selectable)
	{
		ToggleState toggleState = (ToggleState)_value;
		if (selectable.gameObject == _buttonTrue.gameObject && eventData.pointerPress == _buttonTrue.gameObject)
		{
			toggleState = ToggleState.STATE_TRUE;
		}
		else if (selectable.gameObject == _buttonFalse.gameObject && eventData.pointerPress == _buttonFalse.gameObject)
		{
			toggleState = ToggleState.STATE_FALSE;
		}
		if (toggleState != (ToggleState)_value)
		{
			_value = (int)toggleState;
			Locator.GetMenuAudioController().PlayOptionToggle();
			UpdateToggleColors();
			if (this.OnValueChanged != null)
			{
				this.OnValueChanged();
			}
			OnOptionValueChanged();
		}
		Locator.GetMenuInputModule().SelectOnNextUpdate(_navigableButton);
	}

	protected override void OnPointerExitToggleButton(PointerEventData eventData, Selectable selectable)
	{
		UpdateToggleColors();
	}

	protected override void OnPointerEnterToggleButton(PointerEventData eventData, Selectable selectable)
	{
		if (selectable.gameObject == _buttonTrue.gameObject)
		{
			_trueButtonStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
		}
		else if (selectable.gameObject == _buttonFalse.gameObject)
		{
			_falseButtonStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
		}
	}

	private void VirtualSelect(AxisEventData eventData)
	{
		if (_navigableButton == null)
		{
			return;
		}
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
		ToggleState toggleState = (ToggleState)_value;
		Button button;
		Navigation navigation2;
		ToggleState toggleState2;
		if (toggleState == ToggleState.STATE_TRUE)
		{
			button = _buttonFalse;
			navigation2 = _buttonTrue.navigation;
			toggleState2 = ToggleState.STATE_FALSE;
		}
		else
		{
			button = _buttonTrue;
			navigation2 = _buttonFalse.navigation;
			toggleState2 = ToggleState.STATE_TRUE;
		}
		switch (eventData.moveDir)
		{
		case MoveDirection.Up:
			if (navigation2.selectOnUp == button)
			{
				toggleState = toggleState2;
			}
			break;
		case MoveDirection.Down:
			if (navigation2.selectOnDown == button)
			{
				toggleState = toggleState2;
			}
			break;
		case MoveDirection.Left:
			if (navigation2.selectOnLeft == button)
			{
				toggleState = toggleState2;
			}
			break;
		case MoveDirection.Right:
			if (navigation2.selectOnRight == button)
			{
				toggleState = toggleState2;
			}
			break;
		}
		if (_value != (int)toggleState)
		{
			_value = (int)toggleState;
			if (this.OnValueChanged != null)
			{
				this.OnValueChanged();
			}
			OnOptionValueChanged();
			Locator.GetMenuAudioController().PlayOptionToggle();
		}
	}

	public void OnMove(AxisEventData eventData)
	{
		VirtualSelect(eventData);
		UpdateToggleColors();
	}

	protected override void UpdateToggleColors()
	{
		if (_trueButtonStyleApplier == null || _falseButtonStyleApplier == null)
		{
			return;
		}
		UIStyleApplier uIStyleApplier;
		UIStyleApplier uIStyleApplier2;
		if (_value == 1)
		{
			uIStyleApplier = _trueButtonStyleApplier;
			uIStyleApplier2 = _falseButtonStyleApplier;
		}
		else
		{
			uIStyleApplier = _falseButtonStyleApplier;
			uIStyleApplier2 = _trueButtonStyleApplier;
		}
		if (uIStyleApplier != null)
		{
			if (_uiElementSelected)
			{
				uIStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
			}
			else
			{
				uIStyleApplier.ChangeState(UIElementState.INTERMEDIATELY_HIGHLIGHTED);
			}
		}
		if (uIStyleApplier2 != null)
		{
			uIStyleApplier2.ChangeState(UIElementState.NORMAL);
		}
	}
}
