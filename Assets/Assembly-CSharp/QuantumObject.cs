using UnityEngine;

public abstract class QuantumObject : VisibilityObject
{
	public delegate void PreCollapseEvent(QuantumObject quantumObject);

	public delegate void PostCollapseEvent(QuantumObject quantumObject, bool collapsed);

	[SerializeField]
	protected float _maxSnapshotLockRange = 100f;

	[SerializeField]
	protected bool _collapseOnStart = true;

	[SerializeField]
	protected bool _debug;

	[SerializeField]
	protected bool _ignoreRetryQueue;

	private bool _hasEverBeenEnabled;

	private bool _visibleInProbeSnapshot;

	private bool _subscribedToRemoveSnapshotEvent;

	private bool _isPlayerStandingOnObject;

	private float _lastCollapseTime = -1f;

	private bool _wasLocked;

	private bool _isQuantum = true;

	public event PreCollapseEvent OnPreCollapse;

	public event PostCollapseEvent OnPostCollapse;

	protected virtual void Start()
	{
		if (_sector == null)
		{
			CheckEnabled();
		}
		if (_collapseOnStart)
		{
			Collapse(skipInstantVisibilityCheck: true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
		GlobalMessenger.RemoveListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
	}

	protected override void CheckEnabled()
	{
		bool flag = base.enabled;
		base.CheckEnabled();
		if (base.enabled && (!flag || !_hasEverBeenEnabled))
		{
			_hasEverBeenEnabled = true;
			GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", OnProbeSnapshot);
			GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);
		}
		else if (!base.enabled && flag)
		{
			GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", OnProbeSnapshot);
			GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);
		}
	}

	public virtual bool IsPlayerEntangled()
	{
		return _isPlayerStandingOnObject;
	}

	public void SetPlayerStandingOnObject(bool isPlayerStandingOnObject)
	{
		_isPlayerStandingOnObject = isPlayerStandingOnObject;
	}

	public bool AttemptCollapse()
	{
		if (!IsLocked())
		{
			return Collapse();
		}
		return false;
	}

	public bool ForceCollapse()
	{
		if (IsLockedByProbeSnapshot())
		{
			return false;
		}
		_lastCollapseTime = 0f;
		return Collapse(skipInstantVisibilityCheck: true);
	}

	public bool IsLocked()
	{
		if (_isQuantum && !IsLockedByProbeSnapshot() && !IsLockedByActiveCamera())
		{
			return IsLockedByPlayerContact();
		}
		return true;
	}

	public bool IsQuantum()
	{
		return _isQuantum;
	}

	public void SetIsQuantum(bool isQuantum)
	{
		_isQuantum = isQuantum;
	}

	protected bool IsLockedByProbeSnapshot()
	{
		if (_visibleInProbeSnapshot && Locator.GetActiveCamera().CompareTag("MainCamera"))
		{
			return Locator.GetToolModeSwapper().GetToolMode() == ToolMode.Probe;
		}
		return false;
	}

	protected bool IsLockedByActiveCamera()
	{
		if (IsVisible())
		{
			return IsIlluminated();
		}
		return false;
	}

	protected bool IsLockedByPlayerContact()
	{
		if (IsPlayerEntangled())
		{
			if (!IsIlluminated())
			{
				return Locator.GetFlashlight().IsFlashlightOn();
			}
			return true;
		}
		return false;
	}

	protected bool Collapse(bool skipInstantVisibilityCheck = false)
	{
		if (Time.time < _lastCollapseTime + 0.5f || !base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (this.OnPreCollapse != null)
		{
			this.OnPreCollapse(this);
		}
		bool flag = false;
		if (IsPlayerEntangled())
		{
			OWRigidbody component = Locator.GetPlayerTransform().GetComponent<OWRigidbody>();
			RelativeLocationData location = new RelativeLocationData(Locator.GetPlayerTransform().GetComponent<OWRigidbody>(), base.transform);
			flag = ChangeQuantumState(skipInstantVisibilityCheck);
			if (flag)
			{
				component.MoveToRelativeLocation(location, base.transform);
			}
		}
		else
		{
			flag = ChangeQuantumState(skipInstantVisibilityCheck);
		}
		if (this.OnPostCollapse != null)
		{
			this.OnPostCollapse(this, flag);
		}
		if (flag)
		{
			_lastCollapseTime = Time.time;
		}
		return flag;
	}

	protected abstract bool ChangeQuantumState(bool skipInstantVisibilityCheck);

	public bool Retry()
	{
		return Collapse();
	}

	protected override void Update()
	{
		base.Update();
		bool flag = IsLocked();
		if (_wasLocked && !flag && !Collapse() && !_ignoreRetryQueue)
		{
			ShapeManager.AddToRetryQueue(this);
		}
		_wasLocked = flag;
	}

	private void OnProbeSnapshot(ProbeCamera probeCamera)
	{
		float num = Vector3.Distance(base.transform.position, probeCamera.transform.position);
		_visibleInProbeSnapshot = num < _maxSnapshotLockRange && IsIlluminated() && !probeCamera.HasInterference() && CheckVisibilityFromProbe(probeCamera.GetOWCamera());
		if (_visibleInProbeSnapshot && !_subscribedToRemoveSnapshotEvent)
		{
			GlobalMessenger.AddListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
			_subscribedToRemoveSnapshotEvent = true;
		}
		else if (!_visibleInProbeSnapshot && _subscribedToRemoveSnapshotEvent)
		{
			GlobalMessenger.RemoveListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
			_subscribedToRemoveSnapshotEvent = false;
		}
	}

	private void OnProbeSnapshotRemoved()
	{
		_visibleInProbeSnapshot = false;
		_subscribedToRemoveSnapshotEvent = false;
		GlobalMessenger.RemoveListener("Probe Snapshot Removed", OnProbeSnapshotRemoved);
	}

	private void OnSwitchActiveCamera(OWCamera activeCamera)
	{
		if (!IsLockedByProbeSnapshot() && !IsLockedByPlayerContact())
		{
			Collapse(skipInstantVisibilityCheck: true);
		}
	}
}
