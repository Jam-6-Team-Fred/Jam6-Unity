using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class InitialMotion : MonoBehaviour
{
	[SerializeField]
	private OWRigidbody _primaryBody;

	[SerializeField]
	private float _orbitAngle;

	[SerializeField]
	private float _orbitImpulseScalar = 1f;

	[SerializeField]
	private Vector3 _rotationAxis = Vector3.up;

	[SerializeField]
	private bool _isGlobalAxis;

	[SerializeField]
	private float _initAngularSpeed;

	[SerializeField]
	private Vector3 _initLinearDirection = Vector3.forward;

	[SerializeField]
	private float _initLinearSpeed;

	[SerializeField]
	private bool _printAxes;

	[SerializeField]
	private bool _printVelocity;

	private OWRigidbody _satelliteBody;

	private Vector3 _cachedInitVelocity = Vector3.zero;

	private bool _isInitVelocityDirty = true;

	private void Awake()
	{
		if (!_isGlobalAxis)
		{
			_rotationAxis = base.transform.TransformDirection(_rotationAxis);
		}
		_initLinearDirection = base.transform.TransformDirection(_initLinearDirection);
		_satelliteBody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		_satelliteBody.UpdateCenterOfMass();
		Vector3 initVelocity = GetInitVelocity();
		_satelliteBody.AddVelocityChange(initVelocity);
		_satelliteBody.AddAngularVelocityChange(GetInitAngularVelocity());
		if (_printVelocity)
		{
			MonoBehaviour.print("Init Velocity: " + initVelocity);
		}
		if (_printAxes && _primaryBody != null)
		{
			Vector2 orbitEllipseSemiAxes = GetOrbitEllipseSemiAxes();
			MonoBehaviour.print(base.gameObject.name + " orbit semi-axes: " + orbitEllipseSemiAxes);
			MonoBehaviour.print("closest dist to Sun surface: " + (orbitEllipseSemiAxes.x * 2f - Vector3.Distance(Locator.GetSunTransform().position, base.transform.position) - 2000f));
		}
	}

	public Vector2 GetOrbitEllipseSemiAxes()
	{
		if (_primaryBody.GetAttachedGravityVolume() == null)
		{
			return Vector2.zero;
		}
		Vector3 vector = OWPhysics.CalculateOrbitVelocity(_primaryBody, _satelliteBody, _orbitAngle) * _orbitImpulseScalar + _initLinearDirection.normalized * _initLinearSpeed;
		float standardGravitationalParameter = _primaryBody.GetAttachedGravityVolume().GetStandardGravitationalParameter();
		float num = Vector3.Distance(_primaryBody.GetWorldCenterOfMass(), _satelliteBody.GetWorldCenterOfMass());
		float num2 = 1f / (2f / num - Mathf.Pow(vector.magnitude, 2f) / standardGravitationalParameter);
		float y = num2 * Mathf.Sqrt(1f - Mathf.Pow((2f * num - 2f * num2) / (2f * num2), 2f));
		return new Vector2(num2, y);
	}

	public Vector3 GetInitAngularVelocity()
	{
		return _rotationAxis.normalized * _initAngularSpeed;
	}

	public Vector3 GetInitVelocity()
	{
		if (_isInitVelocityDirty)
		{
			_isInitVelocityDirty = false;
			_cachedInitVelocity = _initLinearDirection.normalized * _initLinearSpeed;
			if (_primaryBody != null)
			{
				Vector3 vector = OWPhysics.CalculateOrbitVelocity(_primaryBody, _satelliteBody, _orbitAngle) * _orbitImpulseScalar;
				_cachedInitVelocity += vector;
				if (_printVelocity)
				{
					MonoBehaviour.print("Orbit speed: " + vector.magnitude);
				}
				if (_primaryBody.GetComponent<InitialMotion>() != null)
				{
					_cachedInitVelocity += _primaryBody.GetComponent<InitialMotion>().GetInitVelocity();
				}
			}
		}
		return _cachedInitVelocity;
	}

	private void OnDrawGizmosSelected()
	{
		if (base.enabled && OWGizmos.IsDirectlySelected(base.gameObject))
		{
			// CHANGED
			var rigidbody = GetComponent<Rigidbody>();
			if (_primaryBody != null)
			{
				Vector3 vector = rigidbody.worldCenterOfMass - _primaryBody.GetComponent<Rigidbody>().worldCenterOfMass;
				Vector3 normalized = Vector3.Cross(vector, Vector3.up).normalized;
				normalized = Quaternion.AngleAxis(_orbitAngle, vector) * normalized;
				Gizmos.color = Color.red;
				Gizmos.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + normalized.normalized * 200f * Mathf.Sign(_orbitImpulseScalar));
			}
			if (_initLinearSpeed > 0f)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + base.transform.TransformDirection(_initLinearDirection).normalized * 200f);
			}
			Gizmos.color = Color.green;
			if (_isGlobalAxis)
			{
				Gizmos.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + _rotationAxis.normalized * _initAngularSpeed * 2000f);
			}
			else
			{
				Gizmos.DrawLine(rigidbody.worldCenterOfMass, rigidbody.worldCenterOfMass + base.transform.TransformDirection(_rotationAxis).normalized * _initAngularSpeed * 2000f);
			}
		}
	}

	public OWRigidbody GetPrimaryBody()
	{
		return _primaryBody;
	}

	public void SetPrimaryBody(OWRigidbody primaryBody)
	{
		_primaryBody = primaryBody;
	}
}
