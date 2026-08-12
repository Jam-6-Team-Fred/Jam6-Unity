using UnityEngine;

public class GiantsDeepSunOverrideVolume : SectoredMonoBehaviour, SunLightController.ISunOverrider
{
	private Transform _sunTransform;

	[SerializeField]
	private int _priority = -1;

	[SerializeField]
	private float _cloudsOuterRadius = 950f;

	[SerializeField]
	private float _cloudsInnerRadius = 850f;

	[SerializeField]
	private float _innerIntensity = 0.75f;

	[SerializeField]
	private float _waterOuterRadius = 500f;

	[SerializeField]
	private float _waterInnerRadius = 400f;

	private void OnEnable()
	{
		SunLightController.RegisterSunOverrider(this, _priority);
	}

	private void OnDisable()
	{
		SunLightController.UnregisterSunOverrider(this);
	}

	protected override void OnSectorOccupantsUpdated()
	{
		base.enabled = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
	}

	public SunLightController.SunOverrideSettings ApplySunOverrides(OWCamera owCamera, SunLightController.SunOverrideSettings settings)
	{
		Vector3 vector = owCamera.transform.position - base.transform.position;
		if (vector.sqrMagnitude <= _cloudsOuterRadius * _cloudsOuterRadius)
		{
			float magnitude = vector.magnitude;
			Vector3 lhs = vector / magnitude;
			Vector3 normalized = (Locator.GetSunTransform().position - base.transform.position).normalized;
			float t = Mathf.Clamp01((magnitude - _cloudsInnerRadius) / (_cloudsOuterRadius - _cloudsInnerRadius));
			float num = Mathf.Clamp01((magnitude - _waterInnerRadius) / (_waterOuterRadius - _waterInnerRadius));
			float num2 = Mathf.Max(0f, Vector3.Dot(lhs, normalized));
			settings.sunIntensity = settings.sunIntensity * Mathf.Lerp(_innerIntensity, 1f, t) * num * num2;
		}
		return settings;
	}

	private void OnDrawGizmosSelected()
	{
		if (OWGizmos.IsDirectlySelected(base.gameObject))
		{
			Gizmos.color = new Color(0.5f, 1f, 0.5f);
			Gizmos.DrawWireSphere(base.transform.position, _cloudsOuterRadius);
			Gizmos.color = new Color(0f, 1f, 0f);
			Gizmos.DrawWireSphere(base.transform.position, _cloudsInnerRadius);
			Gizmos.color = new Color(0.5f, 0.5f, 1f);
			Gizmos.DrawWireSphere(base.transform.position, _waterOuterRadius);
			Gizmos.color = new Color(0f, 0f, 1f);
			Gizmos.DrawWireSphere(base.transform.position, _waterInnerRadius);
		}
	}
}
