using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LandingPadSensor : MonoBehaviour
{
	private ShipAudioController _audioController;

	private OWAudioSource _landingPadAudioSrc;

	private OWRigidbody _contactBody;

	private void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
		base.gameObject.layer = LayerMask.NameToLayer("Default");
		_landingPadAudioSrc = GetComponent<OWAudioSource>();
	}

	public OWRigidbody GetContactBody()
	{
		return _contactBody;
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (_audioController == null)
		{
			_audioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		}
		if (!(hitCollider.attachedRigidbody != null))
		{
			return;
		}
		OWRigidbody component = hitCollider.attachedRigidbody.GetComponent<OWRigidbody>();
		if (component != null)
		{
			_contactBody = component;
			if (Time.timeSinceLevelLoad > 1f)
			{
				float magnitude = (_contactBody.GetPointVelocity(base.transform.position) - Locator.GetShipBody().GetPointVelocity(base.transform.position)).magnitude;
				_audioController.PlayLandingPadImpact(_landingPadAudioSrc, magnitude);
			}
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (_audioController == null)
		{
			_audioController = Locator.GetShipTransform().GetComponentInChildren<ShipAudioController>();
		}
		if (hitCollider.attachedRigidbody != null)
		{
			OWRigidbody component = hitCollider.attachedRigidbody.GetComponent<OWRigidbody>();
			if (component != null && component == _contactBody)
			{
				_contactBody = null;
			}
		}
	}
}
