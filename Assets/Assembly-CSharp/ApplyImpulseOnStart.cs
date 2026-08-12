using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class ApplyImpulseOnStart : MonoBehaviour
{
	[SerializeField]
	private float _timeToImpulse;

	[SerializeField]
	private Vector3 _impulse;

	private float _timeActive;

	private OWRigidbody _rigidbody;

	private void Awake()
	{
		_rigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void Update()
	{
		if (_timeActive > _timeToImpulse)
		{
			_rigidbody.AddImpulse(_impulse);
			base.enabled = false;
		}
		_timeActive += Time.deltaTime;
	}
}
