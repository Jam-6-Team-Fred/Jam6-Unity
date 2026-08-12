using UnityEngine;

public class DistantFireController : MonoBehaviour
{
	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private OWLightController _lightController;

	[SerializeField]
	private GameObject[] _objectsToDeactivate;

	private void Awake()
	{
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
	}

	private void OnDetectLight()
	{
		_lightController.FadeTo(0f, 0.5f);
		base.enabled = true;
	}

	private void Update()
	{
		if (_lightController.GetIntensity() < 0.01f)
		{
			for (int i = 0; i < _objectsToDeactivate.Length; i++)
			{
				_objectsToDeactivate[i].SetActive(value: false);
			}
			base.enabled = false;
		}
	}
}
