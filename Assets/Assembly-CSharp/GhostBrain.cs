using System.Collections.Generic;
using UnityEngine;

public class GhostBrain : MonoBehaviour
{
	[SerializeField]
	private bool _debug;

	[SerializeField]
	private string _name = "Pazuzu";

	[SerializeField]
	private bool _startWithLanternConcealed;

	[SerializeField]
	private GhostNode.NodeLayer _nodeLayer = GhostNode.NodeLayer.Red;

	[SerializeField]
	private GhostAction.Name[] _actions;

	[Space]
	[SerializeField]
	private GhostEffects _effects;

	[Header("Call for help behavior")]
	[SerializeField]
	private GhostBrain[] _helperGhosts;

	[SerializeField]
	private OWTriggerVolume _guardVolume;

	[SerializeField]
	private Transform _chokePoint;

	[Header("Reduced Frights Overrides")]
	[SerializeField]
	private bool _reducedFrights_allowChase;

	private GhostController _controller;

	private GhostData _data;

	private GhostSensors _sensors;

	private bool _intruderConfirmedBySelf;

	private bool _intruderConfirmPending;

	private bool _playResponseAudio;

	private float _intruderConfirmTime;

	private List<GhostAction> _actionLibrary = new List<GhostAction>();

	private GhostAction _currentAction;

	private GhostAction _pendingAction;

	private float _pendingActionUtility;

	private float _pendingActionTimer;

	public OWEvent<GhostBrain, GhostData> OnIdentifyIntruder = new OWEvent<GhostBrain, GhostData>(4);

	public string ghostName => _name;

	public GhostNode.NodeLayer nodeLayer
	{
		get
		{
			return _nodeLayer;
		}
		set
		{
			_nodeLayer = value;
			_controller.SetNodeLayer(_nodeLayer);
		}
	}

	public GhostAction.Name GetCurrentActionName()
	{
		if (_currentAction == null)
		{
			return GhostAction.Name.None;
		}
		return _currentAction.GetName();
	}

	public GhostAction GetCurrentAction()
	{
		return _currentAction;
	}

	public GhostAction GetAction(GhostAction.Name actionName)
	{
		for (int i = 0; i < _actionLibrary.Count; i++)
		{
			if (_actionLibrary[i].GetName() == actionName)
			{
				return _actionLibrary[i];
			}
		}
		return null;
	}

	public GhostData.ThreatAwareness GetThreatAwareness()
	{
		return _data.threatAwareness;
	}

	public GhostEffects GetEffects()
	{
		return _effects;
	}

	public bool CheckDreadAudioConditions()
	{
		if (_currentAction == null)
		{
			return false;
		}
		if (_data.playerLocation.distance < 10f && _currentAction.GetName() != GhostAction.Name.Sentry)
		{
			return _currentAction.GetName() != GhostAction.Name.Grab;
		}
		return false;
	}

	public bool CheckFearAudioConditions(bool fearAudioAlreadyPlaying)
	{
		if (_currentAction == null)
		{
			return false;
		}
		if (fearAudioAlreadyPlaying)
		{
			if (_currentAction.GetName() != GhostAction.Name.Chase)
			{
				return _currentAction.GetName() == GhostAction.Name.Grab;
			}
			return true;
		}
		return _currentAction.GetName() == GhostAction.Name.Chase;
	}

	private void Awake()
	{
		_controller = GetComponent<GhostController>();
		_sensors = GetComponent<GhostSensors>();
		_data = new GhostData();
		_controller.OnArriveAtPosition += new OWEvent.OWCallback(OnArriveAtPosition);
		_controller.OnTraversePathNode += new OWEvent<GhostNode>.OWCallback(OnTraversePathNode);
		_controller.OnFaceNode += new OWEvent<GhostNode>.OWCallback(OnFaceNode);
		_controller.OnFinishFaceNodeList += new OWEvent.OWCallback(OnFinishFaceNodeList);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		base.enabled = false;
		_controller.GetDreamLanternController().enabled = false;
		_controller.Initialize(_nodeLayer, _effects);
		_sensors.Initialize(_data, _guardVolume);
		_effects.Initialize(_controller.GetNodeRoot(), _controller, _data);
		_effects.OnCallForHelp += new OWEvent.OWCallback(OnCallForHelp);
		_data.reducedFrights_allowChase = _reducedFrights_allowChase;
		_controller.SetLanternConcealed(_startWithLanternConcealed, playAudio: false);
		_intruderConfirmedBySelf = false;
		_intruderConfirmPending = false;
		_intruderConfirmTime = 0f;
		if (_chokePoint != null)
		{
			_data.hasChokePoint = true;
			_data.chokePointLocalPosition = _controller.WorldToLocalPosition(_chokePoint.position);
			_data.chokePointLocalFacing = _controller.WorldToLocalDirection(_chokePoint.forward);
		}
		for (int i = 0; i < _actions.Length; i++)
		{
			GhostAction ghostAction = GhostAction.CreateAction(_actions[i]);
			ghostAction.Initialize(_data, _controller, _sensors, _effects);
			_actionLibrary.Add(ghostAction);
		}
		ClearPendingAction();
	}

	private void OnDestroy()
	{
		_sensors.RemoveEventListeners();
		_controller.OnArriveAtPosition -= new OWEvent.OWCallback(OnArriveAtPosition);
		_controller.OnTraversePathNode -= new OWEvent<GhostNode>.OWCallback(OnTraversePathNode);
		_controller.OnFaceNode -= new OWEvent<GhostNode>.OWCallback(OnFaceNode);
		_controller.OnFinishFaceNodeList -= new OWEvent.OWCallback(OnFinishFaceNodeList);
		_effects.OnCallForHelp -= new OWEvent.OWCallback(OnCallForHelp);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	public void TabulaRasa()
	{
		_intruderConfirmedBySelf = false;
		_intruderConfirmPending = false;
		_intruderConfirmTime = 0f;
		_playResponseAudio = false;
		_data.TabulaRasa();
	}

	public void Die()
	{
		if (_data.isAlive)
		{
			_data.isAlive = false;
			_controller.StopMoving();
			_controller.StopFacing();
			_controller.ExtinguishLantern();
			_controller.GetCollider().GetComponent<OWCollider>().SetActivation(active: false);
			_controller.GetGrabController().ReleasePlayer();
			_pendingAction = null;
			_currentAction = null;
			_data.currentAction = GhostAction.Name.None;
			_effects.PlayDeathAnimation();
			_effects.PlayDeathEffects();
		}
	}

	public void EscalateThreatAwareness(GhostData.ThreatAwareness newThreatAwareness)
	{
		if (_data.threatAwareness >= newThreatAwareness)
		{
			return;
		}
		_data.threatAwareness = newThreatAwareness;
		if (_data.isAlive && _data.threatAwareness == GhostData.ThreatAwareness.IntruderConfirmed)
		{
			if (_intruderConfirmedBySelf)
			{
				_effects.PlayVoiceAudioFar(AudioType.Ghost_IntruderConfirmed);
			}
			else if (_playResponseAudio)
			{
				_effects.PlayVoiceAudioFar(AudioType.Ghost_IntruderConfirmedResponse);
				_playResponseAudio = false;
			}
		}
	}

	public void WakeUp()
	{
		_data.hasWokenUp = true;
	}

	public bool HearGhostCall(Vector3 playerLocalPosition, float reactDelay, bool playResponseAudio = false)
	{
		if (_data.isAlive && !_data.hasWokenUp)
		{
			return false;
		}
		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed && !_intruderConfirmPending)
		{
			_intruderConfirmedBySelf = false;
			_intruderConfirmPending = true;
			_intruderConfirmTime = Time.time + reactDelay;
			_playResponseAudio = playResponseAudio;
			return true;
		}
		return false;
	}

	public bool HearCallForHelp(Vector3 playerLocalPosition, float reactDelay)
	{
		if (_data.isAlive && !_data.hasWokenUp)
		{
			return false;
		}
		MonoBehaviour.print(_name + " responding to help call");
		if (_data.threatAwareness < GhostData.ThreatAwareness.IntruderConfirmed)
		{
			_data.threatAwareness = GhostData.ThreatAwareness.IntruderConfirmed;
			_intruderConfirmPending = false;
		}
		_effects.PlayRespondToHelpCallAudio(reactDelay);
		_data.reduceGuardUtility = true;
		_data.lastKnownPlayerLocation.UpdateLocalPosition(playerLocalPosition, _controller);
		_data.wasPlayerLocationKnown = true;
		_data.timeSincePlayerLocationKnown = 0f;
		return true;
	}

	public void HintPlayerLocation()
	{
		HintPlayerLocation(_data.playerLocation.localPosition, Time.time);
	}

	public void HintPlayerLocation(Vector3 localPosition, float informationTime)
	{
		if (_data.hasWokenUp && !_data.isPlayerLocationKnown && informationTime > _data.timeLastSawPlayer)
		{
			_data.lastKnownPlayerLocation.UpdateLocalPosition(localPosition, _controller);
			_data.wasPlayerLocationKnown = true;
			_data.timeSincePlayerLocationKnown = 0f;
		}
	}

	private void FixedUpdate()
	{
		if (base.enabled)
		{
			_controller.FixedUpdate_Controller();
			_sensors.FixedUpdate_Sensors();
			_data.FixedUpdate_Data(_controller, _sensors);
			FixedUpdate_ThreatAwareness();
			if (_currentAction != null)
			{
				_currentAction.FixedUpdate_Action();
			}
		}
	}

	private void Update()
	{
		if (base.enabled)
		{
			_controller.Update_Controller();
			_sensors.Update_Sensors();
			_effects.Update_Effects();
			bool flag = false;
			if (_currentAction != null)
			{
				flag = _currentAction.Update_Action();
			}
			if (!flag && _currentAction != null)
			{
				_currentAction.ExitAction();
				_data.previousAction = _currentAction.GetName();
				_currentAction = null;
				_data.currentAction = GhostAction.Name.None;
			}
			if (_data.isAlive && !Locator.GetDreamWorldController().IsExitingDream())
			{
				EvaluateActions();
			}
		}
	}

	private void FixedUpdate_ThreatAwareness()
	{
		if (_data.threatAwareness != GhostData.ThreatAwareness.IntruderConfirmed)
		{
			if (!_intruderConfirmPending && (_data.threatAwareness > GhostData.ThreatAwareness.EverythingIsNormal || _data.playerLocation.distance < 20f || _data.sensor.isPlayerIlluminatedByUs) && (_data.sensor.isPlayerVisible || _data.sensor.inContactWithPlayer))
			{
				_intruderConfirmedBySelf = true;
				_intruderConfirmPending = true;
				float num = Mathf.Lerp(0.1f, 1.5f, Mathf.InverseLerp(5f, 25f, _data.playerLocation.distance));
				_intruderConfirmTime = Time.time + num;
			}
			if (_intruderConfirmPending && Time.time > _intruderConfirmTime)
			{
				EscalateThreatAwareness(GhostData.ThreatAwareness.IntruderConfirmed);
				OnIdentifyIntruder.Invoke(this, _data);
			}
		}
	}

	private void EvaluateActions()
	{
		if (_currentAction != null && !_currentAction.IsInterruptible())
		{
			return;
		}
		float num = float.NegativeInfinity;
		GhostAction ghostAction = null;
		for (int i = 0; i < _actionLibrary.Count; i++)
		{
			float num2 = _actionLibrary[i].CalculateUtility();
			if (num2 > num)
			{
				num = num2;
				ghostAction = _actionLibrary[i];
			}
		}
		bool flag = false;
		if (_pendingAction == null || (ghostAction.GetName() != _pendingAction.GetName() && num > _pendingActionUtility))
		{
			_pendingAction = ghostAction;
			_pendingActionUtility = num;
			_pendingActionTimer = _pendingAction.GetActionDelay();
			flag = true;
		}
		if (_pendingAction != null && _currentAction != null && _pendingAction.GetName() == _currentAction.GetName())
		{
			ClearPendingAction();
			flag = false;
		}
		if (flag)
		{
			_pendingAction.OnSetAsPending();
		}
		if (_pendingAction != null && _pendingActionTimer <= 0f)
		{
			ChangeAction(_pendingAction);
		}
		if (_pendingAction != null)
		{
			_pendingActionTimer -= Time.deltaTime;
		}
	}

	private void ChangeAction(GhostAction.Name actionName)
	{
		for (int i = 0; i < _actionLibrary.Count; i++)
		{
			if (_actionLibrary[i].GetName() == actionName)
			{
				ChangeAction(_actionLibrary[i]);
				return;
			}
		}
		Debug.LogError("Failed to find action in library with name " + actionName);
		Debug.Break();
	}

	private void ChangeAction(GhostAction action)
	{
		if (_currentAction != null)
		{
			_currentAction.ExitAction();
			_data.previousAction = _currentAction.GetName();
		}
		else
		{
			_data.previousAction = GhostAction.Name.None;
		}
		_currentAction = action;
		_data.currentAction = action?.GetName() ?? GhostAction.Name.None;
		if (_currentAction != null)
		{
			_currentAction.EnterAction();
			_data.OnEnterAction(_currentAction.GetName());
		}
		ClearPendingAction();
	}

	private void ClearPendingAction()
	{
		_pendingAction = null;
		_pendingActionUtility = -100f;
		_pendingActionTimer = 0f;
	}

	private void OnArriveAtPosition()
	{
		if (_currentAction != null)
		{
			_currentAction.OnArriveAtPosition();
		}
	}

	private void OnTraversePathNode(GhostNode node)
	{
		if (_currentAction != null)
		{
			_currentAction.OnTraversePathNode(node);
		}
	}

	private void OnFaceNode(GhostNode node)
	{
		if (_currentAction != null)
		{
			_currentAction.OnFaceNode(node);
		}
	}

	private void OnFinishFaceNodeList()
	{
		if (_currentAction != null)
		{
			_currentAction.OnFinishFaceNodeList();
		}
	}

	private void OnCallForHelp()
	{
		if (_helperGhosts != null)
		{
			for (int i = 0; i < _helperGhosts.Length; i++)
			{
				_helperGhosts[i].HearCallForHelp(_data.playerLocation.localPosition, 3f);
			}
		}
	}

	private void OnEnterDreamWorld()
	{
		base.enabled = true;
		_controller.GetDreamLanternController().enabled = true;
	}

	private void OnExitDreamWorld()
	{
		base.enabled = false;
		_controller.GetDreamLanternController().enabled = false;
		ChangeAction(null);
		_data.OnPlayerExitDreamWorld();
	}

	private void OnDrawGizmos()
	{
	}
}
