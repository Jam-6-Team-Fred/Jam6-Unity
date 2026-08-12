using UnityEngine;

public class DeathManager : MonoBehaviour
{
	private bool _isDying;

	private bool _isDead;

	private float _impactDeathSpeed;

	private bool _invincible;

	private DeathType _deathType;

	private DreamCampfire _nearbyDreamFire;

	private bool _fakeMeditationDeath;

	private bool _resurrectAfterDelay;

	private float _resurrectTime;

	private bool _escapedTimeLoop;

	private TimeloopEscapeType _timeloopEscapeType;

	private bool _escapedTimeLoopSequenceComplete;

	private bool _finishedDLC;

	private const float HELMET_CRACK_MIN_IMPACT_SPEED = 10f;

	public DeathType GetDeathType()
	{
		return _deathType;
	}

	public bool GetCrackHelmetOnDeath()
	{
		if (Locator.GetPlayerSuit().IsWearingHelmet())
		{
			if ((_deathType != DeathType.Impact || !(_impactDeathSpeed >= 10f)) && _deathType != DeathType.Crushed)
			{
				return _deathType == DeathType.CrushedByElevator;
			}
			return true;
		}
		return false;
	}

	public void ToggleInvincibility()
	{
		_invincible = !_invincible;
	}

	public bool IsPlayerDying()
	{
		return _isDying;
	}

	public bool IsPlayerDead()
	{
		return _isDead;
	}

	public bool IsFakeMeditationDeath()
	{
		return _fakeMeditationDeath;
	}

	public float GetImpactDeathSpeed()
	{
		return _impactDeathSpeed;
	}

	public void FinishedDLC()
	{
		_finishedDLC = true;
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void Update()
	{
		if (_resurrectAfterDelay && Time.time > _resurrectTime)
		{
			base.enabled = false;
			_resurrectAfterDelay = false;
			_isDying = false;
			if (!_fakeMeditationDeath)
			{
				GlobalMessenger.FireEvent("PlayerResurrection");
				Locator.GetPauseCommandListener().RemovePauseCommandLock();
			}
			else
			{
				Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OpenEyes(1f);
			}
			_fakeMeditationDeath = false;
			_nearbyDreamFire.WakeInDreamWorld();
		}
	}

	public void KillPlayer(DeathType deathType)
	{
		_fakeMeditationDeath = false;
		if (deathType == DeathType.Meditation && CheckShouldWakeInDreamWorld())
		{
			_fakeMeditationDeath = true;
			OWInput.ChangeInputMode(InputMode.None);
			ReticleController.Hide();
			Locator.GetPromptManager().SetPromptsVisible(visible: false);
			GlobalMessenger.FireEvent("FakePlayerMeditationDeath");
			return;
		}
		if (deathType == DeathType.DreamExplosion)
		{
			Achievements.Earn(Achievements.Type.EARLY_ADOPTER);
		}
		if (PlayerState.InDreamWorld() && deathType != DeathType.Dream && deathType != DeathType.DreamExplosion && deathType != DeathType.Supernova && deathType != DeathType.TimeLoop && deathType != DeathType.Meditation)
		{
			Locator.GetDreamWorldController().ExitDreamWorld(deathType);
		}
		else
		{
			if (_isDying || (_invincible && deathType != DeathType.Supernova && deathType != DeathType.BigBang && deathType != DeathType.Meditation && deathType != DeathType.TimeLoop && deathType != DeathType.BlackHole))
			{
				return;
			}
			if (!TimeLoopCoreController.ParadoxExists())
			{
				PlayerResources component = Locator.GetPlayerBody().GetComponent<PlayerResources>();
				if ((deathType == DeathType.TimeLoop || deathType == DeathType.Supernova) && component.GetTotalDamageThisLoop() > 1000f)
				{
					Achievements.Earn(Achievements.Type.DIEHARD);
					PlayerData.SetPersistentCondition("THERE_IS_BUT_VOID", state: true);
				}
				if (((TimeLoop.GetLoopCount() != 1 && TimeLoop.GetSecondsElapsed() < 60f) || (TimeLoop.GetLoopCount() == 1 && Time.timeSinceLevelLoad < 60f && !TimeLoop.IsTimeFlowing())) && deathType != DeathType.Meditation && LoadManager.GetCurrentScene() == OWScene.SolarSystem)
				{
					Achievements.Earn(Achievements.Type.GONE_IN_60_SECONDS);
				}
				if (TimeLoop.GetLoopCount() > 1)
				{
					Achievements.SetHeroStat(Achievements.HeroStat.TIMELOOP_COUNT, (uint)(TimeLoop.GetLoopCount() - 1));
					if (deathType == DeathType.TimeLoop || deathType == DeathType.BigBang || deathType == DeathType.Supernova)
					{
						PlayerData.CompletedFullTimeLoop();
					}
				}
				if (deathType == DeathType.Supernova && !PlayerData.GetPersistentCondition("KILLED_BY_SUPERNOVA_AND_KNOWS_IT") && PlayerData.GetFullTimeLoopsCompleted() > 2 && PlayerData.GetPersistentCondition("HAS_SEEN_SUN_EXPLODE"))
				{
					PlayerData.SetPersistentCondition("KILLED_BY_SUPERNOVA_AND_KNOWS_IT", state: true);
					MonoBehaviour.print("KILLED_BY_SUPERNOVA_AND_KNOWS_IT");
				}
			}
			_isDying = true;
			_deathType = deathType;
			MonoBehaviour.print("Player was killed by " + deathType);
			Locator.GetPauseCommandListener().AddPauseCommandLock();
			PlayerData.SetLastDeathType(deathType);
			GlobalMessenger<DeathType>.FireEvent("PlayerDeath", deathType);
		}
	}

	public void SetImpactDeathSpeed(float speed)
	{
		_impactDeathSpeed = speed;
		MonoBehaviour.print("Player impact death speed: " + speed);
	}

	public void FinishDeathSequence()
	{
		if (_isDead)
		{
			return;
		}
		if (CheckShouldWakeInDreamWorld())
		{
			base.enabled = true;
			_resurrectAfterDelay = true;
			_resurrectTime = Time.time + 2f;
			return;
		}
		_ = _fakeMeditationDeath;
		_isDead = true;
		GlobalMessenger.FireEvent("DeathSequenceComplete");
		if (PlayerData.GetPersistentCondition("DESTROYED_TIMELINE_LAST_SAVE"))
		{
			PlayerData.SetPersistentCondition("DESTROYED_TIMELINE_LAST_SAVE", state: false);
		}
		if (_deathType == DeathType.BigBang)
		{
			if (TimeLoopCoreController.ParadoxExists())
			{
				PlayerData.RevertParadoxLoopCountStates();
			}
			LoadManager.LoadScene(OWScene.Credits_Final, LoadManager.FadeType.ToWhite);
		}
		else if (TimeLoopCoreController.ParadoxExists())
		{
			Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.PARADOX_DEATH, null);
		}
		else if (TimeLoop.IsTimeFlowing() && TimeLoop.IsTimeLoopEnabled())
		{
			if (_finishedDLC)
			{
				GlobalMessenger.FireEvent("TriggerEndOfDLC");
			}
			else
			{
				GlobalMessenger.FireEvent("TriggerFlashback");
			}
		}
		else if (_deathType == DeathType.Meditation && PlayerState.OnQuantumMoon() && Locator.GetQuantumMoon().GetStateIndex() == 5)
		{
			_timeloopEscapeType = TimeloopEscapeType.Quantum;
			FinishEscapeTimeLoopSequence();
		}
		else
		{
			GlobalMessenger.FireEvent("TriggerDeathOutsideTimeLoop");
		}
	}

	public bool HasPlayerEscapedTimeLoop()
	{
		return _escapedTimeLoop;
	}

	public void BeginEscapedTimeLoopSequence(TimeloopEscapeType escapeType)
	{
		if (!_escapedTimeLoop)
		{
			_escapedTimeLoop = true;
			_timeloopEscapeType = escapeType;
			Locator.GetPromptManager().SetPromptsVisible(visible: false);
			ReticleController.Hide();
			GlobalMessenger.FireEvent("PlayerEscapedTimeLoop");
		}
	}

	public void FinishEscapeTimeLoopSequence()
	{
		if (!_escapedTimeLoopSequenceComplete)
		{
			_escapedTimeLoopSequenceComplete = true;
			if (_timeloopEscapeType == TimeloopEscapeType.Quantum)
			{
				GlobalMessenger.FireEvent("TriggerDeathByQuantumMoon");
			}
			else if (_timeloopEscapeType == TimeloopEscapeType.Ringworld)
			{
				GlobalMessenger.FireEvent("TriggerDeathByRingworldEscape");
			}
			else if (_timeloopEscapeType == TimeloopEscapeType.Dreamworld)
			{
				GlobalMessenger.FireEvent("TriggerDeathByDreamworldEscape");
			}
			else
			{
				GlobalMessenger.FireEvent("TriggerDeathByVoid");
			}
		}
	}

	public void SetNearbyDreamFire(DreamCampfire dreamCampfire)
	{
		_nearbyDreamFire = dreamCampfire;
	}

	private bool CheckShouldWakeInDreamWorld()
	{
		if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern && (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() as DreamLanternItem).GetLanternType() != DreamLanternType.Nonfunctioning && _nearbyDreamFire != null && _nearbyDreamFire.GetState() == Campfire.State.LIT && _deathType != DeathType.Supernova)
		{
			return _deathType != DeathType.TimeLoop;
		}
		return false;
	}
}
