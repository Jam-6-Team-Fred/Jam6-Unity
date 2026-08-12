using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectEffect : MonoBehaviour, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	public abstract void OnSelect(BaseEventData eventData);

	public abstract void OnDeselect(BaseEventData eventData);
}
