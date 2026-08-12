using UnityEngine;

public class DreamRaftProjection : DreamObjectProjection
{
	[SerializeField]
	private SphereBounds _visibilityBounds = new SphereBounds(Vector3.zero, 2f);

	private OWRigidbody _body;

	private bool _waitingToSuspend;

	private float _suspendTimer;

	public SphereBounds CalcWorldVisibilityBounds()
	{
		return new SphereBounds(base.transform.TransformPoint(_visibilityBounds.center), _visibilityBounds.radius);
	}

	protected override void Awake()
	{
		base.Awake();
		_body = this.GetAttachedOWRigidbody();
		for (int i = 0; i < _candles.Length; i++)
		{
			_candles[i].OnLitStateChanged += new OWEvent.OWCallback(OnCandleLitStateChanged);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _candles.Length; i++)
		{
			_candles[i].OnLitStateChanged -= new OWEvent.OWCallback(OnCandleLitStateChanged);
		}
	}

	public override void PulseOnAndOff()
	{
		Debug.LogError("Raft projections should not be asked to pulse");
	}

	protected override void UpdateVisibility(bool immediate = false)
	{
		base.UpdateVisibility(immediate);
		if (_visible)
		{
			_body.Unsuspend(restoreCachedVelocity: false);
			_waitingToSuspend = false;
		}
		else if (immediate)
		{
			_body.Suspend();
			_waitingToSuspend = false;
		}
		else
		{
			_waitingToSuspend = true;
			_suspendTimer = 0.5f;
			base.enabled = true;
		}
	}

	protected override void Update()
	{
		if (!_waitingToSuspend)
		{
			base.enabled = false;
			return;
		}
		if (_suspendTimer > 0f)
		{
			_suspendTimer -= Time.deltaTime;
			return;
		}
		_body.Suspend();
		_waitingToSuspend = false;
		base.enabled = false;
	}

	private void OnCandleLitStateChanged()
	{
		if (!_visible)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < _candles.Length; i++)
		{
			if (_candles[i].IsLit())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SetVisible(visible: false);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_visibilityBounds.center, _visibilityBounds.radius);
		}
	}
}
