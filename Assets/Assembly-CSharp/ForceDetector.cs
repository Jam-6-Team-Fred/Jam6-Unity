using UnityEngine;

public abstract class ForceDetector : PriorityDetector
{
	[SerializeField]
	protected float _fieldMultiplier = 1f;

	protected Vector3 _netAcceleration;

	protected OWRigidbody _attachedBody;

	protected ForceDetector _activeInheritedDetector;

	private bool _dirty;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.GetAddComponent<ForceApplier>();
		_attachedBody = base.gameObject.GetAttachedOWRigidbody();
		_attachedBody.RegisterAttachedForceDetector(this);
		_attachedBody.OnSuspendOWRigidbody += OnAttachedBodySuspend;
		_attachedBody.OnPreUnsuspendOWRigidbody += OnAttachedBodyPreUnsuspend;
		_netAcceleration = Vector3.zero;
	}

	private void OnEnable()
	{
		FixedEarlyUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedEarlyUpdateManager.Unregister(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void MakeDirty()
	{
		_dirty = true;
	}

	public Vector3 GetForceAcceleration()
	{
		if (_dirty)
		{
			AccumulateAcceleration();
			_dirty = false;
		}
		return _netAcceleration;
	}

	protected virtual void AccumulateAcceleration()
	{
		_netAcceleration = Vector3.zero;
		for (int num = _activeVolumes.Count - 1; num >= 0; num--)
		{
			if (_activeVolumes[num] == null)
			{
				Debug.LogError("what", base.gameObject);
				Debug.Break();
			}
			_netAcceleration += (_activeVolumes[num] as ForceVolume).CalculateForceAccelerationOnBody(_attachedBody) * _fieldMultiplier;
		}
		if (_activeInheritedDetector != null)
		{
			_netAcceleration += _activeInheritedDetector.GetForceAcceleration();
		}
	}

	public void ManagedFixedUpdate()
	{
		if (_dirty)
		{
			AccumulateAcceleration();
			_dirty = false;
		}
	}

	public override void AddVolume(EffectVolume eVol)
	{
		if (eVol as ForceVolume != null)
		{
			base.AddVolume(eVol);
		}
	}

	public override void RemoveVolume(EffectVolume eVol)
	{
		if (eVol as ForceVolume != null)
		{
			base.RemoveVolume(eVol);
		}
	}

	protected override void OnVolumeActivated(PriorityVolume eVol)
	{
		if (eVol as ForceVolume != null)
		{
			base.OnVolumeActivated(eVol);
		}
	}

	protected override void OnVolumeDeactivated(PriorityVolume eVol)
	{
		if (eVol as ForceVolume != null)
		{
			base.OnVolumeDeactivated(eVol);
		}
	}

	protected void OnAttachedBodySuspend(OWRigidbody body)
	{
		base.enabled = false;
	}

	protected void OnAttachedBodyPreUnsuspend(OWRigidbody body)
	{
		base.enabled = true;
	}
}
