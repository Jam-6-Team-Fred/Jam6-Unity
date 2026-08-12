using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
	public class MenuSelectHandler : MonoBehaviour, IEventSystemHandler, ISelectHandler
	{
		public delegate void SelectableSelectedEvent(Selectable s);

		public event SelectableSelectedEvent OnSelectableSelected;

		public void OnSelect(BaseEventData eventData)
		{
			if (this.OnSelectableSelected != null)
			{
				this.OnSelectableSelected(GetComponent<Selectable>());
			}
		}
	}

	public delegate void ActivateMenuEvent();

	public delegate void DeactivateMenuEvent();

	public delegate void ForceClosedMenuEvent();

	[FormerlySerializedAs("_menuRoot")]
	[SerializeField]
	protected GameObject _menuActivationRoot;

	[SerializeField]
	protected bool _startEnabled;

	[SerializeField]
	protected Selectable _selectOnActivate;

	[Space(10f)]
	[SerializeField]
	protected GameObject _selectableItemsRoot;

	[Space(10f)]
	[SerializeField]
	protected Menu[] _subMenus;

	[Space(10f)]
	[SerializeField]
	protected MenuOption[] _menuOptions;

	[SerializeField]
	protected bool _setMenuNavigationOnActivate = true;

	[Space(10f)]
	[SerializeField]
	protected TooltipDisplay _tooltipDisplay;

	[SerializeField]
	protected bool _addToMenuStackManager = true;

	protected MenuCancelAction _menuCancelAction;

	protected bool _isActivated;

	protected bool _enabledMenu;

	protected bool _initialized;

	protected Selectable _lastSelected;

	protected Selectable[] _listSelectables;

	public event ActivateMenuEvent OnActivateMenu;

	public event DeactivateMenuEvent OnDeactivateMenu;

	public event ForceClosedMenuEvent OnForceClosed;

	public static void SetVerticalNavigation(Menu menu, MenuOption[] menuOptionList)
	{
		Navigation navigation = default(Navigation);
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Selectable selectable4 = null;
		for (int i = 0; i < menuOptionList.Length; i++)
		{
			selectable3 = menuOptionList[i].GetSelectable();
			if (selectable3 == null)
			{
				Debug.Break();
			}
			if (selectable3.gameObject == null)
			{
				Debug.Break();
			}
			if (!selectable3.gameObject.activeSelf)
			{
				if (i == menuOptionList.Length - 1)
				{
					navigation.selectOnDown = selectable;
					selectable2.navigation = navigation;
					Navigation navigation2 = selectable.navigation;
					navigation2.selectOnUp = selectable2;
					selectable.navigation = navigation2;
				}
				continue;
			}
			selectable2 = selectable3;
			if (selectable == null)
			{
				selectable = selectable2;
			}
			navigation = selectable2.navigation;
			navigation.selectOnUp = selectable4;
			if (i == menuOptionList.Length - 1)
			{
				navigation.selectOnDown = selectable;
				Navigation navigation3 = selectable.navigation;
				navigation3.selectOnUp = selectable2;
				selectable.navigation = navigation3;
			}
			else
			{
				navigation.selectOnDown = null;
			}
			selectable2.navigation = navigation;
			if (selectable4 != null)
			{
				Navigation navigation4 = selectable4.navigation;
				navigation4.selectOnDown = selectable2;
				selectable4.navigation = navigation4;
			}
			selectable4 = selectable2;
		}
		menu.SetSelectOnActivate(selectable);
	}

	public virtual void SetSelectOnActivate(Selectable s)
	{
		_selectOnActivate = s;
	}

	public virtual Selectable GetSelectOnActivate()
	{
		return _selectOnActivate;
	}

	public virtual Selectable GetLastSelected()
	{
		return _lastSelected;
	}

	protected void OnMenuItemSelected(Selectable s)
	{
		for (int i = 0; i < _listSelectables.Length; i++)
		{
			if (_listSelectables[i] == s)
			{
				_lastSelected = s;
				break;
			}
		}
	}

	public Menu[] GetSubMenus()
	{
		return _subMenus;
	}

	public MenuOption[] GetMenuOptions()
	{
		return _menuOptions;
	}

	protected virtual void Awake()
	{
		if (_menuActivationRoot == null)
		{
			_menuActivationRoot = base.gameObject;
		}
	}

	protected virtual void InitializeMenu()
	{
		_initialized = true;
		_menuCancelAction = GetComponent<MenuCancelAction>();
		GameObject gameObject = _selectableItemsRoot;
		if (gameObject == null)
		{
			gameObject = _menuActivationRoot;
		}
		if (_tooltipDisplay != null)
		{
			MenuOption[] componentsInChildren = gameObject.GetComponentsInChildren<MenuOption>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetTooltipDisplay(_tooltipDisplay);
			}
		}
		_listSelectables = gameObject.GetComponentsInChildren<Selectable>(includeInactive: true);
		for (int j = 0; j < _listSelectables.Length; j++)
		{
			_listSelectables[j].gameObject.AddComponent<MenuSelectHandler>().OnSelectableSelected += OnMenuItemSelected;
		}
		if (_startEnabled)
		{
			if (MenuStackManager.SharedInstance.GetMenuCount() != 0)
			{
				Debug.LogWarning("Enabling multiple menus on Awake, this will disable menus on the stack");
			}
			EnableMenu(value: true);
		}
	}

	public virtual bool IsMenuEnabled()
	{
		return _enabledMenu;
	}

	public virtual void EnableMenu(bool value)
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
				MenuStackManager.SharedInstance.Pop();
			}
			else
			{
				Debug.LogError("Cannot disable Menu unless it is on the top the MenuLayerManager stack. Current menu on top: " + MenuStackManager.SharedInstance.Peek().gameObject.name);
			}
		}
		else if (_enabledMenu)
		{
			Activate();
			bool flag = false;
			for (int i = 0; i < _subMenus.Length; i++)
			{
				if (_subMenus[i].IsMenuEnabled() && _subMenus[i].GetSelectOnActivate() != null)
				{
					flag = true;
					break;
				}
			}
			if (_selectOnActivate != null && !flag)
			{
				SelectableAudioPlayer component = _selectOnActivate.GetComponent<SelectableAudioPlayer>();
				if (component != null)
				{
					component.SilenceNextSelectEvent();
				}
				Locator.GetMenuInputModule().SelectOnNextUpdate(_selectOnActivate);
				_lastSelected = _selectOnActivate;
			}
		}
		else
		{
			Deactivate();
		}
	}

	public virtual void Activate()
	{
		_isActivated = true;
		_menuActivationRoot.gameObject.SetActive(value: true);
		if (_setMenuNavigationOnActivate)
		{
			SetVerticalNavigation(this, _menuOptions);
		}
		if (this.OnActivateMenu != null)
		{
			this.OnActivateMenu();
		}
	}

	public virtual void Deactivate(bool remainVisible = false)
	{
		_isActivated = false;
		if (!EventSystem.current.alreadySelecting && !Locator.GetMenuInputModule().IsPendingSelection() && (MenuStackManager.SharedInstance.GetMenuCount() <= 0 || !_addToMenuStackManager))
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		if (!remainVisible)
		{
			_menuActivationRoot.gameObject.SetActive(value: false);
		}
		if (this.OnDeactivateMenu != null)
		{
			this.OnDeactivateMenu();
		}
	}

	public virtual void OnCancelEvent(GameObject selectedObj, BaseEventData eventData)
	{
		if (_menuCancelAction != null)
		{
			_menuCancelAction.MenuCancel(selectedObj, eventData);
		}
	}

	public virtual void SetMenuAsForceClosed()
	{
		_enabledMenu = false;
		if (this.OnForceClosed != null)
		{
			this.OnForceClosed();
		}
	}
}
