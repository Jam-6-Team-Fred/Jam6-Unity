using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
	[SerializeField]
	private GameObject _projectilePrefab;

	[SerializeField]
	private float _minSpeed = 50f;

	[SerializeField]
	private float _maxSpeed = 100f;

	[SerializeField]
	private float _angularSpeed;

	[SerializeField]
	private float _launcherRadius;

	[SerializeField]
	private float _launchConeAngle;

	[SerializeField]
	private bool _useLaunchTimer;

	[SerializeField]
	private float _minLaunchDelay = 1f;

	[SerializeField]
	private float _maxLaunchDelay = 5f;

	private float _lastLaunchTime;

	private float _launchDelay;

	private OWRigidbody _parentBody;

	private void Awake()
	{
		_parentBody = base.gameObject.GetAttachedOWRigidbody();
		if (!_useLaunchTimer)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (Time.time > _lastLaunchTime + _launchDelay)
		{
			LaunchProjectile();
			_lastLaunchTime = Time.time;
			_launchDelay = Random.Range(_minLaunchDelay, _maxLaunchDelay);
		}
	}

	public void LaunchProjectile()
	{
		Vector3 vector = base.transform.TransformDirection(UnitSphere.GetPointOnCap(_launchConeAngle)) * _launcherRadius + base.transform.position;
		Vector3 vector2 = (vector - base.transform.position).normalized;
		if (vector2.sqrMagnitude == 0f)
		{
			vector2 = base.transform.forward;
		}
		Quaternion rotation = base.transform.rotation * Quaternion.FromToRotation(base.transform.forward, vector2);
		GameObject obj = Object.Instantiate(_projectilePrefab, vector, rotation);
		obj.transform.parent = base.transform.root;
		OWRigidbody requiredComponent = obj.GetRequiredComponent<OWRigidbody>();
		Vector3 vector3 = vector2 * Random.Range(_minSpeed, _maxSpeed);
		requiredComponent.SetVelocity(_parentBody.GetPointVelocity(vector) + vector3);
		requiredComponent.AddAngularVelocityChange(vector2 * _angularSpeed);
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(base.transform.position, base.transform.forward * (_minSpeed + _maxSpeed) * 0.5f);
			Gizmos.DrawWireSphere(base.transform.position, _launcherRadius);
			Gizmos.color = Color.white;
			Gizmos.DrawRay(base.transform.position, Quaternion.AngleAxis(_launchConeAngle, base.transform.right) * base.transform.forward * _launcherRadius);
			Gizmos.DrawRay(base.transform.position, Quaternion.AngleAxis(_launchConeAngle, -base.transform.right) * base.transform.forward * _launcherRadius);
			Gizmos.DrawRay(base.transform.position, Quaternion.AngleAxis(_launchConeAngle, base.transform.up) * base.transform.forward * _launcherRadius);
			Gizmos.DrawRay(base.transform.position, Quaternion.AngleAxis(_launchConeAngle, -base.transform.up) * base.transform.forward * _launcherRadius);
		}
	}
}
