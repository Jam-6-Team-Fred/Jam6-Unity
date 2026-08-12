using UnityEngine;

public class FlowerLightSwitch : MonoBehaviour
{
	[SerializeField]
	private Transform[] _petalTransforms;

	[SerializeField]
	private LightSensor _lightSensor;

	private bool _open = true;

	private float _degrees;

	private void Awake()
	{
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Update()
	{
		float num = (_open ? 0f : 180f);
		_degrees = Mathf.MoveTowards(_degrees, num, 180f * Time.deltaTime);
		if (OWMath.ApproxEquals(_degrees, num))
		{
			_degrees = num;
			base.enabled = false;
		}
		for (int i = 0; i < _petalTransforms.Length; i++)
		{
			Vector3 localEulerAngles = _petalTransforms[i].localEulerAngles;
			localEulerAngles.z = _degrees;
			_petalTransforms[i].localEulerAngles = localEulerAngles;
		}
	}

	private void OnDetectLight()
	{
		if (_open)
		{
			base.enabled = true;
			_open = false;
		}
	}

	private void OnDetectDarkness()
	{
	}
}
