using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(JoystickListener))]
public class SubmitActionAccountPicker : SubmitAction, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	public delegate void AccountPickerSelectEvent();

	public delegate void AccountPickerDeselectEvent();

	public delegate void AccountPickerSubmitEvent();

	public event AccountPickerSelectEvent OnAccountPickerSelectEvent;

	public event AccountPickerDeselectEvent OnAccountPickerDeselectEvent;

	public event AccountPickerSubmitEvent OnAccountPickerSubmitEvent;

	public override void Submit()
	{
		if (this.OnAccountPickerSubmitEvent != null)
		{
			this.OnAccountPickerSubmitEvent();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (this.OnAccountPickerSelectEvent != null)
		{
			this.OnAccountPickerSelectEvent();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (this.OnAccountPickerDeselectEvent != null)
		{
			this.OnAccountPickerDeselectEvent();
		}
	}
}
