using UnityEngine;

public class LightFlickerController : MonoBehaviour
{
	[SerializeField]
	private Renderer _bubbleRenderer;

	[Space]
	[SerializeField]
	private OWLight2[] _lights;

	[SerializeField]
	private GameObject _lightsRoot;

	[Space]
	[SerializeField]
	private OWEmissiveRenderer[] _renderers;

	[SerializeField]
	private GameObject _renderersRoot;

	private float _flickerScale = 1f;

	private float _flickerDuration;

	private float _flickerDelta;

	private float _startFlickerScale;

	private float _targetFlickerScale;

	private float _flickerStartTime;

	private float _nextFlickerTime;

	private float _minFlickerDelta;

	private float _maxFlickerDelta;

	private float _minFlickerInterval;

	private float _maxFlickerInterval;

	private bool _waitToFlickerOut;

	private float _flickerOnDuration;

	private void OnValidate()
	{
		if (_lightsRoot != null && _lights.Length != 0)
		{
			_lights = new OWLight2[0];
		}
		if (_renderersRoot != null && _renderers.Length != 0)
		{
			_renderers = new OWEmissiveRenderer[0];
		}
	}

	private void Awake()
	{
		if (_lightsRoot != null)
		{
			_lights = _lightsRoot.GetComponentsInChildren<OWLight2>();
		}
		if (_renderersRoot != null)
		{
			_renderers = _renderersRoot.GetComponentsInChildren<OWEmissiveRenderer>();
		}
		GlobalMessenger<float, float>.AddListener("FlickerOffAndOn", FlickerOffAndOn);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		GlobalMessenger<float, float>.RemoveListener("FlickerOffAndOn", FlickerOffAndOn);
	}

	public bool IsFlickering()
	{
		return base.enabled;
	}

	public void FlickerOffAndOn(float offDuration, float onDuration)
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (offDuration > 0f)
			{
				Flicker(0f, offDuration, 0f, 0.5f, 5f, 10f);
				_waitToFlickerOut = true;
				_flickerOnDuration = onDuration;
			}
			else
			{
				_flickerScale = 0f;
				UpdateLightsAndRenderers();
				Flicker(1f, onDuration, 0f, 0.5f, 5f, 10f);
			}
			base.enabled = true;
		}
	}

	private void Flicker(float scale, float duration, float minDelta, float maxDelta, float minInterval, float maxInterval)
	{
		if (duration == 0f)
		{
			_flickerScale = scale;
			UpdateLightsAndRenderers();
			return;
		}
		_startFlickerScale = _flickerScale;
		_targetFlickerScale = scale;
		_flickerDuration = duration;
		_minFlickerDelta = minDelta;
		_maxFlickerDelta = maxDelta;
		_minFlickerInterval = minInterval;
		_maxFlickerInterval = maxInterval;
		_flickerStartTime = (_nextFlickerTime = Time.time);
		base.enabled = true;
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(_flickerStartTime, _flickerStartTime + _flickerDuration, Time.time);
		_flickerScale = Mathf.Lerp(_startFlickerScale, _targetFlickerScale, num) - _flickerDelta;
		if (Time.time > _nextFlickerTime)
		{
			_flickerDelta = Random.Range(_minFlickerDelta, _maxFlickerDelta);
			_nextFlickerTime = Time.time + Random.Range(_minFlickerInterval, _maxFlickerInterval) * Time.deltaTime;
		}
		if (num >= 1f)
		{
			_flickerScale = _targetFlickerScale;
			if (_waitToFlickerOut && _flickerScale <= 0f)
			{
				Flicker(1f, _flickerOnDuration, 0f, 0.5f, 5f, 10f);
				_waitToFlickerOut = false;
			}
			else
			{
				base.enabled = false;
			}
		}
		UpdateLightsAndRenderers();
	}

	private void UpdateLightsAndRenderers()
	{
		if (_bubbleRenderer != null)
		{
			_bubbleRenderer.material.SetAlpha(1f - _flickerScale);
			_bubbleRenderer.enabled = _flickerScale < 1f;
		}
		for (int i = 0; i < _lights.Length; i++)
		{
			_lights[i].SetFlickerScale(_flickerScale);
		}
		for (int j = 0; j < _renderers.Length; j++)
		{
			_renderers[j].SetFlickerScale(_flickerScale);
		}
	}
}
