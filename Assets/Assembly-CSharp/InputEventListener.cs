using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class InputEventListener : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public delegate void BaseInputEvent(BaseEventData eventData, Selectable selectable);

	public delegate void PointerInputEvent(PointerEventData eventData, Selectable selectable);

	private Selectable _selectableComponent;

	public event BaseInputEvent OnSelectEvent;

	public event BaseInputEvent OnDeselectEvent;

	public event PointerInputEvent OnPointerEnterEvent;

	public event PointerInputEvent OnPointerExitEvent;

	public event PointerInputEvent OnPointerUpEvent;

	public event PointerInputEvent OnPointerDownEvent;

	protected virtual void Awake()
	{
		_selectableComponent = this.GetRequiredComponent<Selectable>();
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (this.OnSelectEvent != null)
		{
			this.OnSelectEvent(eventData, _selectableComponent);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (this.OnDeselectEvent != null)
		{
			this.OnDeselectEvent(eventData, _selectableComponent);
		}
	}

	public virtual void OnPointerEnter(PointerEventData pointerEventData)
	{
		if (this.OnPointerEnterEvent != null)
		{
			this.OnPointerEnterEvent(pointerEventData, _selectableComponent);
		}
	}

	public virtual void OnPointerExit(PointerEventData pointerEventData)
	{
		if (this.OnPointerExitEvent != null)
		{
			this.OnPointerExitEvent(pointerEventData, _selectableComponent);
		}
	}

	public virtual void OnPointerUp(PointerEventData pointerEventData)
	{
		if (this.OnPointerUpEvent != null)
		{
			this.OnPointerUpEvent(pointerEventData, _selectableComponent);
		}
	}

	public virtual void OnPointerDown(PointerEventData pointerEventData)
	{
		if (this.OnPointerDownEvent != null)
		{
			this.OnPointerDownEvent(pointerEventData, _selectableComponent);
		}
	}
}
