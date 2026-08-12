using UnityEngine;

public class HauntedConnection : HauntedEntrance
{
	[SerializeField]
	private HauntedRoom _roomLeft;

	protected override void Awake()
	{
		base.Awake();
		_roomLeft.RegisterEntrance(this);
	}

	public override void AddObjectToRoom(GameObject hitObj)
	{
		base.AddObjectToRoom(hitObj);
		_roomLeft.RemoveObjectFromRoom(hitObj);
	}

	public override void RemoveObjectFromRoom(GameObject hitObj)
	{
		base.RemoveObjectFromRoom(hitObj);
		_roomLeft.AddObjectToRoom(hitObj);
	}

	public override HauntedRoom GetConnectedRoom(HauntedRoom origin)
	{
		if (origin == _roomLeft)
		{
			return _roomEntered;
		}
		return _roomLeft;
	}
}
