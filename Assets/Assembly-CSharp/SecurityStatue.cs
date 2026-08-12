using UnityEngine;

public class SecurityStatue : MonoBehaviour
{
	[SerializeField]
	private VisionSensor _visionSensor;

	[SerializeField]
	private LightSensor _lightSensor;

	[SerializeField]
	private Material _alertMaterial;

	[SerializeField]
	private MeshRenderer _alertRenderer;

	private Material _origMaterial;

	private float _alertTime;

	private void Awake()
	{
		_origMaterial = _alertRenderer.sharedMaterial;
		if (_visionSensor != null)
		{
			_visionSensor.OnDetectObject += OnDetectObject;
		}
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		}
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_visionSensor != null)
		{
			_visionSensor.OnDetectObject -= OnDetectObject;
		}
		if (_lightSensor != null)
		{
			_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
			_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
		}
	}

	private void Update()
	{
		if (Time.time > _alertTime + 5f)
		{
			_alertRenderer.sharedMaterial = _origMaterial;
			base.enabled = false;
		}
	}

	private void OnDetectObject(VisionDetector detector)
	{
		_alertRenderer.sharedMaterial = _alertMaterial;
		_alertTime = Time.time;
		base.enabled = true;
	}

	private void OnDetectLight()
	{
		_alertRenderer.sharedMaterial = _alertMaterial;
	}

	private void OnDetectDarkness()
	{
		if (!base.enabled)
		{
			_alertRenderer.sharedMaterial = _origMaterial;
		}
	}
}
