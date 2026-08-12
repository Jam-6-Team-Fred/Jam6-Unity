using System;
using System.Collections.Generic;
using UnityEngine;

public class RapidsHazardVolume : HazardVolume
{
	[Serializable]
	private class TrackedObject
	{
		public OWRigidbody body;

		public HazardDetector hazardDetector;

		public FluidDetector fluidDetector;

		public bool ignoreTorque;

		public bool updateTorque;

		public float torqueStartTime;

		public float torqueAcceleration;

		public float torqueDuration;

		public Vector2 localTorqueAxis;
	}

	[Header("Torque")]
	[SerializeField]
	private float _minTorqueDelay = 0.4f;

	[SerializeField]
	private float _maxTorqueDelay = 0.8f;

	[SerializeField]
	private float _torqueAcceleration = 3.6f;

	[SerializeField]
	private float _minTorqueDuration = 0.5f;

	[SerializeField]
	private float _maxTorqueDuration = 1f;

	private List<TrackedObject> _trackedObjects = new List<TrackedObject>(8);

	private void FixedUpdate()
	{
		if (_trackedObjects.Count == 0)
		{
			base.enabled = false;
			return;
		}
		float time = Time.time;
		for (int i = 0; i < _trackedObjects.Count; i++)
		{
			if (_trackedObjects[i].updateTorque)
			{
				float num = Mathf.InverseLerp(_trackedObjects[i].torqueStartTime, _trackedObjects[i].torqueStartTime + _trackedObjects[i].torqueDuration, time);
				Vector3 vector = base.transform.TransformDirection(new Vector3(_trackedObjects[i].localTorqueAxis.x, 0f, _trackedObjects[i].localTorqueAxis.y));
				_trackedObjects[i].body.AddAngularAcceleration(vector * _trackedObjects[i].torqueAcceleration * num * num);
				if (_trackedObjects[i].body == Locator.GetPlayerController().GetGroundBody())
				{
					RumbleManager.AddRapidsRumble(Mathf.InverseLerp(_minTorqueDuration, _maxTorqueDuration, _trackedObjects[i].torqueDuration), num);
				}
				if (num >= 1f)
				{
					_trackedObjects[i].updateTorque = false;
					_trackedObjects[i].torqueStartTime = time + UnityEngine.Random.Range(_minTorqueDelay, _maxTorqueDelay);
				}
			}
			else if (time >= _trackedObjects[i].torqueStartTime && !_trackedObjects[i].ignoreTorque)
			{
				_trackedObjects[i].updateTorque = true;
				_trackedObjects[i].torqueAcceleration = _torqueAcceleration;
				_trackedObjects[i].torqueDuration = UnityEngine.Random.Range(_minTorqueDuration, _maxTorqueDuration);
				_trackedObjects[i].localTorqueAxis = UnityEngine.Random.insideUnitCircle.normalized;
			}
		}
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		OWRigidbody body = hitObj.GetAttachedOWRigidbody();
		FluidDetector component = hitObj.GetComponent<FluidDetector>();
		if (!component.CompareName(Detector.Name.Player) && body != null && component != null && component.GetAppliesForces() && !_trackedObjects.Exists((TrackedObject x) => x.body == body))
		{
			TrackedObject trackedObject = new TrackedObject();
			trackedObject.body = body;
			trackedObject.hazardDetector = hitObj.GetComponent<HazardDetector>();
			trackedObject.fluidDetector = component;
			trackedObject.torqueStartTime = Time.time + UnityEngine.Random.Range(_minTorqueDelay, _maxTorqueDelay);
			trackedObject.updateTorque = false;
			trackedObject.ignoreTorque = component.CompareName(Detector.Name.Probe);
			_trackedObjects.Add(trackedObject);
			if (trackedObject.hazardDetector != null)
			{
				trackedObject.hazardDetector.AddVolume(this);
			}
			base.enabled = true;
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		OWRigidbody body = hitObj.GetAttachedOWRigidbody();
		FluidDetector component = hitObj.GetComponent<FluidDetector>();
		if (body != null && component != null && component.GetAppliesForces())
		{
			HazardDetector component2 = hitObj.GetComponent<HazardDetector>();
			if (component2 != null)
			{
				component2.RemoveVolume(this);
			}
			_trackedObjects.RemoveAll((TrackedObject x) => x.body == body);
		}
	}

	public override HazardType GetHazardType()
	{
		return HazardType.RAPIDS;
	}
}
