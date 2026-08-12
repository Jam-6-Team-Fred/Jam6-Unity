using UnityEngine;

public class DebrisLeash : MonoBehaviour
{
	private OWRigidbody _attachedBody;

	private OWRigidbody _anchorBody;

	private DetachableFragment _detachableFragment;

	private float _leashLength;

	private float _deccel = 5f;

	private bool _deccelerating;

	private void Awake()
	{
		_attachedBody = base.gameObject.GetRequiredComponent<OWRigidbody>();
		_attachedBody.SetAngularVelocity(Vector3.zero);
		if (base.gameObject.CompareTag("DetachedFragment"))
		{
			_detachableFragment = GetComponentInChildren<DetachableFragment>();
		}
	}

	public void Init(OWRigidbody anchorBody, float leashLength)
	{
		_anchorBody = anchorBody;
		_leashLength = leashLength;
	}

	public void MoveByDistance(float distance)
	{
		if (base.enabled)
		{
			Vector3 vector = _attachedBody.GetPosition() - _anchorBody.GetPosition();
			float num = Mathf.Min(distance, _leashLength - vector.magnitude);
			_attachedBody.SetPosition(_anchorBody.GetPosition() + vector.normalized * num);
		}
	}

	private void FixedUpdate()
	{
		if (!_deccelerating)
		{
			float num = Vector3.Distance(_attachedBody.GetPosition(), _anchorBody.GetPosition());
			float num2 = Mathf.Pow(_attachedBody.GetVelocity().magnitude, 2f) / (2f * _deccel);
			Vector3 vector = _attachedBody.GetVelocity() - _anchorBody.GetVelocity();
			if (num >= _leashLength - num2 && vector.magnitude > 0.1f)
			{
				_deccelerating = true;
			}
			return;
		}
		Vector3 vector2 = _attachedBody.GetVelocity() - _anchorBody.GetVelocity();
		Vector3 velocityChange = -vector2.normalized * Mathf.Min(_deccel * Time.deltaTime, vector2.magnitude);
		if (velocityChange.magnitude < 0.01f)
		{
			_attachedBody.SetVelocity(_anchorBody.GetVelocity());
			_deccelerating = false;
			if (_detachableFragment != null)
			{
				_detachableFragment.ComeToRest(_anchorBody);
			}
			base.enabled = false;
		}
		else
		{
			_attachedBody.AddVelocityChange(velocityChange);
		}
	}
}
