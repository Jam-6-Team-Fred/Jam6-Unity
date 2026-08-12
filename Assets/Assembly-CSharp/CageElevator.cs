using UnityEngine;

public class CageElevator : MonoBehaviour, IItemDropTarget
{
	[Header("References")]
	[SerializeField]
	private OWRigidbody _platformBody;

	[Tooltip("Destination transforms, MUST be in order bottom - top")]
	[SerializeField]
	private ElevatorDestination[] _destinations;

	[SerializeField]
	private GameObject _elevatorFloorStandIn;

	[SerializeField]
	private OWTriggerVolume _killTriggerUpper;

	[SerializeField]
	private OWTriggerVolume _killTriggerLower;

	[SerializeField]
	private TransformAnimator _doorAnimatorLeft;

	[SerializeField]
	private TransformAnimator _doorAnimatorRight;

	[SerializeField]
	private AbstractGhostElevatorInterface _ghostInterface;

	[SerializeField]
	private Transform _chainsRoot;

	[SerializeField]
	private Transform _chainsEndPoint;

	[SerializeField]
	private float _initChainLength = 3f;

	[SerializeField]
	private OWCollider _leftDoorStandIn;

	[SerializeField]
	private OWCollider _rightDoorStandIn;

	[SerializeField]
	private OWCollider _bodyStandIn;

	[SerializeField]
	private OWLightController _topLight;

	[SerializeField]
	private OWRenderer _teleportChainsRenderer;

	[Header("Controls")]
	[SerializeField]
	private int _startDestination;

	[SerializeField]
	private bool _startOpen = true;

	[SerializeField]
	private bool _lightsStartOff;

	[SerializeField]
	private float _ascendAccel;

	[SerializeField]
	private float _ascendMaxSpeed;

	[SerializeField]
	private float _descendAccel;

	[SerializeField]
	private float _descendMaxSpeed;

	[Header("Audio")]
	[SerializeField]
	private OWAudioSource _oneShotAudio;

	[SerializeField]
	private OWAudioSource _loopingAudio;

	[SerializeField]
	private OWAudioSource _mechLoopingAudio;

	public OWEvent<int> OnArrive = new OWEvent<int>(4);

	private float _currentSpeed;

	private int _currentFromIdx;

	private int _currentDestinationIdx;

	private bool _moving;

	private OWRigidbody _parentBody;

	private float _warpDistance;

	private Transform _upperWarp;

	private Transform _lowerWarp;

	private bool _hasWarp;

	private bool _willPassWarp;

	private bool _hasPassedWarp;

	private bool _lateralTeleport;

	private bool _teleportChainsActive;

	private Transform _teleportChainsTransform;

	private int _goUpCommandIndex;

	private int _goDownCommandIndex;

	private OWRenderer _chainsRenderer;

	private MaterialPropertyBlock _chainsPropertyBlock;

	private MaterialPropertyBlock _teleportChainsPropertyBlock;

	private int _playerKillTriggerCount;

	private float _surrogateChainScale;

	public OWRigidbody elevatorBody => _platformBody;

	public OWLightController topLight => _topLight;

	private float currentNormalizedProgress => 1f - Mathf.Clamp01((currentDestination.position - _platformBody.GetPosition()).sqrMagnitude / (currentDestination.position - currentFrom.position).sqrMagnitude);

	private Vector3 destinationVelocity => _platformBody.GetOrigParentBody().GetPointVelocity(currentDestination.position);

	private Vector3 fromVelocity => _platformBody.GetOrigParentBody().GetPointVelocity(currentFrom.position);

	private Transform currentDestination => _destinations[_currentDestinationIdx].transform;

	private Transform currentFrom => _destinations[_currentFromIdx].transform;

	public bool isAtBottom => _currentFromIdx <= 0;

	public bool isAtTop => _currentFromIdx >= _destinations.Length - 1;

	public int currentLevelIdx => _currentFromIdx;

	public bool isMoving => _moving;

	public bool teleportChainsActive
	{
		get
		{
			return _teleportChainsActive;
		}
		set
		{
			if (_teleportChainsRenderer != null)
			{
				_teleportChainsActive = value;
				_teleportChainsRenderer.SetActivation(_teleportChainsActive);
				if (value)
				{
					InitSurrogateChains();
				}
			}
			else
			{
				_teleportChainsActive = false;
			}
		}
	}

	private void Start()
	{
		if (_ghostInterface == null)
		{
			Debug.LogError("Elevator has no interface!", this);
		}
		else
		{
			_ghostInterface.OnActivate += OnSwitchLevel;
			_ghostInterface.OnDownSelected += GoDown;
			_ghostInterface.OnUpSelected += GoUp;
			_ghostInterface.SetStartingPosition(_startDestination != 0);
		}
		if (_destinations == null || _destinations.Length == 0)
		{
			Debug.LogWarning("Elevator has no destinations!", this);
		}
		if (_startDestination < 0 || _startDestination >= _destinations.Length)
		{
			Debug.LogWarning("Elevator given out of range starting floor. Given value will be clamped.", this);
		}
		_currentFromIdx = Mathf.Clamp(_startDestination, 0, _destinations.Length - 1);
		_currentDestinationIdx = _currentFromIdx;
		_parentBody = _platformBody.GetOrigParentBody();
		SetupCallDelegates();
		_killTriggerLower.transform.parent = _destinations[0].transform;
		_killTriggerLower.transform.localPosition = Vector3.zero;
		_killTriggerLower.transform.localRotation = Quaternion.identity;
		_killTriggerLower.OnEntry += OnEnterKillTrigger;
		_killTriggerLower.OnExit += OnExitKillTrigger;
		_killTriggerUpper.OnEntry += OnEnterKillTrigger;
		_killTriggerUpper.OnExit += OnExitKillTrigger;
		if (_chainsRoot == null)
		{
			Debug.LogError("Elevator doesn't have chains!", this);
			Debug.Break();
		}
		_chainsRenderer = _chainsRoot.GetComponent<OWRenderer>();
		SetReached(reached: true, instant: true, _startOpen);
		_chainsPropertyBlock = new MaterialPropertyBlock();
		if (_teleportChainsRenderer != null)
		{
			_teleportChainsPropertyBlock = new MaterialPropertyBlock();
		}
		teleportChainsActive = false;
		if (_teleportChainsRenderer != null)
		{
			_teleportChainsTransform = _teleportChainsRenderer.transform;
		}
		UpdateChainVisuals();
		if (_lightsStartOff)
		{
			_topLight.FadeTo(0f, 0f);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _destinations.Length; i++)
		{
			_destinations[i].OnElevatorCalled -= new OWEvent<int>.OWCallback(GoToDestination);
		}
		_killTriggerLower.OnEntry -= OnEnterKillTrigger;
		_killTriggerLower.OnExit -= OnExitKillTrigger;
		_killTriggerUpper.OnEntry -= OnEnterKillTrigger;
		_killTriggerUpper.OnExit -= OnExitKillTrigger;
		if (_ghostInterface != null)
		{
			_ghostInterface.OnActivate -= OnSwitchLevel;
			_ghostInterface.OnDownSelected -= GoDown;
			_ghostInterface.OnUpSelected -= GoUp;
		}
	}

	private void SetupCallDelegates()
	{
		for (int i = 0; i < _destinations.Length; i++)
		{
			int floorIndex = i;
			_destinations[i].SetFloorIndex(floorIndex);
			_destinations[i].SetElevator(this);
			_destinations[i].OnElevatorCalled += new OWEvent<int>.OWCallback(GoToDestination);
		}
	}

	private bool IsGoingUp()
	{
		return _platformBody.transform.InverseTransformPoint(currentDestination.position).y > 0f;
	}

	private bool HasPassedWarp()
	{
		if (!_hasWarp)
		{
			return true;
		}
		return _hasPassedWarp;
	}

	private float GetRemainingDistance(Vector3 toTarget)
	{
		float magnitude = toTarget.magnitude;
		if (!_hasWarp)
		{
			return magnitude;
		}
		float result = magnitude - _warpDistance;
		if (!HasPassedWarp())
		{
			return result;
		}
		return magnitude;
	}

	private void FixedUpdate()
	{
		Vector3 vector = Vector3.Project(currentDestination.position - _platformBody.GetPosition(), base.transform.up);
		vector = ((!IsGoingUp()) ? (-vector) : vector);
		float remainingDistance = GetRemainingDistance(vector);
		if (remainingDistance > 0.01f)
		{
			Vector3 pointVelocity = _parentBody.GetPointVelocity(_platformBody.GetPosition());
			float num = (IsGoingUp() ? _ascendAccel : (0f - _descendAccel));
			if (Mathf.Sign(_currentSpeed) == Mathf.Sign(_platformBody.transform.InverseTransformPoint(currentDestination.position).y) && Mathf.Pow(_currentSpeed, 2f) / Mathf.Abs(num) >= remainingDistance)
			{
				num = 0f - num;
			}
			_currentSpeed = Mathf.Clamp(_currentSpeed + num * Time.fixedDeltaTime, 0f - _descendMaxSpeed, _ascendMaxSpeed);
			_currentSpeed = ((remainingDistance / Time.fixedDeltaTime < Mathf.Abs(_currentSpeed)) ? (Mathf.Sign(_currentSpeed) * remainingDistance / Time.fixedDeltaTime) : _currentSpeed);
			Vector3 vector2 = vector.normalized * _currentSpeed;
			_platformBody.SetVelocity(pointVelocity + vector2);
			_platformBody.SetAngularVelocity(_parentBody.GetAngularVelocity());
		}
		else
		{
			SetReached(reached: true);
			OnArrive.Invoke(_currentDestinationIdx);
		}
	}

	private void LateUpdate()
	{
		UpdateChainVisuals();
	}

	private void GoUp()
	{
		if (!_moving || !IsGoingUp())
		{
			_currentDestinationIdx = Mathf.Clamp(_currentDestinationIdx + 1, 0, _destinations.Length - 1);
			SetReached(reached: false);
		}
	}

	private void GoDown()
	{
		if (!_moving || IsGoingUp())
		{
			_currentDestinationIdx = Mathf.Clamp(_currentDestinationIdx - 1, 0, _destinations.Length - 1);
			SetReached(reached: false);
		}
	}

	private void GoToFloor(int floorIdx)
	{
		if (floorIdx < 0 || floorIdx >= _destinations.Length)
		{
			Debug.LogError("Elevator given out of range floor order", this);
			return;
		}
		_currentDestinationIdx = floorIdx;
		SetReached(reached: false);
	}

	public void SetHasWarp(Transform upperWarp, Transform lowerWarp)
	{
		if (!_hasWarp)
		{
			_upperWarp = upperWarp;
			_lowerWarp = lowerWarp;
			Vector3 vector = upperWarp.position - lowerWarp.position;
			_warpDistance = Vector3.Project(vector, base.transform.up).magnitude;
			_lateralTeleport = Vector3.Dot(vector, lowerWarp.up) - 1f > 0.0001f;
			_hasWarp = true;
		}
	}

	public void Warped(bool toLower)
	{
		_hasPassedWarp = true;
		teleportChainsActive = toLower && _lateralTeleport;
		UpdateChainVisuals();
	}

	private void SetReached(bool reached, bool instant = false, bool openDoor = true)
	{
		if (reached)
		{
			_currentFromIdx = _currentDestinationIdx;
			_loopingAudio.FadeOut(0.2f);
			_mechLoopingAudio.FadeOut(0.2f);
			_oneShotAudio.PlayOneShot(AudioType.CageElevator_End);
			if (openDoor)
			{
				if (!instant)
				{
					_doorAnimatorLeft.RotateToLocalEulerAngles(new Vector3(0f, 24f, 0f), 0.5f);
					_doorAnimatorRight.RotateToLocalEulerAngles(new Vector3(0f, -24f, 0f), 0.5f);
				}
				else
				{
					_doorAnimatorLeft.transform.localEulerAngles = new Vector3(0f, 24f, 0f);
					_doorAnimatorRight.transform.localEulerAngles = new Vector3(0f, -24f, 0f);
				}
			}
			_currentSpeed = 0f;
			Transform transform = currentFrom;
			_platformBody.Suspend();
			_platformBody.transform.parent = transform;
			_platformBody.transform.localPosition = Vector3.zero;
			_platformBody.transform.localRotation = Quaternion.identity;
			_elevatorFloorStandIn.transform.SetPositionAndRotation(transform.position, transform.rotation);
			EnableStandInColliders(!openDoor);
			_moving = false;
			base.enabled = false;
			_ghostInterface.OnActionComplete();
		}
		else
		{
			if (_currentDestinationIdx == _currentFromIdx && !_moving)
			{
				return;
			}
			if (!_moving)
			{
				DisableStandInColliders();
				_platformBody.transform.parent = null;
				_platformBody.Unsuspend();
				_oneShotAudio.PlayOneShot(AudioType.CageElevator_Start);
				_loopingAudio.FadeIn(0.2f);
				_mechLoopingAudio.FadeIn(0.2f);
				_doorAnimatorLeft.RotateToOriginalLocalRotation(0.5f);
				_doorAnimatorRight.RotateToOriginalLocalRotation(0.5f);
				_moving = true;
				base.enabled = true;
				if (_lightsStartOff)
				{
					_topLight.FadeTo(1f, 0.2f);
				}
			}
			if (_hasWarp)
			{
				_willPassWarp = DetermineWillPassWarp();
				_hasPassedWarp = !_willPassWarp;
			}
			SurveyorProbe probe = Locator.GetProbe();
			if (probe != null && probe.transform.IsChildOf(_elevatorFloorStandIn.transform))
			{
				probe.Unanchor();
				probe.GetAnchor().AnchorToObject(_platformBody.gameObject, -probe.transform.forward, probe.transform.position);
			}
		}
	}

	private void DisableStandInColliders()
	{
		_bodyStandIn.SetActivation(active: false);
		_leftDoorStandIn.SetActivation(active: false);
		_rightDoorStandIn.SetActivation(active: false);
	}

	private void EnableStandInColliders(bool enableDoor)
	{
		_bodyStandIn.SetActivation(active: true);
		_leftDoorStandIn.SetActivation(enableDoor);
		_rightDoorStandIn.SetActivation(enableDoor);
	}

	private void InitSurrogateChains()
	{
		float magnitude = Vector3.Project(_upperWarp.position - _chainsEndPoint.position, -_teleportChainsRenderer.transform.parent.up).magnitude;
		_surrogateChainScale = magnitude / _initChainLength;
		_teleportChainsTransform.localPosition = -_teleportChainsTransform.parent.up * magnitude;
		_teleportChainsTransform.localScale = new Vector3(1f, _surrogateChainScale, 1f);
		_teleportChainsRenderer.GetRenderer().GetPropertyBlock(_teleportChainsPropertyBlock);
		_teleportChainsPropertyBlock.SetVector("_MainTex_ST", new Vector4(_surrogateChainScale, 1f, 0f, 0f));
		_teleportChainsRenderer.GetRenderer().SetPropertyBlock(_teleportChainsPropertyBlock);
	}

	private void UpdateChainVisuals()
	{
		Vector3 vector = (teleportChainsActive ? _lowerWarp.position : _chainsEndPoint.position);
		float num = Vector3.Project(_chainsRoot.position - vector, _platformBody.transform.up).magnitude / _initChainLength;
		_chainsRoot.transform.localScale = new Vector3(1f, num, 1f);
		_chainsRenderer.GetRenderer().GetPropertyBlock(_chainsPropertyBlock);
		_chainsPropertyBlock.SetVector("_MainTex_ST", new Vector4(num, 1f, 0f, 0f));
		_chainsRenderer.GetRenderer().SetPropertyBlock(_chainsPropertyBlock);
		if (teleportChainsActive)
		{
			float num2 = Vector3.Project(_chainsRoot.position - _chainsEndPoint.position, _teleportChainsTransform.parent.up).magnitude - _warpDistance;
			_teleportChainsRenderer.GetRenderer().GetPropertyBlock(_teleportChainsPropertyBlock);
			_teleportChainsPropertyBlock.SetVector("_MainTex_ST", new Vector4(_surrogateChainScale, 1f, Mathf.Max(0f, num2 / _initChainLength), 0f));
			_teleportChainsRenderer.GetRenderer().SetPropertyBlock(_teleportChainsPropertyBlock);
		}
	}

	private bool DetermineWillPassWarp()
	{
		Vector3 vector = currentDestination.position - _platformBody.GetPosition();
		Ray ray = new Ray(_platformBody.GetPosition(), vector.normalized);
		float enter = 0f;
		float enter2 = 0f;
		Plane plane = new Plane(_upperWarp.up, _upperWarp.position);
		Plane plane2 = new Plane(_lowerWarp.up, _lowerWarp.position);
		if (plane.Raycast(ray, out enter2) && enter2 * enter2 <= vector.sqrMagnitude)
		{
			return true;
		}
		if (plane2.Raycast(ray, out enter) && enter * enter <= vector.sqrMagnitude)
		{
			return true;
		}
		return false;
	}

	public Transform GetItemDropTargetTransform(GameObject raycastTarget)
	{
		return _platformBody.transform;
	}

	public void AddDroppedItem(GameObject dropTarget, OWItem item)
	{
	}

	private void OnSwitchLevel()
	{
		if (!_moving)
		{
			if (isAtBottom)
			{
				GoUp();
			}
			else
			{
				GoDown();
			}
		}
		else if (IsGoingUp())
		{
			GoDown();
		}
		else
		{
			GoUp();
		}
	}

	public void GoToDestination(int floorIdx)
	{
		_ghostInterface.SetStartingPosition(floorIdx != 0);
		GoToFloor(floorIdx);
	}

	private void OnEnterKillTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerKillTriggerCount++;
			if (_playerKillTriggerCount >= 2 && _moving && _currentSpeed < 0f)
			{
				Locator.GetDeathManager().KillPlayer(DeathType.CrushedByElevator);
				RumbleManager.PlayerCrushedByElevator();
				Achievements.Earn(Achievements.Type.FLAT_HEARTHER);
				_currentSpeed = 0f;
			}
		}
	}

	private void OnExitKillTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerKillTriggerCount--;
		}
	}
}
