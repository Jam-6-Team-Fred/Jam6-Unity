using UnityEngine;

public class LandingPadManager : MonoBehaviour
{
	private LandingPadSensor[] _landingPadSensors;

	private OWRigidbody _shipBody;

	private bool _isLanded;

	private bool _wasLanded;

	private int _padContacts;

	private void Awake()
	{
		_landingPadSensors = GetComponentsInChildren<LandingPadSensor>();
		_shipBody = base.gameObject.GetAttachedOWRigidbody();
	}

	public bool IsLanded()
	{
		return _isLanded;
	}

	public int GetContactCount()
	{
		return _padContacts;
	}

	private void FixedUpdate()
	{
		_isLanded = true;
		_padContacts = 0;
		OWRigidbody oWRigidbody = null;
		for (int i = 0; i < _landingPadSensors.Length; i++)
		{
			OWRigidbody contactBody = _landingPadSensors[i].GetContactBody();
			if (contactBody != null)
			{
				_padContacts++;
				if (oWRigidbody == null)
				{
					oWRigidbody = contactBody;
				}
				if (oWRigidbody != contactBody)
				{
					_isLanded = false;
				}
			}
			else
			{
				_isLanded = false;
			}
		}
		if (_isLanded && (_shipBody.GetVelocity() - oWRigidbody.GetPointVelocity(_shipBody.GetWorldCenterOfMass())).sqrMagnitude > 25f)
		{
			_isLanded = false;
		}
		if (!_isLanded && _wasLanded && PlayerState.AtFlightConsole())
		{
			GlobalMessenger.FireEvent("ShipLiftoff");
		}
		_wasLanded = _isLanded;
	}
}
