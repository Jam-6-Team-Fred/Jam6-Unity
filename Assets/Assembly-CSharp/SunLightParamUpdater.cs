using UnityEngine;

[ExecuteInEditMode]
public class SunLightParamUpdater : MonoBehaviour
{
	private Light sunLight;

	private SunLightController _sunLightController;

	private SunController _sunController;

	private int _propID_SunPosition;

	private int _propID_OWSunPositionRange;

	private int _propID_OWSunColorIntensity;

	private void LateUpdate()
	{
		if (sunLight == null)
		{
			sunLight = GetComponent<Light>();
			_sunLightController = GetComponent<SunLightController>();
			_propID_SunPosition = Shader.PropertyToID("_SunPosition");
			_propID_OWSunPositionRange = Shader.PropertyToID("_OWSunPositionRange");
			_propID_OWSunColorIntensity = Shader.PropertyToID("_OWSunColorIntensity");
		}
		if (_sunController == null && Locator.GetSunTransform() != null)
		{
			_sunController = Locator.GetSunTransform().GetComponent<SunController>();
		}
		if ((bool)sunLight)
		{
			Vector3 position = base.transform.position;
			float w = 2000f;
			if (_sunController != null)
			{
				w = (_sunController.HasSupernovaStarted() ? _sunController.GetSupernovaRadius() : _sunController.GetSurfaceRadius());
			}
			float range = sunLight.range;
			Color color = ((_sunLightController != null) ? _sunLightController.sunColor : sunLight.color);
			float w2 = ((_sunLightController != null) ? _sunLightController.sunIntensity : sunLight.intensity);
			Shader.SetGlobalVector(_propID_SunPosition, new Vector4(position.x, position.y, position.z, w));
			Shader.SetGlobalVector(_propID_OWSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
			Shader.SetGlobalVector(_propID_OWSunColorIntensity, new Vector4(color.r, color.g, color.b, w2));
		}
	}
}
