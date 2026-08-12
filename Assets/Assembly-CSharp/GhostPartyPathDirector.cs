using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostPartyPathDirector : GhostDirector
{
	[Serializable]
	private struct GhostSpawnLocation
	{
		public Transform spawnTransform;

		public AbstractDoor spawnDoor;

		[NonSerialized]
		public float spawnDoorTimer;
	}

	[Serializable]
	private struct GhostFinalDestination
	{
		public Transform destinationTransform;

		public ProxyGhostController proxyGhost;
	}

	[Space]
	[SerializeField]
	private GhostSpawnLocation[] _ghostSpawns = new GhostSpawnLocation[0];

	[SerializeField]
	private float _minGhostDispatchDelay = 10f;

	[SerializeField]
	private float _maxGhostDispatchDelay = 90f;

	[SerializeField]
	private GhostFinalDestination[] _ghostFinalDestinations = new GhostFinalDestination[0];

	[SerializeField]
	private Transform[] _ghostOverflowFinalDestinations = new Transform[0];

	[SerializeField]
	private OWTriggerVolume _respawnBlockTrigger;

	private List<GhostBrain> _waitingGhosts;

	private List<GhostBrain> _dispatchedGhosts;

	private GhostBrain _lastDispatchedGhost;

	private float _nextGhostDispatchTime;

	private int _numArrivedGhosts;

	private int _numEnabledGhostProxies;

	private bool _disableGhostProxies;

	private bool _connectedCampfireExtinguished;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < _ghostSpawns.Length; i++)
		{
			_ghostSpawns[i].spawnDoorTimer = 0f;
		}
		for (int j = 0; j < _directedGhosts.Length; j++)
		{
			_directedGhosts[j].OnIdentifyIntruder += new OWEvent<GhostBrain, GhostData>.OWCallback(OnGhostIdentifyIntruder);
		}
		_waitingGhosts = new List<GhostBrain>(_directedGhosts);
		_dispatchedGhosts = new List<GhostBrain>(_directedGhosts.Length);
		_lastDispatchedGhost = null;
		_nextGhostDispatchTime = 0f;
		_numArrivedGhosts = 0;
		_numEnabledGhostProxies = 0;
		_connectedCampfireExtinguished = false;
		SecretSettings.TryGetBool("DisablePartygoerProxies", out _disableGhostProxies);
	}

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < _ghostFinalDestinations.Length; i++)
		{
			if (_ghostFinalDestinations[i].proxyGhost != null)
			{
				_ghostFinalDestinations[i].proxyGhost.Hide();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			_directedGhosts[i].OnIdentifyIntruder -= new OWEvent<GhostBrain, GhostData>.OWCallback(OnGhostIdentifyIntruder);
		}
	}

	private void Update()
	{
		if (_connectedCampfireExtinguished)
		{
			return;
		}
		for (int num = _dispatchedGhosts.Count - 1; num >= 0; num--)
		{
			GhostBrain ghostBrain = _dispatchedGhosts[num];
			if (ghostBrain.GetCurrentActionName() == GhostAction.Name.PartyPath)
			{
				PartyPathAction partyPathAction = ghostBrain.GetCurrentAction() as PartyPathAction;
				if (partyPathAction.hasReachedEndOfPath)
				{
					if (!partyPathAction.isMovingToFinalPosition)
					{
						Transform transform = ((_numArrivedGhosts >= _ghostFinalDestinations.Length) ? _ghostOverflowFinalDestinations[_numArrivedGhosts % _ghostOverflowFinalDestinations.Length].transform : _ghostFinalDestinations[_numArrivedGhosts].destinationTransform);
						partyPathAction.MoveToFinalPosition(transform.position);
						_numArrivedGhosts++;
					}
					if (!_respawnBlockTrigger.IsTrackingObject(Locator.GetPlayerDetector()))
					{
						_dispatchedGhosts.QuickRemoveAt(num);
						ghostBrain.transform.position = _ghostSpawns[UnityEngine.Random.Range(0, _ghostSpawns.Length)].spawnTransform.position;
						ghostBrain.transform.eulerAngles = Vector3.up * _ghostSpawns[UnityEngine.Random.Range(0, _ghostSpawns.Length)].spawnTransform.eulerAngles.y;
						ghostBrain.TabulaRasa();
						partyPathAction.ResetPath();
						if (!_disableGhostProxies && _numEnabledGhostProxies < _ghostFinalDestinations.Length)
						{
							if (_ghostFinalDestinations[_numEnabledGhostProxies].proxyGhost != null)
							{
								_ghostFinalDestinations[_numEnabledGhostProxies].proxyGhost.Reveal();
							}
							_numEnabledGhostProxies++;
						}
						_waitingGhosts.Add(ghostBrain);
					}
				}
			}
		}
		if (_waitingGhosts.Count > 0 && _waitingGhosts[0].GetCurrentActionName() == GhostAction.Name.PartyPath && (_dispatchedGhosts.Count == 0 || Time.timeSinceLevelLoad > _nextGhostDispatchTime))
		{
			GhostBrain ghostBrain2 = _waitingGhosts[0];
			int num2 = UnityEngine.Random.Range(0, _ghostSpawns.Length);
			ghostBrain2.transform.position = _ghostSpawns[num2].spawnTransform.position;
			ghostBrain2.transform.eulerAngles = Vector3.up * _ghostSpawns[num2].spawnTransform.eulerAngles.y;
			(ghostBrain2.GetCurrentAction() as PartyPathAction).StartFollowPath();
			_ghostSpawns[num2].spawnDoor.Open();
			_ghostSpawns[num2].spawnDoorTimer = Time.timeSinceLevelLoad + 4f;
			_waitingGhosts.RemoveAt(0);
			_lastDispatchedGhost = ghostBrain2;
			_dispatchedGhosts.Add(ghostBrain2);
			_nextGhostDispatchTime = Time.timeSinceLevelLoad + UnityEngine.Random.Range(_minGhostDispatchDelay, _maxGhostDispatchDelay);
		}
		for (int i = 0; i < _ghostSpawns.Length; i++)
		{
			if (_ghostSpawns[i].spawnDoor.IsOpen() && Time.timeSinceLevelLoad > _ghostSpawns[i].spawnDoorTimer)
			{
				_ghostSpawns[i].spawnDoor.Close();
			}
		}
	}

	protected override void OnConnectedCampfireExtinguished()
	{
		base.OnConnectedCampfireExtinguished();
		_connectedCampfireExtinguished = true;
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			for (int i = 0; i < _ghostSpawns.Length; i++)
			{
				_ghostSpawns[i].spawnDoor.Open();
			}
		}
		else
		{
			for (int j = 0; j < _ghostSpawns.Length; j++)
			{
				_ghostSpawns[j].spawnDoor.SetOpenImmediate(open: true);
			}
		}
		for (int k = 0; k < _numEnabledGhostProxies; k++)
		{
			if (_ghostFinalDestinations[k].proxyGhost != null)
			{
				_ghostFinalDestinations[k].proxyGhost.Die();
			}
		}
	}

	private void OnGhostIdentifyIntruder(GhostBrain ghostBrain, GhostData ghostData)
	{
		float num = UnityEngine.Random.Range(2f, 3f);
		for (int i = 0; i < _directedGhosts.Length; i++)
		{
			if (!(_directedGhosts[i] == ghostBrain))
			{
				bool flag = _directedGhosts[i].GetCurrentActionName() != GhostAction.Name.PartyPath || ((PartyPathAction)_directedGhosts[i].GetCurrentAction()).allowHearGhostCall;
				float num2 = Vector3.Distance(ghostBrain.transform.position, _directedGhosts[i].transform.position);
				if (flag && num2 < 50f && _directedGhosts[i].HearGhostCall(ghostData.playerLocation.localPosition, num, playResponseAudio: true))
				{
					_directedGhosts[i].HintPlayerLocation();
					num += UnityEngine.Random.Range(2f, 3f);
					MonoBehaviour.print(base.gameObject.name + " called to " + _directedGhosts[i].gameObject.name + "   Distance: " + num2 + "   Allowed: " + flag.ToString());
				}
			}
		}
	}
}
