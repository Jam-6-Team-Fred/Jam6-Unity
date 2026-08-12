using UnityEngine;

[RequireComponent(typeof(OWRigidbody))]
public class RandomAngularVelocity : MonoBehaviour
{
	[SerializeField]
	private float _minSpeed;

	[SerializeField]
	private float _maxSpeed;

	private OWRigidbody _owRigidbody;

	private void Awake()
	{
		_owRigidbody = this.GetRequiredComponent<OWRigidbody>();
	}

	private void Start()
	{
		Vector3 normalized = Random.insideUnitSphere.normalized;
		float num = Random.Range(_minSpeed, _maxSpeed);
		_owRigidbody.AddAngularVelocityChange(normalized * num);
		Object.Destroy(this);
	}
}
