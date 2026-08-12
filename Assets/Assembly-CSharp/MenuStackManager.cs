using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuStackManager
{
	public delegate void MenuPopEvent(Menu poppedMenu);

	public delegate void MenuPushEvent(Menu pushedMenu);

	private static MenuStackManager s_instance;

	private Stack<Menu> _menuStack;

	private Stack<Selectable> _lastSelectedStack;

	private OWMenuInputModule _inputModule;

	public static MenuStackManager SharedInstance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new MenuStackManager();
				s_instance.Initialize();
			}
			return s_instance;
		}
	}

	public event MenuPopEvent OnMenuPop;

	public event MenuPushEvent OnMenuPush;

	private void Initialize()
	{
		_inputModule = Locator.GetMenuInputModule();
		_lastSelectedStack = new Stack<Selectable>();
		_menuStack = new Stack<Menu>();
		LoadManager.OnStartSceneLoad += OnStartSceneLoad;
	}

	private void OnDestroy()
	{
		LoadManager.OnStartSceneLoad -= OnStartSceneLoad;
	}

	private void OnStartSceneLoad(OWScene originalScene, OWScene loadScene)
	{
		_lastSelectedStack.Clear();
		_menuStack.Clear();
	}

	public void ForceClearStack()
	{
		if (_inputModule == null)
		{
			_inputModule = Locator.GetMenuInputModule();
		}
		Menu menu = null;
		if (_menuStack.Count > 0)
		{
			menu = _menuStack.Pop();
			menu.Deactivate();
			_inputModule.OnInputModuleCancel -= menu.OnCancelEvent;
			menu.SetMenuAsForceClosed();
		}
		while (_menuStack.Count > 0)
		{
			menu = _menuStack.Pop();
			menu.Deactivate();
			menu.SetMenuAsForceClosed();
		}
		_lastSelectedStack.Clear();
	}

	public Menu Pop(bool playCloseAudio = true)
	{
		if (_inputModule == null)
		{
			_inputModule = Locator.GetMenuInputModule();
		}
		Menu menu = null;
		if (_menuStack.Count > 0)
		{
			menu = _menuStack.Pop();
			menu.Deactivate();
			_inputModule.OnInputModuleCancel -= menu.OnCancelEvent;
		}
		if (_menuStack.Count > 0)
		{
			Menu menu2 = _menuStack.Peek();
			menu2.Activate();
			_inputModule.OnInputModuleCancel += menu2.OnCancelEvent;
		}
		if (_lastSelectedStack.Count > 0)
		{
			Selectable selectable = _lastSelectedStack.Pop();
			if (selectable != null || (selectable == null && OWInput.UsingGamepad()))
			{
				if (selectable != null)
				{
					SelectableAudioPlayer component = selectable.GetComponent<SelectableAudioPlayer>();
					if (component != null)
					{
						component.SilenceNextSelectEvent();
					}
				}
				_inputModule.SelectOnNextUpdate(selectable);
			}
		}
		if (this.OnMenuPop != null)
		{
			this.OnMenuPop(menu);
		}
		if (playCloseAudio)
		{
			Locator.GetMenuAudioController().PlayClosePauseMenu();
		}
		return menu;
	}

	public void Push(Menu menu, bool keepPreviousMenuVisible = false, bool muteSoundEffect = false)
	{
		if (_inputModule == null)
		{
			_inputModule = Locator.GetMenuInputModule();
		}
		bool flag = _menuStack.Count == 0;
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			_lastSelectedStack.Push(EventSystem.current.currentSelectedGameObject.GetRequiredComponent<Selectable>());
		}
		else if (!flag)
		{
			_lastSelectedStack.Push(_menuStack.Peek().GetSelectOnActivate());
		}
		if (!flag)
		{
			Menu menu2 = _menuStack.Peek();
			menu2.Deactivate(keepPreviousMenuVisible);
			_inputModule.OnInputModuleCancel -= menu2.OnCancelEvent;
		}
		_menuStack.Push(menu);
		menu.Activate();
		Selectable selectOnActivate = menu.GetSelectOnActivate();
		if (selectOnActivate != null || (selectOnActivate == null && OWInput.UsingGamepad()))
		{
			if (selectOnActivate != null)
			{
				SelectableAudioPlayer component = selectOnActivate.GetComponent<SelectableAudioPlayer>();
				if (component != null)
				{
					component.SilenceNextSelectEvent();
				}
			}
			_inputModule.SelectOnNextUpdate(selectOnActivate);
		}
		_inputModule.OnInputModuleCancel += menu.OnCancelEvent;
		if (this.OnMenuPush != null)
		{
			this.OnMenuPush(menu);
		}
		if (!muteSoundEffect)
		{
			Locator.GetMenuAudioController().PlayOpenPauseMenu();
		}
	}

	public Menu Peek()
	{
		if (_menuStack.Count == 0)
		{
			return null;
		}
		return _menuStack.Peek();
	}

	public Selectable PeekSelectOnActivate()
	{
		if (_lastSelectedStack.Count == 0)
		{
			return null;
		}
		return _lastSelectedStack.Peek();
	}

	public int GetMenuCount()
	{
		return _menuStack.Count;
	}
}
