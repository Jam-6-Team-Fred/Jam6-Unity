using UnityEngine;

public class PlayerCameraEffectController : MonoBehaviour
{
	public delegate void RealityShatterEffectEvent();

	[Header("Damage Effects")]
	[SerializeField]
	private float _winceFadeLength = 1f;

	[SerializeField]
	private AnimationCurve _winceEffectCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	private AnimationCurve _winceExposure = AnimationCurve.Linear(0f, 0f, 100f, 1f);

	[SerializeField]
	private AnimationCurve _winceSaturation = AnimationCurve.Linear(0f, 1f, 100f, 0f);

	[SerializeField]
	private AnimationCurve _winceContrast = AnimationCurve.Linear(0f, 1f, 100f, 2f);

	[SerializeField]
	private float _phospheneFadeLength = 5f;

	[SerializeField]
	private AnimationCurve _phospheneBrightness = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[Header("Start Effects")]
	[SerializeField]
	private float _wakeLength = 2f;

	[SerializeField]
	private AnimationCurve _fastWakeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _calmWakeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Death Effects")]
	[SerializeField]
	private float _defaultDeathFadeLength = 0.3f;

	[SerializeField]
	private float _impactSlowFadeLength = 5f;

	[SerializeField]
	private float _suicideSlowFadeLength = 5f;

	[SerializeField]
	private AnimationCurve _impactSlowCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _impactQuickFadeLength = 0.3f;

	[SerializeField]
	private float _impactQuickDeathSpeed = 100f;

	[SerializeField]
	private float _asphyxiationFadeLength = 5f;

	[SerializeField]
	private float _energyFadeLength = 1f;

	[SerializeField]
	private float _bigBangFadeLength = 5f;

	[SerializeField]
	private float _digestionFadeLength = 5f;

	[SerializeField]
	private float _timeLoopFadeLength = 2f;

	[SerializeField]
	private Texture2D _timeLoopEyeMask;

	[SerializeField]
	private float _timeLoopBlendWidth = 0.1f;

	[SerializeField]
	private AnimationCurve _timeLoopEyeMaskCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _timeLoopLinesProgressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _escapeTimeLoopFadeLength = 5f;

	[SerializeField]
	private float _blackHoleFadeLength = 0.5f;

	[SerializeField]
	private float _realityShatterLength = 1f;

	[SerializeField]
	private float _dreamFadeLength = 0.5f;

	[SerializeField]
	private AnimationCurve _realityShardShatterCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _realityShardOffsetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _realityShardDissolveWidthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _realityShardDissolveProgressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private OWCamera _owCamera;

	private PlayerResources _playerResources;

	private bool _isWincing;

	private float _winceDamage;

	private float _winceStartTime;

	private bool _isOpeningEyes;

	private bool _isClosingEyes;

	private float _eyeAnimStartTime;

	private float _eyeAnimDuration;

	private AnimationCurve _wakeCurve;

	private float _lastOpenness;

	private bool _isDying;

	private bool _deathSequenceFinished;

	private DeathType _deathType;

	private float _impactDeathSpeed;

	private float _deathStartTime;

	private float _deathFadeLength;

	private bool _isConsciousnessFading;

	private float _consciousnessFadeStartTime;

	private float _consciousnessFadeLength;

	private RealityShatterImageEffect _realityShatterEffect;

	private bool _isRealityShattering;

	private bool _isRealityShatterEffectComplete;

	private float _realityShatterStartTime;

	private ScreenPrompt _wakePrompt;

	private bool _waitForWakeInput;

	public event RealityShatterEffectEvent OnRealityShatterEffectComplete;

	private void Awake()
	{
		_owCamera = this.GetRequiredComponent<OWCamera>();
		_realityShatterEffect = GetComponent<RealityShatterImageEffect>();
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("PlayerEscapedTimeLoop", OnPlayerEscapeTimeLoop);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
		GlobalMessenger.AddListener("FakePlayerMeditationDeath", OnFakePlayerMeditationDeath);
	}

	private void Start()
	{
		_playerResources = Locator.GetPlayerTransform().GetRequiredComponent<PlayerResources>();
		_playerResources.OnInstantDamage += OnInstantDamage;
	}

	private void OnDestroy()
	{
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("PlayerEscapedTimeLoop", OnPlayerEscapeTimeLoop);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
		GlobalMessenger.RemoveListener("FakePlayerMeditationDeath", OnFakePlayerMeditationDeath);
		if (_playerResources != null)
		{
			_playerResources.OnInstantDamage -= OnInstantDamage;
		}
		if (_waitForWakeInput)
		{
			PauseCommandListener pauseCommandListener = Locator.GetPauseCommandListener();
			if (pauseCommandListener != null)
			{
				pauseCommandListener.RemovePauseCommandLock();
			}
			OWTime.Unpause(OWTime.PauseType.Sleeping);
		}
	}

	public void OpenEyes(float animDuration, bool useFastWakeCurve = false)
	{
		OpenEyes(animDuration, useFastWakeCurve ? _fastWakeCurve : _calmWakeCurve);
	}

	public void OpenEyes(float animDuration, AnimationCurve wakeCurve)
	{
		_lastOpenness = 0f;
		_wakeCurve = wakeCurve;
		_isOpeningEyes = true;
		_isClosingEyes = false;
		_eyeAnimDuration = animDuration;
		_eyeAnimStartTime = Time.time;
		_owCamera.postProcessingSettings.eyeMask.openness = 0f;
		_owCamera.postProcessingSettings.bloom.threshold = 0f;
		_owCamera.postProcessingSettings.eyeMaskEnabled = true;
	}

	public void CloseEyes(float animDuration)
	{
		_isOpeningEyes = false;
		_isClosingEyes = true;
		_eyeAnimDuration = animDuration;
		_eyeAnimStartTime = Time.time;
		_owCamera.postProcessingSettings.eyeMaskEnabled = true;
	}

	public void PlayRealityShatterEffect()
	{
		if (_realityShatterEffect != null)
		{
			_realityShatterEffect.enabled = true;
			_isRealityShattering = true;
			_isRealityShatterEffectComplete = false;
			_realityShatterStartTime = Time.time;
		}
	}

	public bool IsRealityShatterEffectComplete()
	{
		return _isRealityShatterEffectComplete;
	}

	private void Update()
	{
		if (_waitForWakeInput && LateInitializerManager.isDoneInitializing)
		{
			if (!_wakePrompt.IsVisible())
			{
				_wakePrompt.SetVisibility(isVisible: true);
			}
			if (OWInput.IsNewlyPressed(InputLibrary.interact))
			{
				_waitForWakeInput = false;
				LateInitializerManager.pauseOnInitialization = false;
				Locator.GetPauseCommandListener().RemovePauseCommandLock();
				Locator.GetPromptManager().RemoveScreenPrompt(_wakePrompt);
				OWTime.Unpause(OWTime.PauseType.Sleeping);
				WakeUp();
			}
		}
		if (_playerResources.GetHazardDetector().GetNetDamagePerSecond() > 0f)
		{
			ApplyExposureDamage();
		}
		if (_isDying)
		{
			float num = Mathf.InverseLerp(_deathStartTime, _deathStartTime + _deathFadeLength, Time.time);
			switch (_deathType)
			{
			case DeathType.Default:
			case DeathType.Impact:
			case DeathType.Crushed:
			case DeathType.Dream:
				if (_impactDeathSpeed < _impactQuickDeathSpeed)
				{
					_owCamera.postProcessingSettings.eyeMask.openness = 1f - _impactSlowCurve.Evaluate(num);
					_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, -5f, _impactSlowCurve.Evaluate(num));
				}
				else
				{
					_owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
					_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
				}
				break;
			case DeathType.CrushedByElevator:
				_owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
				break;
			case DeathType.Asphyxiation:
			{
				float t2 = (2f - num) * num;
				_owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
				_owCamera.postProcessingSettings.bloom.threshold = Mathf.Lerp(_owCamera.postProcessingSettings.bloomDefault.threshold, 0f, t2);
				_owCamera.postProcessingSettings.bloom.radius = Mathf.Lerp(_owCamera.postProcessingSettings.bloomDefault.radius, 7f, t2);
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, t2);
				_owCamera.postProcessingSettings.colorGrading.saturation = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.saturation, 0f, t2);
				break;
			}
			case DeathType.Energy:
			case DeathType.Supernova:
			case DeathType.Lava:
			case DeathType.DreamExplosion:
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
				break;
			case DeathType.BigBang:
			{
				float t = Mathf.Clamp01((Time.time - _deathStartTime) / _energyFadeLength);
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, t);
				break;
			}
			case DeathType.Digestion:
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num);
				break;
			case DeathType.Meditation:
				_owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num);
				break;
			case DeathType.TimeLoop:
				_owCamera.postProcessingSettings.eyeMask.openness = 1f - _timeLoopEyeMaskCurve.Evaluate(num);
				_owCamera.postProcessingSettings.eyeMask.linesProgress = _timeLoopLinesProgressionCurve.Evaluate(num);
				break;
			case DeathType.BlackHole:
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num);
				break;
			default:
				_owCamera.postProcessingSettings.eyeMask.openness = 1f - num;
				_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, 10f, num);
				break;
			}
			if (num >= 1f && !_deathSequenceFinished)
			{
				_deathSequenceFinished = true;
				if (Locator.GetDeathManager().IsFakeMeditationDeath())
				{
					ResetDeathSequence();
					_owCamera.postProcessingSettings.eyeMask.openness = 0f;
				}
				Locator.GetDeathManager().FinishDeathSequence();
			}
		}
		else if (_isOpeningEyes)
		{
			float num2 = Mathf.Clamp01((Time.time - _eyeAnimStartTime) / _eyeAnimDuration);
			float num3 = _wakeCurve.Evaluate(num2);
			_owCamera.postProcessingSettings.eyeMask.openness = num3;
			_owCamera.postProcessingSettings.bloom.threshold = Mathf.Lerp(0f, _owCamera.postProcessingSettings.bloomDefault.threshold, num3);
			if (_lastOpenness >= 0.5f && num3 < 0.5f)
			{
				GlobalMessenger.FireEvent("PlayerBlink");
			}
			_lastOpenness = num3;
			if (num2 >= 1f)
			{
				_owCamera.postProcessingSettings.eyeMaskEnabled = false;
				_isOpeningEyes = false;
				GlobalMessenger.FireEvent("FinishOpenEyes");
			}
		}
		else if (_isClosingEyes)
		{
			float num4 = Mathf.Clamp01((Time.time - _eyeAnimStartTime) / _eyeAnimDuration);
			_owCamera.postProcessingSettings.eyeMask.openness = 1f - num4;
			_owCamera.postProcessingSettings.bloom.threshold = Mathf.Lerp(0f, _owCamera.postProcessingSettings.bloomDefault.threshold, 1f - num4);
			if (num4 >= 1f)
			{
				_isClosingEyes = false;
			}
		}
		else if (_isWincing)
		{
			float num5 = Mathf.Clamp01((Time.time - _winceStartTime) / _winceFadeLength);
			float t3 = 1f - _winceEffectCurve.Evaluate(num5);
			_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(_winceExposure.Evaluate(_winceDamage), _owCamera.postProcessingSettings.colorGradingDefault.postExposure, t3);
			_owCamera.postProcessingSettings.colorGrading.saturation = Mathf.Lerp(_winceSaturation.Evaluate(_winceDamage), _owCamera.postProcessingSettings.colorGradingDefault.saturation, t3);
			_owCamera.postProcessingSettings.colorGrading.contrast = Mathf.Lerp(_winceContrast.Evaluate(_winceDamage), _owCamera.postProcessingSettings.colorGradingDefault.contrast, t3);
			if (num5 >= 1f)
			{
				_isWincing = false;
				_winceDamage = 0f;
			}
		}
		if (_isConsciousnessFading)
		{
			float num6 = Mathf.Clamp01((Time.time - _consciousnessFadeStartTime) / _consciousnessFadeLength);
			_owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Min(Mathf.Lerp(_owCamera.postProcessingSettings.colorGradingDefault.postExposure, -10f, num6), _owCamera.postProcessingSettings.colorGrading.postExposure);
			AudioListener.volume = 1f - num6;
			if (num6 >= 1f)
			{
				Locator.GetDeathManager().FinishEscapeTimeLoopSequence();
			}
		}
		if (_isRealityShattering && !_isRealityShatterEffectComplete)
		{
			float num7 = Time.time - _realityShatterStartTime;
			_realityShatterEffect.SetShatterParameters(_realityShardShatterCurve.Evaluate(num7), _realityShardOffsetCurve.Evaluate(num7), _realityShardDissolveWidthCurve.Evaluate(num7), _realityShardDissolveProgressCurve.Evaluate(num7));
			if (num7 >= _realityShatterLength)
			{
				_isRealityShatterEffectComplete = true;
				if (this.OnRealityShatterEffectComplete != null)
				{
					this.OnRealityShatterEffectComplete();
				}
			}
		}
		if (_owCamera.postProcessingSettings.phosphenesEnabled)
		{
			float num8 = Mathf.Clamp01((Time.time - _winceStartTime) / _phospheneFadeLength);
			_owCamera.postProcessingSettings.phosphenes.visibility = 1f - num8;
			_owCamera.postProcessingSettings.phosphenes.brightness = _phospheneBrightness.Evaluate(num8);
			if (num8 >= 1f)
			{
				_owCamera.postProcessingSettings.phosphenesEnabled = false;
			}
		}
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		if (base.gameObject.CompareTag("MainCamera") && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
		{
			if (LoadManager.GetPreviousScene() == OWScene.TitleScreen)
			{
				_owCamera.postProcessingSettings.eyeMask.openness = 0f;
				_owCamera.postProcessingSettings.bloom.threshold = 0f;
				_owCamera.postProcessingSettings.eyeMaskEnabled = true;
				_waitForWakeInput = true;
				_wakePrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.WakeUpPrompt));
				_wakePrompt.SetVisibility(isVisible: false);
				Locator.GetPromptManager().AddScreenPrompt(_wakePrompt, PromptPosition.Center);
				OWTime.Pause(OWTime.PauseType.Sleeping);
				Locator.GetPauseCommandListener().AddPauseCommandLock();
			}
			else
			{
				WakeUp();
			}
		}
	}

	private void WakeUp()
	{
		if (TimeLoop.GetLoopCount() == 1 || PlayerData.GetLastDeathType() == DeathType.Supernova || PlayerData.GetLastDeathType() == DeathType.Meditation)
		{
			OpenEyes(_wakeLength, _calmWakeCurve);
		}
		else
		{
			OpenEyes(_wakeLength, _fastWakeCurve);
		}
		GlobalMessenger.FireEvent("WakeUp");
	}

	private void OnFakePlayerMeditationDeath()
	{
		_isDying = true;
		_deathType = DeathType.Meditation;
		_deathStartTime = Time.time;
		_impactDeathSpeed = 0f;
		_owCamera.postProcessingSettings.colorGradingEnabled = true;
		_owCamera.postProcessingSettings.eyeMaskEnabled = true;
		_deathFadeLength = _suicideSlowFadeLength;
		Locator.GetPlayerDeathAudio().PlayDeathAudio(_deathType, _deathFadeLength);
		RumbleManager.PlayDeathRumble(_deathType, _deathFadeLength);
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		_isDying = true;
		_deathType = deathType;
		_deathStartTime = Time.time;
		_impactDeathSpeed = 0f;
		if (deathType == DeathType.Impact)
		{
			_impactDeathSpeed = Locator.GetDeathManager().GetImpactDeathSpeed();
		}
		if (deathType == DeathType.Impact || deathType == DeathType.Asphyxiation || deathType == DeathType.Meditation || deathType == DeathType.Crushed || deathType == DeathType.CrushedByElevator)
		{
			_owCamera.postProcessingSettings.colorGradingEnabled = true;
			_owCamera.postProcessingSettings.eyeMaskEnabled = true;
		}
		switch (_deathType)
		{
		case DeathType.Default:
		case DeathType.Impact:
		case DeathType.Crushed:
			_deathFadeLength = ((_impactDeathSpeed < _impactQuickDeathSpeed) ? _impactSlowFadeLength : _impactQuickFadeLength);
			break;
		case DeathType.CrushedByElevator:
			_deathFadeLength = _impactQuickFadeLength;
			break;
		case DeathType.Asphyxiation:
			_deathFadeLength = _asphyxiationFadeLength;
			break;
		case DeathType.Energy:
		case DeathType.Supernova:
		case DeathType.DreamExplosion:
			_deathFadeLength = _energyFadeLength;
			break;
		case DeathType.BigBang:
			_deathFadeLength = _bigBangFadeLength;
			break;
		case DeathType.Digestion:
			_deathFadeLength = _digestionFadeLength;
			break;
		case DeathType.Meditation:
			_deathFadeLength = _suicideSlowFadeLength;
			break;
		case DeathType.TimeLoop:
			_deathFadeLength = _timeLoopFadeLength;
			_owCamera.postProcessingSettings.eyeMaskEnabled = true;
			_owCamera.postProcessingSettings.eyeMask.edgeColorMode = true;
			_owCamera.postProcessingSettings.eyeMask.eyeMask = _timeLoopEyeMask;
			_owCamera.postProcessingSettings.eyeMask.blendWidth = _timeLoopBlendWidth;
			_owCamera.postProcessingSettings.eyeMask.linesEnabled = true;
			break;
		case DeathType.BlackHole:
			_deathFadeLength = _blackHoleFadeLength;
			break;
		case DeathType.Dream:
			_deathFadeLength = _dreamFadeLength;
			break;
		default:
			_deathFadeLength = _defaultDeathFadeLength;
			break;
		}
		Locator.GetPlayerDeathAudio().PlayDeathAudio(deathType, _deathFadeLength);
		RumbleManager.PlayDeathRumble(deathType, _deathFadeLength);
	}

	private void OnPlayerEscapeTimeLoop()
	{
		_isConsciousnessFading = true;
		_consciousnessFadeStartTime = Time.time;
		_consciousnessFadeLength = _escapeTimeLoopFadeLength;
	}

	private void OnPlayerResurrection()
	{
		ResetDeathSequence();
		OpenEyes(1f, useFastWakeCurve: true);
	}

	private void ResetDeathSequence()
	{
		_isDying = false;
		_deathSequenceFinished = false;
		_deathType = DeathType.Default;
		_owCamera.postProcessingSettings.eyeMask = _owCamera.postProcessingSettings.eyeMaskDefault;
		_owCamera.postProcessingSettings.phosphenes = _owCamera.postProcessingSettings.phosphenesDefault;
		_owCamera.postProcessingSettings.bloom = _owCamera.postProcessingSettings.bloomDefault;
		_owCamera.postProcessingSettings.colorGrading = _owCamera.postProcessingSettings.colorGradingDefault;
	}

	private void ApplyExposureDamage()
	{
		float num = 1f;
		if (!_isWincing)
		{
			_isWincing = true;
			_winceDamage = num;
			_winceStartTime = Time.time;
			_owCamera.postProcessingSettings.colorGradingEnabled = true;
			_owCamera.postProcessingSettings.eyeMaskEnabled = true;
			_owCamera.postProcessingSettings.phosphenesEnabled = true;
		}
		else if (!_isDying)
		{
			float postExposure = _owCamera.postProcessingSettings.colorGrading.postExposure;
			if (_winceExposure.Evaluate(num) > postExposure)
			{
				_winceDamage = num;
				_winceStartTime = Time.time;
			}
		}
	}

	private void OnInstantDamage(float damage, InstantDamageType type)
	{
		if (!_isWincing)
		{
			_isWincing = true;
			_winceDamage = damage;
			_winceStartTime = Time.time;
			_owCamera.postProcessingSettings.colorGradingEnabled = true;
			_owCamera.postProcessingSettings.eyeMaskEnabled = true;
			_owCamera.postProcessingSettings.phosphenesEnabled = true;
		}
		else
		{
			float postExposure = _owCamera.postProcessingSettings.colorGrading.postExposure;
			if (_winceExposure.Evaluate(damage) > postExposure)
			{
				_winceDamage = damage;
				_winceStartTime = Time.time;
			}
		}
	}
}
