using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ModelShipLandingSpot : MonoBehaviour
{
	private OWRigidbody _modelShipBody;

	private OWRigidbody _planetBody;

	private float _initLandTime;

	private bool _isLanded;

	private OWCollider _owCollider;

	private void Awake()
	{
		_owCollider = base.gameObject.GetAddComponent<OWCollider>();
		_owCollider.SetActivation(active: false);
		_owCollider.Assert(OWLayerMask.effectVolumeMask, isTrigger: true);
		_planetBody = this.GetAttachedOWRigidbody();
		base.enabled = false;
		GlobalMessenger<OWRigidbody>.AddListener("EnterRemoteFlightConsole", OnEnterRemoteFlightConsole);
		GlobalMessenger.AddListener("ExitRemoteFlightConsole", OnExitRemoteFlightConsole);
	}

	private void OnDestroy()
	{
		GlobalMessenger<OWRigidbody>.RemoveListener("EnterRemoteFlightConsole", OnEnterRemoteFlightConsole);
		GlobalMessenger.RemoveListener("ExitRemoteFlightConsole", OnExitRemoteFlightConsole);
	}

	private void OnEnterRemoteFlightConsole(OWRigidbody modelShipBody)
	{
		_owCollider.SetActivation(active: true);
	}

	private void OnExitRemoteFlightConsole()
	{
		_owCollider.SetActivation(active: false);
	}

	private void FixedUpdate()
	{
		Vector3 vector = _planetBody.GetPointVelocity(_modelShipBody.GetPosition()) - _modelShipBody.GetVelocity();
		Vector3 vector2 = _planetBody.GetAngularVelocity() - _modelShipBody.GetAngularVelocity();
		if (!_isLanded && vector.magnitude < 0.1f && vector2.magnitude < 0.01f)
		{
			_initLandTime = Time.time;
			_isLanded = true;
		}
		else if (_isLanded && Time.time > _initLandTime + 0.2f)
		{
			DialogueConditionManager.SharedInstance.SetConditionState("CrashedModelRocket");
			DialogueConditionManager.SharedInstance.SetConditionState("LandedModelRocket", conditionState: true);
			base.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider hitCollider)
	{
		if (hitCollider.gameObject.CompareTag("ModelShipDetector"))
		{
			base.enabled = true;
			_isLanded = false;
			if (_modelShipBody == null)
			{
				_modelShipBody = hitCollider.GetAttachedOWRigidbody();
			}
		}
	}

	private void OnTriggerExit(Collider hitCollider)
	{
		if (hitCollider.gameObject.CompareTag("ModelShipDetector"))
		{
			base.enabled = false;
			_isLanded = false;
		}
	}
}
