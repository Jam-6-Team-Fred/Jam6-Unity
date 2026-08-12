using UnityEngine;

public class DreamCampfire : Campfire
{
	[Space]
	[SerializeField]
	private DreamArrivalPoint.Location _dreamArrivalLocation;

	[SerializeField]
	private OWTriggerVolume[] _entrywayVolumes;

	[SerializeField]
	private CustomCollisionChecker _collisionChecker;

	[SerializeField]
	private OWTriggerVolume _dreamOnDeathVolume;

	[SerializeField]
	private AlarmBell _alarmBell;

	[SerializeField]
	private OWFlameController _mummyCircleFlameController;

	[SerializeField]
	private OWLightController _houseLightController;

	public OWEvent OnDreamCampfireExtinguished = new OWEvent(1);

	private DreamArrivalPoint _arrivalPoint;

	private bool _shortenWakePromptDelay;

	protected override void Awake()
	{
		base.Awake();
		if (_collisionChecker != null)
		{
			_collisionChecker.OnEnterCustomCollider += new OWEvent.OWCallback(OnEnterCustomCollider);
		}
		if (_dreamOnDeathVolume != null)
		{
			_dreamOnDeathVolume.OnEntry += OnEnterDreamOnDeathVolume;
			_dreamOnDeathVolume.OnExit += OnExitDreamOnDeathVolume;
		}
		Locator.RegisterDreamCampfire(this, _dreamArrivalLocation);
	}

	protected override void Start()
	{
		base.Start();
		_shortenWakePromptDelay = PlayerData.GetPersistentCondition("HAS_DOZED_INTO_DREAM_WORLD");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_collisionChecker != null)
		{
			_collisionChecker.OnEnterCustomCollider -= new OWEvent.OWCallback(OnEnterCustomCollider);
		}
		if (_dreamOnDeathVolume != null)
		{
			_dreamOnDeathVolume.OnEntry -= OnEnterDreamOnDeathVolume;
			_dreamOnDeathVolume.OnExit -= OnExitDreamOnDeathVolume;
		}
		Locator.UnregisterDreamCampfire(this, _dreamArrivalLocation);
	}

	public DreamArrivalPoint.Location GetLocation()
	{
		return _dreamArrivalLocation;
	}

	public AlarmBell GetAlarmBell()
	{
		return _alarmBell;
	}

	public void WakeInDreamWorld()
	{
		if (_dreamArrivalLocation == DreamArrivalPoint.Location.Undefined)
		{
			Debug.LogWarning("Failed to enter dream world because target location is Undefined", this);
			return;
		}
		DreamArrivalPoint dreamArrivalPoint = Locator.GetDreamArrivalPoint(_dreamArrivalLocation);
		if (dreamArrivalPoint == null)
		{
			Debug.LogWarning(string.Concat("Failed to enter dream world because location ", _dreamArrivalLocation, " could not be found"), this);
			return;
		}
		RelativeLocationData relativeLocation = new RelativeLocationData(Locator.GetPlayerBody(), GetComponentInParent<OWRigidbody>(), base.transform);
		Locator.GetDreamWorldController().EnterDreamWorld(this, dreamArrivalPoint, relativeLocation);
		for (int i = 0; i < _entrywayVolumes.Length; i++)
		{
			_entrywayVolumes[i].RemoveObjectFromVolume(Locator.GetPlayerDetector().gameObject);
			_entrywayVolumes[i].RemoveObjectFromVolume(Locator.GetPlayerCameraDetector().gameObject);
		}
	}

	public void OnEnterDreamWorld()
	{
		SetInteractionEnabled(enabled: true);
	}

	public void OnExitDreamWorld()
	{
		for (int i = 0; i < _entrywayVolumes.Length; i++)
		{
			_entrywayVolumes[i].AddObjectToVolume(Locator.GetPlayerDetector().gameObject);
			_entrywayVolumes[i].AddObjectToVolume(Locator.GetPlayerCameraDetector().gameObject);
		}
	}

	protected override float GetWakePromptDelay()
	{
		if (!_shortenWakePromptDelay)
		{
			return base.GetWakePromptDelay();
		}
		return 1f;
	}

	protected override bool CheckUnequipToolWhileSleeping()
	{
		if (Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Item)
		{
			return Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern;
		}
		return true;
	}

	protected override bool CanSleepHereNow()
	{
		if (_state == State.LIT && OWInput.IsInputMode(InputMode.Character))
		{
			return TimeLoop.GetSecondsRemaining() > -30f;
		}
		return false;
	}

	protected override bool ShouldWakeUp()
	{
		if (!OWInput.IsInputMode(InputMode.None) || (!OWInput.IsNewlyPressed(InputLibrary.interact) && !OWInput.IsNewlyPressed(InputLibrary.cancel) && !OWInput.IsNewlyPressed(InputLibrary.interactSecondary)))
		{
			return TimeLoop.GetSecondsRemaining() < -30f;
		}
		return true;
	}

	protected override void OnStopSleeping()
	{
		if (_state == State.LIT && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern && ((DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem()).GetLanternType() != DreamLanternType.Nonfunctioning)
		{
			WakeInDreamWorld();
			SetInteractionEnabled(enabled: false);
			if (!PlayerData.GetPersistentCondition("HAS_DOZED_INTO_DREAM_WORLD"))
			{
				PlayerData.SetPersistentCondition("HAS_DOZED_INTO_DREAM_WORLD", state: true);
			}
		}
		else
		{
			GlobalMessenger.FireEvent("StopSleepingAtDreamCampfire");
		}
	}

	private void OnEnterDreamOnDeathVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetDeathManager().SetNearbyDreamFire(this);
		}
	}

	private void OnExitDreamOnDeathVolume(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			Locator.GetDeathManager().SetNearbyDreamFire(null);
		}
	}

	private void OnEnterCustomCollider()
	{
		SetState(State.UNLIT);
		StopSleeping(sudden: true);
		_oneShotAudio.PlayOneShot(AudioType.DreamFire_Extinguish);
		SetInteractionEnabled(enabled: false);
		_collisionChecker.OnEnterCustomCollider -= new OWEvent.OWCallback(OnEnterCustomCollider);
		if (_mummyCircleFlameController != null)
		{
			_mummyCircleFlameController.FadeTo(0f, 0.2f);
		}
		if (_houseLightController != null)
		{
			_houseLightController.FadeTo(0f, 0.2f);
		}
		OnDreamCampfireExtinguished.Invoke();
	}
}
