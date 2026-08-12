using System.Collections.Generic;
using UnityEngine;

public class DarkMatterVolume : HazardVolume
{
	private class TrackedHazardDetector
	{
		public HazardDetector hazardDetector;

		public Vector3 prevLocalPosition;

		public float prevTimeInVolume;

		public float prevDistanceTraveled;
	}

	[Space]
	[SerializeField]
	protected ParticleSystem _particleTrail;

	[SerializeField]
	protected float _rateOverTime = 5f;

	[SerializeField]
	protected float _rateOverDistance = 3f;

	private List<TrackedHazardDetector> _trackedDetectors;

	protected override void Awake()
	{
		base.Awake();
		_trackedDetectors = new List<TrackedHazardDetector>(4);
	}

	private void LateUpdate()
	{
		if (_particleTrail == null || _trackedDetectors.Count == 0)
		{
			base.enabled = false;
			return;
		}
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		emitParams.applyShapeToPosition = true;
		for (int i = 0; i < _trackedDetectors.Count; i++)
		{
			Vector3 position = _trackedDetectors[i].hazardDetector.transform.position;
			Vector3 vector = _particleTrail.transform.InverseTransformPoint(position);
			float num = _trackedDetectors[i].prevTimeInVolume + Time.deltaTime;
			float num2 = _trackedDetectors[i].prevDistanceTraveled + Vector3.Distance(_trackedDetectors[i].prevLocalPosition, vector);
			int num3 = Mathf.FloorToInt(num * _rateOverTime) - Mathf.FloorToInt(_trackedDetectors[i].prevTimeInVolume * _rateOverTime);
			num3 += Mathf.FloorToInt(num2 * _rateOverDistance) - Mathf.FloorToInt(_trackedDetectors[i].prevDistanceTraveled * _rateOverDistance);
			for (int j = 0; j < num3; j++)
			{
				float t = (float)(j + 1) / (float)num3;
				emitParams.position = Vector3.Lerp(_trackedDetectors[i].prevLocalPosition, vector, t);
				_particleTrail.Emit(emitParams, 1);
			}
			_trackedDetectors[i].prevLocalPosition = vector;
			_trackedDetectors[i].prevTimeInVolume = num;
			_trackedDetectors[i].prevDistanceTraveled = num2;
		}
	}

	protected override void OnEffectVolumeEnter(GameObject hitObj)
	{
		HazardDetector detector = hitObj.GetComponent<HazardDetector>();
		if (!(detector != null))
		{
			return;
		}
		detector.AddVolume(this);
		if (!detector.CompareTag("ShipDetector"))
		{
			if (!_trackedDetectors.Exists((TrackedHazardDetector x) => x.hazardDetector == detector))
			{
				TrackedHazardDetector trackedHazardDetector = new TrackedHazardDetector();
				trackedHazardDetector.hazardDetector = detector;
				trackedHazardDetector.prevLocalPosition = ((_particleTrail != null) ? _particleTrail.transform.InverseTransformPoint(detector.transform.position) : Vector3.zero);
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
		return HazardType.DARKMATTER;
	}
}
