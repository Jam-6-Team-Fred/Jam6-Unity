using UnityEngine;

public class PlayerIlluminationFeedback : MonoBehaviour
{
	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private OWLightController _lightController;

	private bool _illuminatedByLantern;

	private void Awake()
	{
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Start()
	{
		_lightController.SetIntensity(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Update()
	{
		bool illuminatedByLantern = _illuminatedByLantern;
		_illuminatedByLantern = _lightSensor.IsIlluminatedByGhostLantern();
		if (!illuminatedByLantern && _illuminatedByLantern)
		{
			_lightController.FadeTo(1f, 0.5f);
		}
		else if (illuminatedByLantern && !_illuminatedByLantern)
		{
			_lightController.FadeTo(0f, 0.5f);
		}
	}

	private void OnDetectLight()
	{
		base.enabled = true;
	}

	private void OnDetectDarkness()
	{
		base.enabled = false;
		_illuminatedByLantern = false;
		_lightController.FadeTo(0f, 0.5f);
	}
}
