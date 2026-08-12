using UnityEngine;

public class DreamElevatorWarpVolume : MonoBehaviour
{
	[SerializeField]
	private Transform _destinationTransform;

	[SerializeField]
	private Sector _destinationSector;

	[SerializeField]
	private CageElevator _trackedElevator;

	[SerializeField]
	private int _ID = -1;

	private OWRigidbody _attachedBody;

	private OWTriggerVolume _triggerVolume;

	private OWRigidbody _destinationBody;

	private bool _trackingElevator;

	private bool _trackingPlayer;

	private bool _isLowerWarp;

	private float _prevPlayerRelativeDirection;

	private void Awake()
	{
		_attachedBody = this.GetAttachedOWRigidbody();
		_triggerVolume = base.gameObject.GetAddComponent<OWTriggerVolume>();
		_destinationBody = _destinationTransform.GetAttachedOWRigidbody();
		_isLowerWarp = Vector3.Dot(base.transform.up, _destinationTransform.position - base.transform.position) > 0f;
		_triggerVolume.OnEntry += OnEnterTriggerVolume;
		_triggerVolume.OnExit += OnExitTriggerVolume;
		if (!_isLowerWarp)
		{
			_trackedElevator.SetHasWarp(base.transform, _destinationTransform);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEnterTriggerVolume;
		_triggerVolume.OnExit -= OnExitTriggerVolume;
	}

	private void FixedUpdate()
	{
		if (!_trackingPlayer)
		{
			return;
		}
		OWRigidbody playerBody = Locator.GetPlayerBody();
		float num = Vector3.Dot(base.transform.up, playerBody.transform.position - base.transform.position);
		if (num != _prevPlayerRelativeDirection && IsEqualToWarpDirection(num))
		{
			if (_trackingElevator)
			{
				WarpBody(_trackedElevator.elevatorBody, _destinationBody, _destinationTransform);
				_trackedElevator.Warped(!_isLowerWarp);
				_trackingElevator = false;
			}
			WarpBody(playerBody, _destinationBody, _destinationTransform);
			_trackingPlayer = false;
			base.enabled = false;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_destinationSector.AddOccupant(Locator.GetPlayerSectorDetector());
		}
		_prevPlayerRelativeDirection = num;
	}

	private bool IsEqualToWarpDirection(float direction)
	{
		if (!(direction > 0f) || !_isLowerWarp)
		{
			if (direction < 0f)
			{
				return !_isLowerWarp;
			}
			return false;
		}
		return true;
	}

	private void WarpBody(OWRigidbody bodyToWarp, OWRigidbody destinationBody, Transform destinationTransform)
	{
		bodyToWarp.MoveToRelativeLocation(new RelativeLocationData(bodyToWarp, _attachedBody, base.transform), destinationBody, destinationTransform);
	}

	private void OnEnterTriggerVolume(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody == _trackedElevator.elevatorBody && !_trackingElevator)
		{
			Debug.Log("[Elevator Warp] elevator enter trigger");
			_trackingElevator = true;
		}
		else if (attachedOWRigidbody.CompareTag("Player") && !_trackingPlayer)
		{
			Achievement_Ghost.ReachLibrary(_ID);
			_prevPlayerRelativeDirection = Vector3.Dot(base.transform.up, attachedOWRigidbody.transform.position - base.transform.position);
			_trackingPlayer = true;
			base.enabled = true;
		}
	}

	private void OnExitTriggerVolume(GameObject hitObj)
	{
		OWRigidbody attachedOWRigidbody = hitObj.GetAttachedOWRigidbody();
		if (attachedOWRigidbody == _trackedElevator.elevatorBody && _trackingElevator)
		{
			float direction = Vector3.Dot(base.transform.up, attachedOWRigidbody.transform.position - base.transform.position);
			if (IsEqualToWarpDirection(direction))
			{
				Debug.Log("[Elevator Warp] warping elevator due to trigger exit");
				WarpBody(attachedOWRigidbody, _destinationBody, _destinationTransform);
				_trackedElevator.Warped(!_isLowerWarp);
				_trackingElevator = false;
			}
		}
		else if (attachedOWRigidbody.CompareTag("Player") && _trackingPlayer)
		{
			_trackingPlayer = false;
			base.enabled = false;
		}
	}
}
