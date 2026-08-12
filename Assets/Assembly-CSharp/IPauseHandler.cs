using UnityEngine.EventSystems;

public interface IPauseHandler : IEventSystemHandler
{
	void OnPause(BaseEventData eventData);
}
