using System.Collections.Generic;
using UnityEngine;

public class AlignShipWithReferenceFrame : AlignWithDirection
{
	private ReferenceFrameTracker _rfTracker;

	private LandingPadManager _landingManager;

	private List<OWRigidbody> _collidingBodyList;

	private float _lastCollideTime;

	public bool IsAligning()
	{
		if (_doAlignment)
		{
			return base.enabled;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		base.enabled = false;
		_collidingBodyList = new List<OWRigidbody>(8);
		GlobalMessenger<ReferenceFrame>.AddListener("EnterLandingMode", OnEnterLandingMode);
		GlobalMessenger.AddListener("ExitLandingMode", OnExitLandingMode);
	}

	protected override void Start()
	{
		base.Start();
		_rfTracker = base.gameObject.GetTaggedComponent<ReferenceFrameTracker>("Player");
		_landingManager = Locator.GetShipBody().GetComponent<LandingPadManager>();
	}

	private void OnDestroy()
	{
		GlobalMessenger<ReferenceFrame>.RemoveListener("EnterLandingMode", OnEnterLandingMode);
		GlobalMessenger.RemoveListener("ExitLandingMode", OnExitLandingMode);
	}

	protected override Vector3 GetAlignmentDirection()
	{
		ReferenceFrame referenceFrame = _rfTracker.GetReferenceFrame();
		if (referenceFrame == null)
		{
			return _currentDirection;
		}
		return referenceFrame.GetPosition() - _owRigidbody.GetWorldCenterOfMass();
	}

	protected override bool CheckAlignmentRequirements()
	{
		if (_collidingBodyList.Count == 0 && _landingManager.GetContactCount() == 0)
		{
			return Time.time > _lastCollideTime + 3f;
		}
		return false;
	}

	private void OnEnterLandingMode(ReferenceFrame referenceFrame)
	{
		base.enabled = true;
	}

	private void OnExitLandingMode()
	{
		base.enabled = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.rigidbody != null)
		{
			OWRigidbody requiredComponent = collision.rigidbody.GetRequiredComponent<OWRigidbody>();
			if (!_collidingBodyList.Contains(requiredComponent) && !collision.gameObject.CompareTag("Player"))
			{
				_collidingBodyList.Add(requiredComponent);
				requiredComponent.OnDestroyOWRigidbody += OnDestroyOWRigidbody;
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (!(collision.rigidbody != null))
		{
			return;
		}
		OWRigidbody requiredComponent = collision.rigidbody.GetRequiredComponent<OWRigidbody>();
		if (_collidingBodyList.Contains(requiredComponent))
		{
			_collidingBodyList.Remove(requiredComponent);
			requiredComponent.OnDestroyOWRigidbody -= OnDestroyOWRigidbody;
			if (_collidingBodyList.Count == 0)
			{
				_lastCollideTime = Time.time;
			}
		}
	}

	private void OnDestroyOWRigidbody(OWRigidbody destroyedBody)
	{
		_collidingBodyList.Remove(destroyedBody);
		destroyedBody.OnDestroyOWRigidbody -= OnDestroyOWRigidbody;
	}
}
