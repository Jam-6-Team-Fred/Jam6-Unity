using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class TabbedNavigation : MonoBehaviour
{
	protected Selectable _selectable;

	[SerializeField]
	protected bool _enableOnlyWhenOnKeyboard;

	[SerializeField]
	protected Selectable _tabForwardSelectable;

	[SerializeField]
	protected Selectable _tabBackwardSelectable;

	private void Awake()
	{
		_selectable = this.GetRequiredComponent<Selectable>();
		Locator.GetMenuInputModule().OnInputModuleTab += OnInputModuleTabEvent;
	}

	private void OnDestroy()
	{
		if (Locator.GetMenuInputModule() != null)
		{
			Locator.GetMenuInputModule().OnInputModuleTab -= OnInputModuleTabEvent;
		}
	}

	protected virtual void OnInputModuleTabEvent(GameObject selectedObj, TabEventData eventData)
	{
		if (_selectable.gameObject.activeInHierarchy && _selectable.gameObject == selectedObj)
		{
			if (eventData.moveDirection == 1)
			{
				TabForward();
			}
			if (eventData.moveDirection == -1)
			{
				TabBackward();
			}
		}
	}

	protected virtual void TabForward()
	{
		if (_tabForwardSelectable != null)
		{
			_tabForwardSelectable.Select();
		}
	}

	protected virtual void TabBackward()
	{
		if (_tabBackwardSelectable != null)
		{
			_tabBackwardSelectable.Select();
		}
	}
}
