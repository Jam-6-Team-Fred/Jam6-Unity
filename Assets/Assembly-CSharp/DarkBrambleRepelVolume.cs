using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereShape))]
[RequireComponent(typeof(OWTriggerVolume))]
public class DarkBrambleRepelVolume : MonoBehaviour
{
	[SerializeField]
	private float _innerRadius;

	private SphereShape _sphereShape;

	private OWTriggerVolume _trigger;

	private OWRigidbody _parentBody;

	private List<OWRigidbody> _trackedBodies;

	private void Awake()
	{
		_parentBody = base.gameObject.GetAttachedOWRigidbody();
		_sphereShape = base.gameObject.GetRequiredComponent<SphereShape>();
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		_trackedBodies = new List<OWRigidbody>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _trackedBodies.Count; i++)
		{
			if (!_trackedBodies[i].CompareTag("Player") || !PlayerState.IsInsideShip())
			{
				Vector3 planeNormal = base.transform.position - _trackedBodies[i].GetPosition();
				float magnitude = planeNormal.magnitude;
				Vector3 vector = _trackedBodies[i].GetVelocity() - _parentBody.GetVelocity();
				float num = Mathf.InverseLerp(_sphereShape.radius, _innerRadius, magnitude);
				Vector3 velocityChange = (Vector3.ProjectOnPlane(vector, planeNormal).normalized * vector.magnitude - vector) * num;
				_trackedBodies[i].AddVelocityChange(velocityChange);
			}
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if ((hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ShipDetector") || hitObj.CompareTag("ProbeDetector")) && Vector3.Distance(base.transform.position, hitObj.transform.position) > _innerRadius)
		{
			_trackedBodies.Add(hitObj.GetAttachedOWRigidbody());
			base.enabled = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") || hitObj.CompareTag("ShipDetector") || hitObj.CompareTag("ProbeDetector"))
		{
			_trackedBodies.Remove(hitObj.GetAttachedOWRigidbody());
			if (_trackedBodies.Count == 0)
			{
				base.enabled = false;
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, _innerRadius);
	}
}
