using UnityEngine;

public abstract class GhostAction
{
	public enum Name
	{
		None = -1,
		Wait = 0,
		Sleep = 10,
		Sleepwalk = 20,
		PartyPath = 21,
		PartyHouse = 22,
		ElevatorWalk = 23,
		Sentry = 36,
		SearchForIntruder = 40,
		Guard = 42,
		IdentifyIntruder = 45,
		CallForHelp = 46,
		Chase = 50,
		Hunt = 51,
		Stalk = 52,
		Grab = 60
	}

	protected GhostData _data;

	protected GhostController _controller;

	protected GhostSensors _sensors;

	protected GhostEffects _effects;

	protected Transform _transform;

	protected bool _running;

	protected float _enterTime;

	public static GhostAction CreateAction(Name name)
	{
		GhostAction ghostAction;
		switch (name)
		{
		case Name.Wait:
			ghostAction = new WaitAction();
			break;
		case Name.Sleep:
			ghostAction = new SleepAction();
			break;
		case Name.Sleepwalk:
			ghostAction = new SleepwalkAction();
			break;
		case Name.PartyPath:
			ghostAction = new PartyPathAction();
			break;
		case Name.PartyHouse:
			ghostAction = new PartyHouseAction();
			break;
		case Name.ElevatorWalk:
			ghostAction = new ElevatorWalkAction();
			break;
		case Name.Sentry:
			ghostAction = new SentryAction();
			break;
		case Name.SearchForIntruder:
			ghostAction = new SearchAction();
			break;
		case Name.Guard:
			ghostAction = new GuardAction();
			break;
		case Name.IdentifyIntruder:
			ghostAction = new IdentifyIntruderAction();
			break;
		case Name.CallForHelp:
			ghostAction = new CallForHelpAction();
			break;
		case Name.Chase:
			ghostAction = new ChaseAction();
			break;
		case Name.Hunt:
			ghostAction = new HuntAction();
			break;
		case Name.Stalk:
			ghostAction = new StalkAction();
			break;
		case Name.Grab:
			ghostAction = new GrabAction();
			break;
		default:
			Debug.LogError("Failed to create action from name " + name);
			return null;
		}
		if (ghostAction.GetName() != name)
		{
			Debug.LogError(string.Concat("New action name ", ghostAction.GetName(), "does not match supplied name ", name));
			Debug.Break();
		}
		return ghostAction;
	}

	public virtual void Initialize(GhostData data, GhostController controller, GhostSensors sensors, GhostEffects effects)
	{
		_data = data;
		_controller = controller;
		_sensors = sensors;
		_effects = effects;
		_transform = _controller.transform;
	}

	public void EnterAction()
	{
		_running = true;
		_enterTime = Time.time;
		OnEnterAction();
	}

	public void ExitAction()
	{
		_running = false;
		OnExitAction();
	}

	public abstract Name GetName();

	public abstract float CalculateUtility();

	public abstract bool Update_Action();

	public virtual bool IsInterruptible()
	{
		return true;
	}

	public virtual float GetActionDelay()
	{
		return 0f;
	}

	public virtual void FixedUpdate_Action()
	{
	}

	public virtual void OnArriveAtPosition()
	{
	}

	public virtual void OnTraversePathNode(GhostNode node)
	{
	}

	public virtual void OnFaceNode(GhostNode node)
	{
	}

	public virtual void OnFinishFaceNodeList()
	{
	}

	public virtual void DrawGizmos(bool isGhostSelected)
	{
	}

	public virtual void OnSetAsPending()
	{
	}

	protected virtual void OnEnterAction()
	{
	}

	protected virtual void OnExitAction()
	{
	}

	protected float GetActionTimeElapsed()
	{
		if (!_running)
		{
			return -1f;
		}
		return Time.time - _enterTime;
	}
}
