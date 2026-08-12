using UnityEngine;

public class NomaiCairnRock : MonoBehaviour
{
	private NomaiWallText _childText;

	private Vector3 _origPos;

	private Quaternion _origRot;

	private Transform _origParent;

	private OWRigidbody _owRigidbody;

	private ForceDetector _forceDetector;

	private FluidDetector _fluidDetector;

	private OWCollider _owCollider;

	private float _returnDuration;

	private float _returnStartTime;

	private Vector3 _returnStartPos;

	private Quaternion _returnStartRot;

	private bool _returning;

	private void Awake()
	{
		_childText = GetComponentInChildren<NomaiWallText>();
		_origPos = base.transform.localPosition;
		_origRot = base.transform.localRotation;
		_origParent = base.transform.parent;
		_owCollider = base.gameObject.GetAddComponent<OWCollider>();
		base.enabled = false;
	}

	public void MakeRigidbody(OWRigidbody parentBody)
	{
		if (_childText != null)
		{
			_childText.GetComponent<OWCollider>().SetActivation(active: false);
		}
		base.transform.parent = null;
		base.gameObject.layer = LayerMask.NameToLayer("PhysicalDetector");
		_owRigidbody = base.gameObject.AddComponent<OWRigidbody>();
		_forceDetector = base.gameObject.AddComponent<DynamicForceDetector>();
		_fluidDetector = base.gameObject.AddComponent<DynamicFluidDetector>();
		_owRigidbody.SetVelocity(parentBody.GetPointVelocity(base.transform.position));
		_owRigidbody.SetMass(0.001f);
	}

	public bool HasReturnedToCairn()
	{
		if (!base.enabled)
		{
			return _owRigidbody == null;
		}
		return false;
	}

	public void ReturnAfterSeconds(float delay, float returnDuration)
	{
		if (_owRigidbody != null)
		{
			base.enabled = true;
			_returnStartTime = Time.time + delay;
			_returnDuration = returnDuration;
		}
	}

	private void FixedUpdate()
	{
		if (!_returning && Time.time > _returnStartTime)
		{
			_returning = true;
			_returnStartTime = Time.time;
			_owCollider.SetActivation(active: false);
			base.gameObject.layer = LayerMask.NameToLayer("Default");
			Object.Destroy(_owRigidbody);
			Object.Destroy(_forceDetector);
			Object.Destroy(_fluidDetector);
			Object.Destroy(GetComponent<ForceApplier>());
			base.transform.parent = _origParent;
			_returnStartPos = base.transform.localPosition;
			_returnStartRot = base.transform.localRotation;
		}
		else
		{
			if (!_returning)
			{
				return;
			}
			float t = Mathf.Clamp01((Time.time - _returnStartTime) / _returnDuration);
			t = Mathf.SmoothStep(0f, 1f, t);
			base.transform.localPosition = Vector3.Lerp(_returnStartPos, _origPos, t);
			base.transform.localRotation = Quaternion.Slerp(_returnStartRot, _origRot, t);
			if (t >= 1f)
			{
				if (_childText != null)
				{
					_childText.GetComponent<OWCollider>().SetActivation(active: true);
				}
				_owCollider.SetActivation(active: true);
				base.enabled = false;
				_returning = false;
			}
		}
	}
}
