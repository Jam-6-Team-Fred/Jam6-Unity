using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LightSensorEffects : MonoBehaviour
{
	[SerializeField]
	private Material _glowMaterial;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private bool _useNewLensEffect = true;

	[SerializeField]
	private float _fadeInLength = 0.0625f;

	[SerializeField]
	private float _fadeOutLength = 0.125f;

	[SerializeField]
	private AudioLoopCrossfader _audioLoopCrossfader;

	private LightSensor _lightSensor;

	private Material _origMaterial;

	private OWRenderer _lightRenderer;

	private float _lightT;

	private bool _isLit;

	private readonly int _propID_LightIntensity = Shader.PropertyToID("_LightIntensity");

	private void Awake()
	{
		_origMaterial = _renderer.sharedMaterial;
		_lightRenderer = _renderer.gameObject.GetAddComponent<OWRenderer>();
		_lightSensor = GetComponent<LightSensor>();
		_lightSensor.OnDetectLight += new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness += new OWEvent.OWCallback(OnDetectDarkness);
		_lightRenderer.SetMaterialProperty(_propID_LightIntensity, 0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_lightSensor.OnDetectLight -= new OWEvent.OWCallback(OnDetectLight);
		_lightSensor.OnDetectDarkness -= new OWEvent.OWCallback(OnDetectDarkness);
	}

	private void Update()
	{
		float num = (_isLit ? 1f : 0f);
		float num2 = (_isLit ? _fadeInLength : _fadeOutLength);
		_lightT = Mathf.MoveTowards(_lightT, num, 1f / num2 * Time.deltaTime);
		if (OWMath.ApproxEquals(_lightT, num))
		{
			_lightT = num;
			base.enabled = false;
		}
		_lightRenderer.SetMaterialProperty(_propID_LightIntensity, _lightT * _lightT);
	}

	private void OnDetectLight()
	{
		if (_useNewLensEffect)
		{
			_isLit = true;
			base.enabled = true;
		}
		else
		{
			_renderer.sharedMaterial = _glowMaterial;
		}
		if (_audioLoopCrossfader != null)
		{
			_audioLoopCrossfader.Play();
		}
	}

	private void OnDetectDarkness()
	{
		if (_useNewLensEffect)
		{
			_isLit = false;
			base.enabled = true;
		}
		else
		{
			_renderer.sharedMaterial = _origMaterial;
		}
		if (_audioLoopCrossfader != null)
		{
			_audioLoopCrossfader.Stop();
		}
	}
}
