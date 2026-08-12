using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class SpeedTrapVolume : MonoBehaviour
{
	[SerializeField]
	private float _speedLimit = 10f;

	[SerializeField]
	private float _acceleration = 3f;

	private OWTriggerVolume _trigger;

	private OWRigidbody _parentBody;

	private OWRigidbody _playerBody;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_parentBody = base.gameObject.GetAttachedOWRigidbody();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void FixedUpdate()
	{
		Vector3 vector = _playerBody.GetVelocity() - _parentBody.GetPointVelocity(_playerBody.GetPosition());
		if (vector.magnitude > _speedLimit)
		{
			_playerBody.AddVelocityChange(-vector * Time.deltaTime * _acceleration);
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerBody = Locator.GetPlayerBody();
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_playerBody = null;
			base.enabled = false;
		}
	}
}
