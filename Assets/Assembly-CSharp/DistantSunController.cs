using UnityEngine;

public class DistantSunController : MonoBehaviour
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	private Transform _model;

	[SerializeField]
	private Transform _playerArrivalPos;

	private bool _updateSupernova;

	private float _supernovaStartTime;

	private float _origIntensity;

	private float _origScale;

	private void Start()
	{
		_origScale = _model.localScale.x;
		_origIntensity = _light.intensity;
		if (Locator.GetEyeStateManager().GetState() != 0)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
		}
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("TriggerSupernova", OnTriggerSupernova);
	}

	private void Update()
	{
		if (_updateSupernova)
		{
			float num = Mathf.Clamp01((Time.time - _supernovaStartTime) / 30f);
			float num2 = 0f - Mathf.Pow(2f * num - 1f, 2f) + 1f;
			_light.intensity = _origIntensity + num2 * 2f;
			_model.localScale = Vector3.one * _origScale * num2 * 300f;
			if (num >= 1f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private void OnTriggerSupernova()
	{
		_updateSupernova = true;
		_supernovaStartTime = Time.time;
	}
}
