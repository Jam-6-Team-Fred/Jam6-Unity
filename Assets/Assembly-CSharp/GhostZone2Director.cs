using System;
using UnityEngine;

public class GhostZone2Director : GhostDirector
{
	[Serializable]
	public struct ElevatorPair
	{
		public CageElevator elevator;

		public GhostNodeMap nodeMap;

		public GhostBrain ghost;

		public bool cityDestination;
	}

	private struct ElevatorStatus
	{
		public ElevatorPair elevatorPair;

		public bool activated;

		public bool lightsDeactivated;

		public bool deactivated;

		public float timeSinceArrival;

		public ElevatorWalkAction elevatorAction;

		public GhostController ghostController;
	}

	[Space]
	[SerializeField]
	private DreamObjectProjector _lightsProjector;

	[SerializeField]
	private DreamObjectProjector _raftProjector;

	[SerializeField]
	private OWAudioSource _ghostHowlAudioSource;

	[SerializeField]
	private OWTriggerVolume _undergroundVolume;

	[SerializeField]
	private OWTriggerVolume _raftDockVolume;

	[SerializeField]
	private OWTriggerVolume _upperTowerEscapeVolume;

	[Space]
	[SerializeField]
	private GhostBrain[] _cityGhosts = new GhostBrain[0];

	[SerializeField]
	private GhostBrain[] _undergroundGhosts = new GhostBrain[0];

	[SerializeField]
	private ElevatorPair[] _elevators = new ElevatorPair[0];

	[SerializeField]
	private GhostNodeMap _cityNodeMap;

	[SerializeField]
	private GhostNodeMap _undercityNodeMap;

	[SerializeField]
	private CageElevator _ghostTutorialElevator;

	[SerializeField]
	private OWTriggerVolume _ghostTutorialArrival;

	[SerializeField]
	private AlarmTotem _finalTotem;

	[SerializeField]
	private Transform _teleportNode;

	[SerializeField]
	private Transform _cityGhostTeleportNode;

	private bool _lightsProjectorExtinguished;

	private bool _ghostsAlerted;

	private bool _playerIdentifiedInCity;

	private bool _playerIdentifiedOnRaftDock;

	private float _ghostAlertTime;

	private ElevatorStatus[] _elevatorsStatus;

	protected override void Awake()
	{
		base.Awake();
		_lightsProjector.OnProjectorExtinguished += new OWEvent.OWCallback(OnLightsExtinguished);
		_undergroundVolume.OnEntry += OnEnterUnderground;
		_undergroundVolume.OnExit += OnExitUnderground;
		_raftDockVolume.OnEntry += OnEnterRaftZone;
		_raftDockVolume.OnExit += OnExitRaftZone;
		_upperTowerEscapeVolume.OnEntry += OnEscapingToTower;
		GlobalMessenger.AddListener("ExitDreamWorld", OnLeavingDreamworld);
		_finalTotem.OnRinging += OnAlarmRinging;
		for (int i = 0; i < _cityGhosts.Length; i++)
		{
			_cityGhosts[i].OnIdentifyIntruder += new OWEvent<GhostBrain, GhostData>.OWCallback(OnCityGhostsIdentifiedIntruder);
		}
		_ghostTutorialArrival.OnEntry += OnStartGhostTutorial;
		_elevatorsStatus = new ElevatorStatus[_elevators.Length];
		for (int j = 0; j < _elevators.Length; j++)
		{
			_elevatorsStatus[j].elevatorPair = _elevators[j];
			_elevatorsStatus[j].activated = false;
			_elevatorsStatus[j].deactivated = false;
			_elevatorsStatus[j].lightsDeactivated = false;
		}
	}

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < _cityGhosts.Length; i++)
		{
			_cityGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
		}
		for (int j = 0; j < _undergroundGhosts.Length; j++)
		{
			_undergroundGhosts[j].EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_lightsProjector.OnProjectorExtinguished -= new OWEvent.OWCallback(OnLightsExtinguished);
		_undergroundVolume.OnEntry -= OnEnterUnderground;
		_undergroundVolume.OnExit -= OnExitUnderground;
		_finalTotem.OnRinging -= OnAlarmRinging;
		_upperTowerEscapeVolume.OnEntry -= OnEscapingToTower;
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnLeavingDreamworld);
		for (int i = 0; i < _cityGhosts.Length; i++)
		{
			_cityGhosts[i].OnIdentifyIntruder -= new OWEvent<GhostBrain, GhostData>.OWCallback(OnCityGhostsIdentifiedIntruder);
		}
		_ghostTutorialArrival.OnEntry -= OnStartGhostTutorial;
	}

	private void Update()
	{
		if (!_lightsProjectorExtinguished)
		{
			return;
		}
		if (_ghostsAreAwake && !_ghostsAlerted && Time.time >= _ghostAlertTime)
		{
			_ghostHowlAudioSource.PlayOneShot(AudioType.Ghost_SomeoneIsInHereHowl);
			_ghostsAlerted = true;
		}
		if (_playerIdentifiedOnRaftDock && (_cityGhosts[1].GetCurrentActionName() == GhostAction.Name.Stalk || _cityGhosts[1].GetCurrentActionName() == GhostAction.Name.Chase))
		{
			_raftProjector.SetLit(lit: false);
		}
		for (int i = 0; i < _elevatorsStatus.Length; i++)
		{
			if (!_elevatorsStatus[i].activated && _elevatorsStatus[i].elevatorAction.reachedEndOfPath)
			{
				_elevatorsStatus[i].ghostController.SetNodeMap(_elevatorsStatus[i].elevatorPair.nodeMap);
				_elevatorsStatus[i].elevatorPair.elevator.topLight.FadeTo(1f, 0.2f);
				_elevatorsStatus[i].elevatorPair.elevator.GoToDestination(0);
				_elevatorsStatus[i].activated = true;
			}
			if (!_elevatorsStatus[i].lightsDeactivated && _elevatorsStatus[i].activated && _elevatorsStatus[i].elevatorPair.elevator.isAtBottom)
			{
				_elevatorsStatus[i].lightsDeactivated = true;
				_elevatorsStatus[i].elevatorPair.elevator.topLight.FadeTo(0f, 0.2f);
				if (_elevatorsStatus[i].elevatorPair.cityDestination)
				{
					_elevatorsStatus[i].ghostController.SetNodeMap(_cityNodeMap);
				}
				else
				{
					_elevatorsStatus[i].ghostController.SetNodeMap(_undercityNodeMap);
				}
				if (i == 1)
				{
					_elevatorsStatus[i].ghostController.gameObject.GetComponent<Transform>().position = _teleportNode.position;
				}
				_elevatorsStatus[i].elevatorAction.UseElevator();
				_elevatorsStatus[i].timeSinceArrival = Time.time;
			}
			if (_elevatorsStatus[i].lightsDeactivated && _elevatorsStatus[i].activated && !_elevatorsStatus[i].deactivated && Time.time >= _elevatorsStatus[i].timeSinceArrival + 2f)
			{
				_elevatorsStatus[i].elevatorPair.elevator.GoToDestination(1);
				_elevatorsStatus[i].deactivated = true;
			}
		}
	}

	private void OnLightsExtinguished()
	{
		_lightsProjectorExtinguished = true;
		WakeGhosts();
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			_directedGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.SomeoneIsInHere);
			_directedGhosts[i].GetEffects().CancelStompyFootsteps();
		}
		for (int j = 0; j < _elevatorsStatus.Length; j++)
		{
			_elevatorsStatus[j].elevatorPair.elevator.topLight.FadeTo(0f, 0.2f);
			_elevatorsStatus[j].elevatorAction = _elevators[j].ghost.GetAction(GhostAction.Name.ElevatorWalk) as ElevatorWalkAction;
			_elevatorsStatus[j].elevatorAction.CallToUseElevator();
			_elevatorsStatus[j].ghostController = _elevatorsStatus[j].elevatorPair.ghost.GetComponent<GhostController>();
		}
		_ghostAlertTime = Time.time + 2f;
	}

	private void OnEnterUnderground(GameObject hitObj)
	{
		if (_lightsProjectorExtinguished && hitObj.CompareTag("PlayerDetector"))
		{
			for (int i = 0; i < _undergroundGhosts.Length; i++)
			{
				_undergroundGhosts[i].HearGhostCall(Vector3.one, 0f);
			}
		}
	}

	private void OnExitUnderground(GameObject hitObj)
	{
		if (_lightsProjectorExtinguished)
		{
			hitObj.CompareTag("PlayerDetector");
		}
	}

	private void OnEnterRaftZone(GameObject hitObj)
	{
		if (_lightsProjectorExtinguished && hitObj.CompareTag("PlayerDetector"))
		{
			_playerIdentifiedOnRaftDock = true;
		}
	}

	private void OnExitRaftZone(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerIdentifiedOnRaftDock = false;
		}
	}

	private void OnAlarmRinging()
	{
		for (int i = 0; i < _undergroundGhosts.Length; i++)
		{
			_undergroundGhosts[i].HintPlayerLocation(_finalTotem.gameObject.GetComponent<Transform>().position, 0f);
		}
	}

	private void OnEscapingToTower(GameObject hitObj)
	{
		if (_lightsProjectorExtinguished && hitObj.CompareTag("PlayerDetector") && (_cityGhosts[1].GetCurrentActionName() == GhostAction.Name.Stalk || _cityGhosts[1].GetCurrentActionName() == GhostAction.Name.Chase))
		{
			_cityGhosts[1].gameObject.GetComponent<Transform>().position = _cityGhostTeleportNode.position;
			_cityGhosts[1].nodeLayer = GhostNode.NodeLayer.Purple;
			_cityGhosts[1].TabulaRasa();
			_cityGhosts[1].HearGhostCall(_cityGhostTeleportNode.position, 0f);
		}
	}

	private void OnLeavingDreamworld()
	{
		if (_lightsProjectorExtinguished && _cityGhosts[1].nodeLayer == GhostNode.NodeLayer.Purple)
		{
			_cityGhosts[1].gameObject.GetComponent<Transform>().position = _cityGhostTeleportNode.position;
			_cityGhosts[1].nodeLayer = GhostNode.NodeLayer.Orange;
			_cityGhosts[1].TabulaRasa();
			_cityGhosts[1].HearGhostCall(_cityGhostTeleportNode.position, 0f);
		}
	}

	private void OnStartGhostTutorial(GameObject hitObj)
	{
		if (_lightsProjectorExtinguished && hitObj.CompareTag("PlayerDetector") && !_ghostTutorialElevator.isAtTop)
		{
			_ghostTutorialElevator.GoToDestination(1);
			for (int i = 0; i < _cityGhosts.Length; i++)
			{
				_cityGhosts[i].EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
			}
		}
	}

	private void OnCityGhostsIdentifiedIntruder(GhostBrain ghostBrain, GhostData ghostData)
	{
		if (_playerIdentifiedInCity)
		{
			return;
		}
		_playerIdentifiedInCity = true;
		float num = UnityEngine.Random.Range(2f, 3f);
		for (int i = 0; i < _cityGhosts.Length; i++)
		{
			if (!(_cityGhosts[i] == ghostBrain) && _cityGhosts[i].HearGhostCall(ghostData.playerLocation.localPosition, num))
			{
				num += UnityEngine.Random.Range(2f, 3f);
			}
		}
	}
}
