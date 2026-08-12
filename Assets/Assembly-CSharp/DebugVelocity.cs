using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class DebugVelocity : MonoBehaviour
{
	private const float VELOCITY_SCALE_FACTOR = 1f;

	private const float ANGULAR_VELOCITY_SCALE_FACTOR = 500f;

	[SerializeField]
	private bool _printVelocity;

	[SerializeField]
	private bool _drawVelocity;

	[SerializeField]
	private bool _printAngularVelocity;

	[SerializeField]
	private bool _drawAngularVelocity;

	[SerializeField]
	private bool _printAcceleration;

	private OWRigidbody _owRigidbody;

	private void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void FixedUpdate()
	{
		Vector3 worldCenterOfMass = _owRigidbody.GetWorldCenterOfMass();
		if (_drawVelocity)
		{
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + _owRigidbody.GetVelocity() * 1f, Color.red);
		}
		if (_drawAngularVelocity)
		{
			Debug.DrawLine(worldCenterOfMass, worldCenterOfMass + _owRigidbody.GetAngularVelocity() * 500f, Color.green);
		}
		if (_printVelocity)
		{
			MonoBehaviour.print(_owRigidbody.GetVelocity().magnitude);
		}
		if (_printAngularVelocity)
		{
			MonoBehaviour.print(_owRigidbody.GetAngularVelocity().magnitude);
		}
		if (_printAcceleration)
		{
			MonoBehaviour.print(_owRigidbody.GetAcceleration().magnitude);
		}
	}
}
