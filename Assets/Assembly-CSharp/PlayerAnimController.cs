using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimController : MonoBehaviour
{
	public delegate void PlayerAnimationEvent();

	private Animator _animator;

	private RuntimeAnimatorController _baseAnimController;

	private PlayerCharacterController _playerController;

	private PlayerResources _playerResources;

	private ThrusterModel _playerJetpack;

	[SerializeField]
	private GameObject _unsuitedGroup;

	[SerializeField]
	private GameObject _suitedGroup;

	[SerializeField]
	private GameObject[] _rightArmObjects;

	[SerializeField]
	private AnimatorOverrideController _unsuitedAnimOverride;

	private bool _leftFootGrounded;

	private bool _rightFootGrounded;

	private float _ungroundedTime;

	private bool _justBecameGrounded;

	private bool _justTookFallDamage;

	private bool _rightArmHidden;

	private int _defaultLayer;

	private int _probeOnlyLayer;

	public event PlayerAnimationEvent OnLeftFootGrounded;

	public event PlayerAnimationEvent OnLeftFootLift;

	public event PlayerAnimationEvent OnRightFootGrounded;

	public event PlayerAnimationEvent OnRightFootLift;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_baseAnimController = _animator.runtimeAnimatorController;
		_leftFootGrounded = false;
		_rightFootGrounded = false;
		_ungroundedTime = 0f;
		_justBecameGrounded = false;
		_justTookFallDamage = false;
		_rightArmHidden = false;
		_defaultLayer = LayerMask.NameToLayer("Default");
		_probeOnlyLayer = LayerMask.NameToLayer("VisibleToProbe");
		GlobalMessenger.AddListener("SuitUp", OnPutOnSuit);
		GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.AddListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.AddListener("ExitMapView", OnExitMapView);
	}

	private void Start()
	{
		_playerController = Locator.GetPlayerTransform().GetRequiredComponent<PlayerCharacterController>();
		_playerResources = Locator.GetPlayerTransform().GetRequiredComponent<PlayerResources>();
		_playerJetpack = Locator.GetPlayerTransform().GetRequiredComponent<ThrusterModel>();
		if (Locator.GetPlayerSuit().IsWearingSuit())
		{
			OnPutOnSuit();
		}
		else
		{
			OnRemoveSuit();
		}
		_playerController.OnJump += OnPlayerJump;
		_playerController.OnBecomeGrounded += OnPlayerGrounded;
		_playerController.OnBecomeUngrounded += OnPlayerUngrounded;
		_playerResources.OnInstantDamage += OnInstantDamage;
	}

	private void OnDestroy()
	{
		if ((bool)_playerController)
		{
			_playerController.OnJump -= OnPlayerJump;
			_playerController.OnBecomeGrounded -= OnPlayerGrounded;
			_playerController.OnBecomeUngrounded -= OnPlayerUngrounded;
		}
		if ((bool)_playerResources)
		{
			_playerResources.OnInstantDamage -= OnInstantDamage;
		}
		GlobalMessenger.RemoveListener("SuitUp", OnPutOnSuit);
		GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
		GlobalMessenger.RemoveListener("EnterMapView", OnEnterMapView);
		GlobalMessenger.RemoveListener("ExitMapView", OnExitMapView);
	}

	private void LateUpdate()
	{
		bool flag = _playerController.IsGrounded();
		bool flag2 = PlayerState.IsAttached();
		bool flag3 = PlayerState.InZeroG();
		bool flag4 = _playerJetpack.GetLocalAcceleration().y > 0f;
		Vector3 vector = Vector3.zero;
		if (!flag2)
		{
			vector = _playerController.GetRelativeGroundVelocity();
		}
		if (Mathf.Abs(vector.x) < 0.05f)
		{
			vector.x = 0f;
		}
		if (Mathf.Abs(vector.z) < 0.05f)
		{
			vector.z = 0f;
		}
		if (flag4)
		{
			_ungroundedTime = Time.time;
		}
		float num = 0f;
		float num2 = 0f;
		OWRigidbody lastGroundBody = _playerController.GetLastGroundBody();
		if (!flag && !flag2 && !flag3 && lastGroundBody != null)
		{
			num = (_playerController.GetAttachedOWRigidbody().GetVelocity() - lastGroundBody.GetPointVelocity(_playerController.transform.position)).magnitude;
			num2 = Time.time - _ungroundedTime;
		}
		_animator.SetFloat("RunSpeedX", vector.x / 3f);
		_animator.SetFloat("RunSpeedY", vector.z / 3f);
		_animator.SetFloat("TurnSpeed", _playerController.GetTurning());
		_animator.SetBool("Grounded", flag || flag2 || PlayerState.IsRecentlyDetached());
		_animator.SetLayerWeight(1, _playerController.GetJumpCrouchFraction());
		_animator.SetFloat("FreefallSpeed", num / 15f * (num2 / 3f));
		_animator.SetBool("InZeroG", flag3 || flag4);
		_animator.SetBool("UsingJetpack", flag3 && PlayerState.IsWearingSuit());
		if (_justBecameGrounded)
		{
			if (_justTookFallDamage)
			{
				_animator.SetTrigger("LandHard");
			}
			else
			{
				_animator.SetTrigger("Land");
			}
		}
		if (flag)
		{
			float @float = _animator.GetFloat("LeftFootLift");
			if (!_leftFootGrounded && @float < 0.333f)
			{
				_leftFootGrounded = true;
				if (this.OnLeftFootGrounded != null)
				{
					this.OnLeftFootGrounded();
				}
			}
			else if (_leftFootGrounded && @float > 0.666f)
			{
				_leftFootGrounded = false;
				if (this.OnLeftFootLift != null)
				{
					this.OnLeftFootLift();
				}
			}
			float float2 = _animator.GetFloat("RightFootLift");
			if (!_rightFootGrounded && float2 < 0.333f)
			{
				_rightFootGrounded = true;
				if (this.OnRightFootGrounded != null)
				{
					this.OnRightFootGrounded();
				}
			}
			else if (_rightFootGrounded && float2 > 0.666f)
			{
				_rightFootGrounded = false;
				if (this.OnRightFootLift != null)
				{
					this.OnRightFootLift();
				}
			}
		}
		_justBecameGrounded = false;
		_justTookFallDamage = false;
		bool flag5 = Locator.GetToolModeSwapper().GetToolMode() != ToolMode.None;
		if ((flag5 && !_rightArmHidden) || (!flag5 && _rightArmHidden))
		{
			_rightArmHidden = flag5;
			for (int i = 0; i < _rightArmObjects.Length; i++)
			{
				_rightArmObjects[i].layer = (_rightArmHidden ? _probeOnlyLayer : _defaultLayer);
			}
		}
	}

	private void OnPlayerJump()
	{
		_ungroundedTime = Time.time;
		if (base.isActiveAndEnabled)
		{
			_animator.SetTrigger("Jump");
		}
	}

	private void OnPlayerGrounded()
	{
		if (base.isActiveAndEnabled && !PlayerState.IsRecentlyDetached())
		{
			_justBecameGrounded = true;
		}
	}

	private void OnPlayerUngrounded()
	{
		_ungroundedTime = Time.time;
	}

	private void OnInstantDamage(float instantDamage, InstantDamageType damageType)
	{
		if (base.isActiveAndEnabled && damageType == InstantDamageType.Impact)
		{
			_justTookFallDamage = true;
		}
	}

	private void OnPutOnSuit()
	{
		_animator.runtimeAnimatorController = _baseAnimController;
		_unsuitedGroup.SetActive(value: false);
		_suitedGroup.SetActive(!PlayerState.InMapView());
	}

	private void OnRemoveSuit()
	{
		_animator.runtimeAnimatorController = _unsuitedAnimOverride;
		_unsuitedGroup.SetActive(!PlayerState.InMapView());
		_suitedGroup.SetActive(value: false);
	}

	private void OnEnterMapView()
	{
		_unsuitedGroup.SetActive(value: false);
		_suitedGroup.SetActive(value: false);
	}

	private void OnExitMapView()
	{
		if (Locator.GetPlayerSuit().IsWearingSuit())
		{
			_suitedGroup.SetActive(value: true);
		}
		else
		{
			_unsuitedGroup.SetActive(value: true);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!(_animator == null))
		{
			Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.LeftToes);
			if ((bool)boneTransform)
			{
				Gizmos.color = (_leftFootGrounded ? Color.blue : Color.red);
				OWGizmos.DrawWireArc(boneTransform.position, base.transform.up, base.transform.forward, -180f, 0.25f);
			}
			Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.RightToes);
			if ((bool)boneTransform2)
			{
				Gizmos.color = (_rightFootGrounded ? Color.blue : Color.red);
				OWGizmos.DrawWireArc(boneTransform2.position, base.transform.up, base.transform.forward, 180f, 0.25f);
			}
		}
	}
}
