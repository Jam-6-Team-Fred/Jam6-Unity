using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class JellyfishController : SectoredMonoBehaviour
{
	[SerializeField]
	private float _upperLimit = 400f;

	[SerializeField]
	private float _lowerLimit = 200f;

	[SerializeField]
	private float _upwardsAcceleration = 10f;

	[SerializeField]
	private float _downwardsAcceleration = 20f;

	[SerializeField]
	private FluidVolume _attractiveFluidVolume;

	private OWRigidbody _planetBody;

	private OWRigidbody _jellyfishBody;

	private bool _isRising = true;

	public bool IsRising()
	{
		return _isRising;
	}

	protected override void Awake()
	{
		base.Awake();
		_jellyfishBody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void Start()
	{
		_planetBody = _jellyfishBody.GetOrigParentBody();
		AlignWithTargetBody component = GetComponent<AlignWithTargetBody>();
		if (component != null)
		{
			component.SetTargetBody(_planetBody);
		}
		float num = Random.Range(_lowerLimit, _upperLimit);
		Vector3 vector = _jellyfishBody.GetPosition() - _planetBody.GetPosition();
		base.transform.position = _planetBody.GetPosition() + vector.normalized * num;
		_isRising = Random.value > 0.5f;
		_attractiveFluidVolume.SetVolumeActivation(!_isRising);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		if (!base.gameObject.activeSelf && _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			base.gameObject.SetActive(value: true);
			_jellyfishBody.Unsuspend();
		}
		else if (base.gameObject.activeSelf && !_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship))
		{
			_jellyfishBody.Suspend();
			base.gameObject.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		float sqrMagnitude = (_jellyfishBody.GetPosition() - _planetBody.GetPosition()).sqrMagnitude;
		if (_isRising)
		{
			_jellyfishBody.AddAcceleration(base.transform.up * _upwardsAcceleration);
			if (sqrMagnitude > _upperLimit * _upperLimit)
			{
				_isRising = false;
				_attractiveFluidVolume.SetVolumeActivation(active: true);
			}
		}
		else
		{
			_jellyfishBody.AddAcceleration(-base.transform.up * _downwardsAcceleration);
			if (sqrMagnitude < _lowerLimit * _lowerLimit)
			{
				_isRising = true;
				_attractiveFluidVolume.SetVolumeActivation(active: false);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		// CHANGED
		if (OWGizmos.IsDirectlySelected(base.gameObject) && base.transform.parent != null)
		{
			OWRigidbody oWRigidbody = ((_planetBody != null) ? _planetBody : base.transform.parent.GetComponentInParent<OWRigidbody>());
			if (oWRigidbody != null)
			{
				Vector3 position = oWRigidbody.transform.position;
				Vector3 vector = Vector3.Normalize(base.transform.position - position);
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(position + vector * _lowerLimit, position + vector * _upperLimit);
				Gizmos.DrawSphere(position + vector * _lowerLimit, 10f);
				Gizmos.DrawSphere(position + vector * _upperLimit, 10f);
			}
		}
	}
}
