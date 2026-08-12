using UnityEngine;

public class PlanetoidRuleset : RulesetVolume
{
	[SerializeField]
	private float _daySideConeAngle = 180f;

	[SerializeField]
	private float _horizonRadius;

	[SerializeField]
	private bool _useMinimap = true;

	[Space]
	[SerializeField]
	private bool _useAltimeter = true;

	[SerializeField]
	private float _altitudeFloor = 200f;

	[SerializeField]
	private float _altitudeCeiling = 500f;

	[Space]
	[SerializeField]
	private float _shuttleLandingRadius;

	[Space]
	[SerializeField]
	private SandLevelController _sandLevelController;

	public GravityVolume GetGravityVolume()
	{
		return _attachedBody.GetAttachedGravityVolume();
	}

	public bool IsDayAtPosition(Vector3 worldPosition)
	{
		if (Locator.GetSunTransform() != null)
		{
			Vector3 from = Locator.GetSunTransform().position - base.transform.position;
			Vector3 to = worldPosition - base.transform.position;
			return Vector3.Angle(from, to) < _daySideConeAngle * 0.5f;
		}
		return false;
	}

	public float GetHorizonRadius()
	{
		if (_sandLevelController != null)
		{
			return Mathf.Max(_horizonRadius, _sandLevelController.GetRadius());
		}
		return _horizonRadius;
	}

	public float GetShuttleLandingRadius()
	{
		return _shuttleLandingRadius;
	}

	public bool GetUseMinimap()
	{
		return _useMinimap;
	}

	public bool GetUseAltimeter(Vector3 fromWorldPosition)
	{
		bool flag = (base.transform.position - fromWorldPosition).sqrMagnitude <= _altitudeCeiling * _altitudeCeiling;
		return _useAltimeter && flag;
	}

	public float GetAltitude(float distToCenter)
	{
		return distToCenter - _altitudeFloor;
	}

	public float GetNormalizedAltitude(float distToCenter)
	{
		return (distToCenter - _altitudeFloor) / (_altitudeCeiling - _altitudeFloor);
	}

	public float AltitudeToNormalizedAltitude(float altitude)
	{
		return altitude / (_altitudeCeiling - _altitudeFloor);
	}

	public float GetAltitudeFloor()
	{
		return _altitudeFloor;
	}

	public float GetAltitudeCeiling()
	{
		return _altitudeCeiling;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(base.transform.position, _horizonRadius);
			if (_useAltimeter)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(base.transform.position, _altitudeFloor);
				Gizmos.DrawWireSphere(base.transform.position, _altitudeCeiling);
			}
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(base.transform.position, _shuttleLandingRadius);
		}
	}
}
