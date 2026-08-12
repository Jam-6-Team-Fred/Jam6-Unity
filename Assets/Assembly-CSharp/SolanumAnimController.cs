using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SolanumAnimController : MonoBehaviour
{
	public delegate void AnimEvent(int data);

	private Animator _animator;

	[SerializeField]
	private DampedSpringQuat _lookSpring = new DampedSpringQuat();

	[SerializeField]
	private GameObject _staffLocked;

	[SerializeField]
	private GameObject _staffUnlocked;

	[SerializeField]
	private OWAudioSource _symbolsAudioSource;

	[SerializeField]
	private OWAudioSource _foleyAudioSource;

	private AnimatorStateEvents _animatorStateEvents;

	private Transform _headBoneTransform;

	private Transform _playerCameraTransform;

	private Transform _leftHandTransform;

	private Transform _rightHandTransform;

	private bool _performingAction = true;

	private bool _creatingWordStones;

	private bool _startingWrite;

	private Quaternion _currentLookRotation = Quaternion.identity;

	private Vector3 _localLookPosition = new Vector3(0f, 0f, 10f);

	public bool isPerformingAction => _performingAction;

	public bool isStartingWrite => _startingWrite;

	public event AnimEvent OnTouchRock;

	public event AnimEvent OnWriteResponse;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_headBoneTransform = _animator.GetBoneTransform(HumanBodyBones.Head);
		_leftHandTransform = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
		_rightHandTransform = _animator.GetBoneTransform(HumanBodyBones.RightHand);
		_staffUnlocked.SetActive(value: false);
		GlobalMessenger.AddListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void OnDestroy()
	{
		if (_animatorStateEvents != null)
		{
			_animatorStateEvents.OnEnterState -= OnEnterAnimatorState;
		}
		GlobalMessenger.RemoveListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void Start()
	{
		_playerCameraTransform = Locator.GetPlayerCamera().transform;
	}

	private void LateUpdate()
	{
		if (_animatorStateEvents == null)
		{
			_animatorStateEvents = _animator.GetBehaviour<AnimatorStateEvents>();
			_animatorStateEvents.OnEnterState += OnEnterAnimatorState;
		}
		Quaternion targetValue = Quaternion.LookRotation(_playerCameraTransform.position - _headBoneTransform.position, base.transform.up);
		_currentLookRotation = _lookSpring.Update(_currentLookRotation, targetValue, Time.deltaTime);
		Vector3 position = _headBoneTransform.position + _currentLookRotation * Vector3.forward;
		_localLookPosition = base.transform.InverseTransformPoint(position);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		_animator.SetLookAtPosition(base.transform.TransformPoint(_localLookPosition));
		_animator.SetLookAtWeight(_animator.GetFloat("PlayerLookWeight"), 0.5f, 0.9f, 0f);
		float @float = _animator.GetFloat("StaffPlantedWeight");
		_animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandTransform.position);
		_animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandTransform.rotation * Quaternion.AngleAxis(-90f, Vector3.up));
		_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, @float);
		_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, @float);
		_animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTransform.position);
		_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.15f);
	}

	private void Footstep()
	{
		_foleyAudioSource.PlayOneShot(AudioType.MovementNomaiMetalFootstep);
	}

	private void Audio_EnterWriting()
	{
		_foleyAudioSource.PlayOneShot(AudioType.SolanumEnterWriting);
	}

	private void Audio_ExitWriting()
	{
		_foleyAudioSource.PlayOneShot(AudioType.SolanumExitWriting);
	}

	private void Audio_EnterIcon()
	{
		_foleyAudioSource.PlayOneShot(AudioType.SolanumEnterIcon);
	}

	private void Audio_ExitIcon()
	{
		_foleyAudioSource.PlayOneShot(AudioType.SolanumExitIcon);
	}

	private void Audio_EnterRaiseCairn()
	{
		_foleyAudioSource.PlayOneShot(AudioType.SolanumEnterRaiseCairn);
	}

	private void Audio_ExitRaiseCairn()
	{
		_foleyAudioSource.PlayOneShot(AudioType.SolanumExitRaiseCairn);
	}

	public void StartWatchingPlayer()
	{
		_animator.SetBool("WatchingPlayer", value: true);
	}

	public void StopWatchingPlayer()
	{
		_animator.SetBool("WatchingPlayer", value: false);
		_performingAction = true;
	}

	public void StartConversation()
	{
		_animator.SetBool("ListeningToPlayer", value: true);
	}

	public void EndConversation()
	{
		_animator.SetBool("ListeningToPlayer", value: false);
	}

	public void PlayCreateWordStones()
	{
		_animator.SetTrigger("CreateWordStones");
		_performingAction = true;
		_creatingWordStones = true;
		_staffLocked.SetActive(value: false);
		_staffUnlocked.SetActive(value: true);
	}

	public void PlayGestureToWordStones()
	{
		_animator.SetTrigger("GestureToWordStones");
		_performingAction = true;
	}

	public void PlayRaiseCairns()
	{
		_animator.SetTrigger("RaiseCairns");
		_performingAction = true;
	}

	public void PlayGestureToCairns()
	{
		_animator.SetTrigger("GestureToCairns");
		_performingAction = true;
	}

	public void StartWritingMessage()
	{
		_animator.SetBool("WritingResponse", value: true);
		_performingAction = true;
		_startingWrite = true;
	}

	public void StopWritingMessage(bool gestureToText)
	{
		_animator.SetBool("GestureOnFinishWriting", gestureToText);
		_animator.SetBool("WritingResponse", value: false);
	}

	public bool IsPlayerLooking()
	{
		Vector3 to = _headBoneTransform.position - _playerCameraTransform.position;
		return Vector3.Angle(_playerCameraTransform.forward, to) < 30f;
	}

	private void OnEnterAnimatorState(AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.IsName("Watching Player"))
		{
			_performingAction = false;
			if (_creatingWordStones)
			{
				_creatingWordStones = false;
				_staffLocked.SetActive(value: true);
				_staffUnlocked.SetActive(value: false);
			}
		}
	}

	private void AnimEvent_TouchRock(int rockIndex)
	{
		if (rockIndex == 0)
		{
			_symbolsAudioSource.PlayOneShot(AudioType.SolanumSymbolReveal);
		}
		if (this.OnTouchRock != null)
		{
			this.OnTouchRock(rockIndex);
		}
	}

	private void AnimEvent_WriteResponse()
	{
		if (_startingWrite)
		{
			_startingWrite = false;
			if (this.OnWriteResponse != null)
			{
				this.OnWriteResponse(-1);
			}
		}
	}

	private void OnEnableBigHeadMode()
	{
		Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.Neck);
		Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.Head);
		boneTransform.localScale = new Vector3(1.58f, 1.58f, 1.58f);
		boneTransform2.localScale = new Vector3(1.58f, 1.58f, 1.58f);
	}
}
