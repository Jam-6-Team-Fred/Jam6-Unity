using UnityEngine;

public class ShipDetachableLeg : MonoBehaviour
{
	public delegate void DetachEvent(ShipDetachableLeg shipDetachableLeg);

	[SerializeField]
	private float _legMass = 0.05f;

	[SerializeField]
	private GameObject _impactAudioPrefab;

	private OWRigidbody _attachedBody;

	private OWRigidbody _detachedBody;

	private Collider[] _colliders;

	public OWRigidbody detachedBody => _detachedBody;

	public bool isDetached => _detachedBody != null;

	public event DetachEvent OnLegDetach;

	private void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
		_detachedBody = null;
		_colliders = GetComponentsInChildren<Collider>();
	}

	public OWRigidbody Detach()
	{
		if (isDetached)
		{
			return null;
		}
		base.gameObject.name += "_Body";
		base.gameObject.transform.parent = null;
		base.gameObject.AddComponent<Rigidbody>();
		OWRigidbody oWRigidbody = base.gameObject.AddComponent<OWRigidbody>();
		base.gameObject.AddComponent<ImpactSensor>();
		oWRigidbody.SetMass(_legMass);
		_attachedBody.SetMass(_attachedBody.GetMass() - _legMass);
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(oWRigidbody.GetWorldCenterOfMass());
		oWRigidbody.SetVelocity(pointVelocity);
		oWRigidbody.SetAngularVelocity(_attachedBody.GetAngularVelocity());
		GameObject obj = new GameObject("DetectorVolume");
		obj.layer = LayerMask.NameToLayer("AdvancedDetector");
		obj.transform.SetParent(oWRigidbody.transform, worldPositionStays: false);
		obj.transform.position = oWRigidbody.GetWorldCenterOfMass();
		obj.AddComponent<SphereCollider>();
		SphereShape sphereShape = obj.AddComponent<SphereShape>();
		sphereShape.SetCollisionMode(Shape.CollisionMode.Detector);
		obj.AddComponent<SectorDetector>().SetOccupantType(DynamicOccupant.Ship);
		obj.AddComponent<DynamicForceDetector>();
		DynamicFluidDetector dynamicFluidDetector = obj.AddComponent<DynamicFluidDetector>();
		dynamicFluidDetector.GetBuoyancyData().density = 5f;
		dynamicFluidDetector.GetBuoyancyData().boundingRadius = sphereShape.radius;
		obj.AddComponent<HazardDetector>();
		Object.Instantiate(_impactAudioPrefab, oWRigidbody.transform).transform.position = oWRigidbody.GetWorldCenterOfMass();
		_detachedBody = oWRigidbody;
		if (this.OnLegDetach != null)
		{
			this.OnLegDetach(this);
		}
		return oWRigidbody;
	}

	public bool ContainsCollider(Collider collider)
	{
		for (int i = 0; i < _colliders.Length; i++)
		{
			if (_colliders[i] == collider)
			{
				return true;
			}
		}
		return false;
	}
}
