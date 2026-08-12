using UnityEngine;

public class GhostRandomWalk : MonoBehaviour
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Transform _normalRef;

	[SerializeField]
	private Transform _eyeLevel;

	[SerializeField]
	private Vector2 _changeDirIntervalRange;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private float _turnSpeed;

	[SerializeField]
	private Collider _collider;

	private float _nextChangeDirTime;

	private Vector3 _destination;

	private void Start()
	{
		_animator.SetFloat("Speed", 1f);
		_destination = base.transform.position;
	}

	private void Update()
	{
		if (Locator.GetProbe() != null)
		{
			_destination = Locator.GetProbe().transform.position;
		}
		base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((_destination - base.transform.position).normalized, _normalRef.up), _normalRef.up);
		if (Vector3.Distance(base.transform.position, _destination) > 2f)
		{
			_animator.SetFloat("Speed", 1f, 0.2f, Time.deltaTime);
			MoveForward();
		}
		else if (_animator.GetFloat("Speed") <= 0.02f)
		{
			_animator.SetFloat("Speed", 0f);
		}
		else
		{
			_animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
		}
	}

	private void LateUpdate()
	{
		Vector3 normalized = Vector3.ProjectOnPlane(_eyeLevel.InverseTransformPoint(Locator.GetPlayerCamera().transform.position), Vector3.right).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(_eyeLevel.InverseTransformPoint(Locator.GetPlayerCamera().transform.position), Vector3.up).normalized;
		_animator.SetFloat("AimX", normalized2.x);
		_animator.SetFloat("AimY", normalized.y);
	}

	private void SelectDirection()
	{
		Vector3 vector = _destination - base.transform.position;
		vector = Vector3.ProjectOnPlane(vector, _normalRef.up);
		float yAngle = OWMath.Angle(base.transform.forward, vector.normalized, _normalRef.up);
		base.transform.Rotate(0f, yAngle, 0f, Space.Self);
	}

	private void MoveForward()
	{
		Vector3 origin = _collider.bounds.center + base.transform.forward * _speed * Time.deltaTime;
		if (Physics.Raycast(new Ray(origin, -_normalRef.up), out var hitInfo, 100f))
		{
			origin = hitInfo.point;
		}
		base.transform.position = new Vector3(origin.x, Mathf.Lerp(base.transform.position.y, origin.y, 0.5f), origin.z);
	}
}
