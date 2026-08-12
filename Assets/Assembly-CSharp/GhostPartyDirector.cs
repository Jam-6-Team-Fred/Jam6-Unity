using System.Collections.Generic;
using GhostEnums;
using UnityEngine;

public class GhostPartyDirector : GhostDirector
{
	[Space]
	[SerializeField]
	private OWTriggerVolume _doorGhostTrigger;

	[SerializeField]
	private OWTriggerVolume _doorTrapTrigger;

	[SerializeField]
	private AbstractDoor _entryDoor;

	[Space]
	[SerializeField]
	private OWTriggerVolume _ambushTrigger;

	[SerializeField]
	private float _initialAmbushDelay = 10f;

	[SerializeField]
	private float _secondaryAmbushDelay = 5f;

	[SerializeField]
	private GhostBrain _fireplaceGhost;

	[SerializeField]
	private GhostBrain[] _ambushGhosts = new GhostBrain[0];

	[Space]
	[SerializeField]
	private Sector _audioSector;

	[SerializeField]
	private PartyMusicController _partyMusicController;

	[Space]
	[SerializeField]
	private GameObject _shipLogFactObject;

	private List<GhostBrain> _ghostsWaitingToAmbush;

	private int _partyHouseDoorGhostCount;

	private bool _doorTriggered;

	private bool _ambushTriggeredThisLoop;

	private bool _ambushTriggered;

	private bool _waitingToAmbushInitial;

	private bool _waitingToAmbushSecondary;

	private float _ambushTriggerTime;

	private bool _connectedCampfireExtinguished;

	protected override void Awake()
	{
		base.Awake();
		_ghostsWaitingToAmbush = new List<GhostBrain>(_ambushGhosts);
		_doorGhostTrigger.OnEntry += OnEnterPartyHouseDoorTrigger;
		_doorGhostTrigger.OnExit += OnExitPartyHouseDoorTrigger;
		_doorTrapTrigger.OnEntry += OnEnterDoorTrigger;
		_ambushTrigger.OnEntry += OnEnterAmbushTrigger;
		_audioSector.OnOccupantEnterSector += new OWEvent<SectorDetector>.OWCallback(OnEnterSector);
		_audioSector.OnOccupantExitSector += new OWEvent<SectorDetector>.OWCallback(OnExitSector);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_doorGhostTrigger.OnEntry -= OnEnterPartyHouseDoorTrigger;
		_doorGhostTrigger.OnExit -= OnExitPartyHouseDoorTrigger;
		_doorTrapTrigger.OnEntry -= OnEnterDoorTrigger;
		_ambushTrigger.OnEntry -= OnEnterAmbushTrigger;
		_audioSector.OnOccupantEnterSector -= new OWEvent<SectorDetector>.OWCallback(OnEnterSector);
		_audioSector.OnOccupantExitSector -= new OWEvent<SectorDetector>.OWCallback(OnExitSector);
	}

	private void Update()
	{
		if (!_connectedCampfireExtinguished && _ambushTriggered && Time.time > _ambushTriggerTime)
		{
			if (_waitingToAmbushInitial)
			{
				_waitingToAmbushInitial = false;
				UnlockGhostForAmbush(firstAmbush: true);
				_waitingToAmbushSecondary = true;
				_ambushTriggerTime = Time.time + _secondaryAmbushDelay;
			}
			else if (_waitingToAmbushSecondary)
			{
				_waitingToAmbushSecondary = false;
				UnlockGhostForAmbush(firstAmbush: false);
			}
		}
	}

	private void UnlockGhostForAmbush(bool firstAmbush)
	{
		if (_ghostsWaitingToAmbush.Count != 0)
		{
			int index = Random.Range(0, _ghostsWaitingToAmbush.Count);
			(_ghostsWaitingToAmbush[index].GetAction(GhostAction.Name.PartyHouse) as PartyHouseAction).AllowChasePlayer();
			_ghostsWaitingToAmbush[index].HintPlayerLocation();
			if (firstAmbush)
			{
				_ghostsWaitingToAmbush[index].GetEffects().PlayVoiceAudioNear(AudioType.Ghost_Stalk);
			}
			_ghostsWaitingToAmbush.QuickRemoveAt(index);
		}
	}

	protected override void OnConnectedCampfireExtinguished()
	{
		base.OnConnectedCampfireExtinguished();
		_connectedCampfireExtinguished = true;
		_doorGhostTrigger.SetTriggerActivation(active: false);
		_doorTrapTrigger.SetTriggerActivation(active: false);
		_ambushTrigger.SetTriggerActivation(active: false);
		_partyMusicController.FadeOut(1f);
		_shipLogFactObject.SetActive(value: false);
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_entryDoor.Open();
		}
		else
		{
			_entryDoor.SetOpenImmediate(open: true);
		}
	}

	private void OnEnterPartyHouseDoorTrigger(GameObject hitObj)
	{
		if (!_connectedCampfireExtinguished && hitObj.CompareTag("GhostDetector"))
		{
			if (!_entryDoor.IsOpen())
			{
				_entryDoor.Open();
			}
			_partyHouseDoorGhostCount++;
		}
	}

	private void OnExitPartyHouseDoorTrigger(GameObject hitObj)
	{
		if (!_connectedCampfireExtinguished && hitObj.CompareTag("GhostDetector"))
		{
			_partyHouseDoorGhostCount--;
			if (_partyHouseDoorGhostCount <= 0 && _doorTriggered)
			{
				_entryDoor.Close();
			}
		}
	}

	private void OnEnterDoorTrigger(GameObject hitObj)
	{
		if (!_doorTriggered && hitObj.CompareTag("PlayerDetector"))
		{
			_doorTriggered = true;
			_partyMusicController.StaggerStop();
			_entryDoor.Close();
			for (int i = 0; i < _directedGhosts.Length; i++)
			{
				_directedGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
			}
		}
	}

	private void OnEnterAmbushTrigger(GameObject hitObj)
	{
		if (!_ambushTriggered && hitObj.CompareTag("PlayerDetector"))
		{
			_ambushTriggeredThisLoop = true;
			_ambushTriggered = true;
			_waitingToAmbushInitial = true;
			_ambushTriggerTime = Time.time + (_ambushTriggeredThisLoop ? _secondaryAmbushDelay : _initialAmbushDelay);
			(_fireplaceGhost.GetAction(GhostAction.Name.PartyHouse) as PartyHouseAction).LookAtPlayer(0f, TurnSpeed.MEDIUM);
			for (int i = 0; i < _ambushGhosts.Length; i++)
			{
				float delay = i;
				(_ambushGhosts[i].GetAction(GhostAction.Name.PartyHouse) as PartyHouseAction).LookAtPlayer(delay);
			}
		}
	}

	private void OnEnterSector(SectorDetector detector)
	{
		if ((!(_connectedDreamCampfire != null) || _connectedDreamCampfire.GetState() != 0) && detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_partyMusicController.FadeIn(3f);
			_ghostsWaitingToAmbush.Clear();
			_ghostsWaitingToAmbush.AddRange(_ambushGhosts);
			for (int i = 0; i < _directedGhosts.Length; i++)
			{
				(_directedGhosts[i].GetAction(GhostAction.Name.PartyHouse) as PartyHouseAction).ResetAllowChasePlayer();
			}
		}
	}

	private void OnExitSector(SectorDetector detector)
	{
		if ((!(_connectedDreamCampfire != null) || _connectedDreamCampfire.GetState() != 0) && detector.GetOccupantType() == DynamicOccupant.Player)
		{
			_doorTriggered = false;
			_ambushTriggered = false;
			_waitingToAmbushInitial = false;
			_waitingToAmbushSecondary = false;
			_ambushTriggerTime = 0f;
			_partyMusicController.FadeOut(3f);
			_entryDoor.SetOpenImmediate(open: true);
		}
	}
}
