using UnityEngine;
using UnityEngine.EventSystems;

public class SubmitAction : MonoBehaviour, IEventSystemHandler, IPointerClickHandler, ISubmitHandler
{
	public delegate void SubmitActionEvent();

	public event SubmitActionEvent OnSubmitAction;

	public virtual void OnSubmit(BaseEventData eventData)
	{
		if (!eventData.used)
		{
			Submit();
		}
	}

	public virtual void OnPointerClick(PointerEventData pointerEventData)
	{
		if (!pointerEventData.used)
		{
			Submit();
		}
	}

	public virtual void Submit()
	{
		if (this.OnSubmitAction != null)
		{
			this.OnSubmitAction();
		}
	}
}
