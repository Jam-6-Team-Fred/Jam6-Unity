using System.Collections.Generic;
using UnityEngine;

public class ProbeDestructionDetector : Detector
{
	private SurveyorProbe _probe;

	private List<ProbeSafetyVolume> _safetyVolumes;

	protected override void Awake()
	{
		base.Awake();
		_probe = GetComponentInParent<SurveyorProbe>();
		_safetyVolumes = new List<ProbeSafetyVolume>(4);
		if (_probe == null)
		{
			Debug.LogError("Could not find SurveyorProbe in parents");
			Debug.Break();
		}
	}

	private void Start()
	{
		base.enabled = false;
	}

	public void AddSafetyVolume(ProbeSafetyVolume safetyVolume)
	{
		_safetyVolumes.SafeAdd(safetyVolume);
	}

	public void RemoveSafetyVolume(ProbeSafetyVolume safetyVolume)
	{
		_safetyVolumes.Remove(safetyVolume);
		if (_safetyVolumes.Count == 0 && _activeVolumes.Count > 0)
		{
			base.enabled = true;
		}
	}

	protected override void OnVolumeAdded(EffectVolume volume)
	{
		if (_safetyVolumes.Count == 0)
		{
			base.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		if (_activeVolumes.Count > 0 && _safetyVolumes.Count == 0)
		{
			DialogueConditionManager.SharedInstance.SetConditionState("PROBE_ENTERED_EYE", conditionState: true);
			Object.Destroy(_probe.gameObject);
			Debug.Log("PROBE DESTROYED (ENTERED THE EYE)");
		}
		base.enabled = false;
	}
}
