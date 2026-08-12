using UnityEngine;

public class TimelineObliterationController : MonoBehaviour
{
	public enum ObliterationType
	{
		TIME_LOOP_EXPERIMENT = 0,
		TIME_LOOP_CORE = 1,
		PARADOX_DEATH = 2,
		TIME_LOOP_CORE_AT_EYE = 3,
		EXODIA = 4
	}

	private enum Effect
	{
		NONE = 0,
		VOID_SHADOW = 1,
		WORLD_CRACKS = 2,
		SCREEN_SHATTER = 3
	}

	public delegate void TimelineObliterationEvent();

	[SerializeField]
	private TimelineObliterationEffect[] _voidCrackEffectList;

	[SerializeField]
	private float _timeToCrackEffect;

	[SerializeField]
	private float _timeToScreenEffect;

	[SerializeField]
	private float _timeToParadoxDeathScreenEffect;

	[SerializeField]
	private OWAudioSource _screenShatterAudioSource;

	[SerializeField]
	private OWAudioSource _voidCrackAudioSource;

	private Effect[] _triggeredEffects = new Effect[3];

	private bool _effectStarted;

	private bool _paradoxRemoteTrigger;

	private float _totalEffectTime;

	private int _completedCracks;

	private float _totalCrackTime;

	private VoidShadowEffectController _voidShadowEffect;

	private PlayerCameraEffectController _cameraEffect;

	private bool _hasPlayedShadowAudio;

	private bool _hasPlayedCrackAudio;

	private static bool s_isRealityEnding;

	private static bool s_hasRealityEnded;

	private static bool s_paradoxCoreProbeActive;

	private static bool s_paradoxExpProbeActive;

	public event TimelineObliterationEvent OnTimelineStartObliteration;

	public static bool IsRealityEnding()
	{
		return s_isRealityEnding;
	}

	public static bool HasRealityEnded()
	{
		return s_hasRealityEnded;
	}

	public static void ResetHasRealityEnded()
	{
		s_hasRealityEnded = false;
	}

	public static bool IsParadoxProbeActive()
	{
		if (!s_paradoxCoreProbeActive)
		{
			return s_paradoxExpProbeActive;
		}
		return true;
	}

	public static void SetParadoxCoreProbeActive(bool value)
	{
		s_paradoxCoreProbeActive = value;
	}

	public static void SetParadoxExperimentProbeActive(bool value)
	{
		s_paradoxExpProbeActive = value;
	}

	private void Awake()
	{
		ResetHasRealityEnded();
		SetParadoxCoreProbeActive(value: false);
		SetParadoxExperimentProbeActive(value: false);
		s_isRealityEnding = false;
	}

	private void Update()
	{
		if (_effectStarted)
		{
			_totalEffectTime += Time.deltaTime;
			if (_paradoxRemoteTrigger)
			{
				if (_totalEffectTime > _timeToParadoxDeathScreenEffect && _triggeredEffects[2] == Effect.NONE)
				{
					TriggerScreenEffect();
				}
				return;
			}
			if (!_hasPlayedShadowAudio && _totalEffectTime > 1f)
			{
				_voidCrackAudioSource.PlayOneShot(AudioType.TimelineEndEffect_Shadow);
				_hasPlayedShadowAudio = true;
			}
			if (!_hasPlayedCrackAudio && _totalEffectTime > _timeToCrackEffect + 2f)
			{
				_voidCrackAudioSource.PlayOneShot(AudioType.TimelineEndEffect_Cracks);
				_hasPlayedCrackAudio = true;
			}
			if (_totalEffectTime > _timeToCrackEffect && _triggeredEffects[1] == Effect.NONE)
			{
				TriggerCrackEffect();
			}
			if (_totalEffectTime > _timeToScreenEffect && _triggeredEffects[2] == Effect.NONE)
			{
				TriggerScreenEffect();
			}
		}
		else if (Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().GetState() < EyeState.IntoTheVortex && EyeStateManager.ParadoxExists() && TimeLoop.GetSecondsRemaining() < -28f)
		{
			BeginTimelineObliteration(ObliterationType.TIME_LOOP_CORE_AT_EYE, null);
		}
		else if (PlayerState.OnQuantumMoon() && Locator.GetQuantumMoon().GetStateIndex() == 5 && TimeLoopCoreController.ParadoxExists() && TimeLoop.GetSecondsRemaining() < -28f)
		{
			BeginTimelineObliteration(ObliterationType.TIME_LOOP_CORE_AT_EYE, null);
			PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE", state: false);
			PlayerData.SetPersistentCondition("PROBE_ENTERED_TIMELOOPCORE", state: false);
			if (PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE"))
			{
				PlayerData.SetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE_MULTIPLE", state: false);
			}
		}
	}

	public void BeginTimelineObliteration(ObliterationType destructionType, ITimelineObliterator paradoxObject)
	{
		_voidShadowEffect = null;
		_effectStarted = true;
		s_isRealityEnding = true;
		Locator.GetPromptManager().SetPromptsVisible(visible: false);
		ReticleController.Hide();
		Locator.GetAudioMixer().MixEndTimes(1f);
		RumbleManager.PlayEndOfRealityRumble();
		if (destructionType == ObliterationType.PARADOX_DEATH || destructionType == ObliterationType.TIME_LOOP_CORE_AT_EYE)
		{
			_paradoxRemoteTrigger = true;
		}
		else
		{
			_voidShadowEffect = paradoxObject.GetVoidShadowEffect();
			if (_voidShadowEffect != null)
			{
				_triggeredEffects[0] = Effect.VOID_SHADOW;
				_voidShadowEffect.OnEffectComplete += OnVoidShadowEffectComplete;
				_voidShadowEffect.PlayEffect();
				_voidCrackAudioSource.transform.parent = _voidShadowEffect.transform;
				_voidCrackAudioSource.transform.localPosition = Vector3.zero;
			}
		}
		if (this.OnTimelineStartObliteration != null)
		{
			this.OnTimelineStartObliteration();
		}
	}

	private void OnVoidShadowEffectComplete()
	{
		_voidShadowEffect.OnEffectComplete -= OnVoidShadowEffectComplete;
	}

	private void TriggerCrackEffect()
	{
		_triggeredEffects[1] = Effect.WORLD_CRACKS;
		_completedCracks = 0;
		_totalCrackTime = 0f;
		for (int i = 0; i < _voidCrackEffectList.Length; i++)
		{
			if (_voidCrackEffectList[i].effectTime > _totalCrackTime)
			{
				_totalCrackTime = _voidCrackEffectList[i].effectTime;
			}
		}
		if (_voidCrackEffectList != null)
		{
			for (int j = 0; j < _voidCrackEffectList.Length; j++)
			{
				_voidCrackEffectList[j].OnCrackEffectComplete += OnCrackEffectComplete;
				_voidCrackEffectList[j].transform.SetParent(_voidShadowEffect.transform);
				_voidCrackEffectList[j].transform.localPosition = _voidShadowEffect.voidCracksParentOffset;
				_voidCrackEffectList[j].PlayEffect();
			}
		}
	}

	private void OnCrackEffectComplete()
	{
		_completedCracks++;
		if (_completedCracks == _voidCrackEffectList.Length)
		{
			for (int i = 0; i < _voidCrackEffectList.Length; i++)
			{
				_voidCrackEffectList[i].OnCrackEffectComplete -= OnCrackEffectComplete;
			}
		}
	}

	private void TriggerScreenEffect()
	{
		_triggeredEffects[2] = Effect.SCREEN_SHATTER;
		_cameraEffect = Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>();
		_cameraEffect.OnRealityShatterEffectComplete += CompleteTimelineObliteration;
		_cameraEffect.PlayRealityShatterEffect();
		if (_screenShatterAudioSource != null)
		{
			_screenShatterAudioSource.PlayOneShot(AudioType.TimelineEndEffect_Shatter);
		}
	}

	private void CompleteTimelineObliteration()
	{
		_cameraEffect.OnRealityShatterEffectComplete -= CompleteTimelineObliteration;
		s_hasRealityEnded = true;
		PlayerData.SetPersistentCondition("DESTROYED_TIMELINE_LAST_SAVE", state: true);
		PlayerData.RevertParadoxLoopCountStates();
		GlobalMessenger.FireEvent("TriggerDeathOfReality");
	}
}
