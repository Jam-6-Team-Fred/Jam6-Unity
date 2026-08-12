using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InertiaTensorCalibrator : MonoBehaviour
{
	private float _lastRecenterTime;

	private float _interval = 5f;

	private Quaternion _initInertiaTensorRotation;

	private Rigidbody _rigidbody;

	private void Awake()
	{
		_rigidbody = this.GetRequiredComponent<Rigidbody>();
	}

	private void Start()
	{
		_interval = Random.Range(5f, 10f);
		_initInertiaTensorRotation = _rigidbody.inertiaTensorRotation;
	}

	private void FixedUpdate()
	{
		if (_rigidbody.inertiaTensorRotation != _initInertiaTensorRotation)
		{
			_initInertiaTensorRotation = _rigidbody.inertiaTensorRotation;
		}
		else if (Time.time > _lastRecenterTime + _interval)
		{
			_lastRecenterTime = Time.time;
			_interval = Random.Range(5f, 10f);
			_rigidbody.inertiaTensorRotation = _initInertiaTensorRotation;
		}
	}
}
