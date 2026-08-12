using UnityEngine;

public class TestCameraEasing : MonoBehaviour
{
	private Quaternion _lastRotation;

	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.Slerp(_lastRotation, base.transform.parent.rotation, 0.5f);
		_lastRotation = base.transform.parent.rotation;
	}
}
