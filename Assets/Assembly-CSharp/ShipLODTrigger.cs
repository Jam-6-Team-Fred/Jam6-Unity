using UnityEngine;

public class ShipLODTrigger : MonoBehaviour
{
	[SerializeField]
	private float _radius = 10f;

	private Transform _transform;

	private Transform _playerTransform;

	private SurveyorProbe _probe;

	private Transform _probeTransform;

	private bool _playerInRadius;

	private bool _probeInRadius;

	public OWEvent OnTriggerUpdated = new OWEvent(8);

	public float radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public bool isPlayerInTrigger => _playerInRadius;

	public bool isProbeInTrigger => _probeInRadius;

	private void Start()
	{
		_transform = base.transform;
		_playerTransform = Locator.GetPlayerTransform();
		_probe = Locator.GetProbe();
		_probeTransform = _probe.transform;
	}

	private void OnDisable()
	{
		_playerInRadius = false;
		_probeInRadius = false;
		OnTriggerUpdated.Invoke();
	}

	private void FixedUpdate()
	{
		bool playerInRadius = _playerInRadius;
		bool probeInRadius = _probeInRadius;
		if (_playerTransform != null)
		{
			_playerInRadius = Vector3.SqrMagnitude(_transform.position - _playerTransform.position) < _radius * _radius;
		}
		if (_probeTransform != null)
		{
			_probeInRadius = _probe != null && _probe.IsLaunched() && Vector3.SqrMagnitude(_transform.position - _probeTransform.position) < _radius * _radius;
		}
		if (playerInRadius != _playerInRadius || probeInRadius != _probeInRadius)
		{
			OnTriggerUpdated.Invoke();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
			Gizmos.DrawSphere(base.transform.position, _radius);
		}
	}
}
