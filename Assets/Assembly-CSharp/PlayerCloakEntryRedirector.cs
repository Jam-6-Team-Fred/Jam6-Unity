using UnityEngine;

public class PlayerCloakEntryRedirector : MonoBehaviour
{
	[SerializeField]
	private CloakFieldController _cloakField;

	[SerializeField]
	private Transform _velocityTarget;

	[SerializeField]
	private float _targetRepositionAngle = 30f;

	[SerializeField]
	private float _maxEntryAngle = 90f;

	private OWRigidbody _cloakBody;

	private bool _playerJustEnteredCloak;

	private void Awake()
	{
		_cloakBody = _cloakField.GetAttachedOWRigidbody();
		if (_velocityTarget == null)
		{
			_velocityTarget = base.transform;
		}
		_cloakField.OnPlayerEnter += new OWEvent.OWCallback(OnPlayerEnter);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_cloakField.OnPlayerEnter -= new OWEvent.OWCallback(OnPlayerEnter);
	}

	private void OnPlayerEnter()
	{
		_playerJustEnteredCloak = true;
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		if (_playerJustEnteredCloak)
		{
			if (!PlayerState.IsInsideShip() || (Locator.GetReferenceFrame() != null && Locator.GetReferenceFrame().GetOWRigidBody() == _cloakBody))
			{
				_playerJustEnteredCloak = false;
				base.enabled = false;
				return;
			}
			OWRigidbody shipBody = Locator.GetShipBody();
			Vector3 vector = base.transform.InverseTransformPoint(shipBody.transform.position);
			float num = Vector3.Angle(vector, Vector3.up);
			if (num <= _maxEntryAngle)
			{
				Quaternion quaternion = Quaternion.Inverse(base.transform.rotation) * shipBody.transform.rotation;
				Vector3 vector2 = base.transform.InverseTransformDirection(shipBody.GetVelocity() - _cloakBody.GetVelocity());
				Vector3 vector3 = base.transform.InverseTransformPoint(_velocityTarget.position);
				if (num > _targetRepositionAngle)
				{
					Quaternion quaternion2 = Quaternion.AngleAxis(num - _targetRepositionAngle, Vector3.Cross(vector, Vector3.up));
					vector = quaternion2 * vector;
					quaternion = quaternion2 * quaternion;
				}
				vector2 = (vector3 - vector).normalized * vector2.magnitude;
				Vector3 position = base.transform.TransformPoint(vector);
				Quaternion rotation = base.transform.rotation * quaternion;
				Vector3 velocity = _cloakBody.GetVelocity() + base.transform.TransformDirection(vector2);
				shipBody.SetRotation(rotation);
				shipBody.SetPosition(position);
				shipBody.SetVelocity(velocity);
			}
		}
		_playerJustEnteredCloak = false;
		base.enabled = false;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, Vector3.one);
			Vector3 vector = Quaternion.AngleAxis(_maxEntryAngle, Vector3.forward) * Vector3.up;
			Gizmos.color = Color.green;
			Gizmos.DrawLine(Vector3.zero, Vector3.up * 800f);
			Gizmos.DrawLine(Vector3.zero, vector * 800f);
			OWGizmos.DrawWireArc(Vector3.zero, Vector3.forward, Vector3.up, _maxEntryAngle, 800f);
			Vector3 vector2 = Quaternion.AngleAxis(_targetRepositionAngle, Vector3.forward) * Vector3.up;
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(Vector3.zero, vector2 * 900f);
		}
	}
}
