using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SubmitEffect : MonoBehaviour, IEventSystemHandler, IPointerClickHandler, ISubmitHandler
{
	public void OnSubmit(BaseEventData eventData)
	{
		ActivateEffect();
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		ActivateEffect();
	}

	protected abstract void ActivateEffect();
}
