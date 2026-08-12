using UnityEngine;

[ExecuteInEditMode]
public class RingworldSunLightParamUpdater : MonoBehaviour
{
	private int _propID_OWRingworldSunPositionRange = Shader.PropertyToID("_OWRingworldSunPositionRange");

	private Light _sunLight;

	private void LateUpdate()
	{
		if (_sunLight == null)
		{
			_sunLight = GetComponent<Light>();
		}
		if ((bool)_sunLight)
		{
			Vector3 position = base.transform.position;
			float range = _sunLight.range;
			Shader.SetGlobalVector(_propID_OWRingworldSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
		}
	}
}
