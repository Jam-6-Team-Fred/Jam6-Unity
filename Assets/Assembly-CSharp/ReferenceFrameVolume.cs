using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ReferenceFrameVolume : MonoBehaviour
{
	[SerializeField]
	protected ReferenceFrame _referenceFrame;

	[Space(10f)]
	[SerializeField]
	protected float _minColliderRadius = 5f;

	[SerializeField]
	protected float _maxColliderRadius = 10f;

	[Space(10f)]
	[SerializeField]
	protected bool _isPrimaryVolume = true;

	[SerializeField]
	protected bool _isCloseRangeVolume = true;

	protected Transform _transform;

	protected OWRigidbody _attachedOWRigidbody;

	protected SphereCollider _sphereCollider;

	public float MinColliderRadius
	{
		get
		{
			return _minColliderRadius;
		}
		set
		{
			_minColliderRadius = value;
		}
	}

	public float MaxColliderRadius
	{
		get
		{
			return _maxColliderRadius;
		}
		set
		{
			_maxColliderRadius = value;
		}
	}

	private void OnValidate()
	{
		int num = (_isCloseRangeVolume ? LayerMask.NameToLayer("CloseRangeRFVolume") : LayerMask.NameToLayer("ReferenceFrameVolume"));
		if (base.gameObject.layer != num)
		{
			base.gameObject.layer = num;
		}
		Collider component = GetComponent<Collider>();
		if (!component.isTrigger)
		{
			component.isTrigger = true;
		}
	}

	protected virtual void Awake()
	{
		_transform = base.transform;
		_sphereCollider = GetComponent<SphereCollider>();
		if (!_sphereCollider.isTrigger)
		{
			Debug.LogError("ReferenceFrameVolumes must be trigger volumes!", base.gameObject);
			Debug.Break();
		}
		_attachedOWRigidbody = base.gameObject.GetAttachedOWRigidbody();
		if (!OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.closeRangeRFMask) && !OWLayerMask.IsLayerInMask(base.gameObject.layer, OWLayerMask.longRangeRFMask))
		{
			Debug.LogError("ReferenceFrameVolume is not on the right layer!", base.gameObject);
			Debug.Break();
		}
		if (_isPrimaryVolume)
		{
			_attachedOWRigidbody.SetAttachedReferenceFrameVolume(this);
		}
		_referenceFrame.Init(_attachedOWRigidbody, _transform.position);
	}

	private void OnEnable()
	{
		FixedUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedUpdateManager.Unregister(this);
	}

	public bool IsPrimaryVolume()
	{
		return _isPrimaryVolume;
	}

	public ReferenceFrame GetReferenceFrame()
	{
		return _referenceFrame;
	}

	public void ManagedFixedUpdate(Vector3 activeCameraPos)
	{
		float sqrMagnitude = (_transform.position - activeCameraPos).sqrMagnitude;
		float num = ((sqrMagnitude < _minColliderRadius * _minColliderRadius * 4f) ? _minColliderRadius : ((!(sqrMagnitude > _maxColliderRadius * _maxColliderRadius * 4f)) ? Mathf.Lerp(_minColliderRadius, _maxColliderRadius, Mathf.InverseLerp(_minColliderRadius * 2f, _maxColliderRadius * 2f, Mathf.Sqrt(sqrMagnitude))) : _maxColliderRadius));
		if (!OWMath.ApproxEquals(_sphereCollider.radius, num))
		{
			_sphereCollider.radius = num;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _referenceFrame.GetMinSuitTargetDistance());
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _referenceFrame.GetMaxTargetDistance());
			Gizmos.color = new Color(0.25f, 0.25f, 1f, 1f);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _referenceFrame.GetAutopilotArrivalDistance());
			Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _referenceFrame.GetAutoAlignmentDistance());
			Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
			Gizmos.DrawWireSphere(base.transform.position, _referenceFrame.GetMinMatchAngularVelocityDistance());
			Gizmos.DrawWireSphere(base.transform.position, _referenceFrame.GetMaxMatchAngularVelocityDistance());
			Gizmos.color = new Color(1f, 1f, 1f, 1f);
			OWGizmos.DrawBillboardedWireCircle(base.transform.position, _referenceFrame.GetBracketsRadius());
			SphereCollider component = GetComponent<SphereCollider>();
			float value = Vector3.Distance(Camera.current.transform.position, base.transform.position);
			float radius = Mathf.Lerp(_minColliderRadius, _maxColliderRadius, Mathf.InverseLerp(_minColliderRadius * 2f, _maxColliderRadius * 2f, value));
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.25f);
			Gizmos.DrawWireSphere(component.center, _minColliderRadius);
			Gizmos.DrawWireSphere(component.center, _maxColliderRadius);
			Gizmos.color = new Color(0.5f, 1f, 0.5f, 1f);
			Gizmos.DrawWireSphere(component.center, radius);
		}
	}
}
