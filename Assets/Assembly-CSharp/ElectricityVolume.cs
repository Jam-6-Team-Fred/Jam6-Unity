using System.Collections.Generic;
using UnityEngine;

public class ElectricityVolume : HazardVolume
{
	private class TrackedHazardDetector
	{
		public HazardDetector hazardDetector;

		public float lastShockTime;

		public bool wasInsulated;
	}

	[Space]
	[SerializeField]
	private float _shockRepeatTime = 1f;

	[Space]
	[SerializeField]
	private float _kickback = 10f;

	[SerializeField]
	private bool _shellKickback;

	[SerializeField]
	private float _shellThickness = 1f;

	[Space]
	[SerializeField]
	private OWAudioSource[] _shockAudioPool;

	private static float s_shipElectroKnockoutTime;

	private List<TrackedHazardDetector> _trackedDetectors;

	private int _audioPoolIndex;

	protected override void Awake()
	{
		base.Awake();
		_trackedDetectors = new List<TrackedHazardDetector>(4);
	}

	private void LateUpdate()
	{
		if (_trackedDetectors.Count == 0)
		{
			base.enabled = false;
			return;
		}
		for (int i = 0; i < _trackedDetectors.Count; i++)
		{
			bool flag = _trackedDetectors[i].hazardDetector.IsInsulated();
			if (!flag && (_trackedDetectors[i].wasInsulated || Time.timeSinceLevelLoad > _trackedDetectors[i].lastShockTime + _shockRepeatTime))
			{
				if (_trackedDetectors[i].hazardDetector.CompareTag("PlayerDetector"))
				{
					_trackedDetectors[i].hazardDetector.GetAttachedOWRigidbody().GetComponent<PlayerResources>().ApplyInstantDamage(_firstContactDamage, _firstContactDamageType);
				}
				ApplyShock(_trackedDetectors[i].hazardDetector);
				_trackedDetectors[i].lastShockTime = Time.timeSinceLevelLoad;
			}
			_trackedDetectors[i].wasInsulated = flag;
		}
	}

	private void ApplyShock(HazardDetector detector)
	{
		detector.PlayElectricityEffects();
		if (_shockAudioPool.Length != 0)
		{
			_shockAudioPool[_audioPoolIndex].transform.position = detector.transform.position;
			_shockAudioPool[_audioPoolIndex].PlayOneShot(AudioType.ElectricShock);
			_audioPoolIndex++;
			if (_audioPoolIndex >= _shockAudioPool.Length)
			{
				_audioPoolIndex = 0;
			}
		}
		OWRigidbody attachedOWRigidbody = detector.GetAttachedOWRigidbody();
		if (!(attachedOWRigidbody != null))
		{
			return;
		}
		Vector3 normalized = (detector.transform.position - base.transform.position).normalized;
		if (!_shellKickback || !(_triggerVolume.GetPenetrationDistance(detector.transform.position) >= _shellThickness))
		{
			Vector3 vector = attachedOWRigidbody.GetVelocity() - _attachedBody.GetPointVelocity(detector.transform.position);
			attachedOWRigidbody.AddVelocityChange(-Vector3.Project(vector, normalized));
			attachedOWRigidbody.AddVelocityChange(normalized * _kickback);
		}
		if (!detector.CompareTag("PlayerDetector"))
		{
			attachedOWRigidbody.AddAngularVelocityChange(Random.onUnitSphere * 10f);
		}
		if (!detector.CompareTag("ShipDetector"))
		{
			return;
		}
		ShipDamageController component = attachedOWRigidbody.GetComponent<ShipDamageController>();
		if (component.IsElectricalFailed())
		{
			if (Time.timeSinceLevelLoad > s_shipElectroKnockoutTime + 3f)
			{
				component.TriggerReactorCritical();
			}
		}
		else
		{
			component.TriggerElectricalFailure();
			s_shipElectroKnockoutTime = Time.timeSinceLevelLoad;
		}
		attachedOWRigidbody.GetComponentInChildren<ShipCockpitController>().LockUpControls(3f);
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		HazardDetector detector = hitObj.GetComponent<HazardDetector>();
		if (detector != null)
		{
			detector.AddVolume(this);
			bool flag = detector.IsInsulated();
			if (!flag)
			{
				ApplyShock(detector);
			}
			if (!_trackedDetectors.Exists((TrackedHazardDetector x) => x.hazardDetector == detector))
			{
				TrackedHazardDetector trackedHazardDetector = new TrackedHazardDetector();
				trackedHazardDetector.hazardDetector = detector;
				trackedHazardDetector.lastShockTime = Time.timeSinceLevelLoad;
				trackedHazardDetector.wasInsulated = flag;
				_trackedDetectors.Add(trackedHazardDetector);
			}
			base.enabled = true;
		}
	}

	protected override void OnEffectVolumeExit(GameObject hitObj)
	{
		base.OnEffectVolumeExit(hitObj);
		HazardDetector detector = hitObj.GetComponent<HazardDetector>();
		_trackedDetectors.RemoveAll((TrackedHazardDetector x) => x.hazardDetector == detector);
	}

	public override HazardType GetHazardType()
	{
		return HazardType.ELECTRICITY;
	}
}
