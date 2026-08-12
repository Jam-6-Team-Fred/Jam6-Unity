using System.Collections.Generic;
using UnityEngine;

public class HauntingGhost : MonoBehaviour
{
	private enum GhostState
	{
		NONE = 0,
		KILLING = 1,
		LURING = 2,
		HUNTING = 3
	}

	[SerializeField]
	private HauntedRoom _startingLocation;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private LightCodeReader _lightCodeOpener;

	[Space]
	[SerializeField]
	private bool _seeInAdjacentRooms;

	[SerializeField]
	private HauntedCandle _candle;

	private HauntedRoom _currentLocation;

	private HauntedRoom _targetLocation;

	private HauntedRoom _previousLocation;

	private float _timeLastAction;

	private GhostState _state;

	private void Awake()
	{
		_startingLocation.OnEntry += AddObjectToCurrentRoom;
		_currentLocation = _startingLocation;
		_state = GhostState.NONE;
		if (_candle != null)
		{
			_candle.OnOldDreamCandleLit += new OWEvent.OWCallback(OnOldDreamCandleLit);
		}
	}

	private void Start()
	{
		_lightCodeOpener.ChangeLightsRoot(_startingLocation.GetLights());
		_targetLocation = null;
	}

	private void Destroy()
	{
		_currentLocation.OnEntry -= AddObjectToCurrentRoom;
		if (_candle != null)
		{
			_candle.OnOldDreamCandleLit -= new OWEvent.OWCallback(OnOldDreamCandleLit);
		}
	}

	private void OnOldDreamCandleLit()
	{
		if (!_candle.IsLit())
		{
			_state = GhostState.HUNTING;
		}
	}

	public void AddObjectToCurrentRoom(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_currentLocation.CloseDoors(lockIt: true);
			_timeLastAction = Time.time;
			_state = GhostState.KILLING;
			_lightCodeOpener.ChangeLightCode(LightCodeName.FAST, reverses: false);
			_lightCodeOpener.ChangePauseTime(0.01f);
		}
	}

	private void Move(HauntedRoom room)
	{
		if (_candle != null && _candle.p_room == room && !room.HasPlayer())
		{
			_candle.Lit(lit: true);
			_state = GhostState.NONE;
		}
		_previousLocation = _currentLocation;
		_currentLocation.OnEntry -= AddObjectToCurrentRoom;
		_currentLocation.SetLightsIntensity(0f);
		_currentLocation = room;
		_currentLocation.OnEntry += AddObjectToCurrentRoom;
		_lightCodeOpener.ChangeLightsRoot(_currentLocation.GetLights());
		if (_currentLocation.HasPlayer())
		{
			AddObjectToCurrentRoom(Locator.GetPlayerDetector());
		}
	}

	private void FixedUpdate()
	{
		switch (_state)
		{
		case GhostState.KILLING:
			if (Time.time - _timeLastAction > 5f)
			{
				if (_currentLocation.HasPlayer())
				{
					_oneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectTarget);
					Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
					base.enabled = false;
				}
				else if (_candle != null)
				{
					_state = GhostState.HUNTING;
					_targetLocation = null;
					_timeLastAction = Time.time;
				}
				else
				{
					_state = GhostState.LURING;
					_lightCodeOpener.ChangeLightCode(LightCodeName.WAKE, reverses: false);
				}
			}
			else
			{
				_currentLocation.SetLightsFlickering((int)(Time.time * 10f));
			}
			break;
		case GhostState.NONE:
		{
			if (!_seeInAdjacentRooms)
			{
				break;
			}
			List<HauntedRoom> adjacent2 = _currentLocation.GetAdjacent();
			for (int i = 0; i < adjacent2.Count; i++)
			{
				if (adjacent2[i].HasPlayer())
				{
					adjacent2[i].CloseDoors(lockIt: false);
					adjacent2[i].SetLightsIntensity(0f);
					_state = GhostState.LURING;
					_lightCodeOpener.ChangeLightCode(LightCodeName.WAKE, reverses: false);
				}
			}
			break;
		}
		case GhostState.HUNTING:
			if (_targetLocation == null)
			{
				List<HauntedRoom> adjacent = _currentLocation.GetAdjacent();
				adjacent.Remove(_previousLocation);
				_targetLocation = adjacent[Random.Range(0, adjacent.Count)];
				_timeLastAction = Time.time;
				_lightCodeOpener.ChangeLightCode(LightCodeName.WAKE, reverses: false);
			}
			else if (Time.time - _timeLastAction > 8f)
			{
				Debug.Log(_targetLocation.name);
				if (_targetLocation.HasOpenDoorTo(_currentLocation))
				{
					Move(_targetLocation);
					_targetLocation = null;
				}
				else
				{
					_targetLocation = null;
				}
			}
			break;
		case GhostState.LURING:
			break;
		}
	}
}
