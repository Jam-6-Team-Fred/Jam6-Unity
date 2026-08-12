using UnityEngine;

public class NomaiInterfaceOrb : SectoredMonoBehaviour
{
	private static MaterialPropertyBlock s_matPropBlock;

	private static int s_propID_EmissionColor;

	[SerializeField]
	private Collider _interactibleCollider;

	[SerializeField]
	private SphereCollider _physicalCollider;

	[Space]
	[SerializeField]
	private Renderer _glowRenderer;

	[SerializeField]
	private Renderer _extraGlowRenderer;

	[SerializeField]
	private Light _glowLight;

	[SerializeField]
	private Light _extraGlowLight;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _draggingGlowColor = Color.white;

	[Space]
	[SerializeField]
	private float _startDragDist = 5f;

	[SerializeField]
	private float _cancelDragDist = 15f;

	[SerializeField]
	private float _maxSpeed = 5f;

	[Space]
	[SerializeField]
	private GameObject _slotRoot;

	[SerializeField]
	private bool _freezeLocalZAxis;

	[SerializeField]
	private bool _freezeLocalRotation;

	[SerializeField]
	private bool _isQuantum;

	[SerializeField]
	private bool _isOnFragment;

	[Space]
	[SerializeField]
	private bool _applyForcesWhileMoving;

	[SerializeField]
	private OWRail[] _safetyRails;

	[SerializeField]
	private float _maxRailDistance;

	protected OWRigidbody _orbBody;

	private OWRigidbody _parentBody;

	private AstroObject _parentAstroObject;

	private NomaiInterfaceSlot[] _slots = new NomaiInterfaceSlot[0];

	private NomaiInterfaceSlot _occupiedSlot;

	private OWCollider _owCollider;

	private bool _isBeingDragged;

	private bool _loseFocusToStartDrag;

	private bool _belowSand;

	private bool _centeredInSlot;

	private Vector3 _localTargetPos;

	private float _enterSlotTime;

	private float _origLocalZPos;

	private int _lockCount;

	private float _unsuspendTime;

	private float _speedFraction;

	private float _glowFraction;

	private float _glowIntensity;

	private Color _glowBaseColor;

	private NomaiOrbAudio _orbAudio;

	private QuantumObject[] _quantumObjects;

	private DetachableFragment _parentFragment;

	private ForceApplier _forceApplier;

	private void OnValidate()
	{
		if (_freezeLocalRotation && _extraGlowRenderer == null)
		{
			Debug.LogWarning("Don't freeze rotation unless it's a double orb (Nomai gateways only)");
			_freezeLocalRotation = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_orbBody = this.GetAttachedOWRigidbody();
		_origLocalZPos = _orbBody.GetOrigParent().InverseTransformPoint(base.transform.position).z;
		_owCollider = _physicalCollider.gameObject.GetAddComponent<OWCollider>();
		if (_glowRenderer != null)
		{
			if (s_matPropBlock == null)
			{
				s_matPropBlock = new MaterialPropertyBlock();
				s_propID_EmissionColor = Shader.PropertyToID("_EmissionColor");
			}
			_glowBaseColor = _glowRenderer.sharedMaterial.GetColor(s_propID_EmissionColor);
			s_matPropBlock.SetColor(s_propID_EmissionColor, _glowBaseColor);
			_glowRenderer.SetPropertyBlock(s_matPropBlock);
			if (_extraGlowRenderer != null)
			{
				_extraGlowRenderer.SetPropertyBlock(s_matPropBlock);
			}
		}
		if (_glowLight != null)
		{
			_glowIntensity = _glowLight.intensity;
		}
		if (_slotRoot != null)
		{
			_slots = _slotRoot.GetComponentsInChildren<NomaiInterfaceSlot>();
		}
		_orbAudio = GetComponent<NomaiOrbAudio>();
		if (_isQuantum)
		{
			_quantumObjects = _orbBody.GetOrigParent().GetComponentsInParent<QuantumObject>();
			for (int i = 0; i < _quantumObjects.Length; i++)
			{
				_quantumObjects[i].OnPreCollapse += OnPreCollapse;
				_quantumObjects[i].OnPostCollapse += OnPostCollapse;
			}
		}
		if (_isOnFragment)
		{
			_parentFragment = _orbBody.GetOrigParent().GetComponentInParent<DetachableFragment>();
			if (_parentFragment == null)
			{
				Debug.LogError("Couldn't find DetachableFragment in hierarchy!", this);
				return;
			}
			_parentFragment.OnDetachFragment += OnParentFragmentDetach;
			_parentFragment.OnBeginWarpScaling += OnBeginWarpScaling;
			_parentFragment.OnEndWarpScaling += OnEndWarpScaling;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_isQuantum)
		{
			for (int i = 0; i < _quantumObjects.Length; i++)
			{
				if (_quantumObjects[i] != null)
				{
					_quantumObjects[i].OnPreCollapse -= OnPreCollapse;
					_quantumObjects[i].OnPostCollapse -= OnPostCollapse;
				}
			}
		}
		if (_parentFragment != null)
		{
			_parentFragment.OnDetachFragment -= OnParentFragmentDetach;
			_parentFragment.OnBeginWarpScaling -= OnBeginWarpScaling;
			_parentFragment.OnEndWarpScaling -= OnEndWarpScaling;
		}
	}

	private void Start()
	{
		_forceApplier = GetComponent<ForceApplier>();
		_parentBody = _orbBody.GetOrigParentBody();
		_parentAstroObject = _parentBody.GetComponent<AstroObject>();
		SetTargetPosition(base.transform.position);
		if (_sector != null)
		{
			AddLock();
		}
		CheckSlotCollision(playAudio: false);
		base.enabled = false;
	}

	public void SetParentBody(OWRigidbody parentBody)
	{
		_parentBody = parentBody;
		_parentAstroObject = _parentBody.GetComponent<AstroObject>();
		if (_orbBody.IsSuspended())
		{
			_orbBody.ChangeSuspensionBody(_parentBody);
		}
		if (_isQuantum)
		{
			for (int i = 0; i < _quantumObjects.Length; i++)
			{
				_quantumObjects[i].OnPreCollapse -= OnPreCollapse;
				_quantumObjects[i].OnPostCollapse -= OnPostCollapse;
			}
			_quantumObjects = null;
		}
	}

	public void AddLock()
	{
		if (_parentBody == null)
		{
			_parentBody = _orbBody.GetOrigParentBody();
		}
		AddLock(_orbBody.GetOrigParent(), _parentBody);
	}

	public void AddLock(Transform suspendParent, OWRigidbody suspensionRigidbody)
	{
		_lockCount++;
		if (_lockCount == 1)
		{
			CancelDrag();
			_orbBody.Suspend(suspendParent, suspensionRigidbody);
			if (_orbAudio != null)
			{
				_orbAudio.StopAllAudio();
			}
			_isBeingDragged = false;
			_centeredInSlot = false;
			_speedFraction = 0f;
		}
	}

	public void RemoveLock()
	{
		_lockCount--;
	}

	public void RemoveAllLocks()
	{
		_lockCount = 0;
	}

	public bool HasLock()
	{
		return _lockCount > 0;
	}

	public bool IsBeingDragged()
	{
		return _isBeingDragged;
	}

	public void OnLoseStartDragFocus()
	{
		_loseFocusToStartDrag = false;
	}

	public bool StartDragFromPosition(Vector3 manipPos)
	{
		if (_orbBody.IsSuspended())
		{
			return false;
		}
		if (RecentlyEnteredSlot())
		{
			_loseFocusToStartDrag = true;
		}
		if (Vector3.Distance(manipPos, base.transform.position) < _startDragDist)
		{
			if (!_loseFocusToStartDrag)
			{
				_isBeingDragged = true;
				_interactibleCollider.enabled = false;
				if (_orbAudio != null)
				{
					_orbAudio.PlayStartDragClip();
				}
			}
		}
		else
		{
			_loseFocusToStartDrag = false;
		}
		return _isBeingDragged;
	}

	public bool UpdateDragFromPosition(Vector3 manipPos, Vector3 targetPos)
	{
		if (Vector3.Distance(manipPos, base.transform.position) < _cancelDragDist)
		{
			SetTargetPosition(targetPos);
		}
		else
		{
			CancelDrag();
		}
		return _isBeingDragged;
	}

	public void CancelDrag()
	{
		_isBeingDragged = false;
		_interactibleCollider.enabled = true;
	}

	public void SetOrbPosition(Vector3 position)
	{
		base.transform.position = position;
		_isBeingDragged = true;
		CheckSlotCollision(playAudio: false);
		_isBeingDragged = false;
	}

	protected override void OnSectorOccupantAdded(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			RemoveLock();
			_unsuspendTime = Time.time + 2f;
		}
	}

	protected override void OnSectorOccupantRemoved(SectorDetector sectorDetector)
	{
		if (sectorDetector.GetOccupantType() == DynamicOccupant.Player)
		{
			AddLock();
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	public NomaiInterfaceSlot GetCurrentSlot()
	{
		return _occupiedSlot;
	}

	private bool RecentlyEnteredSlot()
	{
		if (_occupiedSlot != null)
		{
			return Time.time - _enterSlotTime < 0.5f;
		}
		return false;
	}

	private void SetTargetPosition(Vector3 worldPos)
	{
		_localTargetPos = _orbBody.GetOrigParent().InverseTransformPoint(worldPos);
		if (_freezeLocalZAxis)
		{
			_localTargetPos.z = _origLocalZPos;
		}
	}

	private void Update()
	{
		float target = (_isBeingDragged ? 1f : 0f);
		_glowFraction = Mathf.MoveTowards(_glowFraction, target, Time.deltaTime * 3f);
		if (_glowRenderer != null)
		{
			Color value = Color.Lerp(_glowBaseColor, _draggingGlowColor, _glowFraction);
			s_matPropBlock.SetColor(s_propID_EmissionColor, value);
			_glowRenderer.SetPropertyBlock(s_matPropBlock);
			if (_extraGlowRenderer != null)
			{
				_extraGlowRenderer.SetPropertyBlock(s_matPropBlock);
			}
		}
		if (_glowLight != null)
		{
			_glowLight.intensity = _glowIntensity * _glowFraction;
		}
		if (_extraGlowLight != null)
		{
			_extraGlowLight.intensity = _glowIntensity * _glowFraction;
		}
		if (_orbAudio != null)
		{
			_orbAudio.UpdateMovementAudio(_isBeingDragged, _speedFraction);
		}
	}

	private void FixedUpdate()
	{
		_forceApplier.SetApplyForces(applyForces: true);
		if (_parentAstroObject != null && _parentAstroObject.GetSandLevelController() != null && _sector.GetName() != Sector.Name.TimeLoopDevice)
		{
			CheckSandLevel();
		}
		if (_orbBody.IsSuspended())
		{
			if (_occupiedSlot != null && _occupiedSlot.HasActivationListeners() && !_centeredInSlot && _orbBody.GetOrigParent() != null)
			{
				Transform parent = _orbBody.transform.parent;
				Vector3 vector = parent.InverseTransformPoint(base.transform.position);
				Vector3 vector2 = parent.InverseTransformPoint(_occupiedSlot.transform.position);
				if (parent.Equals(_orbBody.GetOrigParent()))
				{
					if (_freezeLocalZAxis)
					{
						vector.z = _origLocalZPos;
						vector2.z = _origLocalZPos;
					}
					if (_freezeLocalRotation)
					{
						Vector3 forward = _orbBody.GetOrigParent().forward;
						base.transform.forward = forward;
					}
				}
				_orbBody.transform.localPosition = Vector3.MoveTowards(vector, vector2, Time.deltaTime);
				if (Vector3.Distance(vector, vector2) < 0.01f)
				{
					_centeredInSlot = true;
				}
			}
			if (_lockCount == 0 && Time.time > _unsuspendTime)
			{
				_orbBody.Unsuspend(restoreCachedVelocity: false);
			}
			return;
		}
		CheckSlotCollision();
		Vector3 direction = _parentBody.GetPointVelocity(_orbBody.GetPosition()) - _orbBody.GetVelocity();
		if (_freezeLocalZAxis)
		{
			if (_orbBody.GetOrigParent() != null)
			{
				Vector3 position = _orbBody.GetOrigParent().InverseTransformPoint(base.transform.position);
				position.z = _origLocalZPos;
				_orbBody.MoveToPosition(_orbBody.GetOrigParent().TransformPoint(position));
				Vector3 direction2 = _orbBody.GetOrigParent().InverseTransformDirection(direction);
				direction2.z = 0f;
				direction = _orbBody.GetOrigParent().TransformDirection(direction2);
			}
			else
			{
				Debug.LogError("Orb body's original parent is NULL!");
				Debug.Break();
			}
		}
		if (_freezeLocalRotation)
		{
			if (_orbBody.GetOrigParent() != null)
			{
				Vector3 forward2 = _orbBody.GetOrigParent().forward;
				_orbBody.SetAngularVelocity(Vector3.zero);
				_orbBody.AddAngularVelocityChange(OWPhysics.FromToAngularVelocity(base.transform.forward, forward2));
			}
			else
			{
				Debug.LogError("Orb body's original parent is NULL!");
				Debug.Break();
			}
		}
		if (_safetyRails.Length != 0)
		{
			float num = float.PositiveInfinity;
			Vector3 vector3 = Vector3.zero;
			for (int i = 0; i < _safetyRails.Length; i++)
			{
				Vector3 closestPoint;
				float num2 = _safetyRails[i].FindClosestPointOnRail(base.transform.position, out closestPoint);
				if (num2 < num)
				{
					num = num2;
					vector3 = closestPoint;
				}
			}
			if (num > _maxRailDistance)
			{
				base.transform.position = vector3;
				_orbBody.SetVelocity(_parentBody.GetPointVelocity(vector3));
				CancelDrag();
			}
		}
		_speedFraction = direction.magnitude / _maxSpeed;
		if (!_isBeingDragged && ((_occupiedSlot != null && _occupiedSlot.IsAttractive()) || _orbBody.IsSuspended()))
		{
			_speedFraction = 0f;
		}
		if (_isBeingDragged)
		{
			MoveTowardPosition(_orbBody.GetOrigParent().TransformPoint(_localTargetPos));
		}
		else if (_occupiedSlot != null && _occupiedSlot.IsAttractive())
		{
			MoveTowardPosition(_occupiedSlot.transform.position);
		}
	}

	private void CheckSlotCollision(bool playAudio = true)
	{
		if (_occupiedSlot == null)
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				if (_slots[i] != null && _slots[i].CheckOrbCollision(this))
				{
					_occupiedSlot = _slots[i];
					_enterSlotTime = Time.time;
					if (_slots[i].CancelsDragOnCollision())
					{
						CancelDrag();
					}
					if (playAudio && _orbAudio != null && _slots[i].GetPlayActivationAudio())
					{
						_orbAudio.PlaySlotActivatedClip();
					}
					break;
				}
			}
		}
		else if ((!_occupiedSlot.IsAttractive() || _isBeingDragged) && !_occupiedSlot.CheckOrbCollision(this))
		{
			_occupiedSlot = null;
		}
		_owCollider.SetActivation(_occupiedSlot == null || !_occupiedSlot.IsAttractive() || _isBeingDragged);
	}

	private void MoveTowardPosition(Vector3 targetPos)
	{
		Vector3 vector = targetPos - _orbBody.GetPosition();
		float a = 5f * Mathf.Pow(vector.magnitude, 0.5f);
		float b = Mathf.Min(_maxSpeed, vector.magnitude / Time.deltaTime);
		a = Mathf.Min(a, b);
		Vector3 pointVelocity = _parentBody.GetPointVelocity(_orbBody.GetPosition());
		Vector3 vector2 = vector.normalized * a;
		_orbBody.SetVelocity(pointVelocity + vector2);
		if (!_applyForcesWhileMoving)
		{
			_forceApplier.SetApplyForces(applyForces: false);
		}
	}

	private void CheckSandLevel()
	{
		float num = Vector3.Distance(base.transform.position, _parentAstroObject.GetSandLevelController().transform.position) - _parentAstroObject.GetSandLevelController().GetRadius();
		bool belowSand = _belowSand;
		_belowSand = num < _physicalCollider.radius + 0.01f;
		if (!belowSand && _belowSand)
		{
			AddLock();
		}
		else if (belowSand && !_belowSand)
		{
			RemoveLock();
		}
	}

	private void OnPreCollapse(QuantumObject obj)
	{
		AddLock();
	}

	private void OnPostCollapse(QuantumObject obj, bool changedState)
	{
		RemoveLock();
	}

	private void OnParentFragmentDetach(OWRigidbody fragmentBody, OWRigidbody parentBody)
	{
		SetParentBody(fragmentBody);
		GetComponent<ConstantForceDetector>().ClearAllFields();
	}

	private void OnBeginWarpScaling()
	{
		AddLock();
	}

	private void OnEndWarpScaling()
	{
		RemoveLock();
	}
}
