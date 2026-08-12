using UnityEngine;

public class ShipDetachableModule : ShipModule
{
	public delegate void DetachEvent(ShipDetachableModule shipDetachableModule);

	[SerializeField]
	private float _mass = 0.1f;

	[SerializeField]
	private Vector3 _localCenterOfMass = Vector3.zero;

	[SerializeField]
	private Vector3 _detectorCenter = Vector3.zero;

	[SerializeField]
	private float _detectorRadius = 2f;

	[SerializeField]
	private GameObject _impactAudioPrefab;

	[SerializeField]
	private ShipLODTrigger _moduleLODTrigger;

	private OWRigidbody _attachedBody;

	private OWRigidbody _detachedBody;

	private ProxyShadowCaster[] _proxyShadowCasters;

	private IShipGroup[] _shipLODGroups;

	public OWRigidbody detachedBody => _detachedBody;

	public bool isDetached => _detachedBody != null;

	public event DetachEvent OnModuleDetach;

	protected override void Awake()
	{
		base.Awake();
		_attachedBody = this.GetAttachedOWRigidbody();
		_detachedBody = null;
		_proxyShadowCasters = GetComponentsInChildren<ProxyShadowCaster>();
		_shipLODGroups = GetComponentsInChildren<IShipGroup>();
	}

	public override void ApplyImpact(ImpactData impact)
	{
		if (isDetached)
		{
			return;
		}
		base.ApplyImpact(impact);
		for (int i = 0; i < _hulls.Length; i++)
		{
			if (_hulls[i].integrity <= 0f)
			{
				Detach();
				break;
			}
		}
	}

	public OWRigidbody Detach()
	{
		if (isDetached)
		{
			return null;
		}
		for (int i = 0; i < _proxyShadowCasters.Length; i++)
		{
			_proxyShadowCasters[i].enabled = false;
		}
		base.gameObject.name += "_Body";
		base.transform.parent = null;
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		OWRigidbody oWRigidbody = base.gameObject.AddComponent<OWRigidbody>();
		base.gameObject.AddComponent<ImpactSensor>();
		oWRigidbody.SetMass(_mass);
		_attachedBody.SetMass(_attachedBody.GetMass() - _mass);
		oWRigidbody.SetCenterOfMass(_localCenterOfMass);
		Vector3 pointVelocity = _attachedBody.GetPointVelocity(oWRigidbody.GetWorldCenterOfMass());
		oWRigidbody.SetVelocity(pointVelocity);
		oWRigidbody.SetAngularVelocity(_attachedBody.GetAngularVelocity());
		rigidbody.angularDrag = 0.1f;
		GameObject obj = new GameObject("DetectorVolume");
		obj.layer = LayerMask.NameToLayer("AdvancedDetector");
		obj.transform.SetParent(oWRigidbody.transform, worldPositionStays: false);
		obj.transform.position = oWRigidbody.GetWorldCenterOfMass();
		SphereCollider sphereCollider = obj.AddComponent<SphereCollider>();
		sphereCollider.center = _detectorCenter;
		sphereCollider.radius = _detectorRadius;
		SphereShape sphereShape = obj.AddComponent<SphereShape>();
		sphereShape.SetCollisionMode(Shape.CollisionMode.Detector);
		sphereShape.center = _detectorCenter;
		sphereShape.radius = _detectorRadius;
		obj.AddComponent<SectorDetector>().SetOccupantType(DynamicOccupant.Ship);
		obj.AddComponent<DynamicForceDetector>();
		DynamicFluidDetector dynamicFluidDetector = obj.AddComponent<DynamicFluidDetector>();
		dynamicFluidDetector.GetBuoyancyData().density = 1.1f;
		dynamicFluidDetector.GetBuoyancyData().boundingRadius = _detectorRadius;
		obj.AddComponent<HazardDetector>();
		Object.Instantiate(_impactAudioPrefab, oWRigidbody.transform).transform.position = oWRigidbody.GetWorldCenterOfMass();
		for (int j = 0; j < _shipLODGroups.Length; j++)
		{
			_shipLODGroups[j].SetLODTrigger(_moduleLODTrigger);
		}
		_detachedBody = oWRigidbody;
		if (this.OnModuleDetach != null)
		{
			this.OnModuleDetach(this);
		}
		return oWRigidbody;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(_localCenterOfMass, 0.25f);
		}
	}
}
