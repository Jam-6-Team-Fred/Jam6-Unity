using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class RelativeAnchor : MonoBehaviour
{
	[SerializeField]
	private float _anchorRange = 1000f;

	private OWRigidbody _relativeBody;

	private OWRigidbody _owRigidbody;

	private Vector3 _targetRelativeVelocity = Vector3.zero;

	private void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
		base.enabled = false;
	}

	public void SetRelativeBody(OWRigidbody relativeBody)
	{
		_relativeBody = relativeBody;
		base.enabled = true;
	}

	public void SetRelativeVelocity(Vector3 relativeVelocity)
	{
		_targetRelativeVelocity = relativeVelocity;
	}

	private void FixedUpdate()
	{
		if (_relativeBody == null)
		{
			base.enabled = false;
		}
		if (Vector3.Distance(_relativeBody.GetPosition(), _owRigidbody.GetPosition()) < _anchorRange)
		{
			Vector3 relativeVelocity = _relativeBody.GetRelativeVelocity(_owRigidbody);
			Vector3 velocityChange = _targetRelativeVelocity - relativeVelocity;
			_owRigidbody.AddVelocityChange(velocityChange);
		}
	}
}
