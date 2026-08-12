using UnityEngine.EventSystems;

public static class OWUIEvents
{
	public static ExecuteEvents.EventFunction<ITabHandler> tabEventHandler => Execute;

	private static void Execute(ITabHandler handler, BaseEventData eventData)
	{
		handler.OnTabEvent(ExecuteEvents.ValidateEventData<TabEventData>(eventData));
	}
}
