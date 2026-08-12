using UnityEngine.EventSystems;

public static class OWExecuteEvents
{
	private static readonly ExecuteEvents.EventFunction<IPauseHandler> s_PauseHandler = Execute;

	public static ExecuteEvents.EventFunction<IPauseHandler> pauseHandler => s_PauseHandler;

	private static void Execute(IPauseHandler handler, BaseEventData eventData)
	{
		handler.OnPause(eventData);
	}
}
