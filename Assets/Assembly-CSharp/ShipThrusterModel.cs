using UnityEngine;

public class ShipThrusterModel : ThrusterModel
{
	private bool _leftBankEnabled = true;

	private bool _rightBankEnabled = true;

	public void SetThrusterBankEnabled(ThrusterBank thrusterBank, bool enabled)
	{
		switch (thrusterBank)
		{
		case ThrusterBank.Left:
			_leftBankEnabled = enabled;
			break;
		case ThrusterBank.Right:
			_rightBankEnabled = enabled;
			break;
		}
	}

	public override bool IsThrusterBankEnabled(ThrusterBank thrusterBank)
	{
		switch (thrusterBank)
		{
		case ThrusterBank.Left:
			return _leftBankEnabled;
		case ThrusterBank.Right:
			return _rightBankEnabled;
		default:
			return false;
		}
	}

	protected override void FireTranslationalThrusters()
	{
		_localAcceleration = _translationalInput * _maxTranslationalThrust;
		_localAcceleration.x = Mathf.Clamp(_localAcceleration.x, 0f - _maxTranslationalThrust, _maxTranslationalThrust);
		_localAcceleration.y = Mathf.Clamp(_localAcceleration.y, 0f - _maxTranslationalThrust, _maxTranslationalThrust);
		_localAcceleration.z = Mathf.Clamp(_localAcceleration.z, 0f - _maxTranslationalThrust, _maxTranslationalThrust);
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (!_leftBankEnabled && !_rightBankEnabled)
		{
			_localAcceleration = Vector3.zero;
		}
		else if (!_leftBankEnabled)
		{
			_localAcceleration.x = Mathf.Min(_localAcceleration.x, 0f);
			_localAcceleration.y *= 0.5f;
			_localAcceleration.z *= 0.5f;
			vector = Vector3.right;
			vector2 = Vector3.right;
		}
		else if (!_rightBankEnabled)
		{
			_localAcceleration.x = Mathf.Max(_localAcceleration.x, 0f);
			_localAcceleration.y *= 0.5f;
			_localAcceleration.z *= 0.5f;
			vector = -Vector3.right;
			vector2 = -Vector3.right;
		}
		if (_localAcceleration.magnitude > 0f)
		{
			_isTranslationalFiring = true;
			_owRigidbody.AddLocalAcceleration(new Vector3(_localAcceleration.x, 0f, 0f));
			_owRigidbody.AddLocalAcceleration(new Vector3(0f, _localAcceleration.y, 0f), _owRigidbody.GetCenterOfMass() + vector * 4f);
			_owRigidbody.AddLocalAcceleration(new Vector3(0f, 0f, _localAcceleration.z), _owRigidbody.GetCenterOfMass() + vector2);
		}
		else
		{
			_isTranslationalFiring = false;
		}
	}
}
