using System;
using UnityEngine;

public class ProbeHorizonTracker : MonoBehaviour
{
	private OWRigidbody _probeBody;

	private SurveyorProbe _probe;

	private PlanetoidRuleset _planetoidRuleset;

	private float _horizonOverride;

	private void Awake()
	{
		_probeBody = this.GetAttachedOWRigidbody();
		_probe = _probeBody.GetRequiredComponent<SurveyorProbe>();
		_probe.OnAnchorProbe += OnProbeAnchorOrRetrieve;
		_probe.OnRetrieveProbe += OnProbeAnchorOrRetrieve;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_probe.OnAnchorProbe -= OnProbeAnchorOrRetrieve;
		_probe.OnRetrieveProbe -= OnProbeAnchorOrRetrieve;
	}

	public void TrackHorizon(PlanetoidRuleset planetoidRuleset, float horizonOverride = 0f)
	{
		base.enabled = true;
		_planetoidRuleset = planetoidRuleset;
		_horizonOverride = horizonOverride;
	}

	private void FixedUpdate()
	{
		Vector3 lhs = _planetoidRuleset.transform.position - _probeBody.GetPosition();
		float magnitude = lhs.magnitude;
		float num = _planetoidRuleset.GetHorizonRadius();
		if (_horizonOverride != 0f && magnitude > _horizonOverride && magnitude < num)
		{
			num = _horizonOverride;
		}
		float num2 = magnitude - num;
		if (num2 <= 0f || num2 > 200f)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.FromToRotation(base.transform.forward, base.transform.parent.forward) * base.transform.rotation, 0.02f);
			return;
		}
		float num3 = Mathf.Asin(num / magnitude);
		float num4 = magnitude * Mathf.Cos(num3);
		Vector3 relativeVelocity = _planetoidRuleset.GetAttachedOWRigidbody().GetRelativeVelocity(_probeBody);
		Vector3 axis = Vector3.Cross(lhs, relativeVelocity);
		Vector3 toDirection = Quaternion.AngleAxis(num3 * 180f / (float)Math.PI, axis) * (lhs.normalized * num4);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.FromToRotation(base.transform.forward, toDirection) * base.transform.rotation, 0.05f);
	}

	private void OnProbeAnchorOrRetrieve()
	{
		base.transform.localRotation = Quaternion.identity;
		base.enabled = false;
	}
}
