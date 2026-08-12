using GhostEnums;
using UnityEngine;

public class GhostEffects : MonoBehaviour
{
	public enum MovementStyle
	{
		Normal = 0,
		Stalk = 1,
		Chase = 2
	}

	public enum DeathAnimStyle
	{
		Random = -1,
		Reaching = 0,
		Reaching_Jittery = 1,
		Crunchy = 2,
		DramaClub = 3
	}

	protected static class AnimatorKeys
	{
		public static readonly int AnimCurve_LanternIKOverride = Animator.StringToHash("LanternIKOverride");

		public static readonly int AnimCurve_GrabWindow = Animator.StringToHash("GrabWindow");

		public static readonly int Trigger_Default = Animator.StringToHash("Default");

		public static readonly int Trigger_Grab = Animator.StringToHash("Grab");

		public static readonly int Trigger_Sleeping = Animator.StringToHash("Sleeping");

		public static readonly int Trigger_WakingUp = Animator.StringToHash("WakingUp");

		public static readonly int Int_MoveStyle = Animator.StringToHash("MoveStyle");

		public static readonly int Float_MoveDirectionX = Animator.StringToHash("MoveDirectionX");

		public static readonly int Float_MoveDirectionY = Animator.StringToHash("MoveDirectionY");

		public static readonly int Float_MoveSlope = Animator.StringToHash("MoveSlope");

		public static readonly int Float_TurnSpeed = Animator.StringToHash("TurnSpeed");

		public static readonly int Trigger_Death = Animator.StringToHash("Death");

		public static readonly int Int_DeathType = Animator.StringToHash("DeathType");

		public static readonly int AnimCurve_DeathFade = Animator.StringToHash("DeathFade");

		public static readonly int Trigger_BlowOutLantern = Animator.StringToHash("BlowOutLantern");

		public static readonly int Trigger_BlowOutLanternFast = Animator.StringToHash("BlowOutLanternFast");

		public static readonly int Trigger_SnapNeck = Animator.StringToHash("SnapNeck");

		public static readonly int Trigger_CallForHelp = Animator.StringToHash("CallForHelp");
	}

	private readonly int _propID_DissolveProgress = Shader.PropertyToID("_DissolveProgress");

	[SerializeField]
	private Sector _sector;

	[Space]
	[SerializeField]
	private DeathAnimStyle _deathAnimStyle = DeathAnimStyle.Random;

	[SerializeField]
	private bool _ignoreSleepAnimation;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _lanternAudioSource;

	[SerializeField]
	private OWAudioSource _voiceAudioSourceNear;

	[SerializeField]
	private OWAudioSource _voiceAudioSourceFar;

	[SerializeField]
	private OWAudioSource _feetAudioSourceNear;

	[SerializeField]
	private OWAudioSource _feetAudioSourceFar;

	[Header("Visuals")]
	[SerializeField]
	private OWRenderer[] _dissolveRenderers;

	[SerializeField]
	private OWRenderer[] _ditherRenderers;

	[SerializeField]
	private OWEmissiveRenderer[] _eyeRenderers;

	[SerializeField]
	private ParticleSystem _deathParticleSystem;

	[Header("IK")]
	[SerializeField]
	private GhostIK _ghostIKController;

	[SerializeField]
	private Transform _ikTargetHoldingLantern;

	[SerializeField]
	private Transform _ikHintHoldingLantern;

	protected Animator _animator;

	protected GhostController _controller;

	private GhostData _data;

	private MovementStyle _movementStyle;

	private float _footstepTimer = 0.5f;

	private float _eyeGlow;

	private float _prevEyeEmissionScale = -1f;

	private DampedSpring2D _moveSpeedSpring = new DampedSpring2D(50f, 1f);

	private Vector2 _smoothedMoveSpeed = Vector2.zero;

	private DampedSpring _moveSlopeSpring = new DampedSpring(50f, 1f);

	private float _smoothedMoveSlope;

	private DampedSpring _turnSpeedSpring = new DampedSpring(50f, 1f);

	private float _smoothedTurnSpeed;

	private bool _waitForPreviousVoiceClip;

	private float _voiceClipDonePlayingTime;

	private bool _waitForGrabWindow;

	private bool _waitToRespondToHelpCall;

	private float _respondToHelpCallTime;

	private bool _playingDeathSequence;

	private bool _deathAnimComplete;

	private bool _stompyFootsteps;

	public OWEvent OnHeadRaiseComplete = new OWEvent(4);

	public OWEvent OnHandRaiseComplete = new OWEvent(4);

	public OWEvent OnGrabComplete = new OWEvent(4);

	public OWEvent OnLiftPlayer = new OWEvent(4);

	public OWEvent OnExtinguishPlayerLantern = new OWEvent(4);

	public OWEvent OnSnapPlayerNeck = new OWEvent(4);

	public OWEvent OnCallForHelp = new OWEvent(1);

	public OWEvent OnGhostDeathComplete = new OWEvent(4);

	public void Initialize(Transform nodeRoot, GhostController controller, GhostData data)
	{
		_animator = GetComponent<Animator>();
		_controller = controller;
		_data = data;
		if (_sector != null)
		{
			_animator.keepAnimatorControllerStateOnDisable = true;
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_ghostIKController != null)
		{
			_ghostIKController.OnLeftFootHitGround += new OWEvent<SurfaceType>.OWCallback(OnLeftFootHitGround);
			_ghostIKController.OnRightFootHitGround += new OWEvent<SurfaceType>.OWCallback(OnRightFootHitGround);
		}
		if (_feetAudioSourceFar != null)
		{
			_stompyFootsteps = true;
		}
		SetEyeGlow(_eyeGlow);
		GlobalMessenger.AddListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void OnDestroy()
	{
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		GlobalMessenger.RemoveListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void OnSectorOccupantsUpdated()
	{
		_animator.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	public void CancelStompyFootsteps()
	{
		_stompyFootsteps = false;
	}

	public virtual void PlaySleepAnimation()
	{
		_animator.SetTrigger(_ignoreSleepAnimation ? AnimatorKeys.Trigger_Default : AnimatorKeys.Trigger_Sleeping);
	}

	public virtual void PlayWakeUpAnimation()
	{
		_animator.SetTrigger(AnimatorKeys.Trigger_WakingUp);
	}

	public virtual void PlayDefaultAnimation()
	{
		_animator.SetTrigger(AnimatorKeys.Trigger_Default);
	}

	public virtual void PlayGrabAnimation()
	{
		_animator.SetTrigger(AnimatorKeys.Trigger_Grab);
		_waitForGrabWindow = true;
	}

	public virtual void PlayBlowOutLanternAnimation(bool fast = false)
	{
		_animator.SetTrigger(fast ? AnimatorKeys.Trigger_BlowOutLanternFast : AnimatorKeys.Trigger_BlowOutLantern);
	}

	public virtual void PlaySnapNeckAnimation()
	{
		_animator.SetTrigger(AnimatorKeys.Trigger_SnapNeck);
	}

	public virtual void PlayCallForHelpAnimation()
	{
		_animator.SetTrigger(AnimatorKeys.Trigger_CallForHelp);
	}

	public virtual void PlayDeathAnimation()
	{
		PlayDeathAnimation(_deathAnimStyle);
	}

	public virtual void PlayDeathAnimation(DeathAnimStyle deathAnimStyle)
	{
		if (deathAnimStyle == DeathAnimStyle.Random)
		{
			deathAnimStyle = (DeathAnimStyle)Random.Range(0, 4);
		}
		_animator.SetInteger(AnimatorKeys.Int_DeathType, (int)deathAnimStyle);
		_animator.SetTrigger(AnimatorKeys.Trigger_Death);
		_deathAnimComplete = false;
	}

	public void PlayVoiceAudioNear(AudioType audioType, float volumeScale = 1f)
	{
		PlayVoiceAudio(_voiceAudioSourceNear, audioType, volumeScale);
	}

	public void PlayVoiceAudioFar(AudioType audioType, float volumeScale = 1f)
	{
		PlayVoiceAudio(_voiceAudioSourceFar, audioType, volumeScale);
	}

	public void StopAllVoiceAudio()
	{
		_voiceAudioSourceFar.Stop();
		_voiceAudioSourceNear.Stop();
		_waitForPreviousVoiceClip = false;
		MonoBehaviour.print("ghost voice audio STOPPED   " + Time.time);
	}

	public void PlayGrabAudio(AudioType audioType)
	{
		_voiceAudioSourceNear.PlayOneShot(audioType);
	}

	public void PlayLanternAudio(AudioType audioType)
	{
		if (_data.playerLocation.distance < _lanternAudioSource.GetAudioSource().maxDistance + 5f)
		{
			_lanternAudioSource.PlayOneShot(audioType);
		}
	}

	public void PlayRespondToHelpCallAudio(float delay)
	{
		_waitToRespondToHelpCall = true;
		_respondToHelpCallTime = Time.time + delay;
	}

	public void SetMovementStyle(MovementStyle style)
	{
		_movementStyle = style;
		_animator.SetInteger(AnimatorKeys.Int_MoveStyle, (int)_movementStyle);
	}

	public void PlayDeathEffects()
	{
		_playingDeathSequence = true;
		if (_deathParticleSystem != null)
		{
			_deathParticleSystem.Play();
		}
	}

	public void Update_Effects()
	{
		if (_waitToRespondToHelpCall && Time.time >= _respondToHelpCallTime)
		{
			_waitToRespondToHelpCall = false;
			PlayVoiceAudioFar(AudioType.Ghost_CallForHelpResponse);
		}
		if (_waitForGrabWindow && _animator.GetFloat(AnimatorKeys.AnimCurve_GrabWindow) > 0.5f)
		{
			_waitForGrabWindow = false;
			PlayGrabAudio(AudioType.Ghost_Grab_Swish);
		}
		Update_Footsteps();
		Vector3 relativeVelocity = _controller.GetRelativeVelocity();
		float num = ((_movementStyle == MovementStyle.Chase) ? 8f : 2f);
		float num2 = new Vector2(relativeVelocity.y, relativeVelocity.z).magnitude * Mathf.Sign(relativeVelocity.z);
		Vector2 targetValue = new Vector2(relativeVelocity.x / num, num2 / num);
		_smoothedMoveSpeed = _moveSpeedSpring.Update(_smoothedMoveSpeed, targetValue, Time.deltaTime);
		_animator.SetFloat(AnimatorKeys.Float_MoveDirectionX, _smoothedMoveSpeed.x);
		_animator.SetFloat(AnimatorKeys.Float_MoveDirectionY, _smoothedMoveSpeed.y);
		float num3 = Vector3.SignedAngle(new Vector3(relativeVelocity.x, 0f, relativeVelocity.z), relativeVelocity, Vector3.left);
		float targetValue2 = Mathf.Clamp(num3 / 30f, -1f, 1f);
		if (num3 > 15f && _controller.IsApproachingEndOfIncline())
		{
			targetValue2 = 0f;
		}
		_smoothedMoveSlope = _moveSlopeSpring.Update(_smoothedMoveSlope, targetValue2, Time.deltaTime);
		_animator.SetFloat(AnimatorKeys.Float_MoveSlope, _smoothedMoveSlope);
		_smoothedTurnSpeed = _turnSpeedSpring.Update(_smoothedTurnSpeed, _controller.GetAngularVelocity() / 90f, Time.deltaTime);
		_animator.SetFloat(AnimatorKeys.Float_TurnSpeed, _smoothedTurnSpeed);
		float target = (_data.sensor.isIlluminated ? 1f : 0f);
		float num4 = (_data.sensor.isIlluminated ? 8f : 0.8f);
		_eyeGlow = Mathf.MoveTowards(_eyeGlow, target, Time.deltaTime * num4);
		float t = (Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController()
			.GetLight()
			.GetFlickerScale() - 1f + 0.07f) / 0.14f;
		t = Mathf.Lerp(0.7f, 1f, t);
		SetEyeGlow(_eyeGlow * t);
		if (!_playingDeathSequence)
		{
			return;
		}
		float @float = _animator.GetFloat(AnimatorKeys.AnimCurve_DeathFade);
		for (int i = 0; i < _dissolveRenderers.Length; i++)
		{
			_dissolveRenderers[i].SetMaterialProperty(_propID_DissolveProgress, @float);
		}
		for (int j = 0; j < _ditherRenderers.Length; j++)
		{
			_ditherRenderers[j].SetDitherFade(@float);
		}
		if (_deathAnimComplete && (_deathParticleSystem == null || !_deathParticleSystem.isPlaying))
		{
			_playingDeathSequence = false;
			_controller.gameObject.SetActive(value: false);
			OnGhostDeathComplete.Invoke();
			for (int k = 0; k < _dissolveRenderers.Length; k++)
			{
				_dissolveRenderers[k].SetMaterialProperty(_propID_DissolveProgress, 0f);
			}
			for (int l = 0; l < _ditherRenderers.Length; l++)
			{
				_ditherRenderers[l].SetDitherFade(0f);
			}
		}
	}

	protected virtual void Update_Footsteps()
	{
		if (_controller.GetSpeed() > 0.1f)
		{
			_footstepTimer -= Time.deltaTime;
			if (_footstepTimer <= 0f)
			{
				bool flag = _controller.GetSpeed() > GhostConstants.GetMoveSpeed(MoveType.INVESTIGATE) + 0.5f;
				_footstepTimer = (flag ? Random.Range(0.45f, 0.55f) : 1.2f);
				if (AllowFootstepAudio(usingTimer: true))
				{
					PlayFootstepAudio(_ghostIKController.lastLeftFootSurfaceType, flag);
				}
			}
		}
		else if (_footstepTimer < 0.4f)
		{
			_footstepTimer = 0.5f;
			if (AllowFootstepAudio(usingTimer: true))
			{
				PlayFootstepAudio(_ghostIKController.lastLeftFootSurfaceType, running: false);
			}
		}
		else
		{
			_footstepTimer = 0.5f;
		}
	}

	protected virtual void OnAnimatorIK(int layerIndex)
	{
		float @float = _animator.GetFloat(AnimatorKeys.AnimCurve_LanternIKOverride);
		float num = Mathf.Clamp01(1f - @float);
		if (num > 0f)
		{
			_animator.SetIKPosition(AvatarIKGoal.RightHand, _ikTargetHoldingLantern.position);
			_animator.SetIKHintPosition(AvatarIKHint.RightElbow, _ikHintHoldingLantern.position);
			_animator.SetIKRotation(AvatarIKGoal.RightHand, _ikTargetHoldingLantern.rotation);
			_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.9f * num);
			_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.9f * num);
			_animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0.5f * num);
		}
	}

	protected virtual void OnEnableBigHeadMode()
	{
		_animator.GetBoneTransform(HumanBodyBones.Head).localScale = new Vector3(2.5f, 2.5f, 2.5f);
	}

	private void SetEyeGlow(float glow)
	{
		if (!OWMath.ApproxEquals(glow, _prevEyeEmissionScale))
		{
			for (int i = 0; i < _eyeRenderers.Length; i++)
			{
				_eyeRenderers[i].SetEmissiveScale(glow);
			}
			_prevEyeEmissionScale = glow;
		}
	}

	private void PlayVoiceAudio(OWAudioSource source, AudioType audioType, float volumeScale = 1f)
	{
		if (audioType == AudioType.Ghost_Chase || audioType == AudioType.Ghost_CallForHelp || audioType == AudioType.Ghost_CallForHelpResponse || !_waitForPreviousVoiceClip || !(Time.time < _voiceClipDonePlayingTime))
		{
			AudioClip audioClip = source.PlayOneShot(audioType, volumeScale);
			_waitForPreviousVoiceClip = audioType == AudioType.Ghost_IntruderConfirmed || audioType == AudioType.Ghost_Grab_Shout || audioType == AudioType.Ghost_Grab_Scream || audioType == AudioType.Ghost_Chase || audioType == AudioType.Ghost_Stalk_Fast || audioType == AudioType.Ghost_Laugh;
			_voiceClipDonePlayingTime = Time.time + Mathf.Min(audioClip.length, 1f);
		}
	}

	private void OnRightFootHitGround(SurfaceType surfaceType)
	{
		OnFootHitGround(left: false, surfaceType);
	}

	private void OnLeftFootHitGround(SurfaceType surfaceType)
	{
		OnFootHitGround(left: true, surfaceType);
	}

	private void OnFootHitGround(bool left, SurfaceType surfaceType)
	{
		if (AllowFootstepAudio(usingTimer: false))
		{
			bool running = _controller.GetSpeed() > GhostConstants.GetMoveSpeed(MoveType.INVESTIGATE) + 0.5f;
			PlayFootstepAudio(surfaceType, running);
		}
	}

	private bool AllowFootstepAudio(bool usingTimer)
	{
		bool flag = _stompyFootsteps || _data.currentAction == GhostAction.Name.Chase || _data.currentAction == GhostAction.Name.Grab;
		if (usingTimer != flag)
		{
			return false;
		}
		float num = (_stompyFootsteps ? _feetAudioSourceFar.maxDistance : _feetAudioSourceNear.maxDistance);
		return _data.playerLocation.distance < num + 5f;
	}

	protected void PlayFootstepAudio(SurfaceType surfaceType, bool running)
	{
		OWAudioSource oWAudioSource = _feetAudioSourceNear;
		float pitch = 1f;
		AudioType type;
		switch (surfaceType)
		{
		case SurfaceType.Wood:
		case SurfaceType.Planks:
			type = (running ? AudioType.Ghost_Footstep_Wood_Running : AudioType.Ghost_Footstep_Wood);
			pitch = (running ? 1f : 0.5f);
			break;
		case SurfaceType.Gravel:
			type = (running ? AudioType.Ghost_Footstep_Forest_Running : AudioType.Ghost_Footstep_Gravel);
			pitch = 0.5f;
			break;
		default:
			type = (running ? AudioType.Ghost_Footstep_Forest_Running : AudioType.Ghost_Footstep_Forest);
			break;
		}
		if (_stompyFootsteps)
		{
			oWAudioSource = _feetAudioSourceFar;
			type = AudioType.Ghost_Footstep_Wood_Stompy;
			pitch = 1f;
		}
		oWAudioSource.pitch = pitch;
		oWAudioSource.PlayOneShot(type);
	}

	private void Anim_HeadRaiseComplete()
	{
		OnHeadRaiseComplete.Invoke();
	}

	private void Anim_HandRaiseComplete()
	{
		OnHandRaiseComplete.Invoke();
	}

	private void Anim_GrabComplete()
	{
		OnGrabComplete.Invoke();
	}

	private void Anim_LiftPlayer()
	{
		OnLiftPlayer.Invoke();
	}

	private void Anim_BlowOut_Charge()
	{
		PlayVoiceAudioNear(AudioType.Ghost_BlowOut_Charge);
	}

	private void Anim_BlowOut_Extinguish()
	{
		PlayVoiceAudioNear(AudioType.Ghost_BlowOut_Extinguish);
		RumbleManager.PlayGhostBlowOutLantern();
		OnExtinguishPlayerLantern.Invoke();
	}

	private void Anim_SnapNeck()
	{
		OnSnapPlayerNeck.Invoke();
	}

	private void Anim_SnapNeck_Audio()
	{
		Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.Ghost_NeckSnap);
		RumbleManager.PlayGhostNeckSnap();
	}

	private void Anim_CallForHelp()
	{
		PlayVoiceAudioFar(AudioType.Ghost_CallForHelp);
		OnCallForHelp.Invoke();
	}

	private void Anim_Death_Audio()
	{
		PlayVoiceAudioNear(AudioType.Ghost_DeathSingle);
	}

	private void Anim_Death_Complete()
	{
		_deathAnimComplete = true;
	}
}
