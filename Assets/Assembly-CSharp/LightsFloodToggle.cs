using UnityEngine;

public class LightsFloodToggle : MonoBehaviour
{
	[SerializeField]
	private RingRiverFloodSensor _floodSensor;

	[Space]
	[SerializeField]
	private OWLightController[] _lights;

	[SerializeField]
	private GameObject _lightsRoot;

	private void OnValidate()
	{
		if (_lightsRoot != null)
		{
			_lights = _lightsRoot.GetComponents<OWLightController>();
			_lightsRoot = null;
		}
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
		}
	}

	private void Awake()
	{
		if (_floodSensor != null)
		{
			_floodSensor.OnFloodImpact += new OWEvent.OWCallback(OnFloodImpact);
			return;
		}
		Debug.LogError("Lights flood toggle does not have a flood sensor!", this);
		Debug.Break();
	}

	private void OnDestroy()
	{
		_floodSensor.OnFloodImpact -= new OWEvent.OWCallback(OnFloodImpact);
	}

	private void OnFloodImpact()
	{
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].FadeTo(0f, 1f);
		}
	}
}
