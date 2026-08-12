using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class AngularAccelerator : MonoBehaviour
{
	[SerializeField]
	private Vector3 _rotationAxis = Vector3.up;

	[SerializeField]
	private float _angularAcceleration = 0.001f;

	private OWRigidbody _owRigidbody;

	private void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void FixedUpdate()
	{
		_owRigidbody.AddAngularAcceleration(_rotationAxis.normalized * _angularAcceleration);
	}
}
