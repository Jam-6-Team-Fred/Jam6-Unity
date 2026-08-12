using UnityEngine;

public class HauntedEntrance : MonoBehaviour
{
	[SerializeField]
	private EntrywayTrigger _trigger;

	[SerializeField]
	public LightCodeDoor door;

	[SerializeField]
	protected HauntedRoom _roomEntered;

	protected virtual void Awake()
	{
		_trigger.OnEntry += AddObjectToRoom;
		_trigger.OnExit += RemoveObjectFromRoom;
		_trigger.Register();
		_roomEntered.RegisterEntrance(this);
	}

	public virtual void AddObjectToRoom(GameObject hitObj)
	{
		_roomEntered.AddObjectToRoom(hitObj);
	}

	public virtual void RemoveObjectFromRoom(GameObject hitObj)
	{
		_roomEntered.RemoveObjectFromRoom(hitObj);
	}

	public virtual HauntedRoom GetConnectedRoom(HauntedRoom origin)
	{
		if (origin == null)
		{
			return _roomEntered;
		}
		return null;
	}
}
