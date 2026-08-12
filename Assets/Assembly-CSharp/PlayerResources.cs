using System.Text;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
	public delegate void InstantDamageEvent(float instantDamage, InstantDamageType damageType);

	public delegate void RecoveredHealthEvent(float currentHealth);

	private static float _maxHealth = 100f;

	private static float _lowHeath = 40f;

	private static float _criticalHealth = 15f;

	private static float _maxOxygen = 450f;

	private static float _lowOxygen = 180f;

	private static float _criticalOxygen = 60f;

	private static float _suffocationDuration = 4f;

	private static float _maxFuel = 100f;

	private static float _lowFuel = 50f;

	private static float _criticalFuel = 15f;

	private static float _oxygenAsFuelConsumptionRate = 2f;

	private static float _suitlessMinImpactSpeed = 13f;

	private static float _suitlessMaxImpactSpeed = 20f;

	private static float _minImpactSpeed = 20f;

	private static float _maxImpactSpeed = 40f;

	private static float _undertowMinImpactSpeed = 10f;

	private static float _undertowMaxImpactSpeed = 30f;

	private static float _suitlessInstantDamageMultiplier = 2f;

	private static int _maxSuitPunctures = 5;

	private static float _puncturePatchTime = 1f;

	private static float _oxygenDrainOnPuncture = 10f;

	private static float _oxygenDrainOverTimePerPuncture = 3f;

	[SerializeField]
	private PunctureController[] _punctureEffects = new PunctureController[0];

	private StringBuilder _repairDetailsStringBuilder;

	private float _currentFuel;

	private float _currentHealth;

	private float _currentOxygen;

	private float _oxygenUsedAsFuel;

	private float _currentSuitIntegrity;

	private float _cachedHealth;

	private float _totalDamageSufferedThisLoop;

	private float _startSuffocationTime;

	private int _currentNumPunctures;

	private float _puncturePatchStartTime;

	private float _punctureDamageTime;

	private bool _isWearingTrainingSuit;

	private bool _isHealing;

	private bool _isRefueling;

	private bool _refillingOxygen;

	private bool _canOxygenDrain;

	private bool _isSuffocating;

	private bool _invincible;

	private bool _killingResources;

	private bool _usingOxygenAsPropellant;

	private OWRigidbody _owRigidbody;

	private FluidDetector _fluidDetector;

	private FluidDetector _cameraFluidDetector;

	private HazardDetector _hazardDetector;

	private OxygenDetector _oxygenDetector;

	private ImpactSensor _impactSensor;

	private JetpackThrusterModel _jetpackThruster;

	private PlayerAudioController _playerAudioController;

	private ThrusterFlameColorSwapper _jetpackFlameColorSwapper;

	private ScreenPrompt _patchSuitPrompt;

	private ScreenPrompt _patchSuitPromptDetails;

	private NotificationData _oxygenAsFuelNotification;

	private NotificationData _refuellingAndHealingNotification;

	public event InstantDamageEvent OnInstantDamage;

	public event RecoveredHealthEvent OnRecoverHealth;

	private void Awake()
	{
		_currentFuel = _maxFuel;
		_currentHealth = _maxHealth;
		_currentOxygen = _maxOxygen;
		_currentNumPunctures = 0;
		_puncturePatchStartTime = 0f;
		_oxygenUsedAsFuel = 0f;
		_oxygenAsFuelNotification = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationFuelDepleted), 3f, playSound: false);
		_refuellingAndHealingNotification = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationRefuel), 1f);
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
		_jetpackThruster = this.GetRequiredComponent<JetpackThrusterModel>();
		_impactSensor = this.GetRequiredComponent<ImpactSensor>();
		_jetpackFlameColorSwapper = GetComponentInChildren<ThrusterFlameColorSwapper>();
		GlobalMessenger<float>.AddListener("EatMarshmallow", OnEatMarshmallow);
		GlobalMessenger.AddListener("SuitUp", OnSuitUp);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.AddListener("StopSleepingAtDreamCampfire", OnStopSleepingAtDreamCampfire);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
		_repairDetailsStringBuilder = new StringBuilder(128);
	}

	private void Start()
	{
		_cameraFluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();
		_fluidDetector = Locator.GetPlayerDetector().GetRequiredComponent<FluidDetector>();
		_oxygenDetector = Locator.GetPlayerDetector().GetRequiredComponent<OxygenDetector>();
		_hazardDetector = Locator.GetPlayerDetector().GetRequiredComponent<HazardDetector>();
		_playerAudioController = Locator.GetPlayerTransform().GetRequiredComponentInChildren<PlayerAudioController>();
		_hazardDetector.OnFirstContactDamage += OnFirstContactDamage;
		_impactSensor.OnImpact += OnImpact;
		_patchSuitPrompt = new ScreenPrompt(InputLibrary.interact, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.PatchSuitPrompt));
		_patchSuitPromptDetails = new ScreenPrompt("");
		Locator.GetPromptManager().AddScreenPrompt(_patchSuitPrompt, PromptPosition.Center);
		Locator.GetPromptManager().AddScreenPrompt(_patchSuitPromptDetails, PromptPosition.Center);
		_patchSuitPrompt.SetVisibility(isVisible: false);
	}

	private void OnDestroy()
	{
		_hazardDetector.OnFirstContactDamage -= OnFirstContactDamage;
		_impactSensor.OnImpact -= OnImpact;
		GlobalMessenger<float>.RemoveListener("EatMarshmallow", OnEatMarshmallow);
		GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
		GlobalMessenger.RemoveListener("StopSleepingAtDreamCampfire", OnStopSleepingAtDreamCampfire);
		GlobalMessenger.RemoveListener("PutOnHelmet", OnPutOnHelmet);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
	}

	public HazardDetector GetHazardDetector()
	{
		return _hazardDetector;
	}

	public float GetLowHealth()
	{
		return _lowHeath;
	}

	public float GetCriticalHealth()
	{
		return _criticalHealth;
	}

	public float GetHealth()
	{
		return _currentHealth;
	}

	public float GetHealthFraction()
	{
		return _currentHealth / _maxHealth;
	}

	public float GetOxygenInSeconds()
	{
		return _currentOxygen;
	}

	public float GetLowOxygenInSeconds()
	{
		return _lowOxygen;
	}

	public float GetCriticalOxygenInSeconds()
	{
		return _criticalOxygen;
	}

	public float GetOxygenFraction()
	{
		return _currentOxygen / _maxOxygen;
	}

	public float GetFuel()
	{
		return _currentFuel;
	}

	public float GetLowFuel()
	{
		return _lowFuel;
	}

	public float GetCriticalFuel()
	{
		return _criticalFuel;
	}

	public float GetFuelFraction()
	{
		return _currentFuel / _maxFuel;
	}

	public float GetBoostChargeFraction()
	{
		return _jetpackThruster.GetBoostChargeFraction();
	}

	public float GetBoostMaxThrust()
	{
		return _jetpackThruster.GetBoostMaxThrust();
	}

	public float GetMinImpactSpeed()
	{
		if (Locator.GetPlayerRulesetDetector().GetImpactRuleset() != null)
		{
			return Locator.GetPlayerRulesetDetector().GetImpactRuleset().minImpactSpeed;
		}
		if (PlayerState.InUndertowVolume())
		{
			return _undertowMinImpactSpeed;
		}
		if (!Locator.GetPlayerSuit().IsWearingSuit())
		{
			return _suitlessMinImpactSpeed;
		}
		return _minImpactSpeed;
	}

	public float GetMaxImpactSpeed()
	{
		if (Locator.GetPlayerRulesetDetector().GetImpactRuleset() != null)
		{
			return Locator.GetPlayerRulesetDetector().GetImpactRuleset().maxImpactSpeed;
		}
		if (PlayerState.InUndertowVolume())
		{
			return _undertowMaxImpactSpeed;
		}
		if (!Locator.GetPlayerSuit().IsWearingSuit())
		{
			return _suitlessMaxImpactSpeed;
		}
		return _maxImpactSpeed;
	}

	public float GetTotalDamageThisLoop()
	{
		return _totalDamageSufferedThisLoop;
	}

	public bool IsSuffocating()
	{
		return _isSuffocating;
	}

	public bool IsOxygenPresent()
	{
		if (_oxygenDetector.GetDetectOxygen())
		{
			return !IsUnderwater();
		}
		return false;
	}

	public bool IsUnderwater()
	{
		return _cameraFluidDetector.InFluidType(FluidVolume.Type.WATER);
	}

	public bool IsJetpackUsable()
	{
		if (_currentFuel > 0f || _currentOxygen > 0f || _fluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			return Time.time - _punctureDamageTime > 1f;
		}
		return false;
	}

	public bool IsRefueling()
	{
		return _isRefueling;
	}

	public bool IsRefillingOxygen()
	{
		return _refillingOxygen;
	}

	public bool IsHealing()
	{
		return _isHealing;
	}

	public bool IsSuitPunctured()
	{
		return _currentNumPunctures > 0;
	}

	public bool IsDoneRepairingSuitPuncture()
	{
		return Time.time >= _puncturePatchStartTime + _puncturePatchTime;
	}

	public bool IsOxygenDrainBlockedByDreamWorld()
	{
		if (PlayerState.InDreamWorld())
		{
			return Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern;
		}
		return false;
	}

	public bool IsBoosterAllowed()
	{
		if (!PlayerState.InZeroG() && !Locator.GetPlayerSuit().IsTrainingSuit() && !_cameraFluidDetector.InFluidType(FluidVolume.Type.WATER))
		{
			return _currentFuel > 0f;
		}
		return false;
	}

	public bool IsBoosterFiring()
	{
		return _jetpackThruster.IsBoosterFiring();
	}

	public bool IsInvincible()
	{
		return _invincible;
	}

	public void ToggleInvincibility()
	{
		_invincible = !_invincible;
	}

	public void SetDebugKillResources(bool killResources)
	{
		_killingResources = killResources;
	}

	public void DebugRefillResources()
	{
		_currentHealth = _maxHealth;
		_currentOxygen = _maxOxygen;
		_currentFuel = _maxFuel;
		_killingResources = false;
		_jetpackThruster.DebugResetBoostCharge();
		PatchAllPunctures();
	}

	public void StartRefillResources(bool fuel, bool health, bool dlcFuelTank = false)
	{
		_isRefueling = fuel;
		_isHealing = health;
		string text = string.Empty;
		if (_isRefueling && _maxFuel != _currentFuel)
		{
			text = UITextLibrary.GetString(UITextType.NotificationRefuel);
		}
		if (_isHealing)
		{
			PatchAllPunctures();
			if (_maxHealth != _currentHealth)
			{
				text = ((!(text == string.Empty)) ? UITextLibrary.GetString(UITextType.NotificationHealAndRefuel) : UITextLibrary.GetString(UITextType.NotificationHeal));
			}
		}
		_refuellingAndHealingNotification.displayMessage = text;
		if (!NotificationManager.SharedInstance.IsPinnedNotification(_refuellingAndHealingNotification))
		{
			NotificationManager.SharedInstance.PostNotification(_refuellingAndHealingNotification, pin: true);
		}
	}

	public void StopRefillResources()
	{
		_isRefueling = false;
		_isHealing = false;
		NotificationManager.SharedInstance.UnpinNotification(_refuellingAndHealingNotification);
	}

	public bool ApplyInstantDamage(float damage, InstantDamageType type)
	{
		if (damage > 0f && !_invincible && !PlayerState.IsInsideTheEye())
		{
			if (PlayerState.InDreamWorld() && PlayerState.IsCameraUnderwater())
			{
				return false;
			}
			if (!Locator.GetPlayerSuit().IsWearingSuit() && type != 0)
			{
				damage *= _suitlessInstantDamageMultiplier;
			}
			if (Locator.GetPlayerSuit().IsWearingSuit() || type != 0 || damage >= _maxHealth)
			{
				if (!PlayerState.InDreamWorld())
				{
					_totalDamageSufferedThisLoop += damage;
				}
				_currentHealth -= damage;
			}
			if (this.OnInstantDamage != null)
			{
				this.OnInstantDamage(damage, type);
			}
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (_killingResources)
		{
			DebugKillResources();
			return;
		}
		UpdateFuel();
		UpdateOxygen();
		UpdateHealth();
	}

	private void UpdateFuel()
	{
		float magnitude = _jetpackThruster.GetLocalAcceleration().magnitude;
		if (_isRefueling)
		{
			_currentFuel += _maxFuel * Time.deltaTime;
			if (_currentFuel >= _maxFuel)
			{
				_currentFuel = _maxFuel;
			}
			if (_currentFuel > 0f && _usingOxygenAsPropellant)
			{
				_usingOxygenAsPropellant = false;
			}
		}
		else if (magnitude > 0f && (!_fluidDetector.InFluidType(FluidVolume.Type.WATER) || _jetpackThruster.IsBoosterFiring()))
		{
			float num = Mathf.Min(magnitude, _jetpackThruster.GetMaxTranslationalThrust() * 2f) * 0.1f;
			if (_invincible || PlayerState.IsInsideTheEye())
			{
				num = 0f;
			}
			_currentFuel -= num * Time.deltaTime;
			float b = (Locator.GetPlayerSuit().IsTrainingSuit() ? 10 : 0);
			_currentFuel = Mathf.Max(_currentFuel, b);
			if (_currentFuel == 0f)
			{
				if (!_usingOxygenAsPropellant)
				{
					NotificationManager.SharedInstance.PostNotification(_oxygenAsFuelNotification, pin: true);
					_playerAudioController.PlaySuitWarning();
					_usingOxygenAsPropellant = true;
				}
				if (!_oxygenDetector.GetDetectOxygen())
				{
					float num2 = magnitude * _oxygenAsFuelConsumptionRate * Time.deltaTime * ((_invincible || PlayerState.IsInsideTheEye()) ? 0f : 1f);
					_oxygenUsedAsFuel += num2;
					if (_oxygenUsedAsFuel > 250f)
					{
						Achievements.Earn(Achievements.Type.CUTTING_IT_CLOSE);
						_oxygenUsedAsFuel = 0f;
					}
					_currentOxygen -= num2;
				}
			}
			else
			{
				_usingOxygenAsPropellant = false;
			}
		}
		if (!_usingOxygenAsPropellant && NotificationManager.SharedInstance.IsPinnedNotification(_oxygenAsFuelNotification))
		{
			NotificationManager.SharedInstance.UnpinNotification(_oxygenAsFuelNotification);
		}
	}

	private void UpdateOxygen()
	{
		bool flag = IsOxygenPresent();
		if (_isSuffocating)
		{
			if (flag || _currentOxygen > 0f)
			{
				_isSuffocating = false;
			}
			else if (Time.time > _startSuffocationTime + _suffocationDuration)
			{
				Locator.GetDeathManager().KillPlayer(DeathType.Asphyxiation);
				base.enabled = false;
			}
		}
		else if (flag)
		{
			_canOxygenDrain = true;
			if (!_refillingOxygen && _currentOxygen < _maxOxygen)
			{
				_refillingOxygen = true;
				if (_currentOxygen < _maxOxygen - 5f && _oxygenDetector.GetPlayRefillAudio() && Locator.GetPlayerSuit().IsWearingHelmet())
				{
					Locator.GetPlayerAudioController().PlayRefillOxygen(GetOxygenFraction() > 0.8f);
					string displayMsg = (_oxygenDetector.GetDetectTrees() ? UITextLibrary.GetString(UITextType.NotificationTrees) : UITextLibrary.GetString(UITextType.NotificationO2));
					NotificationData data = new NotificationData(NotificationTarget.Player, displayMsg, 3f, playSound: false);
					NotificationManager.SharedInstance.PostNotification(data);
				}
			}
			else if (_refillingOxygen && _currentOxygen >= _maxOxygen)
			{
				_refillingOxygen = false;
			}
			_currentOxygen = Mathf.MoveTowards(_currentOxygen, _maxOxygen, 100f * Time.deltaTime);
		}
		else
		{
			_refillingOxygen = false;
			float num = 0f;
			if (_canOxygenDrain && !PlayerState.IsInsideTheEye() && !_invincible && !IsOxygenDrainBlockedByDreamWorld())
			{
				float num2 = 1f;
				num = (Locator.GetPlayerSuit().IsWearingHelmet() ? 1f : (_maxOxygen * num2));
				num += (float)_currentNumPunctures * _oxygenDrainOverTimePerPuncture;
			}
			_currentOxygen -= num * Time.deltaTime * ((_invincible || PlayerState.IsInsideTheEye()) ? 0f : 1f);
			_currentOxygen = Mathf.Max(_currentOxygen, 0f);
			if (_currentOxygen <= 0f && !_invincible && !PlayerState.IsInsideTheEye())
			{
				_isSuffocating = true;
				_startSuffocationTime = Time.time;
			}
		}
	}

	private void UpdateHealth()
	{
		float num = _hazardDetector.GetNetDamagePerSecond() * Time.deltaTime * ((_invincible || PlayerState.IsInsideTheEye()) ? 0f : 1f);
		if (!PlayerState.InDreamWorld())
		{
			_totalDamageSufferedThisLoop += num;
		}
		_currentHealth -= num;
		_playerAudioController.UpdateHazardDamage(num, _hazardDetector);
		RumbleManager.UpdateHazardDamage(num, _hazardDetector);
		if (_isHealing)
		{
			_currentHealth += _maxHealth * Time.deltaTime;
			if (_currentHealth >= _maxHealth)
			{
				_currentHealth = _maxHealth;
			}
		}
		if (_currentHealth <= 0f && !_invincible && !PlayerState.IsInsideTheEye() && !Locator.GetDeathManager().IsPlayerDying())
		{
			if (_hazardDetector.InHazardType(HazardVolume.HazardType.DARKMATTER) && TimeLoop.GetLoopCount() > 1)
			{
				PlayerData.SetPersistentCondition("KILLED_IN_GHOST_MATTER", state: true);
			}
			Locator.GetDeathManager().KillPlayer(DeathType.Default);
			return;
		}
		bool flag = PlayerState.IsWearingSuit() && IsSuitPunctured() && OWInput.IsInputMode(InputMode.Character) && !Locator.GetToolModeSwapper().IsSuitPatchingBlocked();
		_patchSuitPrompt.SetVisibility(flag);
		_patchSuitPromptDetails.SetVisibility(OWInput.IsInputMode(InputMode.PatchingSuit));
		if (flag)
		{
			if (OWInput.IsNewlyPressed(InputLibrary.interact))
			{
				_puncturePatchStartTime = Time.time;
				OWInput.ChangeInputMode(InputMode.PatchingSuit);
				_playerAudioController.PlayPatchPuncture();
				_repairDetailsStringBuilder.Length = 0;
				_repairDetailsStringBuilder.Append(UITextLibrary.GetString(UITextType.PatchSuitDetailPrompt));
				_repairDetailsStringBuilder.Append((100f - 100f * (float)_currentNumPunctures / (float)_maxSuitPunctures).ToString());
				_repairDetailsStringBuilder.Append("%");
				_patchSuitPromptDetails.SetText(_repairDetailsStringBuilder.ToString());
			}
		}
		else
		{
			if (!OWInput.IsInputMode(InputMode.PatchingSuit))
			{
				return;
			}
			if (IsSuitPunctured() && OWInput.IsPressed(InputLibrary.interact))
			{
				if (IsDoneRepairingSuitPuncture())
				{
					PatchPuncture();
					_puncturePatchStartTime = Time.time;
					_playerAudioController.PlayPatchPuncture();
				}
				_repairDetailsStringBuilder.Length = 0;
				_repairDetailsStringBuilder.Append(UITextLibrary.GetString(UITextType.PatchSuitDetailPrompt));
				_repairDetailsStringBuilder.Append(Mathf.FloorToInt(100f - 100f * ((float)_currentNumPunctures - (Time.time - _puncturePatchStartTime) / _puncturePatchTime) / (float)_maxSuitPunctures).ToString());
				_repairDetailsStringBuilder.Append("%");
				_patchSuitPromptDetails.SetText(_repairDetailsStringBuilder.ToString());
			}
			else
			{
				OWInput.ChangeInputMode(InputMode.Character);
			}
		}
	}

	private void PatchPuncture()
	{
		if (_currentNumPunctures != 0)
		{
			if (_currentNumPunctures <= _punctureEffects.Length)
			{
				_punctureEffects[_currentNumPunctures - 1].StopPuncture(_puncturePatchTime);
			}
			_currentNumPunctures--;
			_playerAudioController.UpdateSuitPunctures(_currentNumPunctures, _maxSuitPunctures);
		}
	}

	private void PatchAllPunctures()
	{
		_currentNumPunctures = 0;
		_playerAudioController.UpdateSuitPunctures(0, _maxSuitPunctures);
		float num = 0f;
		for (int i = 0; i < _punctureEffects.Length; i++)
		{
			_punctureEffects[i].StopPuncture(num);
			num += 1f;
		}
	}

	private void DebugKillResources()
	{
		if (_currentOxygen <= 0f)
		{
			_currentOxygen = 0f;
			_currentHealth -= Time.deltaTime * _maxHealth;
		}
		if (_currentFuel <= 0f)
		{
			_currentFuel = 0f;
			_currentOxygen -= Time.deltaTime * _maxOxygen;
		}
		else
		{
			_currentFuel -= Time.deltaTime * _maxFuel;
		}
		if (_currentHealth <= 0f)
		{
			Locator.GetDeathManager().KillPlayer(DeathType.Default);
			base.enabled = false;
		}
	}

	private void OnFirstContactDamage(HazardVolume hazardVolume)
	{
		if (!ApplyInstantDamage(hazardVolume.GetFirstContactDamage(), hazardVolume.GetFirstContactDamageType()))
		{
			return;
		}
		_playerAudioController.PlayHazardFirstContactDamage(hazardVolume);
		RumbleManager.PulseFirstContactDamage(hazardVolume.GetFirstContactDamageType());
		if (hazardVolume.GetFirstContactDamageType() != InstantDamageType.Puncture || !Locator.GetPlayerSuit().IsWearingSuit())
		{
			return;
		}
		ApplySuitPuncture();
		if (!PlayerState.InZeroG())
		{
			Vector3 vector = (base.transform.position - hazardVolume.transform.position).normalized * 5f;
			if (Vector3.Dot(vector, base.transform.up) < 0f)
			{
				_owRigidbody.AddVelocityChange(vector);
			}
		}
	}

	private void OnImpact(ImpactData impact)
	{
		float num = Mathf.Clamp01((impact.speed - GetMinImpactSpeed()) / (GetMaxImpactSpeed() - GetMinImpactSpeed()));
		if (PlayerState.InUndertowVolume() && num > 0.001f)
		{
			num = Mathf.Clamp(num, 0.2f, 0.8f);
		}
		bool flag = ApplyInstantDamage(_maxHealth * num, InstantDamageType.Impact);
		if (PlayerState.InUndertowVolume())
		{
			MonoBehaviour.print("undertow impact speed: " + impact.speed + "   max: " + GetMaxImpactSpeed() + "   min: " + GetMinImpactSpeed() + "   fraction: " + num);
		}
		bool flag2 = Locator.GetDreamWorldController() != null && Locator.GetDreamWorldController().IsExitingDream();
		if (flag && _currentHealth <= 0f && !PlayerState.IsDead() && !flag2)
		{
			Locator.GetDeathManager().SetImpactDeathSpeed(impact.speed);
			Locator.GetDeathManager().KillPlayer(DeathType.Impact);
			MonoBehaviour.print(string.Concat("Player killed from impact with ", impact.otherCollider, " attached to ", impact.otherCollider.attachedRigidbody.gameObject.name));
		}
	}

	private void ApplySuitPuncture()
	{
		if (_currentNumPunctures < _maxSuitPunctures)
		{
			_currentNumPunctures++;
			_playerAudioController.UpdateSuitPunctures(_currentNumPunctures, _maxSuitPunctures);
			_punctureDamageTime = Time.time;
			if (_punctureEffects.Length >= _currentNumPunctures)
			{
				_punctureEffects[_currentNumPunctures - 1].StartPuncture();
			}
			_currentOxygen -= _oxygenDrainOnPuncture;
			NotificationData data = new NotificationData(NotificationTarget.Player, UITextLibrary.GetString(UITextType.NotificationSuitPuncture), 3f);
			NotificationManager.SharedInstance.PostNotification(data);
		}
	}

	private void OnEatMarshmallow(float toastedFraction)
	{
		float num = 0f;
		num = ((!(toastedFraction < 0.7f)) ? (1f - (toastedFraction - 0.7f) / 0.3f) : ((toastedFraction - 0.2f) / 0.5f));
		if (toastedFraction >= 1f && PlayerData.EatBurnedMarshmallow() >= 10)
		{
			Achievements.Earn(Achievements.Type.CARCINOGENS);
		}
		if (num > 0.5f)
		{
			PlayerData.EatPerfectMarshmallow();
		}
		num = Mathf.Clamp01(num);
		MonoBehaviour.print("Heal Fraction: " + num);
		_currentHealth = Mathf.Min(_maxHealth, _currentHealth + _maxHealth * num);
		if (this.OnRecoverHealth != null)
		{
			this.OnRecoverHealth(_currentHealth);
		}
	}

	private void OnSuitUp()
	{
		if (PlayerState.InDreamWorld())
		{
			return;
		}
		if (Locator.GetPlayerSuit().IsTrainingSuit())
		{
			_isWearingTrainingSuit = true;
			_currentFuel = _maxFuel;
			_currentOxygen = _maxOxygen;
			_currentNumPunctures = 0;
			for (int i = 0; i < _punctureEffects.Length; i++)
			{
				_punctureEffects[i].StopPuncture();
			}
		}
		_playerAudioController.UpdateSuitPunctures(_currentNumPunctures, _maxSuitPunctures);
		_canOxygenDrain = true;
	}

	private void OnRemoveSuit()
	{
		if (OWInput.IsInputMode(InputMode.PatchingSuit))
		{
			OWInput.ChangeInputMode(InputMode.Character);
		}
		if (_isWearingTrainingSuit)
		{
			_currentFuel = _maxFuel;
			_currentOxygen = _maxOxygen;
			_currentNumPunctures = 0;
			for (int i = 0; i < _punctureEffects.Length; i++)
			{
				_punctureEffects[i].StopPuncture();
			}
		}
		_isWearingTrainingSuit = false;
		_playerAudioController.UpdateSuitPunctures(0, _maxSuitPunctures);
	}

	private void OnEnterDreamWorld()
	{
		base.enabled = true;
		_cachedHealth = _currentHealth;
		_currentHealth = _maxHealth;
	}

	private void OnExitDreamWorld()
	{
		HandleWakeFromDreamCampfire();
		_currentHealth = _cachedHealth;
	}

	private void OnStopSleepingAtDreamCampfire()
	{
		HandleWakeFromDreamCampfire();
	}

	private void HandleWakeFromDreamCampfire()
	{
		base.enabled = true;
		GlobalMessenger.AddListener("PutOnHelmet", OnPutOnHelmet);
		if (_isSuffocating)
		{
			_startSuffocationTime = Time.time;
		}
	}

	private void OnPutOnHelmet()
	{
		GlobalMessenger.RemoveListener("PutOnHelmet", OnPutOnHelmet);
		_currentOxygen = _maxOxygen;
	}

	private void OnPlayerResurrection()
	{
		_currentHealth = _maxHealth;
	}
}
