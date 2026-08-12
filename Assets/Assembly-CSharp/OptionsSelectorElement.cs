using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class OptionsSelectorElement : MenuValueOption, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IMoveHandler
{
	protected enum Direction
	{
		HORIZONTAL = 0,
		VERTICAL = 1
	}

	public delegate void ValueChangedEvent(int optionIndex);

	[SerializeField]
	protected Direction _directionality;

	[SerializeField]
	protected bool _loopAround;

	[SerializeField]
	protected string[] _optionsList;

	[SerializeField]
	private UITextType[] _options;

	[SerializeField]
	protected Text _displayText;

	[SerializeField]
	protected UIStyleApplier _optionsBoxStyleApplier;

	[Header("Arrow Selectables")]
	[SerializeField]
	protected Selectable _selectOnUp;

	[SerializeField]
	protected Selectable _selectOnDown;

	[SerializeField]
	protected Selectable _selectOnLeft;

	[SerializeField]
	protected Selectable _selectOnRight;

	protected bool _initOnNextFrame;

	protected bool _amISelected;

	protected bool _callbacksInitialized;

	public event ValueChangedEvent OnValueChanged;

	protected virtual void Update()
	{
		if (_initOnNextFrame)
		{
			Initialize(_value, _optionsList);
		}
	}

	protected virtual void OnEnable()
	{
		if (!_callbacksInitialized && _optionsList.Length != 0)
		{
			Button button = null;
			if (_selectOnUp != null)
			{
				button = null;
				button = _selectOnUp as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnUpClick);
				}
			}
			if (_selectOnDown != null)
			{
				button = null;
				button = _selectOnDown as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnDownClick);
				}
			}
			if (_selectOnLeft != null)
			{
				button = null;
				button = _selectOnLeft as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnLeftClick);
				}
			}
			if (_selectOnRight != null)
			{
				button = null;
				button = _selectOnRight as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnRightClick);
				}
			}
			_callbacksInitialized = true;
		}
		UpdateOptionsBoxColors();
	}

	protected virtual void OnDisable()
	{
		_amISelected = false;
		UpdateOptionsBoxColors();
		Button button = null;
		if (_selectOnUp != null)
		{
			button = null;
			button = _selectOnUp as Button;
			if (button != null)
			{
				button.onClick.RemoveListener(OnArrowSelectableOnUpClick);
			}
		}
		if (_selectOnDown != null)
		{
			button = null;
			button = _selectOnDown as Button;
			if (button != null)
			{
				button.onClick.RemoveListener(OnArrowSelectableOnDownClick);
			}
		}
		if (_selectOnLeft != null)
		{
			button = null;
			button = _selectOnLeft as Button;
			if (button != null)
			{
				button.onClick.RemoveListener(OnArrowSelectableOnLeftClick);
			}
		}
		if (_selectOnRight != null)
		{
			button = null;
			button = _selectOnRight as Button;
			if (button != null)
			{
				button.onClick.RemoveListener(OnArrowSelectableOnRightClick);
			}
		}
		_callbacksInitialized = false;
	}

	public override void Initialize(int index)
	{
		Initialize(index, _optionsList);
	}

	public virtual void Initialize(int index, string[] displayedOptions)
	{
		base.Initialize(index);
		_value = index;
		_optionsList = displayedOptions;
		if (!_initOnNextFrame)
		{
			_initOnNextFrame = true;
			return;
		}
		if (_selectable == null)
		{
			_selectable = this.GetRequiredComponent<Selectable>();
		}
		UIStyleApplier component = GetComponent<UIStyleApplier>();
		if (component != null && EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			component.ChangeState(UIElementState.NORMAL, force: true);
		}
		if (!_callbacksInitialized)
		{
			Button button = null;
			if (_selectOnUp != null)
			{
				button = null;
				button = _selectOnUp as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnUpClick);
				}
			}
			if (_selectOnDown != null)
			{
				button = null;
				button = _selectOnDown as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnDownClick);
				}
			}
			if (_selectOnLeft != null)
			{
				button = null;
				button = _selectOnLeft as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnLeftClick);
				}
			}
			if (_selectOnRight != null)
			{
				button = null;
				button = _selectOnRight as Button;
				if (button != null)
				{
					button.onClick.AddListener(OnArrowSelectableOnRightClick);
				}
			}
			_callbacksInitialized = true;
		}
		SetSelectedOption();
		UpdateOptionsBoxColors();
		UpdateArrowSelectables();
		_initOnNextFrame = false;
	}

	public virtual string GetSelectedOption()
	{
		return _optionsList[_value];
	}

	protected virtual void SetSelectedOption()
	{
		_displayText.text = _optionsList[_value];
	}

	protected virtual void UpdateArrowSelectables()
	{
		bool interactable = true;
		bool interactable2 = true;
		bool interactable3 = true;
		bool interactable4 = true;
		if (!_loopAround)
		{
			switch (_directionality)
			{
			case Direction.HORIZONTAL:
				if (_value == 0)
				{
					interactable3 = false;
				}
				else if (_value == _optionsList.Length - 1)
				{
					interactable4 = false;
				}
				break;
			case Direction.VERTICAL:
				if (_value == 0)
				{
					interactable = false;
				}
				else if (_value == _optionsList.Length - 1)
				{
					interactable2 = false;
				}
				break;
			}
		}
		if (_selectOnUp != null)
		{
			_selectOnUp.interactable = interactable;
		}
		if (_selectOnDown != null)
		{
			_selectOnDown.interactable = interactable2;
		}
		if (_selectOnLeft != null)
		{
			_selectOnLeft.interactable = interactable3;
		}
		if (_selectOnRight != null)
		{
			_selectOnRight.interactable = interactable4;
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		_amISelected = true;
		UpdateOptionsBoxColors();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		_amISelected = false;
		UpdateOptionsBoxColors();
	}

	public virtual void OnPointerExit(PointerEventData pointerEventData)
	{
		UpdateOptionsBoxColors();
	}

	protected virtual void UpdateOptionsBoxColors()
	{
		if (_optionsBoxStyleApplier != null)
		{
			if (_amISelected)
			{
				_optionsBoxStyleApplier.ChangeState(UIElementState.HIGHLIGHTED);
			}
			else
			{
				_optionsBoxStyleApplier.ChangeState(UIElementState.INTERMEDIATELY_HIGHLIGHTED);
			}
		}
	}

	public virtual void OnMove(AxisEventData eventData)
	{
		if (_optionsList.Length != 0)
		{
			OptionsMove(eventData.moveVector);
		}
	}

	protected virtual void OptionsMove(Vector2 moveVector)
	{
		int num = _value;
		switch (_directionality)
		{
		case Direction.HORIZONTAL:
			if (moveVector.x > 0f)
			{
				num++;
			}
			else if (moveVector.x < 0f)
			{
				num--;
			}
			break;
		case Direction.VERTICAL:
			if (moveVector.y > 0f)
			{
				num--;
			}
			else if (moveVector.y < 0f)
			{
				num++;
			}
			break;
		}
		if (num >= _optionsList.Length)
		{
			num = ((!_loopAround) ? (_optionsList.Length - 1) : 0);
		}
		else if (num < 0)
		{
			num = (_loopAround ? (_optionsList.Length - 1) : 0);
		}
		if (num != _value)
		{
			_value = num;
			if (this.OnValueChanged != null)
			{
				this.OnValueChanged(_value);
			}
			OnOptionValueChanged();
			SetSelectedOption();
			UpdateArrowSelectables();
			Locator.GetMenuAudioController().PlayOptionToggle();
		}
	}

	public virtual void OnArrowSelectableOnUpClick()
	{
		OptionsMove(new Vector2(0f, 1f));
		Locator.GetMenuInputModule().SelectOnNextUpdate(_selectable);
	}

	public virtual void OnArrowSelectableOnDownClick()
	{
		OptionsMove(new Vector2(0f, -1f));
		Locator.GetMenuInputModule().SelectOnNextUpdate(_selectable);
	}

	public virtual void OnArrowSelectableOnLeftClick()
	{
		OptionsMove(new Vector2(-1f, 0f));
		Locator.GetMenuInputModule().SelectOnNextUpdate(_selectable);
	}

	public virtual void OnArrowSelectableOnRightClick()
	{
		OptionsMove(new Vector2(1f, 0f));
		Locator.GetMenuInputModule().SelectOnNextUpdate(_selectable);
	}
}
