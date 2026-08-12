using UnityEngine;

public class ProxyGhostController : SectoredMonoBehaviour
{
	public enum IdleStyle
	{
		Normal = 0,
		Hunched = 1,
		Twitchy = 2,
		HoldInstrument = 3,
		SkyGaze = 4,
		ShoeGaze = 5,
		Lean = 6
	}

	private static class AnimatorKeys
	{
		public static readonly int Trigger_Idle = Animator.StringToHash("Idle");

		public static readonly int Int_IdleType = Animator.StringToHash("IdleType");

		public static readonly int Trigger_Death = Animator.StringToHash("Death");

		public static readonly int Int_DeathType = Animator.StringToHash("DeathType");

		public static readonly int AnimCurve_DeathFade = Animator.StringToHash("DeathFade");

		public static readonly int AnimCurve_LanternIKOverride = Animator.StringToHash("LanternIKOverride");
	}

	private readonly int _propID_DissolveProgress = Shader.PropertyToID("_DissolveProgress");

	[Space]
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private IdleStyle _idleStyle;

	[SerializeField]
	private GhostEffects.DeathAnimStyle _deathStyle = GhostEffects.DeathAnimStyle.Random;

	[Space]
	[SerializeField]
	private OWAudioSource _voiceAudioSource;

	[SerializeField]
	private OWRenderer[] _dissolveRenderers;

	[SerializeField]
	private OWRenderer[] _ditherRenderers;

	[SerializeField]
	private ParticleSystem _deathParticleSystem;

	[SerializeField]
	private GameObject[] _heldProps = new GameObject[0];

	[SerializeField]
	private Animation[] _heldPropDeathAnimations = new Animation[0];

	[Space]
	[SerializeField]
	private bool _rightHandIK;

	[SerializeField]
	private Transform _rightHandIKTarget;

	[SerializeField]
	private bool _leftHandIK;

	[SerializeField]
	private Transform _leftHandIKTarget;

	[SerializeField]
	private bool _rightFootIK;

	[SerializeField]
	private Transform _rightFootIKTarget;

	[SerializeField]
	private bool _leftFootIK;

	[SerializeField]
	private Transform _leftFootIKTarget;

	[SerializeField]
	private bool _headLook;

	[SerializeField]
	private OWTriggerVolume _headLookTriggerVolume;

	private bool _playerInHeadLookTriggerVolume;

	private float _headLookWeight;

	private DampedSpring _headLookWeightSpring = new DampedSpring(10f, 1f);

	private bool _playingDeathSequence;

	private bool _deathAnimComplete;

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		for (int i = 0; i < _heldProps.Length; i++)
		{
			if (_heldProps[i] != null)
			{
				_heldProps[i].SetActive(value: false);
			}
		}
	}

	public void Reveal()
	{
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < _heldProps.Length; i++)
		{
			if (_heldProps[i] != null)
			{
				_heldProps[i].SetActive(value: true);
			}
		}
	}

	public void Die()
	{
		if (!_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			DieImmediate();
			return;
		}
		if (_deathStyle == GhostEffects.DeathAnimStyle.Random)
		{
			_deathStyle = (GhostEffects.DeathAnimStyle)Random.Range(0, 4);
		}
		_animator.SetInteger(AnimatorKeys.Int_DeathType, (int)_deathStyle);
		_animator.SetTrigger(AnimatorKeys.Trigger_Death);
		_deathAnimComplete = false;
		_playingDeathSequence = true;
		if (_deathParticleSystem != null)
		{
			_deathParticleSystem.Play();
		}
		for (int i = 0; i < _heldPropDeathAnimations.Length; i++)
		{
			if (_heldPropDeathAnimations[i] != null)
			{
				_heldPropDeathAnimations[i].Play();
			}
		}
	}

	public void DieImmediate()
	{
		base.gameObject.SetActive(value: false);
		for (int i = 0; i < _heldPropDeathAnimations.Length; i++)
		{
			if (_heldPropDeathAnimations[i] != null)
			{
				_heldPropDeathAnimations[i].Play();
				_heldPropDeathAnimations[i][_heldPropDeathAnimations[i].clip.name].normalizedTime = 1f;
				_heldPropDeathAnimations[i].Sample();
			}
		}
		_deathAnimComplete = true;
	}

	protected override void Awake()
	{
		base.Awake();
		if (_headLookTriggerVolume != null)
		{
			_headLookTriggerVolume.OnEntry += OnHeadLookTriggerVolumeEntry;
			_headLookTriggerVolume.OnExit += OnHeadLookTriggerVolumeExit;
		}
		GlobalMessenger.AddListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_headLookTriggerVolume != null)
		{
			_headLookTriggerVolume.OnEntry -= OnHeadLookTriggerVolumeEntry;
			_headLookTriggerVolume.OnExit -= OnHeadLookTriggerVolumeExit;
		}
		GlobalMessenger.RemoveListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void OnEnable()
	{
		_animator.SetInteger(AnimatorKeys.Int_IdleType, (int)_idleStyle);
		_animator.SetTrigger(AnimatorKeys.Trigger_Idle);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (_sector.ContainsOccupant(DynamicOccupant.Player))
		{
			_animator.enabled = true;
			for (int i = 0; i < _heldPropDeathAnimations.Length; i++)
			{
				if (_heldPropDeathAnimations[i] != null)
				{
					_heldPropDeathAnimations[i].enabled = true;
				}
			}
			base.enabled = true;
			return;
		}
		_animator.enabled = false;
		base.enabled = false;
		if (_playingDeathSequence)
		{
			_playingDeathSequence = false;
			base.gameObject.SetActive(value: false);
			for (int j = 0; j < _dissolveRenderers.Length; j++)
			{
				_dissolveRenderers[j].SetMaterialProperty(_propID_DissolveProgress, 0f);
			}
			for (int k = 0; k < _ditherRenderers.Length; k++)
			{
				_ditherRenderers[k].SetDitherFade(0f);
			}
			if (_deathParticleSystem != null && _deathParticleSystem.isPlaying)
			{
				_deathParticleSystem.Stop();
			}
			for (int l = 0; l < _heldPropDeathAnimations.Length; l++)
			{
				if (_heldPropDeathAnimations[l] != null && _heldPropDeathAnimations[l].isPlaying)
				{
					_heldPropDeathAnimations[l][_heldPropDeathAnimations[l].clip.name].normalizedTime = 1f;
					_heldPropDeathAnimations[l].Sample();
				}
			}
		}
		for (int m = 0; m < _heldPropDeathAnimations.Length; m++)
		{
			if (_heldPropDeathAnimations[m] != null)
			{
				_heldPropDeathAnimations[m].enabled = false;
			}
		}
	}

	private void Update()
	{
		if (_headLook)
		{
			_headLookWeight = Mathf.Clamp01(_headLookWeightSpring.Update(_headLookWeight, _playerInHeadLookTriggerVolume ? 1f : 0f, Time.deltaTime));
		}
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
			base.gameObject.SetActive(value: false);
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

	private void OnAnimatorIK(int layerIndex)
	{
		float num = Mathf.Clamp01(1f - _animator.GetFloat(AnimatorKeys.AnimCurve_LanternIKOverride));
		if (_rightHandIK)
		{
			_animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandIKTarget.position);
			_animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandIKTarget.rotation);
			_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, num);
			_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, num);
		}
		if (_leftHandIK)
		{
			_animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIKTarget.position);
			_animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandIKTarget.rotation);
			_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, num);
			_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, num);
		}
		if (_rightFootIK)
		{
			_animator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootIKTarget.position);
			_animator.SetIKRotation(AvatarIKGoal.RightFoot, _rightFootIKTarget.rotation);
			_animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, num);
			_animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, num);
		}
		if (_leftFootIK)
		{
			_animator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootIKTarget.position);
			_animator.SetIKRotation(AvatarIKGoal.LeftFoot, _leftFootIKTarget.rotation);
			_animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, num);
			_animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, num);
		}
		if (_headLook && _headLookWeight > 0f)
		{
			_animator.SetLookAtPosition(Locator.GetPlayerCamera().transform.position);
			_animator.SetLookAtWeight(_headLookWeight * num, 0.3f, 1f);
		}
	}

	private void OnHeadLookTriggerVolumeEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInHeadLookTriggerVolume = true;
		}
	}

	private void OnHeadLookTriggerVolumeExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerInHeadLookTriggerVolume = false;
		}
	}

	protected virtual void OnEnableBigHeadMode()
	{
		_animator.GetBoneTransform(HumanBodyBones.Head).localScale = new Vector3(2.5f, 2.5f, 2.5f);
	}

	private void Anim_Death_Audio()
	{
		if (_voiceAudioSource != null && (_sector == null || _sector.ContainsOccupant(DynamicOccupant.Player)))
		{
			_voiceAudioSource.PlayOneShot(AudioType.Ghost_DeathSingle);
		}
	}

	private void Anim_Death_Complete()
	{
		_deathAnimComplete = true;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject) && !(_animator == null))
		{
			Gizmos.color = Color.red;
			if (_rightHandIK && _rightHandIKTarget != null)
			{
				Gizmos.DrawLine(_animator.GetBoneTransform(HumanBodyBones.RightHand).position, _rightHandIKTarget.position);
				OWGizmos.DrawWireCircle(_rightHandIKTarget.position + _rightHandIKTarget.forward * 0.2f, _rightHandIKTarget.up, 0.2f);
				Gizmos.DrawRay(_rightHandIKTarget.position, _rightHandIKTarget.forward * 0.5f);
			}
			if (_leftHandIK && _leftHandIKTarget != null)
			{
				Gizmos.DrawLine(_animator.GetBoneTransform(HumanBodyBones.LeftHand).position, _leftHandIKTarget.position);
				OWGizmos.DrawWireCircle(_leftHandIKTarget.position + _leftHandIKTarget.forward * 0.2f, _leftHandIKTarget.up, 0.2f);
				Gizmos.DrawRay(_leftHandIKTarget.position, _leftHandIKTarget.forward * 0.5f);
			}
			if (_rightFootIK && _rightFootIKTarget != null)
			{
				Gizmos.DrawLine(_animator.GetBoneTransform(HumanBodyBones.RightFoot).position, _rightFootIKTarget.position);
				OWGizmos.DrawWireCircle(_rightFootIKTarget.position + _rightFootIKTarget.forward * 0.1f, _rightFootIKTarget.up, 0.1f);
				Gizmos.DrawRay(_rightFootIKTarget.position, _rightFootIKTarget.forward * 0.25f);
			}
			if (_leftFootIK && _leftFootIKTarget != null)
			{
				Gizmos.DrawLine(_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, _leftFootIKTarget.position);
				OWGizmos.DrawWireCircle(_leftFootIKTarget.position + _leftFootIKTarget.forward * 0.1f, _leftFootIKTarget.up, 0.1f);
				Gizmos.DrawRay(_leftFootIKTarget.position, _leftFootIKTarget.forward * 0.25f);
			}
		}
	}
}
