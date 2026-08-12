using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleElement : MenuValueOption, IEventSystemHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, ICancelHandler
{
	public delegate void ElementUIEvent(BaseEventData eventData, ToggleElement selectable);

	public delegate void ToggleEvent(ToggleElement selectable);

	public enum ToggleState
	{
		STATE_TRUE = 1,
		STATE_FALSE = 0
	}

	[SerializeField]
	private Text _displayText;

	[SerializeField]
	private Graphic _toggleGraphic;

	[SerializeField]
	private Button _toggleElementButton;

	[SerializeField]
	private bool _isPreflightChecklist;

	protected InputEventListener _buttonEventListener;

	protected UIStyleApplier _navigationButtonStyleApplier;

	protected bool _initOnNextFrame;

	protected bool _uiElementSelected;

	private bool _mouseClickInElement;

	public event ElementUIEvent OnToggleSelect;

	public event ElementUIEvent OnToggleDeselect;

	public event ElementUIEvent OnToggleCancel;

	public event ElementUIEvent OnToggleSubmit;

	public event ToggleEvent OnToggle;

	private void Update()
	{
		if (_initOnNextFrame)
		{
			Initialize(_value);
		}
	}

	public override void Initialize(int value)
	{
		if (value < 0 || value > 1)
		{
			Debug.LogError("Initialize value for " + base.gameObject.name + " is not 0 or 1");
		}
		base.Initialize(value);
		if (!_initOnNextFrame)
		{
			_initOnNextFrame = true;
			return;
		}
		if (_toggleElementButton == null)
		{
			_toggleElementButton = this.GetRequiredComponent<Button>();
		}
		if (_toggleElementButton != null)
		{
			_navigationButtonStyleApplier = _toggleElementButton.GetComponent<UIStyleApplier>();
			_buttonEventListener = _toggleElementButton.gameObject.GetAddComponent<InputEventListener>();
			_navigationButtonStyleApplier.SetAutoInputStateChangesEnabled(value: false);
			_buttonEventListener.OnPointerEnterEvent += OnPointerEnterToggleButton;
			_buttonEventListener.OnPointerExitEvent += OnPointerExitToggleButton;
			_buttonEventListener.OnPointerUpEvent += OnPointerUpInToggleButton;
			_buttonEventListener.OnPointerDownEvent += OnPointerDownInToggleButton;
		}
		UpdateToggleColors();
		_initOnNextFrame = false;
	}

	public void Toggle()
	{
		if (_value == 1)
		{
			_value = 0;
		}
		else if (_value == 0)
		{
			_value = 1;
		}
		if (this.OnToggle != null)
		{
			this.OnToggle(this);
		}
		OnOptionValueChanged();
		UpdateToggleColors();
		if (_isPreflightChecklist)
		{
			Locator.GetMenuAudioController().PlayChangeTab();
		}
		else
		{
			Locator.GetMenuAudioController().PlayOptionToggle();
		}
	}

	protected virtual void OnEnable()
	{
		UpdateToggleColors();
	}

	protected virtual void OnDisable()
	{
		_uiElementSelected = false;
		_mouseClickInElement = false;
		UpdateToggleColors();
	}

	public virtual bool IsSelected()
	{
		return _uiElementSelected;
	}

	protected virtual void OnPointerUpInToggleButton(PointerEventData eventData, Selectable selectable)
	{
		if (_mouseClickInElement && !eventData.used)
		{
			Toggle();
			if (selectable.gameObject == _toggleElementButton.gameObject && eventData.pointerPress == _toggleElementButton.gameObject)
			{
				selectable.Select();
			}
			UpdateToggleColors();
			eventData.Use();
			if (this.OnToggleSubmit != null)
			{
				this.OnToggleSubmit(eventData, this);
			}
		}
	}

	protected virtual void OnPointerDownInToggleButton(PointerEventData eventData, Selectable selectable)
	{
		if (selectable.gameObject == _toggleElementButton.gameObject)
		{
			_mouseClickInElement = true;
		}
	}

	protected virtual void OnPointerExitToggleButton(PointerEventData eventData, Selectable selectable)
	{
		UpdateToggleColors();
		_mouseClickInElement = false;
	}

	protected virtual void OnPointerEnterToggleButton(PointerEventData eventData, Selectable selectable)
	{
		if (selectable.gameObject == _toggleElementButton.gameObject)
		{
			_navigationButtonStyleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		_uiElementSelected = true;
		if (this.OnToggleSelect != null)
		{
			this.OnToggleSelect(eventData, this);
		}
		UpdateToggleColors();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		_uiElementSelected = false;
		if (this.OnToggleDeselect != null)
		{
			this.OnToggleDeselect(eventData, this);
		}
		UpdateToggleColors();
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		eventData.Use();
		Toggle();
		if (this.OnToggleSubmit != null)
		{
			this.OnToggleSubmit(eventData, this);
		}
	}

	public virtual void OnCancel(BaseEventData eventData)
	{
		if (this.OnToggleCancel != null)
		{
			eventData.Use();
			this.OnToggleCancel(eventData, this);
		}
	}

	protected virtual void UpdateToggleColors()
	{
		if (!(_navigationButtonStyleApplier == null))
		{
			if (_uiElementSelected)
			{
				_navigationButtonStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
			}
			else
			{
				_navigationButtonStyleApplier.ChangeState(UIElementState.NORMAL);
			}
			bool flag = _value == 1;
			if (flag != _toggleGraphic.enabled)
			{
				_toggleGraphic.enabled = flag;
			}
		}
	}

	public void SetDisplayText(string text)
	{
		_displayText.text = text;
	}
}
