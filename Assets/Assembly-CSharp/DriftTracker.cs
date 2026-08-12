using UnityEngine;

public class DriftTracker : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _body1;

	[SerializeField]
	private OWRigidbody _body2;

	[SerializeField]
	private bool _printSpeed;

	[SerializeField]
	private bool _printDistance;

	[SerializeField]
	private bool _printMassData;

	[SerializeField]
	private bool _printInertiaTensor;

	private void Awake()
	{
		if (_printMassData)
		{
			MonoBehaviour.print("AWAKE Center of Mass 2: " + _body2.GetCenterOfMass());
		}
	}

	private void FixedUpdate()
	{
		if (_printDistance)
		{
			float magnitude = (_body1.GetWorldCenterOfMass() - _body2.GetWorldCenterOfMass()).magnitude;
			MonoBehaviour.print("dist: " + magnitude);
		}
		if (_printSpeed)
		{
			float magnitude2 = _body1.GetRelativeVelocity(_body2).magnitude;
			MonoBehaviour.print("Speed: " + magnitude2);
		}
		if (_printMassData)
		{
			MonoBehaviour.print("Center of Mass 2: " + _body2.GetCenterOfMass());
		}
		if (_printInertiaTensor)
		{
			MonoBehaviour.print("Inertia Tensor 2: " + _body2.GetRigidbody().inertiaTensor);
		}
	}
}
