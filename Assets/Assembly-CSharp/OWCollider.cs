using UnityEngine;

[DisallowMultipleComponent]
public class OWCollider : MonoBehaviour
{
	public delegate void ColliderDisabledEvent(OWCollider collider);

	public const float MAX_RESTORATION_DELAY = 2f;

	[SerializeField]
	private DynamicOccupantMask _lodActivationMask;

	[SerializeField]
	private bool _ignorePhysicsSwapDelay;

	[SerializeField]
	private bool _drawBounds;

	[Header("Streaming")]
	[SerializeField]
	private bool _doNotStream;

	private bool _initialized;

	private Collider _collider;

	private bool _active = true;

	private bool _beingScaled;

	private int _lodLevel;

	private bool _parentBodySuspended;

	private bool _physicsRemoved;

	private float _swapPhysicsTime;

	private Mesh _removedMesh;

	private OWRigidbody _parentBody;

	public bool doNotStream => _doNotStream;

	public event ColliderDisabledEvent OnColliderDisabled;

	private void Awake()
	{
		Initialize();
		base.enabled = false;
	}

	private void Initialize()
	{
		_collider = this.GetRequiredComponent<Collider>();
		if (_lodActivationMask == null)
		{
			_lodActivationMask = new DynamicOccupantMask();
		}
		_lodLevel = ((!_collider.enabled) ? 1 : 0);
		_initialized = true;
	}

	private void OnDestroy()
	{
		if (_parentBody != null)
		{
			_parentBody.OnSuspendOWRigidbody -= OnSuspendOWRigidbody;
			_parentBody.OnUnsuspendOWRigidbody -= OnUnsuspendOWRigidbody;
		}
		if (this.OnColliderDisabled != null)
		{
			this.OnColliderDisabled(this);
		}
	}

	public void ClearParentBody()
	{
		if (_parentBody != null)
		{
			if (_parentBodySuspended)
			{
				_parentBodySuspended = false;
				UpdateColliderStatus();
			}
			_parentBody.OnSuspendOWRigidbody -= OnSuspendOWRigidbody;
			_parentBody.OnUnsuspendOWRigidbody -= OnUnsuspendOWRigidbody;
			_parentBody = null;
		}
	}

	public void ListenForParentBodySuspension()
	{
		_parentBody = GetComponentInParent<OWRigidbody>();
		if (_parentBody != null)
		{
			_parentBody.OnSuspendOWRigidbody += OnSuspendOWRigidbody;
			_parentBody.OnUnsuspendOWRigidbody += OnUnsuspendOWRigidbody;
		}
		else
		{
			Debug.LogError("Failed to find parent OWRigidbody", this);
		}
	}

	public void IgnorePhysicsSwapDelay()
	{
		_ignorePhysicsSwapDelay = true;
	}

	public Collider GetCollider()
	{
		if (!_initialized)
		{
			Initialize();
		}
		return _collider;
	}

	public bool IsActive()
	{
		return _active;
	}

	public void SetLODActivationMask(DynamicOccupant mask)
	{
		if (!_initialized)
		{
			Initialize();
		}
		_lodActivationMask.SetMask(mask);
	}

	public void SetActivation(bool active)
	{
		_active = active;
		UpdateColliderStatus();
	}

	public void BeginScaling()
	{
		_beingScaled = true;
		RemovePhysics();
	}

	public void EndScaling()
	{
		_beingScaled = false;
		RestorePhysics();
	}

	public void CheckLODActivation(DynamicOccupant occupantMask, float delayFraction = 0f)
	{
		if (!_initialized)
		{
			Initialize();
		}
		if (_lodActivationMask.ContainsAnyOccupants(occupantMask))
		{
			SetLODLevel(0, delayFraction * 2f);
		}
		else
		{
			SetLODLevel(1, 0.1f);
		}
	}

	public void SetLODLevel(int newLODLevel, float delay = 0f)
	{
		if (newLODLevel != _lodLevel)
		{
			_lodLevel = newLODLevel;
			if (_lodLevel == 0)
			{
				RestorePhysics(delay);
			}
			else
			{
				RemovePhysics(delay);
			}
		}
	}

	private void RemovePhysics(float removeDelay = 0f)
	{
		_physicsRemoved = true;
		if (removeDelay > 0f && !_ignorePhysicsSwapDelay)
		{
			base.enabled = true;
			_swapPhysicsTime = Time.time + removeDelay;
		}
		else
		{
			base.enabled = false;
			UpdateColliderStatus();
		}
	}

	private void RestorePhysics(float restoreDelay = 0f)
	{
		if (_lodLevel <= 0 && !_beingScaled)
		{
			_physicsRemoved = false;
			if (restoreDelay > 0f && !_ignorePhysicsSwapDelay)
			{
				base.enabled = true;
				_swapPhysicsTime = Time.time + restoreDelay;
			}
			else
			{
				base.enabled = false;
				UpdateColliderStatus();
			}
		}
	}

	private void UpdateColliderStatus()
	{
		if (!_initialized)
		{
			Initialize();
		}
		bool flag = _collider.enabled;
		_collider.enabled = _active && !_physicsRemoved && !_beingScaled && !_parentBodySuspended;
		if (this.OnColliderDisabled != null && flag && !_collider.enabled)
		{
			this.OnColliderDisabled(this);
		}
	}

	private void Update()
	{
		if (Time.time > _swapPhysicsTime)
		{
			if (_physicsRemoved)
			{
				RemovePhysics();
			}
			else
			{
				RestorePhysics();
			}
			base.enabled = false;
		}
	}

	private void OnSuspendOWRigidbody(OWRigidbody body)
	{
		_parentBodySuspended = true;
		UpdateColliderStatus();
	}

	private void OnUnsuspendOWRigidbody(OWRigidbody body)
	{
		_parentBodySuspended = false;
		UpdateColliderStatus();
	}

	private void OnDrawGizmos()
	{
		if (_drawBounds)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.size);
		}
	}
}
