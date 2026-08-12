using UnityEngine;

public class FicticiousForceDetector : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _nonInertialFrame;

	[SerializeField]
	private Vector3 _angularFrameVelocity = Vector3.up;

	private bool _go;

	private OWRigidbody _attachedBody;

	private void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			_go = !_go;
			Vector3 angularFrameVelocity = _angularFrameVelocity;
			Vector3 rhs = _attachedBody.GetWorldCenterOfMass() - _nonInertialFrame.GetWorldCenterOfMass();
			_attachedBody.AddVelocityChange(-Vector3.Cross(angularFrameVelocity, rhs));
			MonoBehaviour.print("GOOOO");
		}
	}

	private void FixedUpdate()
	{
		if (_go)
		{
			Vector3 angularFrameVelocity = _angularFrameVelocity;
			Vector3 rhs = _attachedBody.GetWorldCenterOfMass() - _nonInertialFrame.GetWorldCenterOfMass();
			Vector3 vector = _attachedBody.GetVelocity() - _nonInertialFrame.GetVelocity();
			Vector3 vector2 = -2f * Vector3.Cross(angularFrameVelocity, vector);
			Vector3 vector3 = -Vector3.Cross(angularFrameVelocity, Vector3.Cross(angularFrameVelocity, rhs));
			_attachedBody.AddAcceleration(vector2 + vector3);
			MonoBehaviour.print((vector + Vector3.Cross(angularFrameVelocity, rhs)).magnitude);
		}
	}
}
