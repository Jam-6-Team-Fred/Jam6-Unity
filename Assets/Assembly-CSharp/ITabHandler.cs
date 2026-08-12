using UnityEngine.EventSystems;

public interface ITabHandler : IEventSystemHandler
{
	void OnTabEvent(TabEventData eventData);
}
