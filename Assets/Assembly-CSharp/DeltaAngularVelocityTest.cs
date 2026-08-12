using UnityEngine;

public class DeltaAngularVelocityTest : MonoBehaviour
{
	private Quaternion _lastRotation;

	private void LateUpdate()
	{
		MonoBehaviour.print((base.transform.rotation * Quaternion.Inverse(_lastRotation)).eulerAngles.y);
		_lastRotation = base.transform.rotation;
	}
}
