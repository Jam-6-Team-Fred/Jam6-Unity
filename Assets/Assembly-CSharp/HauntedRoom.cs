using System.Collections.Generic;
using UnityEngine;

public class HauntedRoom : MonoBehaviour
{
	public delegate void RoomEvent(GameObject hitObj);

	[SerializeField]
	private GameObject _lightsRoot;

	private List<HauntedEntrance> _entrances;

	private List<HauntedRoom> _adjacent;

	private NomaiLamp[] _lights;

	private bool _playerInRoom;

	public event RoomEvent OnEntry;

	public event RoomEvent OnExit;

	private void OnValidate()
	{
	}

	private void Awake()
	{
		_adjacent = null;
		if (_lightsRoot == null)
		{
			_lightsRoot = base.gameObject;
		}
	}

	private void Start()
	{
		_lights = _lightsRoot.GetComponentsInChildren<NomaiLamp>();
		for (int i = 0; i < _lights.Length; i++)
		{
			for (int j = 0; j < _entrances.Count; j++)
			{
				_entrances[j].door.AddLightToCheckAgainst(_lights[i].GetComponentInChildren<OWLight2>());
			}
		}
	}

	public void RegisterEntrance(HauntedEntrance toAdd)
	{
		if (_entrances == null)
		{
			_entrances = new List<HauntedEntrance>();
		}
		_entrances.Add(toAdd);
	}

	public List<HauntedRoom> GetAdjacent()
	{
		if (_adjacent == null)
		{
			_adjacent = new List<HauntedRoom>();
			for (int i = 0; i < _entrances.Count; i++)
			{
				if (_entrances[i].GetConnectedRoom(this) != null)
				{
					_adjacent.Add(_entrances[i].GetConnectedRoom(this));
				}
			}
		}
		return _adjacent;
	}

	public GameObject GetLights()
	{
		return _lightsRoot;
	}

	public void SetLightsIntensity(float intensity)
	{
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].FadeTo(intensity, 0.01f);
		}
	}

	public void SetLightsFlickering(int flicker)
	{
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].FadeTo((flicker % _lights.Length == i) ? 1f : 0f, 0.01f);
		}
	}

	public bool HasPlayer()
	{
		return _playerInRoom;
	}

	public void AddObjectToRoom(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInRoom = true;
		}
		if (this.OnEntry != null)
		{
			this.OnEntry(hitObj);
		}
	}

	public void RemoveObjectFromRoom(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInRoom = false;
		}
		if (this.OnExit != null)
		{
			this.OnExit(hitObj);
		}
	}

	public bool HasOpenDoorTo(HauntedRoom room)
	{
		for (int i = 0; i < _entrances.Count; i++)
		{
			if (_entrances[i].GetConnectedRoom(this) == room && _entrances[i].door.IsOpen())
			{
				return true;
			}
		}
		return false;
	}

	public void CloseDoors(bool lockIt)
	{
		for (int i = 0; i < _entrances.Count; i++)
		{
			_entrances[i].door.CloseDoor(lockIt);
		}
	}
}
