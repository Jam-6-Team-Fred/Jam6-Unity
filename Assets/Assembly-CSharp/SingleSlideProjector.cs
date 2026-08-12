using UnityEngine;

public class SingleSlideProjector : MonoBehaviour
{
	[SerializeField]
	private LightSensor _lightSensor;

	[Space]
	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private OWRendererFadeController _lightShaftController;

	[SerializeField]
	private OWLightController _bounceLightController;

	[Space]
	[SerializeField]
	private OWCollider _shipLogTriggerCollider;

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
		if (_bounceLightController != null)
		{
			_bounceLightController.SetIntensity(0f);
		}
		_lightController.SetIntensity(0f);
		_lightShaftController.SetFade(0f);
		if (_shipLogTriggerCollider != null)
		{
			_shipLogTriggerCollider.SetActivation(active: false);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void FadeProjectorLightTo(float fade, float duration)
	{
		if (_bounceLightController != null)
		{
			_bounceLightController.FadeTo(fade, duration);
		}
		_lightController.FadeTo(fade, duration);
		_lightShaftController.FadeTo(fade, duration);
	}

	private void OnDetectLight()
	{
		FadeProjectorLightTo(1f, 0.2f);
		if (_shipLogTriggerCollider != null)
		{
			_shipLogTriggerCollider.SetActivation(active: true);
		}
	}

	private void OnDetectDarkness()
	{
		FadeProjectorLightTo(0f, 0.2f);
		if (_shipLogTriggerCollider != null)
		{
			_shipLogTriggerCollider.SetActivation(active: false);
		}
	}
}
