using System;
using UnityEngine;

public class PrisonerEffects : GhostEffects
{
	[Header("Prisoner")]
	[SerializeField]
	private Transform _tableLanternIKTarget;

	[SerializeField]
	private Transform _wallTorchIKTarget;

	[SerializeField]
	private Transform _projectVisionIKTarget;

	[Space]
	[SerializeField]
	private OWAudioSource _handAudioSource;

	[SerializeField]
	private OWAudioSource _foleyAudioSource;

	private bool _leftFootLifted;

	private bool _rightFootLifted;

	private float _prevCurveRotation;

	public OWEvent OnPickUpLantern = new OWEvent(4);

	public OWEvent OnRevealAnimationComplete = new OWEvent(4);

	public OWEvent OnTurnOnLights = new OWEvent(4);

	public OWEvent OnTurnOnLightsAnimationComplete = new OWEvent(4);

	public OWEvent OnPickUpTorch = new OWEvent(4);

	public OWEvent OnPickUpTorchAnimationComplete = new OWEvent(4);

	public OWEvent OnTurn180Complete = new OWEvent(4);

	public OWEvent OnProjectVision = new OWEvent(4);

	public OWEvent OnOfferTorch = new OWEvent(4);

	public OWEvent OnReactToVisionAnimationComplete = new OWEvent(4);

	public OWEvent OnReadyToReceiveTorch = new OWEvent(4);

	public OWEvent OnFarewellTurnComplete = new OWEvent(4);

	public override void PlaySleepAnimation()
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayWakeUpAnimation()
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayGrabAnimation()
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayBlowOutLanternAnimation(bool fast = false)
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlaySnapNeckAnimation()
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayCallForHelpAnimation()
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayDeathAnimation()
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public override void PlayDeathAnimation(DeathAnimStyle deathAnimStyle)
	{
		throw new NotSupportedException("Tried to play an unsupported animation for the prisoner.");
	}

	public void PlayRevealAnimation()
	{
		_animator.SetTrigger("RevealToStand");
	}

	public void PlayTurnOnLightsAnimation()
	{
		_animator.SetTrigger("TurnOnLights");
	}

	public void PlayPickUpTorchAnimation()
	{
		_animator.SetTrigger("PickUpTorch");
	}

	public void Play180TurnAnimation()
	{
		_animator.SetTrigger("Turn180");
	}

	public void PlayProjectVisionAnimation()
	{
		_animator.SetTrigger("ProjectVision");
	}

	public void PlayOfferTorchAnimation()
	{
		_animator.SetTrigger("OfferTorch");
	}

	public void PlayOfferTorchEndAnimation()
	{
		_animator.SetTrigger("OfferTorchEnd");
	}

	public void PlayExperienceVisionAnimation()
	{
		_animator.SetTrigger("ExperienceVision");
	}

	public void PlayReactToVisionAnimation()
	{
		_animator.SetTrigger("ReactToVision");
	}

	public void PlayWaitForTorchReturnAnimation()
	{
		_animator.SetTrigger("WaitForTorchReturn");
	}

	public void PlayFarewellBowAnimation()
	{
		_animator.SetTrigger("FarewellBow");
	}

	protected override void Update_Footsteps()
	{
		bool flag = _animator.GetFloat("LeftFootLift") > 0f;
		bool flag2 = _animator.GetFloat("RightFootLift") > 0f;
		if (_leftFootLifted && !flag)
		{
			PlayFootstepAudio(SurfaceType.Planks, running: false);
		}
		if (_rightFootLifted && !flag2)
		{
			PlayFootstepAudio(SurfaceType.Planks, running: false);
		}
		_leftFootLifted = flag;
		_rightFootLifted = flag2;
	}

	protected override void OnAnimatorIK(int layerIndex)
	{
		AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex);
		float @float = _animator.GetFloat("IKWeightCurve_RightHand");
		float float2 = _animator.GetFloat("IKWeightCurve_LeftHand");
		if (@float > 0f && currentAnimatorStateInfo.IsName("RevealToStand"))
		{
			_animator.SetIKPosition(AvatarIKGoal.RightHand, _tableLanternIKTarget.position);
			_animator.SetIKRotation(AvatarIKGoal.RightHand, _tableLanternIKTarget.rotation);
			_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, @float);
			_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, @float);
		}
		if (float2 > 0f)
		{
			if (currentAnimatorStateInfo.IsName("PickUpTorch"))
			{
				_animator.SetIKPosition(AvatarIKGoal.LeftHand, _wallTorchIKTarget.position);
				_animator.SetIKRotation(AvatarIKGoal.LeftHand, _wallTorchIKTarget.rotation);
				_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, float2);
				_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, float2);
			}
			else if (currentAnimatorStateInfo.IsName("ProjectVision") || currentAnimatorStateInfo.IsName("ProjectVisionLoop"))
			{
				_animator.SetIKPosition(AvatarIKGoal.LeftHand, _projectVisionIKTarget.position);
				_animator.SetIKRotation(AvatarIKGoal.LeftHand, _projectVisionIKTarget.rotation);
				_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, float2);
				_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, float2);
			}
		}
	}

	private void OnAnimatorMove()
	{
		float @float = _animator.GetFloat("ApplyRootMotion");
		float float2 = _animator.GetFloat("ApplyCurveRotation");
		if (@float > 0f)
		{
			_controller.transform.localPosition += _animator.deltaPosition;
			_controller.transform.localRotation *= _animator.deltaRotation;
		}
		if (float2 > 0f)
		{
			float float3 = _animator.GetFloat("ControllerRotationCurve");
			Quaternion quaternion = Quaternion.AngleAxis(float3 - _prevCurveRotation, Vector3.up);
			_controller.transform.localRotation *= quaternion;
			_prevCurveRotation = float3;
		}
		else
		{
			_prevCurveRotation = 0f;
		}
	}

	private void Anim_FocusLantern()
	{
		_controller.ChangeLanternFocus(1f);
	}

	private void Anim_UnfocusLantern()
	{
		_controller.ChangeLanternFocus(0f);
	}

	private void Anim_PickUpLantern()
	{
		OnPickUpLantern.Invoke();
		_handAudioSource.PlayOneShot(AudioType.Prisoner_PickUpArtifact);
	}

	private void Anim_RevealToStandComplete()
	{
		OnRevealAnimationComplete.Invoke();
	}

	private void Anim_TurnOnLights()
	{
		OnTurnOnLights.Invoke();
	}

	private void Anim_TurnOnLightsComplete()
	{
		OnTurnOnLightsAnimationComplete.Invoke();
	}

	private void Anim_PickUpTorch()
	{
		OnPickUpTorch.Invoke();
		_handAudioSource.PlayOneShot(AudioType.Prisoner_PickUpTorch);
	}

	private void Anim_PickUpTorchComplete()
	{
		OnPickUpTorchAnimationComplete.Invoke();
	}

	private void Anim_Turn180Complete()
	{
		OnTurn180Complete.Invoke();
	}

	private void Anim_ProjectVision()
	{
		OnProjectVision.Invoke();
	}

	private void Anim_OfferTorch()
	{
		OnOfferTorch.Invoke();
	}

	private void Anim_ReactToVisionComplete()
	{
		OnReactToVisionAnimationComplete.Invoke();
	}

	private void Anim_ReadyToReceiveTorch()
	{
		OnReadyToReceiveTorch.Invoke();
	}

	private void Anim_FarewellTurnComplete()
	{
		OnFarewellTurnComplete.Invoke();
	}

	private void Anim_ReactToVision_Vocals()
	{
		PlayVoiceAudioNear(AudioType.Prisoner_ReactToVision_Vocals);
	}

	private void Anim_RevealToStand_Vocals_1()
	{
		PlayVoiceAudioNear(AudioType.Prisoner_RevealToStand_Vocals_1);
	}

	private void Anim_RevealToStand_Vocals_2()
	{
		PlayVoiceAudioNear(AudioType.Prisoner_RevealToStand_Vocals_2);
	}

	private void Anim_PlayClothFoley()
	{
		_foleyAudioSource.PlayOneShot(AudioType.Prisoner_ClothFoley);
	}
}
