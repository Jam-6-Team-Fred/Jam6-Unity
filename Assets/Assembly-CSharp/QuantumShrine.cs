using UnityEngine;

public class QuantumShrine : SocketedQuantumObject
{
	[SerializeField]
	private Light _ambientLight;

	[SerializeField]
	private Light[] _lamps;

	[SerializeField]
	private FogOverrideVolume _fogOverride;

	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private NomaiGateway _gate;

	[SerializeField]
	private MeshRenderer _floorRenderer;

	[SerializeField]
	private Material[] _floorMaterials;

	[SerializeField]
	private QuantumSocket _northSocket;

	[SerializeField]
	private GameObject _eyeStateDoorPlug;

	[SerializeField]
	private OWRenderer _exteriorRenderer;

	[SerializeField]
	private NomaiInterfaceOrb[] _childOrbs;

	[SerializeField]
	private OWLightController _exteriorLightController;

	private float _fadeFraction = 1f;

	private bool _isPlayerInside;

	private bool _isProbeInside;

	private float _origAmbientIntensity;

	private Color _origFogTint;

	private bool _fading;

	private bool _orbsLocked;

	protected override void Awake()
	{
		base.Awake();
		_origAmbientIntensity = _ambientLight.intensity;
		_origFogTint = _fogOverride.tint;
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		_eyeStateDoorPlug.SetActive(value: false);
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
	}

	public bool IsPlayerInside()
	{
		return _isPlayerInside;
	}

	public bool IsPlayerInDarkness()
	{
		for (int i = 0; i < _lamps.Length; i++)
		{
			if (_lamps[i].intensity > 0f)
			{
				return false;
			}
		}
		if (_isPlayerInside && _fadeFraction == 0f && !_isProbeInside && !PlayerState.IsFlashlightOn())
		{
			return Locator.GetThrusterLightTracker().GetLightRange() <= 0f;
		}
		return false;
	}

	public void OnQuantumMoonSurfaceStateChanged(int moonState)
	{
		_exteriorRenderer.SetActivation(moonState != -1);
		if (!_orbsLocked && moonState == -1)
		{
			_orbsLocked = true;
			for (int i = 0; i < _childOrbs.Length; i++)
			{
				_childOrbs[i].AddLock();
			}
		}
		else if (_orbsLocked && moonState != -1)
		{
			_orbsLocked = false;
			for (int j = 0; j < _childOrbs.Length; j++)
			{
				_childOrbs[j].RemoveLock();
			}
		}
		if (moonState != -1)
		{
			_floorRenderer.material = _floorMaterials[moonState];
			_eyeStateDoorPlug.SetActive(moonState == 5 && _occupiedSocket != null && !_occupiedSocket.Equals(_northSocket));
		}
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		bool num = base.ChangeQuantumState(skipInstantVisibilityCheck);
		if (num)
		{
			float value = OWMath.Angle(to: Vector3.ProjectOnPlane(Locator.GetPlayerTransform().position - base.transform.position, base.transform.up), from: base.transform.forward, axis: base.transform.up);
			value = OWMath.RoundToNearestMultiple(value, 120f);
			base.transform.rotation = Quaternion.AngleAxis(value, base.transform.up) * base.transform.rotation;
		}
		return num;
	}

	protected override void Update()
	{
		base.Update();
		if (_fading)
		{
			float target = (_isPlayerInside ? (_gate.GetOpenFraction() * 0.7f) : 1f);
			_fadeFraction = Mathf.MoveTowards(_fadeFraction, target, Time.deltaTime * 0.5f);
			_ambientLight.intensity = _origAmbientIntensity * _fadeFraction;
			_fogOverride.tint = Color.Lerp(Color.black, _origFogTint, _fadeFraction);
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = true;
			_fading = true;
			_exteriorLightController.FadeTo(0f, 1f);
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_isProbeInside = true;
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = false;
			_fading = true;
			_exteriorLightController.FadeTo(1f, 1f);
		}
		else if (hitObj.CompareTag("ProbeDetector"))
		{
			_isProbeInside = false;
		}
	}
}
