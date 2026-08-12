using UnityEngine;

public class Minimap : MonoBehaviour
{
	private enum MinimapMode
	{
		Player = 0,
		Ship = 1
	}

	[SerializeField]
	private MinimapMode _minimapMode;

	[SerializeField]
	private Transform _playerMarkerTransform;

	[SerializeField]
	private Transform _shipMarkerTransform;

	[SerializeField]
	private Transform _probeMarkerTransform;

	[SerializeField]
	private ParticleSystem _playerTrailEmitter;

	[SerializeField]
	private ParticleSystem _probeTrailEmitter;

	[SerializeField]
	private Transform _globeMeshTransform;

	[SerializeField]
	private ProbeLauncher _attachedProbeLauncher;

	[SerializeField]
	private Renderer[] _minimapRenderersToSwitchOnOff;

	[SerializeField]
	private ElectricalComponent[] _electricalComponentsToSwitchOnOff;

	private const int TRAILMARKER_COUNT = 100;

	private const float c_maxInclinationPercentage = 0.8f;

	private ParticleSystem.Particle[] _trailParticlesArray;

	private RulesetDetector _playerRulesetDetector;

	private RulesetDetector _shipRulesetDetector;

	private RulesetDetector _probeRulesetDetector;

	private Transform _playerTransform;

	private Transform _shipTransform;

	private SurveyorProbe _activeProbe;

	private int _playerTrailIndex;

	private Vector3 _lastPlayerTrailPos = Vector3.one;

	private ParticleSystemRenderer _playerTrailRenderer;

	private int _probeTrailIndex;

	private Vector3 _lastProbeTrailPos = Vector3.one;

	private ParticleSystemRenderer _probeTrailRenderer;

	private bool _updateMinimap;

	private void Awake()
	{
		_trailParticlesArray = new ParticleSystem.Particle[Mathf.Max(_playerTrailEmitter.main.maxParticles, _probeTrailEmitter.main.maxParticles)];
		_playerTrailRenderer = _playerTrailEmitter.GetComponent<ParticleSystemRenderer>();
		_probeTrailRenderer = _probeTrailEmitter.GetComponent<ParticleSystemRenderer>();
		HideMinimap();
		GlobalMessenger.AddListener("ShipSystemFailure", OnShipDestroyed);
		GlobalMessenger.AddListener("ShipDestroyed", OnShipDestroyed);
		if (_minimapMode == MinimapMode.Player)
		{
			GlobalMessenger.AddListener("SuitUp", OnPutOnSuit);
		}
	}

	private void Start()
	{
		if (_attachedProbeLauncher != null)
		{
			_attachedProbeLauncher.OnLaunchProbe += OnLaunchProbe;
		}
		_playerRulesetDetector = Locator.GetPlayerRulesetDetector();
		_playerTransform = Locator.GetPlayerTransform();
		_shipTransform = Locator.GetShipTransform();
		if (_shipTransform != null)
		{
			_shipRulesetDetector = Locator.GetShipDetector().GetComponent<RulesetDetector>();
		}
	}

	private void OnDestroy()
	{
		if (_activeProbe != null)
		{
			_activeProbe.OnRetrieveProbe -= OnRetrieveProbe;
		}
		if (_attachedProbeLauncher != null)
		{
			_attachedProbeLauncher.OnLaunchProbe -= OnLaunchProbe;
		}
		GlobalMessenger.RemoveListener("ShipSystemFailure", OnShipDestroyed);
		GlobalMessenger.RemoveListener("ShipDestroyed", OnShipDestroyed);
		if (_minimapMode == MinimapMode.Player)
		{
			GlobalMessenger.RemoveListener("SuitUp", OnPutOnSuit);
		}
	}

	private void ShowMinimap()
	{
		ResetTrails(emitOnReset: true);
		SetComponentsEnabled(value: true);
		_updateMinimap = true;
	}

	private void HideMinimap()
	{
		ResetTrails(emitOnReset: false);
		SetComponentsEnabled(value: false);
		_updateMinimap = false;
	}

	private void SetComponentsEnabled(bool value)
	{
		_playerTrailRenderer.enabled = value;
		_probeTrailRenderer.enabled = value;
		for (int i = 0; i < _minimapRenderersToSwitchOnOff.Length; i++)
		{
			_minimapRenderersToSwitchOnOff[i].enabled = value;
		}
		for (int j = 0; j < _electricalComponentsToSwitchOnOff.Length; j++)
		{
			_electricalComponentsToSwitchOnOff[j].SetPowered(value);
		}
	}

	private void Update()
	{
		bool flag = true;
		flag = ((_minimapMode != 0) ? (PlayerState.IsInsideShip() && _playerRulesetDetector.GetUseMinimap()) : (!PlayerState.IsInsideShip() && _playerRulesetDetector.GetUseMinimap() && !Locator.GetPlayerSuit().IsTrainingSuit()));
		if (flag && !_updateMinimap)
		{
			ShowMinimap();
		}
		else if (!flag && _updateMinimap)
		{
			HideMinimap();
		}
		if (_updateMinimap)
		{
			UpdateTrails();
			UpdateRotation();
			UpdateMarkers();
		}
	}

	private void UpdateMarkers()
	{
		_playerMarkerTransform.localPosition = GetLocalMapPosition(_playerTransform);
		_playerMarkerTransform.localRotation = GetLocalMapRotation(_playerTransform);
		if (_shipRulesetDetector != null && _shipRulesetDetector.GetPlanetoidRuleset() == _playerRulesetDetector.GetPlanetoidRuleset())
		{
			_shipMarkerTransform.localPosition = GetLocalMapPosition(_shipTransform);
			_shipMarkerTransform.LookAt(_globeMeshTransform, _globeMeshTransform.up);
		}
		else
		{
			_shipMarkerTransform.localPosition = Vector3.zero;
			_shipMarkerTransform.localRotation = Quaternion.identity;
		}
		if (_probeRulesetDetector != null && _probeRulesetDetector.GetPlanetoidRuleset() == _playerRulesetDetector.GetPlanetoidRuleset())
		{
			_probeMarkerTransform.localPosition = GetLocalMapPosition(_activeProbe.transform);
			_probeMarkerTransform.LookAt(_globeMeshTransform, _globeMeshTransform.up);
		}
		else
		{
			_probeMarkerTransform.localPosition = Vector3.zero;
			_probeMarkerTransform.localRotation = Quaternion.identity;
		}
	}

	private void UpdateRotation()
	{
		Vector3 normalized = _playerMarkerTransform.localPosition.normalized;
		float num = Mathf.Atan2(0f - normalized.x, normalized.z);
		float num2 = Mathf.Asin(normalized.y) * 0.8f;
		Quaternion quaternion = Quaternion.AngleAxis(num * 57.29578f, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(num2 * 57.29578f, Vector3.right);
		base.transform.localRotation = quaternion2 * quaternion;
	}

	private void UpdateTrails()
	{
		if (((PlayerState.IsInsideShip() && _minimapMode == MinimapMode.Ship) || (!PlayerState.IsInsideShip() && _minimapMode == MinimapMode.Player)) && Vector3.Angle(_playerMarkerTransform.localPosition, _lastPlayerTrailPos) > 5f)
		{
			_lastPlayerTrailPos = _playerMarkerTransform.localPosition;
			_playerTrailIndex++;
			if (_playerTrailIndex >= _playerTrailEmitter.particleCount)
			{
				_playerTrailIndex = 0;
			}
			if (_playerTrailEmitter.particleCount == 0)
			{
				_playerTrailEmitter.Clear();
				_playerTrailEmitter.Emit(100);
			}
			int particles = _playerTrailEmitter.GetParticles(_trailParticlesArray);
			Vector3 localPosition = _playerMarkerTransform.localPosition;
			localPosition.Scale(base.transform.localScale);
			_trailParticlesArray[_playerTrailIndex].position = localPosition;
			_playerTrailEmitter.SetParticles(_trailParticlesArray, particles);
		}
		if (_probeRulesetDetector != null && Vector3.Angle(_probeMarkerTransform.localPosition, _lastProbeTrailPos) > 5f)
		{
			_lastProbeTrailPos = _probeMarkerTransform.localPosition;
			_probeTrailIndex++;
			if (_probeTrailIndex >= _probeTrailEmitter.particleCount)
			{
				_probeTrailIndex = 0;
			}
			if (_probeTrailEmitter.particleCount == 0)
			{
				_probeTrailEmitter.Clear();
				_probeTrailEmitter.Emit(100);
			}
			int particles2 = _probeTrailEmitter.GetParticles(_trailParticlesArray);
			Vector3 localPosition2 = _probeMarkerTransform.localPosition;
			localPosition2.Scale(base.transform.localScale);
			_trailParticlesArray[_probeTrailIndex].position = localPosition2;
			_probeTrailEmitter.SetParticles(_trailParticlesArray, particles2);
		}
	}

	private void ResetTrails(bool emitOnReset)
	{
		_playerTrailEmitter.Clear();
		_probeTrailEmitter.Clear();
		if (emitOnReset)
		{
			_playerTrailEmitter.Emit(100);
			_probeTrailEmitter.Emit(100);
		}
	}

	private Vector3 GetLocalMapPosition(Transform worldTransform)
	{
		return Vector3.Scale(_playerRulesetDetector.GetPlanetoidRuleset().transform.InverseTransformPoint(worldTransform.position).normalized * 0.51f, _globeMeshTransform.localScale);
	}

	private Quaternion GetLocalMapRotation(Transform worldTransform)
	{
		Transform transform = _playerRulesetDetector.GetPlanetoidRuleset().transform;
		Quaternion quaternion = Quaternion.Inverse(transform.rotation);
		Vector3 vector = quaternion * worldTransform.rotation * Vector3.forward;
		Vector3 vector2 = quaternion * (worldTransform.position - transform.position);
		return Quaternion.LookRotation(Vector3.ProjectOnPlane(vector, vector2), vector2);
	}

	private void OnLaunchProbe(SurveyorProbe probe)
	{
		_activeProbe = probe;
		_probeRulesetDetector = probe.GetDetectorObject().GetComponent<RulesetDetector>();
		_probeTrailEmitter.Clear();
		_probeTrailEmitter.Emit(100);
		_activeProbe.OnRetrieveProbe += OnRetrieveProbe;
	}

	private void OnRetrieveProbe()
	{
		_activeProbe.OnRetrieveProbe -= OnRetrieveProbe;
		_activeProbe = null;
		_probeRulesetDetector = null;
		_probeMarkerTransform.localPosition = Vector3.zero;
		_probeTrailEmitter.Clear();
		_probeTrailEmitter.Emit(100);
	}

	private void OnShipDestroyed()
	{
		_shipRulesetDetector = null;
	}

	private void OnPutOnSuit()
	{
		base.gameObject.SetActive(!Locator.GetPlayerSuit().IsTrainingSuit());
	}
}
