using System;
using UnityEngine;
using UnityEngine.UI;

public class TabbedMenu : Menu
{
	[Serializable]
	public struct TabSelectablePair
	{
		public TabButton tabButton;

		public Selectable selectable;
	}

	[SerializeField]
	protected TabButton[] _menuTabs;

	[SerializeField]
	protected TabSelectablePair[] _tabSelectablePairs;

	[SerializeField]
	protected Image _tabLeftButtonImg;

	[SerializeField]
	protected Image _tabRightButtonImg;

	protected TabButton _firstSelectedTabButton;

	protected TabButton _lastSelectedTabButton;

	protected bool _tabSelectablesInitialized;

	private Selectable _lastSelectableOnDeactivate;

	protected ScreenPrompt _tabLeftPrompt;

	protected ScreenPrompt _tabRightPrompt;

	protected IInputCommands _tabBackwardCmd;

	protected IInputCommands _tabForwardCmd;

	protected virtual void Initialize()
	{
		if (!_tabSelectablesInitialized)
		{
			MenuStackManager.SharedInstance.OnMenuPop += OnMenuPop;
			_tabBackwardCmd = InputLibrary.tabL;
			_tabForwardCmd = InputLibrary.tabR;
			bool flag = false;
			for (int i = 0; i < _tabSelectablePairs.Length; i++)
			{
				if (_tabSelectablePairs[i].tabButton.GetComponent<Selectable>() == _selectOnActivate)
				{
					flag = true;
					_firstSelectedTabButton = _tabSelectablePairs[i].tabButton;
				}
				_tabSelectablePairs[i].tabButton.OnTabSelect += OnTabButtonSelected;
			}
			if (!flag)
			{
				Debug.LogError("TabbedMenu first selected on Activate is not a TabButton!");
			}
			_tabSelectablesInitialized = true;
		}
		_tabLeftPrompt = new ScreenPrompt(_tabBackwardCmd, "");
		_tabRightPrompt = new ScreenPrompt(_tabForwardCmd, "");
		OnUpdateInputDevice();
		OWInput.SharedInputManager.OnUpdateInputDevice += OnUpdateInputDevice;
	}

	private void OnMenuPop(Menu poppedmenu)
	{
		if (poppedmenu == this)
		{
			_lastSelectableOnDeactivate = null;
		}
	}

	protected virtual void OnDestroy()
	{
		OWInput.SharedInputManager.OnUpdateInputDevice -= OnUpdateInputDevice;
		if (_tabSelectablesInitialized)
		{
			MenuStackManager.SharedInstance.OnMenuPop -= OnMenuPop;
		}
	}

	public override Selectable GetSelectOnActivate()
	{
		Initialize();
		return _selectOnActivate;
	}

	public override void Activate()
	{
		Initialize();
		TabButton tabButton = _firstSelectedTabButton;
		Selectable selectable = MenuStackManager.SharedInstance.PeekSelectOnActivate();
		if (_lastSelectableOnDeactivate != null)
		{
			Menu menu = null;
			for (int i = 0; i < _menuTabs.Length; i++)
			{
				MenuOption[] menuOptions = _menuTabs[i].GetMenu().GetMenuOptions();
				for (int j = 0; j < menuOptions.Length; j++)
				{
					if (menuOptions[j].GetSelectable() == _lastSelectableOnDeactivate)
					{
						tabButton = _menuTabs[i];
						menu = _menuTabs[i].GetMenu();
						selectable = _lastSelectableOnDeactivate;
						break;
					}
				}
				if (menu != null)
				{
					break;
				}
			}
		}
		if (selectable != _lastSelectableOnDeactivate)
		{
			for (int k = 0; k < _tabSelectablePairs.Length; k++)
			{
				if (selectable == _tabSelectablePairs[k].selectable)
				{
					tabButton = _tabSelectablePairs[k].tabButton;
					break;
				}
			}
		}
		SelectTabButton(tabButton);
		base.Activate();
		Locator.GetMenuInputModule().OnInputModuleTab += OnInputModuleTabEvent;
	}

	public override void Deactivate(bool keepPreviousMenuVisible = false)
	{
		_lastSelectableOnDeactivate = Locator.GetEventSystem().currentSelectedGameObject.GetComponent<Selectable>();
		for (int i = 0; i < _tabSelectablePairs.Length; i++)
		{
			_tabSelectablePairs[i].tabButton.Enable(value: false);
		}
		base.Deactivate(keepPreviousMenuVisible);
		Locator.GetMenuInputModule().OnInputModuleTab -= OnInputModuleTabEvent;
	}

	public TabButton GetLastSelectedTabButton()
	{
		return _lastSelectedTabButton;
	}

	public void TabForward()
	{
		if (_lastSelectedTabButton != null)
		{
			_lastSelectedTabButton.SelectAdjacentTab(1);
		}
	}

	public void TabBackward()
	{
		if (_lastSelectedTabButton != null)
		{
			_lastSelectedTabButton.SelectAdjacentTab(-1);
		}
	}

	protected virtual void OnInputModuleTabEvent(GameObject selectedObj, TabEventData eventData)
	{
		if (_lastSelectedTabButton != null)
		{
			_lastSelectedTabButton.SelectAdjacentTab(eventData.moveDirection);
		}
	}

	protected virtual void OnTabButtonSelected(TabButton tabButton)
	{
		SelectTabButton(tabButton);
	}

	protected virtual void SelectTabButton(TabButton tabButton)
	{
		if (_lastSelectedTabButton != null)
		{
			_lastSelectedTabButton.Enable(value: false);
		}
		_lastSelectedTabButton = tabButton;
		_lastSelectedTabButton.Enable(value: true);
	}

	protected virtual void OnUpdateInputDevice()
	{
		if (_tabLeftButtonImg == null || _tabRightButtonImg == null)
		{
			Debug.LogWarning("Serialized Field _tabLeftButtonImg/_tabRightButtonImg Not Set!");
			return;
		}
		if (OWInput.UsingGamepad())
		{
			_tabLeftButtonImg.gameObject.SetActive(value: true);
			_tabRightButtonImg.gameObject.SetActive(value: true);
			_tabLeftButtonImg.sprite = _tabLeftPrompt.GetSpriteList()[0];
			_tabRightButtonImg.sprite = _tabRightPrompt.GetSpriteList()[0];
			_tabLeftButtonImg.preserveAspect = true;
			_tabRightButtonImg.preserveAspect = true;
		}
		else
		{
			_tabLeftButtonImg.gameObject.SetActive(value: false);
			_tabRightButtonImg.gameObject.SetActive(value: false);
		}
		_tabLeftButtonImg.SetAllDirty();
		_tabRightButtonImg.SetAllDirty();
	}
}
