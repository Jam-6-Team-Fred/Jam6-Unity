using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(WaveHeightCalculator))]
public class OWOceanCollider : OWCustomCollider
{
	[SerializeField]
	private float _wavelessRadius = 500f;

	private WaveHeightCalculator _waveHeightCalculator;

	protected override void Awake()
	{
		base.Awake();
		_waveHeightCalculator = GetComponent<WaveHeightCalculator>();
	}

	public void SetWavelessRadius(float radius)
	{
		_wavelessRadius = radius;
	}

	public override float GetDistToSurface(Vector3 worldPoint)
	{
		return Mathf.Max(Vector3.Distance(worldPoint, base.transform.position) - _wavelessRadius, 0f);
	}

	public override bool IsPointInCollider(Vector3 worldPoint)
	{
		return Vector3.Distance(base.transform.position, worldPoint) < _wavelessRadius;
	}

	protected override bool IsTrackerInCollider(TrackedTransform tracker)
	{
		bool flag = true;
		Vector3 vector = Vector3.zero;
		if (tracker.fluidDetector != null)
		{
			vector = (tracker.transform.position - base.transform.position).normalized * tracker.fluidDetector.GetBuoyancyData().boundingRadius;
			flag = tracker.fluidDetector.GetBuoyancyData().checkAgainstWaves;
		}
		if (flag)
		{
			return _waveHeightCalculator.IsPointBelowWaves(tracker.transform.position - vector);
		}
		return IsPointInCollider(tracker.transform.position - vector);
	}
}
