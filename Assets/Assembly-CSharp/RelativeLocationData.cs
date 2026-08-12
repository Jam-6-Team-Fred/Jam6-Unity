using UnityEngine;

public struct RelativeLocationData
{
	public Vector3 localPosition;

	public Quaternion localRotation;

	public Vector3 localRelativeVelocity;

	public RelativeLocationData(Vector3 localPosition, Quaternion localRotation, Vector3 localRelativeVelocity)
	{
		this.localPosition = localPosition;
		this.localRotation = localRotation;
		this.localRelativeVelocity = localRelativeVelocity;
	}

	public RelativeLocationData(OWRigidbody body, Transform relativeTransform)
		: this(body, relativeTransform.GetComponentInParent<OWRigidbody>(), relativeTransform)
	{
	}

	public RelativeLocationData(OWRigidbody body, OWRigidbody relativeBody, Transform relativeTransform = null)
	{
		if (relativeTransform == null)
		{
			relativeTransform = relativeBody.transform;
		}
		localPosition = relativeTransform.InverseTransformPoint(body.transform.position);
		localRotation = Quaternion.Inverse(relativeTransform.rotation) * body.transform.rotation;
		localRelativeVelocity = relativeTransform.InverseTransformDirection(body.GetVelocity() - relativeBody.GetPointVelocity(body.transform.position));
	}
}
