using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabButton : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
	public delegate void TabSelectEvent(TabButton tabButton);

	[SerializeField]
	private Menu _tabbedMenu;

	private Selectable _firstSelectable;

	private Button _mySelectable;

	private bool _styleApplierInitialized;

	private bool _navigationInitialized;

	private bool _initOnNextFrame;

	private bool _iAmEnabled;

	private bool _mouseOver;

	private bool _mousePressed;

	private UIStyleApplier _styleApplier;

	public event TabSelectEvent OnTabSelect;

	private void OnEnable()
	{
		_initOnNextFrame = true;
	}

	private void Update()
	{
		if (_initOnNextFrame)
		{
			EvaluateStyleApplierState();
			_initOnNextFrame = false;
		}
	}

	public Menu GetMenu()
	{
		return _tabbedMenu;
	}

	public Selectable GetSelectable()
	{
		InitializeNavigation();
		return _mySelectable;
	}

	private void InitializeStyleApplier()
	{
		if (!_styleApplierInitialized)
		{
			_styleApplier = GetComponent<UIStyleApplier>();
			if (_styleApplier != null)
			{
				_styleApplier.SetAutoInputStateChangesEnabled(value: false);
			}
			_styleApplierInitialized = true;
		}
	}

	private void InitializeNavigation()
	{
		if (_mySelectable == null)
		{
			_mySelectable = this.GetRequiredComponent<Button>();
		}
		Selectable selectOnActivate = _tabbedMenu.GetSelectOnActivate();
		if (_firstSelectable != selectOnActivate)
		{
			_firstSelectable = selectOnActivate;
			_navigationInitialized = false;
		}
		if (!_navigationInitialized)
		{
			_navigationInitialized = true;
			Selectable selectOnActivate2 = _tabbedMenu.GetSelectOnActivate();
			Navigation navigation = _mySelectable.navigation;
			navigation.selectOnDown = selectOnActivate2;
			_mySelectable.navigation = navigation;
			Navigation navigation2 = selectOnActivate2.navigation;
			selectOnActivate2.navigation = navigation2;
		}
	}

	public void SelectAdjacentTab(int moveDirection)
	{
		Selectable selectable = GetSelectable();
		if (moveDirection == -1 && selectable.FindSelectableOnLeft() != null)
		{
			selectable.FindSelectableOnLeft().Select();
		}
		else if (moveDirection == 1 && selectable.FindSelectableOnRight() != null)
		{
			selectable.FindSelectableOnRight().Select();
		}
	}

	public void Enable(bool value)
	{
		if (_tabbedMenu.IsMenuEnabled() != value)
		{
			_tabbedMenu.EnableMenu(value);
		}
		if (value && _tabbedMenu.GetSelectOnActivate() != null)
		{
			Locator.GetMenuInputModule().SelectOnNextUpdate(_tabbedMenu.GetSelectOnActivate());
		}
		if (_styleApplier == null)
		{
			SetVirtuallySelectedColors(value);
		}
		if (_iAmEnabled != value)
		{
			_iAmEnabled = value;
			EvaluateStyleApplierState();
		}
	}

	private void EvaluateStyleApplierState()
	{
		InitializeStyleApplier();
		if (_styleApplier != null)
		{
			if (_mouseOver && _mousePressed)
			{
				_styleApplier.ChangeState(UIElementState.PRESSED, force: true);
			}
			else if (_mouseOver)
			{
				_styleApplier.ChangeState(UIElementState.ROLLOVER_HIGHLIGHT, force: true);
			}
			else if (_iAmEnabled)
			{
				_styleApplier.ChangeState(UIElementState.INTERMEDIATELY_HIGHLIGHTED, force: true);
			}
			else
			{
				_styleApplier.ChangeState(UIElementState.NORMAL, force: true);
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		InitializeNavigation();
		if (this.OnTabSelect != null)
		{
			this.OnTabSelect(this);
		}
		Selectable selectOnActivate = _tabbedMenu.GetSelectOnActivate();
		SelectableAudioPlayer component = selectOnActivate.GetComponent<SelectableAudioPlayer>();
		if (component != null)
		{
			component.SilenceNextSelectEvent();
		}
		Locator.GetMenuInputModule().SelectOnNextUpdate(selectOnActivate);
		if (!(_tabbedMenu is TabbedSubMenu))
		{
			Locator.GetMenuAudioController().PlayChangeTab();
		}
	}

	public virtual void OnPointerEnter(PointerEventData pointerEventData)
	{
		_mouseOver = true;
		EvaluateStyleApplierState();
	}

	public virtual void OnPointerExit(PointerEventData pointerEventData)
	{
		_mouseOver = false;
		EvaluateStyleApplierState();
	}

	public virtual void OnPointerUp(PointerEventData pointerEventData)
	{
		_mousePressed = false;
		EvaluateStyleApplierState();
	}

	public virtual void OnPointerDown(PointerEventData pointerEventData)
	{
		_mousePressed = true;
		EvaluateStyleApplierState();
	}

	public bool MenuContainsSelectable(Selectable s)
	{
		return s.transform.IsChildOf(_tabbedMenu.transform);
	}

	private void SetVirtuallySelectedColors(bool selected)
	{
		if (_iAmEnabled != selected)
		{
			Selectable selectable = ((!(_mySelectable == null)) ? _mySelectable : GetComponent<Selectable>());
			ColorBlock colors = selectable.colors;
			Color highlightedColor = colors.highlightedColor;
			colors.highlightedColor = colors.normalColor;
			colors.normalColor = highlightedColor;
			selectable.colors = colors;
		}
	}
}
