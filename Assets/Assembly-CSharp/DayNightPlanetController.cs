using UnityEngine;

public class DayNightPlanetController : MonoBehaviour
{
	public delegate void DayNightEvent();

	[SerializeField]
	private OWLight2 _ambientLight;

	[SerializeField]
	private PlanetaryFogController _planetaryFog;

	[SerializeField]
	private MeshRenderer _atmosphereRenderer;

	[SerializeField]
	private OWTriggerVolume _headsVolume;

	[SerializeField]
	private OWTriggerVolume _tailsVolume;

	private InitialMotion _initialMotion;

	private Material _atmosphereMaterial;

	private float _origFogDensity;

	private float _sunAngle;

	private bool _initialized;

	private bool _hasFiredDayEvent;

	private bool _sunAboveHeads;

	private bool _playerOnHeads;

	private bool _playerOnTails;

	public event DayNightEvent OnDayHeads;

	public event DayNightEvent OnDayTails;

	private void Awake()
	{
		_initialMotion = GetComponent<InitialMotion>();
		_atmosphereMaterial = _atmosphereRenderer.material;
		_origFogDensity = _planetaryFog.fogDensity;
		_headsVolume.OnEntry += OnEnterHeads;
		_headsVolume.OnExit += OnExitHeads;
		_tailsVolume.OnEntry += OnEnterTails;
		_tailsVolume.OnExit += OnExitTails;
	}

	private void OnDestroy()
	{
		_headsVolume.OnEntry -= OnEnterHeads;
		_headsVolume.OnExit -= OnExitHeads;
		_tailsVolume.OnEntry -= OnEnterTails;
		_tailsVolume.OnExit -= OnExitTails;
	}

	public float GetSunAngle()
	{
		if (!_initialized)
		{
			Initialize();
		}
		return _sunAngle;
	}

	public bool IsDay(bool heads)
	{
		if (!_initialized)
		{
			Initialize();
		}
		return _sunAboveHeads == heads;
	}

	public bool IsPointOnHeads(Vector3 worldPos)
	{
		return base.transform.InverseTransformPoint(worldPos).y > 0f;
	}

	public bool IsPointOnDaySide(Vector3 worldPos)
	{
		if (!(base.transform.InverseTransformPoint(worldPos).y > 0f) || !IsDay(heads: true))
		{
			if (base.transform.InverseTransformPoint(worldPos).y < 0f)
			{
				return IsDay(heads: false);
			}
			return false;
		}
		return true;
	}

	private void Initialize()
	{
		_initialized = true;
		UpdateSunAngle();
	}

	private void UpdateSunAngle()
	{
		Vector3 vector = Locator.GetSunTransform().position - base.transform.position;
		Vector3 normalized = _initialMotion.GetInitAngularVelocity().normalized;
		vector = Vector3.ProjectOnPlane(vector, normalized);
		_sunAngle = OWMath.Angle(base.transform.up, vector, normalized);
		_sunAboveHeads = Mathf.Abs(_sunAngle) < 90f;
	}

	private void FixedUpdate()
	{
		bool sunAboveHeads = _sunAboveHeads;
		UpdateSunAngle();
		if (sunAboveHeads != _sunAboveHeads || !_hasFiredDayEvent)
		{
			_hasFiredDayEvent = true;
			if (_sunAboveHeads && this.OnDayHeads != null)
			{
				this.OnDayHeads();
			}
			else if (!_sunAboveHeads && this.OnDayTails != null)
			{
				this.OnDayTails();
			}
		}
		float num = 0f;
		if ((_sunAboveHeads && _playerOnHeads) || (!_sunAboveHeads && _playerOnTails))
		{
			float value = (_sunAboveHeads ? (90f - Mathf.Abs(_sunAngle)) : (Mathf.Abs(_sunAngle) - 90f));
			num = Mathf.InverseLerp(0f, 15f, value);
			num *= num;
		}
		float intensityScale = Mathf.MoveTowards(_ambientLight.GetIntensityScale(), num, Time.deltaTime);
		_ambientLight.SetIntensityScale(intensityScale);
		float fogDensity = Mathf.MoveTowards(_planetaryFog.fogDensity, num * _origFogDensity, Time.deltaTime);
		_planetaryFog.fogDensity = fogDensity;
		float value2 = Mathf.MoveTowards(_atmosphereMaterial.GetFloat("_SunIntensity"), num, Time.deltaTime);
		_atmosphereMaterial.SetFloat("_SunIntensity", value2);
	}

	private void OnEnterHeads(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_playerOnHeads = true;
		}
	}

	private void OnExitHeads(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_playerOnHeads = false;
		}
	}

	private void OnEnterTails(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_playerOnTails = true;
		}
	}

	private void OnExitTails(GameObject hitObject)
	{
		if (hitObject.CompareTag("PlayerDetector"))
		{
			_playerOnTails = false;
		}
	}
}
