using UnityEngine;

public class EmergencyHatch : MonoBehaviour
{
	[SerializeField]
	private NomaiInterfaceSlot _openHatchSlot;

	[SerializeField]
	private NomaiInterfaceOrb _orb;

	[SerializeField]
	private float _ejectSpeed = 20f;

	[SerializeField]
	private Vector3 _localAngularVelocity = new Vector3(0f, 0f, 3f);

	[SerializeField]
	private Vector3 _localVelocityOffset;

	private DetachableFragment _fragment;

	private bool _detached;

	private void Awake()
	{
		_fragment = GetComponent<DetachableFragment>();
		_openHatchSlot.OnSlotActivated += OnOpenHatch;
	}

	private void OnDestroy()
	{
		_openHatchSlot.OnSlotActivated -= OnOpenHatch;
	}

	private void OnOpenHatch(NomaiInterfaceSlot slot)
	{
		if (!_detached)
		{
			_detached = true;
			OWRigidbody oWRigidbody = _fragment.Detach();
			oWRigidbody.AddVelocityChange(oWRigidbody.transform.forward * _ejectSpeed);
			oWRigidbody.AddVelocityChange(oWRigidbody.transform.TransformDirection(_localVelocityOffset));
			oWRigidbody.AddAngularVelocityChange(oWRigidbody.transform.TransformDirection(_localAngularVelocity));
			GameObject obj = oWRigidbody.GetComponentInChildren<ForceDetector>().gameObject;
			obj.layer = LayerMask.NameToLayer("AdvancedDetector");
			obj.AddComponent<SectorDetector>().SetOccupantType(DynamicOccupant.Probe);
			if (_orb != null)
			{
				_orb.AddLock();
			}
		}
	}
}
