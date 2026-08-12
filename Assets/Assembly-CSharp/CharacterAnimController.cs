using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimController : SectoredMonoBehaviour
{
	[SerializeField]
	private Animator _secondaryAnimator;

	[SerializeField]
	protected CharacterDialogueTree _dialogueTree;

	[Space]
	public OWTriggerVolume playerTrackingZone;

	public DampedSpring3D lookSpring = new DampedSpring3D();

	public bool lookOnlyWhenTalking = true;

	public float headTrackingWeight = 1f;

	protected bool _playerInHeadZone;

	protected float _currentLookWeight;

	protected bool _inConversation;

	[Space]
	[SerializeField]
	protected float _blinksPerMinute = 15f;

	[SerializeField]
	protected float _blinkDuration = 0.05f;

	[SerializeField]
	protected BlinkBlendshape _blinkStyle;

	[SerializeField]
	protected SkinnedMeshRenderer _skinRenderer;

	[Space]
	[SerializeField]
	protected bool _hasTalkAnimation;

	private Vector3 _currentLookTarget = Vector3.zero;

	private int _numBlendShapes = -1;

	private float _blinkSchedule;

	private float _blinkWeightGoal = 100f;

	private float _blinkWeight;

	protected Animator _animator;

	private bool _sectorActive;

	private bool _fastForwarding;

	private bool _meshLoaded;

	private StreamingRenderMeshHandle _streamingMeshHandle;

	protected override void Awake()
	{
		base.Awake();
		_animator = GetComponent<Animator>();
		if (_dialogueTree != null)
		{
			_dialogueTree.OnStartConversation += OnStartConversation;
			_dialogueTree.OnEndConversation += OnEndConversation;
		}
		if ((bool)playerTrackingZone)
		{
			_playerInHeadZone = false;
			playerTrackingZone.OnEntry += OnZoneEntry;
			playerTrackingZone.OnExit += OnZoneExit;
		}
		if (_skinRenderer != null)
		{
			_streamingMeshHandle = _skinRenderer.GetComponent<StreamingRenderMeshHandle>();
		}
		if (_streamingMeshHandle != null)
		{
			_streamingMeshHandle.OnMeshLoaded += OnMeshLoaded;
			_streamingMeshHandle.OnMeshUnloaded += OnMeshUnloaded;
		}
		if (_skinRenderer != null && _skinRenderer.sharedMesh != null)
		{
			_numBlendShapes = _skinRenderer.sharedMesh.blendShapeCount;
			_meshLoaded = true;
		}
		GlobalMessenger.AddListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.AddListener("EndFastForward", OnEndFastForward);
		GlobalMessenger.AddListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	private void OnZoneExit(GameObject input)
	{
		if (input.CompareTag("PlayerDetector"))
		{
			_playerInHeadZone = false;
		}
	}

	private void OnZoneEntry(GameObject input)
	{
		if (input.CompareTag("PlayerDetector"))
		{
			_playerInHeadZone = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_dialogueTree != null)
		{
			_dialogueTree.OnStartConversation -= OnStartConversation;
			_dialogueTree.OnEndConversation -= OnEndConversation;
		}
		if ((bool)playerTrackingZone)
		{
			playerTrackingZone.OnEntry -= OnZoneEntry;
			playerTrackingZone.OnExit -= OnZoneExit;
		}
		if (_streamingMeshHandle != null)
		{
			_streamingMeshHandle.OnMeshLoaded -= OnMeshLoaded;
			_streamingMeshHandle.OnMeshUnloaded -= OnMeshUnloaded;
		}
		GlobalMessenger.RemoveListener("StartFastForward", OnStartFastForward);
		GlobalMessenger.RemoveListener("EndFastForward", OnEndFastForward);
		GlobalMessenger.RemoveListener("EnableBigHeadMode", OnEnableBigHeadMode);
	}

	protected virtual void UpdateAnimatorActive()
	{
		_animator.enabled = _sectorActive && !_fastForwarding && (_streamingMeshHandle == null || _meshLoaded);
		if (_secondaryAnimator != null)
		{
			_secondaryAnimator.enabled = _animator.enabled;
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		_sectorActive = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		base.enabled = _sectorActive;
		UpdateAnimatorActive();
	}

	protected virtual void OnStartFastForward()
	{
		_fastForwarding = true;
		base.enabled = false;
		UpdateAnimatorActive();
	}

	protected virtual void OnEndFastForward()
	{
		_fastForwarding = false;
		base.enabled = _sectorActive;
		UpdateAnimatorActive();
	}

	protected virtual void OnMeshLoaded()
	{
		_meshLoaded = true;
		_numBlendShapes = _skinRenderer.sharedMesh.blendShapeCount;
		UpdateAnimatorActive();
	}

	protected virtual void OnMeshUnloaded()
	{
		_meshLoaded = false;
		_numBlendShapes = -1;
		UpdateAnimatorActive();
	}

	protected virtual void OnAnimatorIK()
	{
		Vector3 position = Locator.GetActiveCamera().transform.position;
		float b = headTrackingWeight * (float)Mathf.Min(1, (!lookOnlyWhenTalking) ? (_playerInHeadZone ? 1 : 0) : ((_inConversation && _playerInHeadZone) ? 1 : 0));
		_currentLookWeight = Mathf.Lerp(_currentLookWeight, b, Time.deltaTime * 2f);
		_currentLookTarget = lookSpring.Update(_currentLookTarget, position, Time.deltaTime);
		_animator.SetLookAtPosition(_currentLookTarget);
		_animator.SetLookAtWeight(_currentLookWeight);
	}

	protected virtual void LateUpdate()
	{
		if (!_skinRenderer || _numBlendShapes <= 0 || _blinkStyle == BlinkBlendshape.None || _blinksPerMinute <= 0f || _skinRenderer.sharedMesh == null)
		{
			return;
		}
		_blinkWeight = Mathf.Lerp(_blinkWeight, _blinkWeightGoal, Time.deltaTime / _blinkDuration);
		if (Time.time > _blinkSchedule)
		{
			if (_blinkWeightGoal > 0f)
			{
				float num = 60f / Random.Range(_blinksPerMinute, _blinksPerMinute * 1.5f);
				_blinkWeightGoal = 0f;
				_blinkSchedule = Time.time + num;
			}
			else
			{
				_blinkWeightGoal = 100f;
				_blinkSchedule = Time.time + _blinkDuration;
			}
		}
		int num2 = _numBlendShapes - 4;
		switch (_blinkStyle)
		{
		case BlinkBlendshape.FirstFour:
		{
			for (int j = 0; j < 4; j++)
			{
				_skinRenderer.SetBlendShapeWeight(j, _blinkWeight);
			}
			break;
		}
		case BlinkBlendshape.LastFour:
		{
			for (int k = 0; k < 4; k++)
			{
				_skinRenderer.SetBlendShapeWeight(num2 + k, _blinkWeight);
			}
			break;
		}
		case BlinkBlendshape.Gossan:
		{
			for (int i = 0; i < 3; i++)
			{
				_skinRenderer.SetBlendShapeWeight(4 + i, _blinkWeight);
			}
			break;
		}
		}
	}

	protected virtual void OnStartConversation()
	{
		_inConversation = true;
		if (_animator != null && _hasTalkAnimation)
		{
			_animator.SetTrigger("Talking");
		}
	}

	protected virtual void OnEndConversation()
	{
		_inConversation = false;
		if (_animator != null && _hasTalkAnimation)
		{
			_animator.SetTrigger("Idle");
		}
	}

	protected virtual void OnEnableBigHeadMode()
	{
		Transform boneTransform = _animator.GetBoneTransform(HumanBodyBones.Neck);
		Transform boneTransform2 = _animator.GetBoneTransform(HumanBodyBones.Head);
		if (boneTransform == null || _animator.name.Contains("Rutile") || _animator.name.Contains("Tuff") || _animator.name.Contains("Hal") || _animator.name.Contains("Esker"))
		{
			if (boneTransform2 != null)
			{
				boneTransform2.localScale = new Vector3(2.5f, 2.5f, 2.5f);
			}
			return;
		}
		if (boneTransform != null)
		{
			boneTransform.localScale = new Vector3(1.58f, 1.58f, 1.58f);
		}
		if (boneTransform2 != null)
		{
			boneTransform2.localScale = new Vector3(1.58f, 1.58f, 1.58f);
		}
	}
}
