using UnityEngine;

public class LightDarkMask : MonoBehaviour
{
	[SerializeField]
	private OldDreamCandle _linkedCandle;

	[SerializeField]
	private LightSensor _lightSensor;

	private bool _awake;

	private void Awake()
	{
		_linkedCandle.OnOldDreamCandleLit += new OWEvent.OWCallback(OnOldDreamCandleLit);
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
	}

	private void Start()
	{
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_linkedCandle.OnOldDreamCandleLit -= new OWEvent.OWCallback(OnOldDreamCandleLit);
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
	}

	private void FixedUpdate()
	{
		if (!_lightSensor.IsIlluminated())
		{
			base.enabled = false;
		}
		Vector3 target = (base.transform.position - Locator.GetPlayerTransform().position).normalized * 20f;
		Vector3 velocity = Locator.GetPlayerBody().GetVelocity();
		Vector3 vector = Vector3.MoveTowards(velocity, target, 10f * Time.deltaTime);
		Locator.GetPlayerBody().AddVelocityChange(vector - velocity);
	}

	private void OnOldDreamCandleLit()
	{
		_awake = true;
		if (_lightSensor.IsIlluminated())
		{
			OnDetectLight();
		}
	}

	private void OnDetectLight()
	{
		if (_awake)
		{
			base.enabled = true;
		}
	}
}
