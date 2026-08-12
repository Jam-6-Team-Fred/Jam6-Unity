using UnityEngine;

public class AlignmentSunStation : MonoBehaviour
{
	private float _twist;

	private float _savedTwistingSpeed;

	private Transform _transform;

	private Transform _sunTransform;

	private void Start()
	{
		_transform = base.transform;
		_sunTransform = Locator.GetSunTransform();
	}

	private void FixedUpdate()
	{
		if (_savedTwistingSpeed == 0f)
		{
			_savedTwistingSpeed = GetComponent<OWRigidbody>().GetAngularVelocity().z;
		}
		_twist += _savedTwistingSpeed;
		Quaternion quaternion = Quaternion.AngleAxis(_twist, Vector3.down);
		Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.down, _sunTransform.position - _transform.position);
		GetComponent<OWRigidbody>().AddRotation(_transform.rotation * Quaternion.Inverse(quaternion2 * quaternion * Quaternion.identity));
	}
}
