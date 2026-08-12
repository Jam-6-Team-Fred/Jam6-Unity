using UnityEngine;

public class RumbleManager : MonoBehaviour, IPermanentManagerWorker
{
	private class Rumble
	{
		public enum Fade
		{
			UNUSED = 0,
			STEADY = 1,
			PULSE = 2,
			FADE = 3
		}

		public Vector2 m_power;

		public float m_fadeTime;

		public float m_timer;

		public Fade m_fade;

		public uint m_id;

		public Rumble()
		{
			m_fade = Fade.UNUSED;
			m_id = 0u;
		}

		public void Update(float dt)
		{
			Fade fade = m_fade;
			if ((uint)fade > 1u && (uint)(fade - 2) <= 1u)
			{
				m_timer -= dt;
				if (m_timer <= 0f)
				{
					Destroy();
				}
			}
		}

		public Vector2 GetPower()
		{
			if (m_fade == Fade.FADE)
			{
				return m_timer / m_fadeTime * m_power;
			}
			return m_power;
		}

		public bool IsAlive()
		{
			return m_fade != Fade.UNUSED;
		}

		public void Destroy()
		{
			m_fade = Fade.UNUSED;
			m_id = 0u;
		}
	}

	private class TriggerEffect
	{
		public enum Mode
		{
			None = 0,
			ResistanceConstant = 1,
			ResistanceSlope = 2,
			VibrationConstant = 3,
			VibrationSlope = 4
		}

		public bool active;

		public Mode mode;

		public int resistanceStrengthStart;

		public int resistanceStrengthEnd;

		public int vibrationFrequency;

		public int vibrationStrength;

		public byte[] variableStrengthArray;

		public bool strengthFade;

		public float fadeLength;

		public float fadeTimer;

		private InputConsts.InputCommandType inputCommand1;

		private InputConsts.InputCommandType inputCommand2;

		public TriggerEffect(InputConsts.InputCommandType inputCommand1 = InputConsts.InputCommandType.UNDEFINED, InputConsts.InputCommandType inputCommand2 = InputConsts.InputCommandType.UNDEFINED)
		{
			this.inputCommand1 = inputCommand1;
			this.inputCommand2 = inputCommand2;
		}

		public void Clear()
		{
			active = false;
			mode = Mode.None;
			resistanceStrengthStart = 0;
			resistanceStrengthEnd = 0;
			vibrationFrequency = 0;
			vibrationStrength = 0;
			variableStrengthArray = null;
			strengthFade = false;
			fadeLength = 0f;
			fadeTimer = 0f;
		}

		public void Update(float deltaTime)
		{
			if (strengthFade)
			{
				fadeTimer -= deltaTime;
				if (fadeTimer <= 0f)
				{
					active = false;
				}
			}
		}

		public float GetStrengthScale()
		{
			if (strengthFade)
			{
				return Mathf.Clamp01(fadeTimer / fadeLength);
			}
			return 1f;
		}

		public bool AffectsLeftTrigger()
		{
			if (inputCommand1 == InputConsts.InputCommandType.UNDEFINED && this.inputCommand2 == InputConsts.InputCommandType.UNDEFINED)
			{
				return true;
			}
			if (inputCommand1 != 0)
			{
				IInputCommands inputCommand = InputLibrary.GetInputCommand(inputCommand1);
				if (inputCommand != null && inputCommand.AxisID == AxisIdentifier.CTRLR_LTRIGGER)
				{
					return true;
				}
			}
			if (this.inputCommand2 != 0)
			{
				IInputCommands inputCommand2 = InputLibrary.GetInputCommand(this.inputCommand2);
				if (inputCommand2 != null && inputCommand2.AxisID == AxisIdentifier.CTRLR_LTRIGGER)
				{
					return true;
				}
			}
			return false;
		}

		public bool AffectsRightTrigger()
		{
			if (inputCommand1 == InputConsts.InputCommandType.UNDEFINED && this.inputCommand2 == InputConsts.InputCommandType.UNDEFINED)
			{
				return true;
			}
			if (inputCommand1 != 0)
			{
				IInputCommands inputCommand = InputLibrary.GetInputCommand(inputCommand1);
				if (inputCommand != null && inputCommand.AxisID == AxisIdentifier.CTRLR_RTRIGGER)
				{
					return true;
				}
			}
			if (this.inputCommand2 != 0)
			{
				IInputCommands inputCommand2 = InputLibrary.GetInputCommand(this.inputCommand2);
				if (inputCommand2 != null && inputCommand2.AxisID == AxisIdentifier.CTRLR_RTRIGGER)
				{
					return true;
				}
			}
			return false;
		}

		public void SetModeNone()
		{
			Clear();
			active = true;
		}

		public void SetModeResistanceConstant(int resistanceStrength, float strengthFadeLength = 0f)
		{
			Clear();
			active = true;
			mode = Mode.ResistanceConstant;
			resistanceStrengthStart = resistanceStrength;
			if (strengthFadeLength > 0f)
			{
				strengthFade = true;
				fadeLength = strengthFadeLength;
				fadeTimer = strengthFadeLength;
			}
		}

		public void SetModeResistanceSlope(int resistanceStrengthStart, int resistanceStrengthEnd, float strengthFadeLength = 0f)
		{
			Clear();
			active = true;
			mode = Mode.ResistanceSlope;
			this.resistanceStrengthStart = resistanceStrengthStart;
			this.resistanceStrengthEnd = resistanceStrengthEnd;
			if (strengthFadeLength > 0f)
			{
				strengthFade = true;
				fadeLength = strengthFadeLength;
				fadeTimer = strengthFadeLength;
			}
		}

		public void SetModeVibrationConstant(int vibrationFrequency, int vibrationStrength, float strengthFadeLength = 0f)
		{
			Clear();
			active = true;
			mode = Mode.VibrationConstant;
			this.vibrationFrequency = vibrationFrequency;
			this.vibrationStrength = vibrationStrength;
			if (strengthFadeLength > 0f)
			{
				strengthFade = true;
				fadeLength = strengthFadeLength;
				fadeTimer = strengthFadeLength;
			}
		}

		public void SetModeVibrationSlope(int vibrationFrequency, byte[] vibrationStrengthsArray, float strengthFadeLength = 0f)
		{
			Clear();
			active = true;
			mode = Mode.VibrationSlope;
			this.vibrationFrequency = vibrationFrequency;
			variableStrengthArray = vibrationStrengthsArray;
			if (strengthFadeLength > 0f)
			{
				strengthFade = true;
				fadeLength = strengthFadeLength;
				fadeTimer = strengthFadeLength;
			}
		}
	}

	private static uint s_boostID;

	private static uint s_hazardDamageID;

	private static uint s_supernovaID;

	private static uint s_playerTurbulenceID;

	private static uint s_shipTurbulenceID;

	private static uint s_eyeVortexID;

	private static uint s_vesselWarpID;

	private static uint s_waterRumbleID;

	private static uint s_sandRumbleID;

	private static uint s_rapidsRumbleID;

	private static float s_waterRumbleScale;

	private static float s_sandRumbleScale;

	private static float s_rapidsRumbleProgress;

	private static float s_rapidsRumbleScale;

	private static TriggerEffect s_triggerEffectJetpack;

	private static TriggerEffect s_triggerEffectJetpackBoost;

	private static TriggerEffect s_triggerEffectShipControls;

	private static TriggerEffect s_triggerEffectShipIgnition;

	private static TriggerEffect s_triggerEffectMap;

	private const int s_maxRumble = 128;

	private const float s_rumbleLowPower = 1.4285715f;

	private const float s_rumbleHighPower = 1.4285715f;

	private static RumbleManager s_theManager;

	private Rumble[] m_theList;

	private bool m_isEnabled = true;

	private static uint s_nextId = 1u;

	private TriggerEffect[] m_triggerEffectStack;

	public static void PulseProbeLaunch()
	{
		Pulse(0.3f, 0.6f, 0.1f);
	}

	public static void PulseLightImpact()
	{
		Pulse(0.2f, 0.2f, 0.15f);
	}

	public static void PulseMediumImpact()
	{
		Pulse(0.5f, 0.2f, 0.4f);
	}

	public static void PulseHeavyImpact()
	{
		Pulse(1f, 0.5f, 0.7f);
	}

	public static void PulseShipExplode()
	{
		Pulse(1f, 0.8f, 0.8f);
	}

	public static void PulseEject()
	{
		Pulse(0.5f, 0.5f, 0.15f);
	}

	public static void PlayShipIgnition()
	{
		Fade(0.5f, 0.5f, 3f);
		s_triggerEffectShipIgnition.SetModeVibrationConstant(12, 5, 3f);
	}

	public static void PulseQuantumLightning()
	{
		Pulse(0.5f, 0.2f, 0.4f);
	}

	public static void PlayGalaxyZoom()
	{
		Fade(0.4f, 0.3f, 0.5f);
	}

	public static void PlayCosmicInflation()
	{
		Fade(0.5f, 0.2f, 1f);
	}

	public static void PulseFirstContactDamage(InstantDamageType damageType)
	{
		if (damageType == InstantDamageType.Puncture || damageType == InstantDamageType.Electrical)
		{
			Pulse(0.7f, 0.4f, 0.2f);
		}
	}

	public static void PulseRaftImpact(AudioType impactAudio)
	{
		switch (impactAudio)
		{
		case AudioType.Raft_Impact_Light:
			PulseLightImpact();
			break;
		case AudioType.Raft_Impact_Medium:
			PulseMediumImpact();
			break;
		case AudioType.Raft_Impact_Heavy:
			PulseHeavyImpact();
			break;
		}
	}

	public static void PlayStationShudder(float scalar)
	{
		Fade(0.2f * scalar, 0.15f * scalar, 1.5f);
	}

	public static void PlayDamBreak(float scalar)
	{
		Fade(0.7f * scalar, 0.7f * scalar, 1.5f);
	}

	public static void PlayGhostGrab()
	{
		if (!PlayerData.GetReducedFrights())
		{
			Pulse(0.4f, 0.2f, 0.2f);
		}
	}

	public static void PlayGhostBlowOutLantern()
	{
		if (!PlayerData.GetReducedFrights())
		{
			Pulse(0.1f, 0.2f, 0.5f);
		}
	}

	public static void PlayGhostNeckSnap()
	{
		if (!PlayerData.GetReducedFrights())
		{
			Pulse(1f, 0.5f, 0.2f);
		}
	}

	public static void PlayerCrushedByElevator()
	{
		Pulse(1f, 0.5f, 0.4f);
	}

	public static void StartMapMode()
	{
		s_triggerEffectMap.SetModeNone();
	}

	public static void StopMapMode()
	{
		s_triggerEffectMap.Clear();
	}

	public static void StartJetpackBoost()
	{
		Modify(s_boostID, 0.2f, 0.3f);
	}

	public static void StopJetpackBoost()
	{
		Modify(s_boostID, 0f, 0f);
	}

	public static void SetShipThrottleCold()
	{
	}

	public static void SetShipThrottleLocked()
	{
	}

	public static void SetShipThrottleNormal()
	{
	}

	public static void SetShipThrottleOff()
	{
		s_triggerEffectShipControls.Clear();
	}

	public static void StartEyeVortex()
	{
		Modify(s_eyeVortexID, 0.3f, 0.3f);
	}

	public static void StopEyeVortex()
	{
		Modify(s_eyeVortexID, 0f, 0f);
	}

	public static void StartVesselWarp()
	{
		Modify(s_vesselWarpID, 0.3f, 0.3f);
	}

	public static void StopVesselWarp()
	{
		Modify(s_vesselWarpID, 0f, 0f);
	}

	public static void UpdateAirTurbulence(float fluidSpeed, float fluidDensity, bool isShip)
	{
		if (isShip)
		{
			if (PlayerState.IsInsideShip() && fluidDensity < 5f && !PlayerState.IsDead())
			{
				float num = Mathf.InverseLerp(150f, 250f, fluidSpeed);
				Modify(s_shipTurbulenceID, 0.7f * num, 0.2f * num);
				s_triggerEffectShipControls.SetModeVibrationConstant(24, Mathf.RoundToInt(4f * num));
			}
			else
			{
				Modify(s_shipTurbulenceID, 0f, 0f);
				s_triggerEffectShipControls.SetModeNone();
			}
		}
		else if (fluidDensity < 5f && !PlayerState.IsDead())
		{
			float num2 = Mathf.InverseLerp(100f, 200f, fluidSpeed);
			Modify(s_playerTurbulenceID, 0.7f * num2, 0.2f * num2);
		}
		else
		{
			Modify(s_playerTurbulenceID, 0f, 0f);
		}
	}

	public static void UpdateHazardDamage(float damage, HazardDetector hazardDetector)
	{
		if (damage > 0f && !PlayerState.IsDead())
		{
			Modify(s_hazardDamageID, 0.5f, 0.5f);
		}
		else
		{
			Modify(s_hazardDamageID, 0f, 0f);
		}
	}

	public static void UpdateSupernova(float distanceToPlayer)
	{
		if (!PlayerState.IsDead() && !TimelineObliterationController.IsRealityEnding())
		{
			float num = Mathf.InverseLerp(4000f, 0f, distanceToPlayer);
			Modify(s_supernovaID, 0.8f * num, 0.2f * num);
		}
	}

	public static void PlayEndOfRealityRumble()
	{
		Modify(s_supernovaID, 0f, 0f);
	}

	public static void PlayDeathRumble(DeathType deathType, float deathDuration)
	{
		Modify(s_supernovaID, 0f, 0f);
		switch (deathType)
		{
		case DeathType.Energy:
		case DeathType.Supernova:
		case DeathType.BigBang:
		case DeathType.Lava:
		case DeathType.DreamExplosion:
			Fade(0.8f, 0.2f, deathDuration);
			break;
		case DeathType.Digestion:
			Fade(0.2f, 0.2f, deathDuration);
			break;
		case DeathType.Crushed:
			Pulse(0.2f, 0.2f, deathDuration);
			break;
		case DeathType.Default:
		case DeathType.Impact:
		case DeathType.Asphyxiation:
		case DeathType.Meditation:
		case DeathType.TimeLoop:
		case DeathType.BlackHole:
		case DeathType.Dream:
		case DeathType.CrushedByElevator:
			break;
		}
	}

	public static void AddFluidRumble(FluidVolume.Type fluidType, float rumbleScale)
	{
		switch (fluidType)
		{
		case FluidVolume.Type.WATER:
			s_waterRumbleScale = Mathf.Min(s_waterRumbleScale + rumbleScale, 1f);
			break;
		case FluidVolume.Type.SAND:
			s_sandRumbleScale = Mathf.Min(s_sandRumbleScale + rumbleScale, 1f);
			break;
		}
	}

	public static void AddRapidsRumble(float scale, float progress)
	{
		s_rapidsRumbleProgress = progress;
		s_rapidsRumbleScale = scale;
	}

	public static void SetEnabled(bool enabled)
	{
		if (null != s_theManager)
		{
			s_theManager.m_isEnabled = enabled;
		}
	}

	public static bool GetEnabled()
	{
		if (null != s_theManager)
		{
			return s_theManager.m_isEnabled;
		}
		return false;
	}

	private void FixedUpdate()
	{
		Modify(s_waterRumbleID, 0.5f * s_waterRumbleScale, 0.2f * s_waterRumbleScale);
		s_waterRumbleScale = 0f;
		Modify(s_sandRumbleID, 0.1f * s_sandRumbleScale, 0.4f * s_sandRumbleScale);
		s_sandRumbleScale = 0f;
		Modify(s_rapidsRumbleID, Mathf.Lerp(0.1f, 0.4f, s_rapidsRumbleScale) * s_rapidsRumbleProgress, 0.2f * s_rapidsRumbleProgress);
		s_rapidsRumbleProgress = 0f;
		s_rapidsRumbleScale = 0f;
	}

	public void InitializeOnAwake()
	{
		if (null == s_theManager)
		{
			s_theManager = this;
			m_theList = new Rumble[128];
			for (int i = 0; i < 128; i++)
			{
				m_theList[i] = new Rumble();
			}
			s_boostID = Steady(0f, 0f);
			s_hazardDamageID = Steady(0f, 0f);
			s_supernovaID = Steady(0f, 0f);
			s_playerTurbulenceID = Steady(0f, 0f);
			s_shipTurbulenceID = Steady(0f, 0f);
			s_eyeVortexID = Steady(0f, 0f);
			s_vesselWarpID = Steady(0f, 0f);
			s_waterRumbleID = Steady(0f, 0f);
			s_sandRumbleID = Steady(0f, 0f);
			s_rapidsRumbleID = Steady(0f, 0f);
			s_waterRumbleScale = 0f;
			s_sandRumbleScale = 0f;
			s_rapidsRumbleProgress = 0f;
			s_rapidsRumbleScale = 0f;
			s_triggerEffectJetpack = new TriggerEffect(InputConsts.InputCommandType.THRUST_UP, InputConsts.InputCommandType.THRUST_DOWN);
			s_triggerEffectJetpackBoost = new TriggerEffect(InputConsts.InputCommandType.THRUST_UP);
			s_triggerEffectShipControls = new TriggerEffect(InputConsts.InputCommandType.THRUST_UP, InputConsts.InputCommandType.THRUST_DOWN);
			s_triggerEffectShipIgnition = new TriggerEffect(InputConsts.InputCommandType.THRUST_UP, InputConsts.InputCommandType.THRUST_DOWN);
			s_triggerEffectMap = new TriggerEffect();
			m_triggerEffectStack = new TriggerEffect[5] { s_triggerEffectJetpack, s_triggerEffectJetpackBoost, s_triggerEffectShipControls, s_triggerEffectShipIgnition, s_triggerEffectMap };
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Equals(s_theManager))
		{
			Release(s_boostID);
			Release(s_hazardDamageID);
			Release(s_supernovaID);
			Release(s_playerTurbulenceID);
			Release(s_shipTurbulenceID);
			Release(s_eyeVortexID);
			Release(s_vesselWarpID);
			Release(s_waterRumbleID);
			Release(s_sandRumbleID);
			Release(s_rapidsRumbleID);
			s_triggerEffectJetpack.Clear();
			s_triggerEffectJetpackBoost.Clear();
			s_triggerEffectShipControls.Clear();
			s_triggerEffectShipIgnition.Clear();
			s_triggerEffectMap.Clear();
		}
	}

	private static uint Steady(float loPower, float hiPower)
	{
		if (s_theManager == null || !s_theManager.m_isEnabled)
		{
			return 0u;
		}
		return Allocate(loPower, hiPower)?.m_id ?? 0;
	}

	private static uint Pulse(float loPower, float hiPower, float pulseTime)
	{
		if (s_theManager == null || !s_theManager.m_isEnabled)
		{
			return 0u;
		}
		Rumble rumble = Allocate(loPower, hiPower);
		if (rumble != null)
		{
			rumble.m_fade = Rumble.Fade.PULSE;
			rumble.m_timer = pulseTime;
			return rumble.m_id;
		}
		return 0u;
	}

	private static uint Fade(float loPower, float hiPower, float fadeTime)
	{
		if (s_theManager == null || !s_theManager.m_isEnabled)
		{
			return 0u;
		}
		Rumble rumble = Allocate(loPower, hiPower);
		if (rumble != null)
		{
			rumble.m_fade = Rumble.Fade.FADE;
			rumble.m_timer = fadeTime;
			rumble.m_fadeTime = fadeTime;
			return rumble.m_id;
		}
		return 0u;
	}

	private static void Modify(uint id, float loPower, float hiPower)
	{
		if (!(null != s_theManager) || !s_theManager.m_isEnabled)
		{
			return;
		}
		for (int i = 0; i < 128; i++)
		{
			Rumble rumble = s_theManager.m_theList[i];
			if (rumble.m_id == id)
			{
				rumble.m_power = new Vector2(loPower, hiPower);
				break;
			}
		}
	}

	private static Rumble Allocate(float loPower, float hiPower)
	{
		if (null != s_theManager)
		{
			for (int i = 0; i < 128; i++)
			{
				Rumble rumble = s_theManager.m_theList[i];
				if (!rumble.IsAlive())
				{
					rumble.m_fade = Rumble.Fade.STEADY;
					rumble.m_id = s_nextId;
					s_nextId++;
					rumble.m_power = new Vector2(loPower, hiPower);
					return rumble;
				}
			}
		}
		return null;
	}

	private static void Release(uint id)
	{
		if (!(null != s_theManager))
		{
			return;
		}
		for (int i = 0; i < 128; i++)
		{
			Rumble rumble = s_theManager.m_theList[i];
			if (rumble.m_id == id)
			{
				rumble.Destroy();
				break;
			}
		}
	}

	private void Update()
	{
		if (OWTime.IsPaused())
		{
			OWInput.Rumble(0f, 0f);
			return;
		}
		Vector2 zero = Vector2.zero;
		if (m_isEnabled && OWInput.UsingGamepad())
		{
			float deltaTime = Time.deltaTime;
			for (int i = 0; i < m_theList.Length; i++)
			{
				Rumble rumble = m_theList[i];
				if (rumble.IsAlive())
				{
					rumble.Update(deltaTime);
				}
				if (rumble.IsAlive())
				{
					zero += rumble.GetPower();
				}
			}
			zero.x *= 1.4285715f;
			zero.y *= 1.4285715f;
		}
		OWInput.Rumble(zero.y, zero.x);
		if (!m_isEnabled)
		{
			return;
		}
		float deltaTime2 = Time.deltaTime;
		for (int j = 0; j < m_triggerEffectStack.Length; j++)
		{
			m_triggerEffectStack[j].Update(deltaTime2);
		}
		TriggerEffect triggerEffect = null;
		TriggerEffect triggerEffect2 = null;
		int num = m_triggerEffectStack.Length - 1;
		while (num >= 0)
		{
			if (triggerEffect == null && m_triggerEffectStack[num].active && m_triggerEffectStack[num].AffectsLeftTrigger())
			{
				triggerEffect = m_triggerEffectStack[num];
			}
			if (triggerEffect2 == null && m_triggerEffectStack[num].active && m_triggerEffectStack[num].AffectsRightTrigger())
			{
				triggerEffect2 = m_triggerEffectStack[num];
			}
			if (triggerEffect == null || triggerEffect2 == null)
			{
				num--;
				continue;
			}
			break;
		}
	}
}
