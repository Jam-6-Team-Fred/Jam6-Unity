using System;
using UnityEngine;

public class Campfire : MonoBehaviour
{
	public enum State
	{
		UNLIT = 0,
		LIT = 1,
		SMOLDERING = 2
	}

	public delegate void CampfireEvent(Campfire fire);

	private const float ROASTING_DISTANCE = 2f;

	private const float SLEEPING_DISTANCE = 2f;

	[SerializeField]
	private State _initialState = State.LIT;

	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	private bool _canSleepHere = true;

	[SerializeField]
	private bool _lookUpWhileSleeping;

	[Space]
	[SerializeField]
	private float _heatConeBottom;

	[SerializeField]
	private float _heatConeTop;

	[SerializeField]
	private float _heatConeRadius;

	[SerializeField]
	private float _heatFalloffDistance;

	[Space]
	[SerializeField]
	private float _logSphereCenter;

	[SerializeField]
	private float _logSphereRadius;

	[SerializeField]
	private float _rockHeight;

	[Space]
	[SerializeField]
	protected OWAudioSource _audio;

	[SerializeField]
	protected OWAudioSource _oneShotAudio;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWRenderer[] _litRenderers;

	[SerializeField]
	private OWRenderer[] _hideWhileSmolderingRenderers;

	[SerializeField]
	private ParticleSystem[] _smolderingParticles;

	[SerializeField]
	private ParticleSystem[] _litParticles;

	[SerializeField]
	private MeshRenderer _flames;

	[SerializeField]
	private MeshRenderer _embers;

	[SerializeField]
	private MeshRenderer _ash;

	[Space]
	[SerializeField]
	private SingleInteractionVolume _interactVolume;

	[SerializeField]
	private PlayerAttachPoint _attachPoint;

	[SerializeField]
	private HazardVolume _hazardVolume;

	[SerializeField]
	private Transform _burnedSlideReelSocket;

	private PlayerLockOnTargeting _lockOnTargeting;

	protected State _state;

	private bool _isPlayerRoasting;

	private bool _isPlayerSleeping;

	private bool _isTimeFastForwarding;

	private bool _interactVolumeFocus;

	private bool _playerInSector;

	private bool _dropSlideReelMode;

	private bool _hasBurnedSlideReel;

	private float _litFraction;

	private const float c_smolderingLitFraction = 0.4f;

	private float _fastForwardStartTime;

	private float _fastForwardMultiplier = 1f;

	private ScreenPrompt _sleepPrompt;

	private ScreenPrompt _wakePrompt;

	public event CampfireEvent OnCampfireStateChange;

	protected virtual void Awake()
	{
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract += OnPressInteract;
			if (_canSleepHere)
			{
				_interactVolume.OnGainFocus += OnGainFocus;
				_interactVolume.OnLoseFocus += OnLoseFocus;
				_sleepPrompt = new ScreenPrompt(InputLibrary.interactSecondary, UITextLibrary.GetString(UITextType.CampfireDozeOff));
				_wakePrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.WakeUpPrompt));
			}
		}
		else
		{
			_canSleepHere = false;
		}
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.AddListener("FakePlayerMeditationDeath", OnFakePlayerMeditationDeath);
	}

	protected virtual void Start()
	{
		if (Locator.GetPlayerTransform() != null)
		{
			_lockOnTargeting = Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>();
		}
		if (_audio != null)
		{
			_audio.SetLocalVolume(0f);
		}
		if (_canSleepHere)
		{
			_sleepPrompt.SetDisplayState((!CanSleepHereNow()) ? ScreenPrompt.DisplayState.GrayedOut : ScreenPrompt.DisplayState.Normal);
		}
		base.enabled = false;
		SetState(_initialState, forceStateUpdate: true);
	}

	protected virtual void OnDestroy()
	{
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract -= OnPressInteract;
			if (_canSleepHere)
			{
				_interactVolume.OnGainFocus -= OnGainFocus;
				_interactVolume.OnLoseFocus -= OnLoseFocus;
			}
		}
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
		GlobalMessenger.RemoveListener("FakePlayerMeditationDeath", OnFakePlayerMeditationDeath);
	}

	public Sector GetSector()
	{
		return _sector;
	}

	public State GetState()
	{
		return _state;
	}

	public void SetInteractionEnabled(bool enabled)
	{
		if (_interactVolume != null)
		{
			if (enabled)
			{
				_interactVolume.EnableInteraction();
			}
			else
			{
				_interactVolume.DisableInteraction();
			}
		}
	}

	public void PlayOneShot(AudioType audioType)
	{
		_oneShotAudio.PlayOneShot(audioType);
	}

	public void SetInitialState(State initialState)
	{
		_initialState = initialState;
	}

	public void SetState(State newState, bool forceStateUpdate = false)
	{
		if (!(_state != newState || forceStateUpdate))
		{
			return;
		}
		_state = newState;
		_hazardVolume.SetVolumeActivation(_state == State.LIT);
		switch (_state)
		{
		case State.LIT:
		{
			for (int j = 0; j < _litRenderers.Length; j++)
			{
				_litRenderers[j].SetActivation(active: true);
			}
			if (_oneShotAudio != null && !forceStateUpdate)
			{
				_oneShotAudio.PlayOneShot(AudioType.TH_Campfire_Ignite);
			}
			break;
		}
		case State.SMOLDERING:
		{
			for (int k = 0; k < _hideWhileSmolderingRenderers.Length; k++)
			{
				_hideWhileSmolderingRenderers[k].SetActivation(active: false);
			}
			break;
		}
		case State.UNLIT:
		{
			for (int i = 0; i < _litRenderers.Length; i++)
			{
				_litRenderers[i].SetActivation(active: false);
			}
			break;
		}
		}
		CheckParticleActivation(forceStateUpdate);
		CheckLoopingAudioActivation();
		if (_state == State.LIT)
		{
			_lightController.FadeTo(1f, 1f);
		}
		else
		{
			_lightController.FadeTo(0f, forceStateUpdate ? 0f : 1f);
		}
		UITextType promptID = UITextType.LightCampfirePrompt;
		if (_state == State.LIT)
		{
			promptID = UITextType.RoastingPrompt;
		}
		if (_interactVolume != null)
		{
			_interactVolume.ChangePrompt(promptID);
		}
		if (forceStateUpdate)
		{
			switch (_state)
			{
			case State.LIT:
				SetLitFraction(1f);
				break;
			case State.SMOLDERING:
				SetLitFraction(0.4f);
				break;
			case State.UNLIT:
				SetLitFraction(0f);
				break;
			}
		}
		if (!forceStateUpdate && this.OnCampfireStateChange != null)
		{
			this.OnCampfireStateChange(this);
		}
		base.enabled = true;
	}

	public bool CheckStickIntersection(Vector3 stickPivotPos, Vector3 stickPivotForward, out Vector3 intersectPoint)
	{
		return OWMath.RaySphereIntersection(stickPivotPos, stickPivotForward, base.transform.position + base.transform.up * _logSphereCenter, _logSphereRadius, out intersectPoint);
	}

	public float GetRockHeight()
	{
		return _rockHeight;
	}

	public float GetHeatAtPosition(Vector3 worldPosition)
	{
		Vector3 vector = base.transform.TransformPoint(new Vector3(0f, _heatConeBottom, 0f));
		Vector3 vector2 = base.transform.TransformPoint(new Vector3(0f, _heatConeTop, 0f));
		Vector3 vector3 = worldPosition - vector;
		Vector3 vector4 = vector + Vector3.ProjectOnPlane(vector3, base.transform.up).normalized * _heatConeRadius;
		Vector3 from = vector2 - vector4;
		Vector3 to = worldPosition - vector4;
		float num = OWMath.Angle(axis: Vector3.Cross(base.transform.up, vector3), from: from, to: to);
		float value = to.magnitude * Mathf.Sin((float)Math.PI / 180f * num);
		float num2 = Mathf.InverseLerp(_heatFalloffDistance, 0f, value);
		float num3 = Mathf.Lerp(0f, 25.1f, num2 * num2);
		if (_state != State.LIT)
		{
			num3 *= 0.5f;
		}
		return num3;
	}

	public void StopRoasting()
	{
		if (_isPlayerRoasting)
		{
			_attachPoint.DetachPlayer();
			_lockOnTargeting.BreakLock();
			_interactVolume.ResetInteraction();
			if (PlayerState.InZeroG())
			{
				Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().CenterCamera(50f, smoothStep: true);
			}
			_isPlayerRoasting = false;
			GlobalMessenger.FireEvent("ExitRoastingMode");
			if (Locator.GetPlayerSuit().IsWearingSuit())
			{
				Locator.GetPlayerSuit().PutOnHelmet();
			}
		}
	}

	protected virtual bool CheckUnequipToolWhileSleeping()
	{
		return true;
	}

	protected virtual void OnStopSleeping()
	{
		if (_state == State.LIT && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.DreamLantern && ((DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem()).GetLanternType() != DreamLanternType.Nonfunctioning)
		{
			Achievements.Earn(Achievements.Type.SIMULATION);
		}
	}

	private void OnPressInteract()
	{
		if (_state == State.LIT)
		{
			if (_dropSlideReelMode)
			{
				SlideReelItem obj = (SlideReelItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
				Locator.GetToolModeSwapper().GetItemCarryTool().DropItemInstantly(_sector, _burnedSlideReelSocket);
				obj.Burn();
				SetDropSlideReelMode(dropSlideReelMode: false);
				_hasBurnedSlideReel = true;
				_oneShotAudio.PlayOneShot(AudioType.TH_Campfire_Ignite);
			}
			else
			{
				StartRoasting();
			}
		}
		else
		{
			SetState(State.LIT);
			Locator.GetFlashlight().TurnOff(playAudio: false);
			if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.SlideReel)
			{
				SetDropSlideReelMode(dropSlideReelMode: true);
			}
		}
	}

	private void OnGainFocus()
	{
		_interactVolumeFocus = true;
		if (_canSleepHere)
		{
			Locator.GetPromptManager().AddScreenPrompt(_sleepPrompt, PromptPosition.Center);
		}
		if (_state == State.LIT)
		{
			SetDropSlideReelMode(Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == ItemType.SlideReel);
		}
	}

	private void OnLoseFocus()
	{
		_interactVolumeFocus = false;
		if (_canSleepHere)
		{
			Locator.GetPromptManager().RemoveScreenPrompt(_sleepPrompt, PromptPosition.Center);
		}
	}

	private void OnPlayerDeath(DeathType deathType)
	{
		if (_isPlayerRoasting)
		{
			_attachPoint.DetachPlayer();
			_lockOnTargeting.BreakLock();
			_interactVolume.ResetInteraction();
			if (PlayerState.InZeroG())
			{
				Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().CenterCamera(50f, smoothStep: true);
			}
			_isPlayerRoasting = false;
			InputMode inputMode = OWInput.GetInputMode();
			GlobalMessenger.FireEvent("ExitRoastingMode");
			OWInput.ChangeInputMode(inputMode);
		}
	}

	private void OnFakePlayerMeditationDeath()
	{
		OnPlayerDeath(DeathType.Meditation);
	}

	private void SetDropSlideReelMode(bool dropSlideReelMode)
	{
		if (!dropSlideReelMode || (!(_burnedSlideReelSocket == null) && !_hasBurnedSlideReel))
		{
			if (!_dropSlideReelMode && dropSlideReelMode)
			{
				_dropSlideReelMode = true;
				string promptText = UITextLibrary.GetString(UITextType.ItemDropPrompt) + UITextLibrary.GetString(UITextType.ItemSlideReelPrompt);
				_interactVolume.ChangePrompt(promptText);
			}
			else if (_dropSlideReelMode && !dropSlideReelMode)
			{
				_dropSlideReelMode = false;
				_interactVolume.ChangePrompt(UITextType.RoastingPrompt);
			}
		}
	}

	private void StartRoasting()
	{
		Locator.GetToolModeSwapper().UnequipTool();
		_attachPoint.AttachPlayer();
		Vector3 localPosition = Locator.GetPlayerTransform().localPosition;
		Vector3 attachOffset = 2f * new Vector3(localPosition.x, 0f, localPosition.z).normalized + Vector3.up;
		_attachPoint.SetAttachOffset(attachOffset);
		Vector3 localOffset = Vector3.up * 0.75f;
		_lockOnTargeting.LockOn(base.transform, localOffset, 1f, useZoom: true);
		_isPlayerRoasting = true;
		GlobalMessenger<Campfire>.FireEvent("EnterRoastingMode", this);
		if (Locator.GetPlayerSuit().IsWearingSuit())
		{
			Locator.GetPlayerSuit().RemoveHelmet();
		}
		if (_canSleepHere)
		{
			_sleepPrompt.SetVisibility(isVisible: false);
		}
	}

	private void StartSleeping()
	{
		if (CheckUnequipToolWhileSleeping())
		{
			Locator.GetToolModeSwapper().UnequipTool();
		}
		_attachPoint.AttachPlayer();
		_interactVolume.DisableInteraction();
		Vector3 localPosition = Locator.GetPlayerTransform().localPosition;
		Vector3 attachOffset = 2f * new Vector3(localPosition.x, 0f, localPosition.z).normalized + Vector3.up;
		_attachPoint.SetAttachOffset(attachOffset);
		if (_lookUpWhileSleeping)
		{
			_lockOnTargeting.LockOn(base.transform, Vector3.up * 10f, 1f, useZoom: true);
		}
		else
		{
			_lockOnTargeting.LockOn(base.transform, Vector3.up * 0.75f, 1f, useZoom: true);
		}
		Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().CloseEyes(3f);
		Locator.GetAudioMixer().MixSleepAtCampfire(3f);
		Locator.GetPlayerAudioController().OnStartSleepingAtCampfire(this is DreamCampfire);
		_fastForwardStartTime = Time.timeSinceLevelLoad + 3f;
		_isPlayerSleeping = true;
		Locator.GetPromptManager().AddScreenPrompt(_wakePrompt, PromptPosition.Center);
		_sleepPrompt.SetVisibility(isVisible: false);
		_wakePrompt.SetVisibility(isVisible: false);
		OWInput.ChangeInputMode(InputMode.None);
		if (Locator.GetPlayerSuit().IsWearingSuit())
		{
			Locator.GetPlayerSuit().RemoveHelmet();
		}
		Locator.GetFlashlight().TurnOff(playAudio: false);
		GlobalMessenger<bool>.FireEvent("StartSleepingAtCampfire", this is DreamCampfire);
	}

	public void StopSleeping(bool sudden = false)
	{
		if (_isPlayerSleeping)
		{
			if (_isTimeFastForwarding)
			{
				StopFastForwarding();
			}
			_attachPoint.DetachPlayer();
			_lockOnTargeting.BreakLock();
			_interactVolume.EnableInteraction();
			if (_lookUpWhileSleeping || PlayerState.InZeroG())
			{
				Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().CenterCamera(50f, smoothStep: true);
			}
			Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OpenEyes(1f, sudden);
			Locator.GetAudioMixer().UnmixSleepAtCampfire(sudden ? 1f : 3f);
			Locator.GetPlayerAudioController().OnStopSleepingAtCampfire(sudden || Time.timeSinceLevelLoad - _fastForwardStartTime > 60f, sudden);
			_isPlayerSleeping = false;
			Locator.GetPromptManager().RemoveScreenPrompt(_wakePrompt);
			OWInput.ChangeInputMode(InputMode.Character);
			if (Locator.GetPlayerSuit().IsWearingSuit())
			{
				Locator.GetPlayerSuit().PutOnHelmetAfterDelay(2f);
			}
			OnStopSleeping();
			GlobalMessenger.FireEvent("StopSleepingAtCampfire");
		}
	}

	private void StartFastForwarding()
	{
		Locator.GetPlayerCamera().enabled = false;
		_isTimeFastForwarding = true;
		_fastForwardMultiplier = 1f;
		OWTime.SetMaxDeltaTime(1f / 30f);
		GlobalMessenger.FireEvent("StartFastForward");
	}

	private void StopFastForwarding()
	{
		Locator.GetPlayerCamera().enabled = true;
		_isTimeFastForwarding = false;
		_fastForwardMultiplier = 1f;
		OWTime.SetTimeScale(1f);
		OWTime.SetMaxDeltaTime(1f / 15f);
		GlobalMessenger.FireEvent("EndFastForward");
	}

	private void Update()
	{
		float num = 0f;
		switch (_state)
		{
		case State.LIT:
			num = 1f;
			break;
		case State.SMOLDERING:
			num = 0.4f;
			break;
		case State.UNLIT:
			num = 0f;
			break;
		}
		if (_litFraction != num)
		{
			SetLitFraction(Mathf.MoveTowards(_litFraction, num, Time.deltaTime));
		}
		if (_canSleepHere)
		{
			_sleepPrompt.SetVisibility(isVisible: false);
			if (_interactVolumeFocus && !_isPlayerSleeping && !_isPlayerRoasting && OWInput.IsInputMode(InputMode.Character))
			{
				_sleepPrompt.SetVisibility(isVisible: true);
				_sleepPrompt.SetDisplayState((!CanSleepHereNow()) ? ScreenPrompt.DisplayState.GrayedOut : ScreenPrompt.DisplayState.Normal);
				if (OWInput.IsNewlyPressed(InputLibrary.interactSecondary) && CanSleepHereNow())
				{
					StartSleeping();
				}
			}
		}
		if (_isPlayerSleeping && !_isTimeFastForwarding && Time.timeSinceLevelLoad > _fastForwardStartTime)
		{
			StartFastForwarding();
		}
		if (_isTimeFastForwarding)
		{
			_wakePrompt.SetVisibility(OWInput.IsInputMode(InputMode.None) && Time.timeSinceLevelLoad - _fastForwardStartTime > GetWakePromptDelay());
			if (ShouldWakeUp())
			{
				StopSleeping();
			}
			else if (!OWTime.IsPaused())
			{
				_fastForwardMultiplier = Mathf.MoveTowards(_fastForwardMultiplier, 10f, 2f * Time.unscaledDeltaTime);
				OWTime.SetTimeScale(_fastForwardMultiplier);
			}
		}
	}

	protected virtual float GetWakePromptDelay()
	{
		return 10f;
	}

	protected virtual bool CanSleepHereNow()
	{
		if (_state == State.LIT && TimeLoop.IsTimeFlowing() && TimeLoop.GetSecondsRemaining() > 85f)
		{
			return OWInput.IsInputMode(InputMode.Character);
		}
		return false;
	}

	protected virtual bool ShouldWakeUp()
	{
		if (!OWInput.IsInputMode(InputMode.None) || (!OWInput.IsNewlyPressed(InputLibrary.interact) && !OWInput.IsNewlyPressed(InputLibrary.cancel) && !OWInput.IsNewlyPressed(InputLibrary.interactSecondary)))
		{
			return TimeLoop.GetSecondsRemaining() < 85f;
		}
		return true;
	}

	private void SetLitFraction(float fraction)
	{
		_litFraction = fraction;
		Vector2 textureOffset = _flames.material.GetTextureOffset("_MainTex");
		textureOffset.y = 1f - _litFraction;
		_flames.material.SetTextureOffset("_MainTex", textureOffset);
		if (_embers != null)
		{
			_embers.material.SetColor("_EmissionColor", new Color(_litFraction * 1.5f, _litFraction * 1.5f, _litFraction * 1.5f));
		}
		if (_ash != null)
		{
			_ash.material.SetColor("_EmissionColor", new Color(_litFraction * 1.5f, _litFraction * 1.5f, _litFraction * 1.5f));
		}
	}

	private void CheckParticleActivation(bool forceStateUpdate = false)
	{
		for (int i = 0; i < _smolderingParticles.Length; i++)
		{
			_smolderingParticles[i].Stop();
			if (forceStateUpdate)
			{
				_smolderingParticles[i].Clear();
			}
		}
		for (int j = 0; j < _litParticles.Length; j++)
		{
			_litParticles[j].Stop();
			if (forceStateUpdate)
			{
				_litParticles[j].Clear();
			}
		}
		if (!(_sector == null) && !_playerInSector)
		{
			return;
		}
		for (int k = 0; k < _smolderingParticles.Length; k++)
		{
			if (_state == State.SMOLDERING)
			{
				_smolderingParticles[k].Play();
			}
		}
		for (int l = 0; l < _litParticles.Length; l++)
		{
			if (_state == State.LIT)
			{
				_litParticles[l].Play();
			}
		}
	}

	private void CheckLoopingAudioActivation()
	{
		if (_audio == null)
		{
			return;
		}
		if (_sector != null && !_playerInSector)
		{
			_audio.FadeOut(2f);
			return;
		}
		switch (_state)
		{
		case State.LIT:
			_audio.FadeIn(0.5f);
			break;
		case State.SMOLDERING:
			_audio.FadeIn(0.5f, fadeFromNothing: false, randomizePlayhead: false, 0.5f);
			break;
		case State.UNLIT:
			_audio.FadeOut(0.5f);
			break;
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsOccupant(DynamicOccupant.Player);
		if (_playerInSector != flag)
		{
			_playerInSector = flag;
			CheckParticleActivation();
			CheckLoopingAudioActivation();
			base.enabled = _playerInSector;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			Vector3 vector = base.transform.TransformPoint(new Vector3(0f, _heatConeBottom, 0f));
			Vector3 to = base.transform.TransformPoint(new Vector3(0f, _heatConeTop, 0f));
			OWGizmos.DrawWireCircle(vector, base.transform.up, _heatConeRadius);
			Gizmos.DrawLine(vector, to);
			Gizmos.DrawLine(vector + base.transform.right * _heatConeRadius, to);
			Gizmos.DrawLine(vector - base.transform.right * _heatConeRadius, to);
			Gizmos.DrawLine(vector + base.transform.forward * _heatConeRadius, to);
			Gizmos.DrawLine(vector - base.transform.forward * _heatConeRadius, to);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(base.transform.position + base.transform.up * _logSphereCenter, _logSphereRadius);
			OWGizmos.DrawWireCircle(base.transform.position + base.transform.up * _rockHeight, base.transform.up, 0.8f);
		}
	}
}
