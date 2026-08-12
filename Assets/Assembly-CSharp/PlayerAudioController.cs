using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
	[Space(10f)]
	[SerializeField]
	private OWAudioSource _oneShotSource;

	[SerializeField]
	private OWAudioSource _oneShotExternalSource;

	[SerializeField]
	private OWAudioSource _oneShotSleepingAtCampfireSource;

	[SerializeField]
	private OWAudioSource _mapTrackSource;

	[SerializeField]
	private OWAudioSource _repairToolSource;

	[SerializeField]
	private OWAudioSource _translatorSource;

	[SerializeField]
	private OWAudioSource _damageAudioSource;

	[SerializeField]
	private OWAudioSource _damageAudioSourceExternal;

	[SerializeField]
	private OWAudioSource _notificationAudio;

	[SerializeField]
	private OWAudioSource _fluidVolumeSource;

	[SerializeField]
	private OWAudioSource _forceVolumeAudio;

	[SerializeField]
	private OWAudioSource _oxygenLeakSource;

	[SerializeField]
	private OWAudioSource _recorderLoopSource;

	[SerializeField]
	private OWAudioSource _sleepingAtCampfireSource;

	[SerializeField]
	private NomaiTextRevealAudioController[] _nomaiTextAudioControllers;

	private AudioManager _audioManager;

	private FluidDetector _fluidDetector;

	private AudioClip _notificationSfxLoop;

	private int _gravityAudioCount;

	private float _playWearHelmetTime;

	private HazardVolume.HazardType _hazardTypePlaying;

	private void Awake()
	{
		GlobalMessenger<float, float>.AddListener("FlickerOffAndOn", OnFlickerOffAndOn);
		GlobalMessenger.AddListener("SuitUp", PlaySuitUp);
		GlobalMessenger.AddListener("RemoveSuit", PlayRemoveSuit);
		GlobalMessenger.AddListener("PlayerResurrection", OnPlayerResurrection);
	}

	private void Start()
	{
		_audioManager = Locator.GetAudioManager();
		_notificationSfxLoop = _audioManager.GetSingleAudioClip(AudioType.PlayerSuitNotificationTextScroll_LP);
		_fluidDetector = Locator.GetPlayerDetector().GetComponent<FluidDetector>();
		_fluidDetector.OnEnterFluidType += OnEnterFluidType;
		_fluidDetector.OnExitFluidType += OnExitFluidType;
		_oxygenLeakSource.SetLocalVolume(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger<float, float>.RemoveListener("FlickerOffAndOn", OnFlickerOffAndOn);
		GlobalMessenger.RemoveListener("SuitUp", PlaySuitUp);
		GlobalMessenger.RemoveListener("RemoveSuit", PlayRemoveSuit);
		GlobalMessenger.RemoveListener("PlayerResurrection", OnPlayerResurrection);
		_fluidDetector.OnEnterFluidType -= OnEnterFluidType;
		_fluidDetector.OnExitFluidType -= OnExitFluidType;
	}

	private void Update()
	{
		if (Time.time > _playWearHelmetTime)
		{
			base.enabled = false;
			_oneShotSource.PlayOneShot(AudioType.PlayerSuitWearHelmet);
		}
	}

	public void PlayMedkit()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ShipCabinUseMedkit);
	}

	public void PlayRefuel()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ShipCabinUseRefueller);
	}

	public void PlayPlayerSingularityTransit()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.SingularityOnPlayerEnterExit);
	}

	public void PlayWearHelmet()
	{
		if (!PlayerSpacesuit.GetInstantSuitUp())
		{
			base.enabled = true;
			_playWearHelmetTime = Time.time + 0.4f;
		}
	}

	public void PlayRemoveHelmet()
	{
		base.enabled = false;
		if (!PlayerSpacesuit.GetInstantRemoveSuit())
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerSuitRemoveHelmet);
		}
	}

	public void PlayEquipTool()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolTranslatorEquip);
	}

	public void PlayUnequipTool()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolTranslatorUnequip);
	}

	public void PlayProbeSnapshot()
	{
		_oneShotSource.PlayOneShot(AudioType.ToolProbeTakePhoto);
	}

	public void PlayTurnOnFlashlight()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightOn);
	}

	public void PlayTurnOffFlashlight()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightOff);
	}

	public void PlayLockOn()
	{
		_mapTrackSource.PlayOneShot(AudioType.PlayerSuitLockOn);
	}

	public void PlayLockOff()
	{
		_mapTrackSource.PlayOneShot(AudioType.PlayerSuitLockOff);
	}

	public void PlayDialogueEnter(bool isRecording = false)
	{
		if (isRecording)
		{
			_oneShotExternalSource.PlayOneShot(AudioType.TapeRecorder_Start);
			_recorderLoopSource.FadeIn(2f);
		}
		else
		{
			_oneShotExternalSource.PlayOneShot(AudioType.DialogueEnter);
		}
	}

	public void PlayDialogueExit(bool isRecording = false)
	{
		if (!PlayerState.IsDead())
		{
			_oneShotExternalSource.PlayOneShot(isRecording ? AudioType.TapeRecorder_Stop : AudioType.DialogueExit);
		}
		if (isRecording)
		{
			_recorderLoopSource.FadeOut(0.2f);
		}
	}

	public void PlayDialogueHighlightOption()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.Menu_UpDown);
	}

	public void PlayDialogueAdvance()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.DialogueAdvance);
	}

	public void PlayEnterLaunchCodes()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.NonDiaUIAffirmativeSFX);
	}

	public void PlayNegativeUISound()
	{
		_oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
	}

	public void PlayNotificationTextScrolling()
	{
		_notificationAudio.clip = _notificationSfxLoop;
		_notificationAudio.loop = true;
		_notificationAudio.Play();
	}

	public void StopNotificationTextScrolling()
	{
		_notificationAudio.Stop();
	}

	public void PlaySuitWarning()
	{
		if (Locator.GetPlayerSuit().IsWearingHelmet())
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerSuitWarning);
		}
	}

	public void PlaySuitCriticalWarning()
	{
		if (Locator.GetPlayerSuit().IsWearingHelmet())
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerSuitCriticalWarning);
		}
	}

	public void PlayRepairTool()
	{
		_repairToolSource.clip = _audioManager.GetSingleAudioClip(AudioType.ToolRepairing_LP);
		_repairToolSource.Play();
	}

	public void StopRepairTool()
	{
		_repairToolSource.Stop();
	}

	public void PlayRepairCompleteOneShot()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolRepairComplete);
	}

	public void PlayNomaiTextReveal(NomaiWallText wallText)
	{
		for (int i = 0; i < _nomaiTextAudioControllers.Length; i++)
		{
			if (_nomaiTextAudioControllers[i].IsAvailable())
			{
				_nomaiTextAudioControllers[i].PlayTextReveal(wallText);
				break;
			}
		}
	}

	public void PlayTranslateAudio()
	{
		_translatorSource.SetLocalVolume(1f);
		_translatorSource.Play();
	}

	public void StopTranslateAudio()
	{
		_translatorSource.FadeOut(0.1f);
	}

	public void PlayMarshmallowCatchFire()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolMarshmallowIgnite);
	}

	public void PlayMarshmallowBlowOut()
	{
		_oneShotSource.PlayOneShot(AudioType.ToolMarshmallowBlowOut);
	}

	public void PlayMarshmallowEat()
	{
		_oneShotSource.PlayOneShot(AudioType.ToolMarshmallowEat);
	}

	public void PlayMarshmallowEatBurnt()
	{
		_oneShotSource.PlayOneShot(AudioType.ToolMarshmallowEatBurnt);
	}

	public void PlayMarshmallowReplace()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolMarshmallowReplace);
	}

	public void PlayMarshmallowStickEquip()
	{
	}

	public void PlayMarshmallowToss()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.ToolMarshmallowToss);
	}

	public void PlayBadMarshmallowCanPickUp()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.SlideReel_Pickup);
	}

	public void PlayBadMarshmallowEat()
	{
		_oneShotSource.PlayOneShot(AudioType.Menu_KonamiCode);
	}

	public void PlayRefillOxygen(bool shortVersion = false)
	{
		_oneShotSource.PlayOneShot(shortVersion ? AudioType.PlayerSuitOxygenRefill_Short : AudioType.PlayerSuitOxygenRefill);
	}

	public void UpdateSuitPunctures(int punctureCount, int maxPunctureCount)
	{
		float num = (float)punctureCount / (float)maxPunctureCount;
		if (num > 0f)
		{
			if (!_oxygenLeakSource.isPlaying)
			{
				_oxygenLeakSource.SetLocalVolume(0f);
			}
			_oxygenLeakSource.FadeTo(num, 0.2f);
		}
		else
		{
			_oxygenLeakSource.FadeOut(0.2f);
		}
	}

	public void PlayPatchPuncture()
	{
		_oneShotSource.PlayOneShot(AudioType.PlayerSuitPatchPuncture);
	}

	public void PlayPickUpItem(ItemType itemType)
	{
		switch (itemType)
		{
		case ItemType.Scroll:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemScrollPickUp);
			break;
		case ItemType.SharedStone:
		case ItemType.ConversationStone:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemSharedStonePickUp);
			break;
		case ItemType.WarpCore:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCorePickUp);
			break;
		case ItemType.SlideReel:
			_oneShotExternalSource.PlayOneShot(AudioType.SlideReel_Pickup);
			break;
		case ItemType.Lantern:
			_oneShotExternalSource.PlayOneShot(AudioType.Lantern_Pickup);
			break;
		case ItemType.DreamLantern:
		case ItemType.VisionTorch:
			_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Pickup);
			break;
		}
	}

	public void PlayDropItem(ItemType itemType)
	{
		switch (itemType)
		{
		case ItemType.Scroll:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemScrollDrop);
			break;
		case ItemType.SharedStone:
		case ItemType.ConversationStone:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemSharedStoneDrop);
			break;
		case ItemType.WarpCore:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCoreDrop);
			break;
		case ItemType.SlideReel:
			_oneShotExternalSource.PlayOneShot(AudioType.SlideReel_Drop);
			break;
		case ItemType.Lantern:
			_oneShotExternalSource.PlayOneShot(AudioType.Lantern_Drop);
			break;
		case ItemType.DreamLantern:
		case ItemType.VisionTorch:
			_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Drop);
			break;
		}
	}

	public void PlayInsertItem(ItemType itemType)
	{
		switch (itemType)
		{
		case ItemType.Scroll:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemScrollInsert);
			break;
		case ItemType.SharedStone:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemSharedStoneInsert);
			break;
		case ItemType.ConversationStone:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemSharedStoneDrop);
			break;
		case ItemType.WarpCore:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCoreInsert);
			break;
		case ItemType.SlideReel:
			_oneShotExternalSource.PlayOneShot(AudioType.SlideReel_Insert);
			break;
		case ItemType.Lantern:
			_oneShotExternalSource.PlayOneShot(AudioType.Lantern_Insert);
			break;
		case ItemType.DreamLantern:
			_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Insert);
			break;
		case ItemType.VisionTorch:
			_oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_Give);
			break;
		}
	}

	public void PlayRemoveItem(ItemType itemType)
	{
		switch (itemType)
		{
		case ItemType.Scroll:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemScrollRemove);
			break;
		case ItemType.SharedStone:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemSharedStoneRemove);
			break;
		case ItemType.ConversationStone:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemSharedStonePickUp);
			break;
		case ItemType.WarpCore:
			_oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCoreRemove);
			break;
		case ItemType.SlideReel:
			_oneShotExternalSource.PlayOneShot(AudioType.SlideReel_Remove);
			break;
		case ItemType.Lantern:
			_oneShotExternalSource.PlayOneShot(AudioType.Lantern_Remove);
			break;
		case ItemType.DreamLantern:
			_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Remove);
			break;
		case ItemType.VisionTorch:
			_oneShotExternalSource.PlayOneShot(AudioType.VisionTorch_Take);
			break;
		}
	}

	public void PlayHazardFirstContactDamage(HazardVolume hazardVolume)
	{
		HazardVolume.HazardType hazardType = hazardVolume.GetHazardType();
		if (hazardType == HazardVolume.HazardType.GENERAL)
		{
			_oneShotExternalSource.PlayOneShot(AudioType.HazardFirstContactDamage);
		}
	}

	public void UpdateHazardDamage(float damage, HazardDetector hazardDetector)
	{
		HazardVolume.HazardType latestHazardType = hazardDetector.GetLatestHazardType();
		bool flag = damage > 0f && latestHazardType != HazardVolume.HazardType.NONE;
		if (flag)
		{
			if (_hazardTypePlaying != latestHazardType)
			{
				_hazardTypePlaying = latestHazardType;
				AudioType type = AudioType.EnterVolumeDamageHeat_LP;
				if (_hazardTypePlaying == HazardVolume.HazardType.DARKMATTER)
				{
					type = AudioType.EnterVolumeDamageGhostfire_LP;
				}
				else if (_hazardTypePlaying == HazardVolume.HazardType.FIRE)
				{
					type = AudioType.EnterVolumeDamageFire_LP;
				}
				OWAudioSource obj = ((_hazardTypePlaying == HazardVolume.HazardType.HEAT) ? _damageAudioSource : _damageAudioSourceExternal);
				obj.clip = _audioManager.GetSingleAudioClip(type);
				obj.Stop();
				obj.FadeIn(0.2f, fadeFromNothing: true, randomizePlayhead: true);
			}
		}
		else if (!flag && _hazardTypePlaying != 0)
		{
			_hazardTypePlaying = HazardVolume.HazardType.NONE;
			if (_damageAudioSource.isPlaying)
			{
				_damageAudioSource.FadeOut(0.5f);
			}
			if (_damageAudioSourceExternal.isPlaying)
			{
				_damageAudioSourceExternal.FadeOut(0.5f);
			}
		}
	}

	public void OnEnterGravityAudioVolume()
	{
		_gravityAudioCount++;
		if (_gravityAudioCount == 1)
		{
			_forceVolumeAudio.FadeIn(0.5f, fadeFromNothing: false, randomizePlayhead: true);
		}
	}

	public void OnExitGravityAudioVolume()
	{
		_gravityAudioCount--;
		if (_gravityAudioCount <= 0)
		{
			_gravityAudioCount = 0;
			_forceVolumeAudio.FadeOut(0.5f);
		}
	}

	private void OnEnterFluidType(FluidVolume.Type type)
	{
		switch (type)
		{
		case FluidVolume.Type.TRACTOR_BEAM:
			_fluidVolumeSource.AssignAudioLibraryClip(AudioType.NomaiTractorBeamAmbient_LP);
			_fluidVolumeSource.FadeIn(0.2f);
			break;
		case FluidVolume.Type.SAND:
			_fluidVolumeSource.AssignAudioLibraryClip(AudioType.HT_InsideSandfall_Suit_LP);
			_fluidVolumeSource.FadeIn(0.5f);
			break;
		}
	}

	private void OnExitFluidType(FluidVolume.Type type)
	{
		switch (type)
		{
		case FluidVolume.Type.TRACTOR_BEAM:
			_fluidVolumeSource.FadeOut(1f);
			break;
		case FluidVolume.Type.SAND:
			_fluidVolumeSource.FadeOut(0.5f);
			break;
		}
	}

	public void OnStartSleepingAtCampfire(bool isDreamCampfire)
	{
		AudioType audioType = (isDreamCampfire ? AudioType.DreamFire_Crackling_Loop : AudioType.TH_Campfire_LP);
		if (_sleepingAtCampfireSource.audioLibraryClip != audioType)
		{
			_sleepingAtCampfireSource.Stop();
			_sleepingAtCampfireSource.AssignAudioLibraryClip(audioType);
		}
		_sleepingAtCampfireSource.FadeIn(3f, fadeFromNothing: false, randomizePlayhead: false, isDreamCampfire ? 0.5f : 1f);
	}

	public void OnStopSleepingAtCampfire(bool wakeGasp, bool sudden)
	{
		_sleepingAtCampfireSource.FadeOut(1f);
		if (wakeGasp)
		{
			if (sudden)
			{
				_oneShotSleepingAtCampfireSource.PlayOneShot(AudioType.PlayerGasp_Medium);
			}
			else
			{
				_oneShotSleepingAtCampfireSource.PlayOneShot(AudioType.PlayerGasp_Light);
			}
		}
	}

	public void OnExitDreamWorld(AudioType audioType)
	{
		_oneShotSleepingAtCampfireSource.PlayOneShot(audioType);
	}

	public void OnRingWorldCloakEnter()
	{
		_oneShotSource.PlayOneShot(AudioType.Cloak_Entry);
	}

	public void OnRingWorldCloakExit()
	{
		_oneShotSource.PlayOneShot(AudioType.Cloak_Exit);
	}

	public void OnArtifactFocus()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Focus);
	}

	public void OnArtifactUnfocus()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Unfocus);
	}

	public void OnArtifactConceal()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Conceal);
	}

	public void OnArtifactUnconceal()
	{
		_oneShotExternalSource.PlayOneShot(AudioType.Artifact_Unconceal);
	}

	public void OnGrappleTotemZoom()
	{
		_oneShotSource.PlayOneShot(AudioType.GrappleTotem_Zoom);
	}

	public void OnGrappleTotemRetroZoom()
	{
		_oneShotSource.PlayOneShot(AudioType.GrappleTotem_RetroZoom);
	}

	public void PlayOneShotInternal(AudioType audio)
	{
		_oneShotSource.PlayOneShot(audio);
	}

	private void PlaySuitUp()
	{
		if (!PlayerSpacesuit.GetInstantSuitUp())
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerSuitWearSuit);
		}
	}

	private void PlayRemoveSuit()
	{
		if (!PlayerSpacesuit.GetInstantRemoveSuit())
		{
			_oneShotSource.PlayOneShot(AudioType.PlayerSuitRemoveSuit);
		}
	}

	private void OnFlickerOffAndOn(float offDuration, float onDuration)
	{
		if (Locator.GetFlashlight().IsFlashlightOn())
		{
			if (Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye())
			{
				_mapTrackSource.PlayOneShot(AudioType.ToolFlashlightFlicker);
			}
			else
			{
				_oneShotExternalSource.PlayOneShot(AudioType.ToolFlashlightFlicker);
			}
		}
	}

	private void OnPlayerResurrection()
	{
		_hazardTypePlaying = HazardVolume.HazardType.NONE;
		_damageAudioSource.Stop();
		_damageAudioSourceExternal.Stop();
	}
}
