using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OWTriggerVolume))]
public class RingWorldSpeedLimiter : MonoBehaviour
{
	[Serializable]
	protected class TrackedBody
	{
		public OWRigidbody body;

		public Detector.Name name;

		public float deceleration;

		public TrackedBody(OWRigidbody body, Detector.Name name, float deceleration)
		{
			this.body = body;
			this.name = name;
			this.deceleration = deceleration;
		}
	}

	[SerializeField]
	private float _maxSpeed;

	[SerializeField]
	private float _stoppingDistance = 100f;

	[SerializeField]
	private float _maxEntryAngle = 60f;

	private OWRigidbody _ringBody;

	private OWTriggerVolume _trigger;

	private List<TrackedBody> _trackedBodies = new List<TrackedBody>();

	private bool _playerJustExitedDream;

	private void Awake()
	{
		_ringBody = GetComponentInParent<OWRigidbody>();
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
		GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < _trackedBodies.Count; i++)
		{
			bool flag = false;
			Vector3 vector = _trackedBodies[i].body.GetVelocity() - _ringBody.GetVelocity();
			float magnitude = vector.magnitude;
			if (magnitude <= _maxSpeed)
			{
				flag = true;
			}
			else
			{
				bool flag2 = false;
				float num = _trackedBodies[i].deceleration * Time.deltaTime;
				float num2 = _maxSpeed - magnitude;
				if (num2 > num)
				{
					num = num2;
					flag = true;
				}
				if (_trackedBodies[i].name == Detector.Name.Ship)
				{
					Autopilot component = Locator.GetShipTransform().GetComponent<Autopilot>();
					if (component != null && component.IsFlyingToDestination())
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					Vector3 velocityChange = num * vector.normalized;
					_trackedBodies[i].body.AddVelocityChange(velocityChange);
					if (_trackedBodies[i].name == Detector.Name.Ship && PlayerState.IsInsideShip())
					{
						Locator.GetPlayerBody().AddVelocityChange(velocityChange);
					}
				}
			}
			if (flag)
			{
				if (_trackedBodies[i].name == Detector.Name.Ship)
				{
					GlobalMessenger.FireEvent("ShipExitSpeedLimiter");
				}
				_trackedBodies.RemoveAt(i);
				if (_trackedBodies.Count == 0)
				{
					base.enabled = false;
				}
			}
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		DynamicForceDetector component = hitObj.GetComponent<DynamicForceDetector>();
		if (!(component != null) || !component.CompareNameMask(Detector.Name.Player | Detector.Name.Probe | Detector.Name.Ship))
		{
			return;
		}
		if (component.GetName() == Detector.Name.Player && (PlayerState.IsInsideShip() || _playerJustExitedDream))
		{
			_playerJustExitedDream = false;
		}
		else
		{
			if (component.GetName() == Detector.Name.Probe && Locator.GetCloakFieldController().isPlayerInsideCloak)
			{
				return;
			}
			OWRigidbody attachedOWRigidbody = component.GetAttachedOWRigidbody();
			Vector3 from = base.transform.position - attachedOWRigidbody.GetPosition();
			Vector3 to = attachedOWRigidbody.GetVelocity() - _ringBody.GetVelocity();
			float magnitude = to.magnitude;
			if (magnitude > _maxSpeed && Vector3.Angle(from, to) < _maxEntryAngle)
			{
				float deceleration = (_maxSpeed * _maxSpeed - magnitude * magnitude) / (2f * _stoppingDistance);
				TrackedBody item = new TrackedBody(attachedOWRigidbody, component.GetName(), deceleration);
				_trackedBodies.Add(item);
				if (component.GetName() == Detector.Name.Ship)
				{
					GlobalMessenger.FireEvent("ShipEnterSpeedLimiter");
				}
				base.enabled = true;
			}
		}
	}

	private void OnExit(GameObject hitObj)
	{
		DynamicForceDetector component = hitObj.GetComponent<DynamicForceDetector>();
		if (!(component != null))
		{
			return;
		}
		OWRigidbody body = component.GetAttachedOWRigidbody();
		TrackedBody trackedBody = _trackedBodies.Find((TrackedBody i) => i.body == body);
		if (trackedBody != null)
		{
			if (trackedBody.name == Detector.Name.Ship)
			{
				GlobalMessenger.FireEvent("ShipExitSpeedLimiter");
			}
			_trackedBodies.Remove(trackedBody);
			if (_trackedBodies.Count == 0)
			{
				base.enabled = false;
			}
		}
	}

	private void OnExitDreamWorld()
	{
		_playerJustExitedDream = true;
	}
}
