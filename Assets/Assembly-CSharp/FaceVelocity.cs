using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class FaceVelocity : MonoBehaviour
{
	private OWRigidbody _owRigidbody;

	private void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void FixedUpdate()
	{
		_owRigidbody.AddRotation(Quaternion.FromToRotation(base.transform.forward, _owRigidbody.GetVelocity()));
	}
}
