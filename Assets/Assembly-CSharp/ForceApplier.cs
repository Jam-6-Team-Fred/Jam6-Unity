using UnityEngine;

public class ForceApplier : MonoBehaviour
{
	private OWRigidbody _attachedBody;

	private ForceDetector _forceDetector;

	private FluidDetector _fluidDetector;

	private bool _skipFrame;

	private bool _isPlayer;

	private bool _applyForces = true;

	private bool _applyFluids = true;

	private void Awake()
	{
		_attachedBody = base.gameObject.GetAttachedOWRigidbody();
		_attachedBody.OnSuspendOWRigidbody += OnAttachedBodySuspend;
		_attachedBody.OnPreUnsuspendOWRigidbody += OnAttachedBodyPreUnsuspend;
		_attachedBody.OnWarpOWRigidbody += OnWarpOWRigidbody;
		_isPlayer = base.gameObject.CompareTag("PlayerDetector");
	}

	private void Start()
	{
		_forceDetector = GetComponent<ForceDetector>();
		_fluidDetector = GetComponent<FluidDetector>();
	}

	private void OnEnable()
	{
		FixedLateUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedLateUpdateManager.Unregister(this);
	}

	private void OnDestroy()
	{
		_attachedBody.OnSuspendOWRigidbody -= OnAttachedBodySuspend;
		_attachedBody.OnPreUnsuspendOWRigidbody -= OnAttachedBodyPreUnsuspend;
		_attachedBody.OnWarpOWRigidbody -= OnWarpOWRigidbody;
	}

	public bool GetApplyForces()
	{
		return _applyForces;
	}

	public void SetApplyForces(bool applyForces)
	{
		_applyForces = applyForces;
	}

	public bool GetApplyFluids()
	{
		return _applyFluids;
	}

	public void SetApplyFluids(bool applyFluids)
	{
		_applyFluids = applyFluids;
	}

	public void SkipNextFrame()
	{
		_skipFrame = true;
	}

	public void ManagedFixedLateUpdate()
	{
		if (!_skipFrame)
		{
			if (_forceDetector != null && _applyForces)
			{
				_attachedBody.AddAcceleration(_forceDetector.GetForceAcceleration());
			}
			if (_fluidDetector != null && _applyFluids)
			{
				_attachedBody.AddAcceleration(_fluidDetector.GetLinearFluidAcceleration());
				if (!_isPlayer)
				{
					_attachedBody.AddAngularAcceleration(_fluidDetector.GetAngularFluidAcceleration());
				}
			}
		}
		if (_forceDetector != null)
		{
			_forceDetector.MakeDirty();
		}
		if (_fluidDetector != null)
		{
			_fluidDetector.MakeDirty();
		}
		_skipFrame = false;
	}

	private void OnAttachedBodySuspend(OWRigidbody body)
	{
		base.enabled = false;
	}

	private void OnAttachedBodyPreUnsuspend(OWRigidbody body)
	{
		base.enabled = true;
		if (_forceDetector != null)
		{
			_forceDetector.MakeDirty();
		}
		if (_fluidDetector != null)
		{
			_fluidDetector.MakeDirty();
		}
	}

	private void OnWarpOWRigidbody(OWRigidbody body)
	{
		_skipFrame = true;
	}
}
