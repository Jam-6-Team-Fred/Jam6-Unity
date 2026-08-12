using UnityEngine;

public class RelativeExistence : MonoBehaviour
{
	[SerializeField]
	private float _minRelativeSpeed = -1f;

	[SerializeField]
	private float _maxRelativeSpeed = 1f;

	private OWRigidbody _playerBody;

	private OWRigidbody _attachedBody;

	private float _existenceScalar = 1f;

	private void Awake()
	{
		_playerBody = base.gameObject.FindWithRequiredTag("Player").GetRequiredComponent<OWRigidbody>();
		_attachedBody = GetComponent<Collider>().attachedRigidbody.GetRequiredComponent<OWRigidbody>();
	}

	private void Update()
	{
		GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, _existenceScalar);
		if (_existenceScalar <= 0.9f)
		{
			GetComponent<Collider>().isTrigger = true;
		}
		else
		{
			GetComponent<Collider>().isTrigger = false;
		}
	}

	private void FixedUpdate()
	{
		Vector3 lhs = _attachedBody.GetPointVelocity(base.transform.position) - _playerBody.GetVelocity();
		Vector3 rhs = base.transform.position - _playerBody.GetPosition();
		float num = lhs.magnitude * Mathf.Sign(Vector3.Dot(lhs, rhs));
		if (num > _minRelativeSpeed && num < _maxRelativeSpeed)
		{
			_existenceScalar = 1f;
		}
		else
		{
			_existenceScalar = 0f;
		}
	}
}
