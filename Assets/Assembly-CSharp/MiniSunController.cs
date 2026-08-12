using UnityEngine;

public class MiniSunController : MonoBehaviour
{
	[SerializeField]
	private GameObject _miniSunRoot;

	[SerializeField]
	private OWLight _sunLight;

	[SerializeField]
	private AudioSignal _telescopeSignal;

	[SerializeField]
	private AudioSignal _drumSignal;

	private Color _emissionColor;

	private Material _sunMaterial;

	private OWTriggerVolume _trigger;

	private bool _fading;

	private float _fadeValue;

	private float _fadeTarget;

	private float _startFadeValue;

	private float _startFadeTime;

	private void Awake()
	{
		_trigger = base.gameObject.GetRequiredComponent<OWTriggerVolume>();
		_trigger.OnEntry += OnEntry;
		_trigger.OnExit += OnExit;
		TessellatedSphereRenderer[] componentsInChildren = _miniSunRoot.GetComponentsInChildren<TessellatedSphereRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (_sunMaterial == null)
			{
				_sunMaterial = new Material(componentsInChildren[i].sharedMaterial);
				_emissionColor = _sunMaterial.GetColor("_Color");
			}
			componentsInChildren[i].sharedMaterial = _sunMaterial;
		}
		_sunMaterial.SetColor("_Color", _emissionColor * _fadeValue);
	}

	private void Start()
	{
		_sunLight.SetIntensity(_fadeValue);
	}

	private void OnDestroy()
	{
		_trigger.OnEntry -= OnEntry;
		_trigger.OnExit -= OnExit;
	}

	private void Update()
	{
		if (_fading)
		{
			float num = Mathf.InverseLerp(_startFadeTime, _startFadeTime + 2f, Time.time);
			_fadeValue = Mathf.Lerp(_startFadeValue, _fadeTarget, num);
			if (num >= 1f)
			{
				_fading = false;
				_fadeValue = _fadeTarget;
				_sunLight.SetIntensity(_fadeValue);
				_sunMaterial.SetColor("_Color", _emissionColor * _fadeValue);
			}
		}
		if (_fading)
		{
			_sunLight.SetIntensity(_fadeValue);
			_sunMaterial.SetColor("_Color", _emissionColor * _fadeValue);
		}
	}

	private void FadeTo(float targetFade)
	{
		_startFadeTime = Time.time;
		_startFadeValue = _fadeValue;
		_fadeTarget = targetFade;
		_fading = true;
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			FadeTo(1f);
			_telescopeSignal.SetSignalActivation(active: false);
			_drumSignal.SetSignalActivation(active: true);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			FadeTo(0f);
			_telescopeSignal.SetSignalActivation(active: true);
			_drumSignal.SetSignalActivation(active: false);
		}
	}
}
