using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Menu))]
public class MenuCancelAction : MonoBehaviour
{
	public delegate void MenuCancelEvent(GameObject selectedObject, BaseEventData eventData);

	protected Menu _menu;

	public event MenuCancelEvent OnMenuCancel;

	public virtual void MenuCancel(GameObject selectedObject, BaseEventData eventData)
	{
		if (!eventData.used)
		{
			RaiseMenuCancelEvent(selectedObject, eventData);
			CloseMenu();
		}
	}

	protected void RaiseMenuCancelEvent(GameObject selectedObject, BaseEventData eventData)
	{
		if (this.OnMenuCancel != null)
		{
			this.OnMenuCancel(selectedObject, eventData);
		}
	}

	protected virtual void CloseMenu()
	{
		if (_menu == null)
		{
			_menu = this.GetRequiredComponent<Menu>();
		}
		if (_menu.IsMenuEnabled())
		{
			_menu.EnableMenu(value: false);
		}
	}
}
