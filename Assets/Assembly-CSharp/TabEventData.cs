using UnityEngine.EventSystems;

public class TabEventData : BaseEventData
{
	private int _moveDirection;

	public int moveDirection
	{
		get
		{
			return _moveDirection;
		}
		set
		{
			_moveDirection = value;
		}
	}

	public TabEventData(EventSystem eventSystem, int tabDirection)
		: base(eventSystem)
	{
		_moveDirection = tabDirection;
	}
}
