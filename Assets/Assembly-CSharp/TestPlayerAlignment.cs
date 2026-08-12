using UnityEngine;

public class TestPlayerAlignment : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _targetBody;

	private void FixedUpdate()
	{
		base.transform.rotation = Quaternion.AngleAxis(0.1f, base.transform.forward) * base.transform.rotation;
		Vector3 fromDirection = -base.transform.up;
		Vector3 toDirection = _targetBody.transform.position - base.transform.position;
		Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
		base.transform.rotation = quaternion * base.transform.rotation;
	}
}
