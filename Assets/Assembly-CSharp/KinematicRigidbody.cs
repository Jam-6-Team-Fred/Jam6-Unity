using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KinematicRigidbody : MonoBehaviour
{
	public delegate void KinematicCollisionEvent(KinematicCollision collision);

	private Rigidbody _rigidbody;

	private OWRigidbody _owRigidbody;

	private KinematicCollider[] _kinematicColliders;

	[SerializeField]
	private Shape _inertiaApproximationShape;

	private Vector3 _velocity;

	private Vector3 _acceleration;

	private Vector3 _force;

	private Vector3 _angularVelocity;

	private Vector3 _angularAcceleration;

	private Vector3 _torque;

	private Vector3 _velocityAccumulator;

	private Vector3 _angularVelocityAccumulator;

	private Vector3 _inertiaTensor;

	public OWRigidbody owRigidbody => _owRigidbody;

	public KinematicCollider[] kinematicColliders => _kinematicColliders;

	public Vector3 centerOfMass
	{
		get
		{
			return Vector3.zero;
		}
		set
		{
			Debug.LogWarning("Tried to set the center of mass of a KinematicRigidbody; will always be (0,0,0)", this);
		}
	}

	public Vector3 worldCenterOfMass
	{
		get
		{
			if (!_rigidbody)
			{
				return base.transform.position;
			}
			return _rigidbody.position;
		}
		set
		{
			Debug.LogWarning("Tried to set the center of mass of a KinematicRigidbody; will always be (0,0,0)", this);
		}
	}

	public Vector3 velocity
	{
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			return _angularVelocity;
		}
		set
		{
			_angularVelocity = value;
		}
	}

	public Vector3 inertiaTensor
	{
		get
		{
			return _inertiaTensor;
		}
		set
		{
			_inertiaTensor = value;
		}
	}

	public event KinematicCollisionEvent OnCollision;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		_owRigidbody = GetComponent<OWRigidbody>();
		_kinematicColliders = GetComponentsInChildren<KinematicCollider>(includeInactive: true);
		_velocity = _rigidbody.velocity;
		_angularVelocity = _rigidbody.angularVelocity;
		_acceleration = Vector3.zero;
		_force = Vector3.zero;
		_angularAcceleration = Vector3.zero;
		_torque = Vector3.zero;
		_velocityAccumulator = Vector3.zero;
		_angularVelocityAccumulator = Vector3.zero;
		if (_inertiaApproximationShape != null)
		{
			Vector3 vector = Quaternion.Inverse(_inertiaApproximationShape.transform.rotation) * base.transform.rotation * _inertiaApproximationShape.GetLocalInertiaTensor();
			Vector3 vector2 = base.transform.InverseTransformPoint(_inertiaApproximationShape.GetWorldSpaceCenter()) - centerOfMass;
			Vector3 vector3 = new Vector3(vector2.x * vector2.x, vector2.y * vector2.y, vector2.z * vector2.z);
			_inertiaTensor = (vector + vector3) * _owRigidbody.GetMass();
		}
		else
		{
			_inertiaTensor = Vector3.zero;
		}
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		FixedLateUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedLateUpdateManager.Unregister(this);
	}

	public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (!_owRigidbody.IsSuspended())
		{
			switch (mode)
			{
			case ForceMode.Force:
				_force += force;
				break;
			case ForceMode.Acceleration:
				_acceleration += force;
				break;
			case ForceMode.Impulse:
				_velocityAccumulator += force / _owRigidbody.GetMass();
				break;
			case ForceMode.VelocityChange:
				_velocityAccumulator += force;
				break;
			case (ForceMode)3:
			case (ForceMode)4:
				break;
			}
		}
	}

	public void AddForce(float x, float y, float z, ForceMode mode = ForceMode.Force)
	{
		AddForce(new Vector3(x, y, z), mode);
	}

	public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
	{
	}

	public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force)
	{
		AddForce(_rigidbody.transform.TransformVector(force), mode);
	}

	public void AddRelativeForce(float x, float y, float z, ForceMode mode = ForceMode.Force)
	{
		AddRelativeForce(new Vector3(x, y, z), mode);
	}

	public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
	{
		if (!_owRigidbody.IsSuspended())
		{
			switch (mode)
			{
			case ForceMode.Force:
				_torque += torque;
				break;
			case ForceMode.Acceleration:
				_angularAcceleration += torque;
				break;
			case ForceMode.Impulse:
				_angularVelocityAccumulator += torque / _owRigidbody.GetMass();
				break;
			case ForceMode.VelocityChange:
				_angularVelocityAccumulator += torque;
				break;
			case (ForceMode)3:
			case (ForceMode)4:
				break;
			}
		}
	}

	public void AddTorque(float x, float y, float z, ForceMode mode = ForceMode.Force)
	{
		AddTorque(new Vector3(x, y, z), mode);
	}

	public void AddRelativeTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
	{
		AddTorque(_rigidbody.transform.TransformVector(torque), mode);
	}

	public void AddRelativeTorque(float x, float y, float z, ForceMode mode = ForceMode.Force)
	{
		AddRelativeTorque(new Vector3(x, y, z), mode);
	}

	public Vector3 GetPointVelocity(Vector3 worldPoint)
	{
		Vector3 rhs = worldPoint - _rigidbody.worldCenterOfMass;
		Vector3 vector = Vector3.Cross(_angularVelocity, rhs);
		return _velocity + vector;
	}

	public Vector3 GetRelativePointVelocity(Vector3 relativePoint)
	{
		return GetPointVelocity(_rigidbody.transform.TransformVector(relativePoint));
	}

	public void Integrate(float fixedDeltaTime)
	{
		float num = 1f / _owRigidbody.GetMass();
		_acceleration += _force * num;
		_velocity += _acceleration * fixedDeltaTime + _velocityAccumulator;
		_angularAcceleration += _torque * num;
		_angularVelocity += _angularAcceleration * fixedDeltaTime + _angularVelocityAccumulator;
		float num2 = Mathf.Max(_angularVelocity.magnitude, Mathf.Epsilon);
		_angularVelocity = _angularVelocity / num2 * Mathf.Min(num2, _rigidbody.maxAngularVelocity);
		Vector3 vector = _velocity * fixedDeltaTime;
		Quaternion quaternion = Quaternion.Euler(_angularVelocity * fixedDeltaTime * 57.29578f);
		_rigidbody.MovePosition(_rigidbody.position + vector);
		_rigidbody.MoveRotation(quaternion * _rigidbody.rotation);
		_acceleration = Vector3.zero;
		_force = Vector3.zero;
		_angularAcceleration = Vector3.zero;
		_torque = Vector3.zero;
		_velocityAccumulator = Vector3.zero;
		_angularVelocityAccumulator = Vector3.zero;
	}

	public Vector3 IntegratePosition()
	{
		_acceleration += _force / _owRigidbody.GetMass();
		_velocity += _acceleration * Time.fixedDeltaTime + _velocityAccumulator;
		return _velocity * Time.fixedDeltaTime;
	}

	public Quaternion IntegrateRotation()
	{
		_angularAcceleration += _torque / _owRigidbody.GetMass();
		_angularVelocity += _angularAcceleration * Time.fixedDeltaTime + _angularVelocityAccumulator;
		float num = Mathf.Max(_angularVelocity.magnitude, Mathf.Epsilon);
		_angularVelocity = _angularVelocity / num * Mathf.Min(num, _rigidbody.maxAngularVelocity);
		return Quaternion.Euler(_angularVelocity * Time.fixedDeltaTime * 57.29578f);
	}

	public void Move(Vector3 position, Quaternion rotation)
	{
		_rigidbody.MovePosition(position);
		_rigidbody.MoveRotation(rotation);
	}

	public void ResetAccumulators()
	{
		_acceleration = Vector3.zero;
		_force = Vector3.zero;
		_angularAcceleration = Vector3.zero;
		_torque = Vector3.zero;
		_velocityAccumulator = Vector3.zero;
		_angularVelocityAccumulator = Vector3.zero;
	}

	public void FireCollisionEvent(KinematicCollision collision)
	{
		if (this.OnCollision != null)
		{
			this.OnCollision(collision);
		}
	}
}
