using UnityEngine;

[RequireComponent(typeof(Light))]
public class NightLight : SectoredMonoBehaviour
{
	private DayNightTracker _dayNightTracker;

	private OWLight2 _light;

	[SerializeField]
	private OWRenderer[] _emissiveRenderers;

	[Space(10f)]
	[SerializeField]
	private float _dayLightIntensityMultiplier = 0.5f;

	[SerializeField]
	private float _dayEmissionIntensityMultiplier = 0.5f;

	[Space(10f)]
	[SerializeField]
	private float _fadeLength = 5f;

	private bool _activeInSector;

	private bool _fading;

	private float _nightLightIntensity;

	private Color _nightEmissionColor;

	private float _startLightIntensity;

	private float _targetLightIntensity;

	private Color _startEmissionColor;

	private Color _currentEmissionColor;

	private Color _targetEmissionColor;

	private float _fadeStartTime;

	protected override void Awake()
	{
		base.Awake();
		_light = base.gameObject.GetAddComponent<OWLight2>();
		_nightLightIntensity = _light.GetIntensity();
		for (int i = 0; i < _emissiveRenderers.Length; i++)
		{
			if (_emissiveRenderers[i] != null)
			{
				_nightEmissionColor = _emissiveRenderers[i].GetOriginalEmissionColor();
				_currentEmissionColor = _nightEmissionColor;
				break;
			}
		}
		_activeInSector = false;
		_fading = false;
		base.enabled = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_dayNightTracker != null)
		{
			_dayNightTracker.OnSunrise -= OnSunrise;
			_dayNightTracker.OnSunset -= OnSunset;
		}
	}

	public void SetDayNightTracker(DayNightTracker dayNightTracker)
	{
		if (_dayNightTracker == null)
		{
			_dayNightTracker = dayNightTracker;
			_dayNightTracker.OnSunrise += OnSunrise;
			_dayNightTracker.OnSunset += OnSunset;
		}
		else
		{
			Debug.LogError("We've already set up a day-night tracker!");
			Debug.Break();
		}
	}

	protected override void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (flag && !_activeInSector && _fading)
		{
			base.enabled = true;
			Update();
		}
		_activeInSector = flag;
	}

	private void Update()
	{
		float num = Mathf.Clamp01((Time.time - _fadeStartTime) / _fadeLength);
		_light.SetIntensity(Mathf.Lerp(_startLightIntensity, _targetLightIntensity, num));
		_currentEmissionColor = Color.Lerp(_startEmissionColor, _targetEmissionColor, num);
		for (int i = 0; i < _emissiveRenderers.Length; i++)
		{
			if (_emissiveRenderers[i] != null)
			{
				_emissiveRenderers[i].SetEmissionColor(_currentEmissionColor);
			}
		}
		if (num >= 1f)
		{
			base.enabled = false;
			_fading = false;
		}
	}

	private void StartFade()
	{
		_startLightIntensity = _light.GetIntensity();
		_startEmissionColor = _currentEmissionColor;
		_fadeStartTime = Time.time;
		_fading = true;
		if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			base.enabled = true;
		}
	}

	private void OnSunrise()
	{
		_targetLightIntensity = _nightLightIntensity * _dayLightIntensityMultiplier;
		_targetEmissionColor = _nightEmissionColor * _dayEmissionIntensityMultiplier;
		StartFade();
	}

	private void OnSunset()
	{
		_targetLightIntensity = _nightLightIntensity;
		_targetEmissionColor = _nightEmissionColor;
		StartFade();
	}
}
