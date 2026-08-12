using UnityEngine;

public class DreamLamp : MonoBehaviour
{
	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private Transform _ghostTransform;

	private float _litTime;

	private void Awake()
	{
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void Start()
	{
		_lightController.SetIntensity(0f);
	}

	private void OnDestroy()
	{
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void Update()
	{
		if (!_lightSensor.IsIlluminated() && Time.time > _litTime + 1f)
		{
			base.enabled = false;
			_lightController.FadeTo(0f, 10f);
		}
	}

	private void OnDetectLight()
	{
		_litTime = Time.time;
		_lightController.FadeTo(1f, 1f);
	}

	private void OnDetectDarkness()
	{
	}
}
