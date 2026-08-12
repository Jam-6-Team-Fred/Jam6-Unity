using System.Collections.Generic;
using UnityEngine;

public class SingleLightSensor : LightSensor
{
	[SerializeField]
	private Sector _sector;

	[SerializeField]
	private bool _startIlluminated;

	[SerializeField]
	private bool _preserveStateWhileDisabled;

	[Space]
	[SerializeField]
	private bool _detectFlashlight;

	[SerializeField]
	private bool _detectProbe;

	[SerializeField]
	private bool _detectDreamLanterns;

	[SerializeField]
	private bool _detectSimpleLanterns;

	[SerializeField]
	private float _lanternFocusThreshold;

	[Space]
	[SerializeField]
	private bool _checkForOcclusion = true;

	[SerializeField]
	private float _maxDistance = float.PositiveInfinity;

	[SerializeField]
	private float _maxSimpleLanternDistance = float.PositiveInfinity;

	[SerializeField]
	private float _maxSpotHalfAngle = float.PositiveInfinity;

	[Space]
	[SerializeField]
	private bool _directionalSensor;

	[SerializeField]
	private Vector3 _localDirection = Vector3.forward;

	[SerializeField]
	private float _detectionAngle = 90f;

	[Space]
	[SerializeField]
	private float _sensorRadius;

	[SerializeField]
	private Vector3 _localSensorOffset;

	private int _fixedUpdateFrameDelayCount;

	private bool _illuminated;

	private List<DreamLanternController> _illuminatingDreamLanternList;

	private LightSourceDetector _lightDetector;

	private List<ILightSource> _lightSources;

	private LightSourceType _lightSourceMask;

	private static RaycastHit[] s_raycastHitBuffer = new RaycastHit[32];

	private void Reset()
	{
		_sector = GetComponentInParent<Sector>();
	}

	private void Awake()
	{
		if (_sector == null)
		{
			_sector = GetComponentInParent<Sector>();
		}
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated += new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		_lightDetector = GetComponentInChildren<LightSourceDetector>();
		if (_detectDreamLanterns)
		{
			_illuminatingDreamLanternList = new List<DreamLanternController>(16);
		}
	}

	private void Start()
	{
		if (_lightDetector != null)
		{
			_lightSources = new List<ILightSource>();
			_lightSourceMask = LightSourceType.VOLUME_ONLY;
			if (_detectFlashlight)
			{
				_lightSourceMask |= LightSourceType.FLASHLIGHT;
			}
			if (_detectProbe)
			{
				_lightSourceMask |= LightSourceType.PROBE;
			}
			if (_detectDreamLanterns)
			{
				_lightSourceMask |= LightSourceType.DREAM_LANTERN;
			}
			if (_detectSimpleLanterns)
			{
				_lightSourceMask |= LightSourceType.SIMPLE_LANTERN;
			}
			_lightDetector.OnLightVolumeEnter += OnLightSourceEnter;
			_lightDetector.OnLightVolumeExit += OnLightSourceExit;
		}
		else
		{
			Debug.LogError("LightSensor has no LightSourceDetector", this);
		}
		if (_sector != null)
		{
			base.enabled = false;
			_lightDetector.GetShape().enabled = false;
			if (_startIlluminated)
			{
				_illuminated = true;
				OnDetectLight.Invoke();
			}
		}
	}

	private void OnEnable()
	{
		FixedEarlyUpdateManager.Register(this);
	}

	private void OnDisable()
	{
		FixedEarlyUpdateManager.Unregister(this);
	}

	private void OnDestroy()
	{
		if (_sector != null)
		{
			_sector.OnSectorOccupantsUpdated -= new OWEvent.OWCallback(OnSectorOccupantsUpdated);
		}
		if (_lightDetector != null)
		{
			_lightDetector.OnLightVolumeEnter -= OnLightSourceEnter;
			_lightDetector.OnLightVolumeExit -= OnLightSourceExit;
		}
	}

	public void SetDetectorActive(bool active)
	{
		if (!(_lightDetector == null))
		{
			if (_lightDetector.GetShape() != null)
			{
				_lightDetector.GetShape().SetActivation(active);
			}
			else
			{
				Debug.LogError("Light sensor detector activation only supported for shapes!!!");
			}
		}
	}

	private void OnSectorOccupantsUpdated()
	{
		bool flag = _sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe);
		if (flag && !base.enabled)
		{
			base.enabled = true;
			_lightDetector.GetShape().enabled = true;
			if (_preserveStateWhileDisabled)
			{
				_fixedUpdateFrameDelayCount = 10;
			}
		}
		else
		{
			if (flag || !base.enabled)
			{
				return;
			}
			base.enabled = false;
			_lightDetector.GetShape().enabled = false;
			if (!_preserveStateWhileDisabled)
			{
				if (_illuminated)
				{
					OnDetectDarkness.Invoke();
				}
				_illuminated = false;
			}
		}
	}

	private void OnLightSourceEnter(LightSourceVolume volume)
	{
		ILightSource lightSource = volume.GetLightSource();
		if (lightSource != null && _lightSources != null && !_lightSources.Contains(lightSource))
		{
			_lightSources.Add(lightSource);
		}
	}

	private void OnLightSourceExit(LightSourceVolume volume)
	{
		ILightSource lightSource = volume.GetLightSource();
		if (lightSource != null && _lightSources != null && _lightSources.Contains(lightSource))
		{
			_lightSources.Remove(lightSource);
		}
	}

	public override bool IsIlluminated()
	{
		return _illuminated;
	}

	public override bool IsIlluminatedByGhostLantern()
	{
		if (_illuminated && _illuminatingDreamLanternList.Count > 0)
		{
			for (int i = 0; i < _illuminatingDreamLanternList.Count; i++)
			{
				if (_illuminatingDreamLanternList[i] != Locator.GetDreamWorldController().GetPlayerLantern().GetLanternController())
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool IsIlluminatedByLantern(DreamLanternController lantern)
	{
		if (!_illuminated)
		{
			return false;
		}
		for (int i = 0; i < _illuminatingDreamLanternList.Count; i++)
		{
			if (_illuminatingDreamLanternList[i] == lantern)
			{
				return true;
			}
		}
		return false;
	}

	public void ManagedFixedUpdate()
	{
		if (_fixedUpdateFrameDelayCount > 0)
		{
			_fixedUpdateFrameDelayCount--;
			return;
		}
		bool illuminated = _illuminated;
		UpdateIllumination();
		if (!illuminated && _illuminated)
		{
			OnDetectLight.Invoke();
		}
		else if (illuminated && !_illuminated)
		{
			OnDetectDarkness.Invoke();
		}
	}

	private void UpdateIllumination()
	{
		_illuminated = false;
		if (_illuminatingDreamLanternList != null)
		{
			_illuminatingDreamLanternList.Clear();
		}
		if (_lightSources == null || _lightSources.Count == 0)
		{
			return;
		}
		Vector3 vector = base.transform.TransformPoint(_localSensorOffset);
		Vector3 sensorWorldDir = Vector3.zero;
		if (_directionalSensor)
		{
			sensorWorldDir = base.transform.TransformDirection(_localDirection).normalized;
		}
		for (int i = 0; i < _lightSources.Count; i++)
		{
			if ((_lightSourceMask & _lightSources[i].GetLightSourceType()) != _lightSources[i].GetLightSourceType() || !_lightSources[i].CheckIlluminationAtPoint(vector, _sensorRadius, _maxDistance))
			{
				continue;
			}
			OWLight2 oWLight = null;
			bool flag = false;
			switch (_lightSources[i].GetLightSourceType())
			{
			case LightSourceType.FLASHLIGHT:
			{
				Vector3 position = Locator.GetPlayerCamera().transform.position;
				Vector3 to = base.transform.position - position;
				if (Vector3.Angle(Locator.GetPlayerCamera().transform.forward, to) <= _maxSpotHalfAngle && !CheckOcclusion(position, vector, sensorWorldDir))
				{
					_illuminated = true;
				}
				break;
			}
			case LightSourceType.PROBE:
			{
				SurveyorProbe probe = Locator.GetProbe();
				if (probe != null && probe.IsLaunched() && !probe.IsRetrieving() && probe.CheckIlluminationAtPoint(vector, _sensorRadius, _maxDistance) && !CheckOcclusion(probe.GetLightSourcePosition(), vector, sensorWorldDir))
				{
					_illuminated = true;
				}
				break;
			}
			case LightSourceType.SIMPLE_LANTERN:
			{
				OWLight2[] lights = _lightSources[i].GetLights();
				for (int j = 0; j < lights.Length; j++)
				{
					oWLight = lights[j];
					flag = oWLight.GetLight().shadows != 0 && oWLight.GetLight().shadowStrength > 0.5f;
					float maxDistance = Mathf.Min(_maxSimpleLanternDistance, _maxDistance);
					if (oWLight.CheckIlluminationAtPoint(vector, _sensorRadius, maxDistance) && !CheckOcclusion(oWLight.transform.position, vector, sensorWorldDir, flag))
					{
						_illuminated = true;
						break;
					}
				}
				break;
			}
			case LightSourceType.DREAM_LANTERN:
			{
				DreamLanternController dreamLanternController = _lightSources[i] as DreamLanternController;
				if (dreamLanternController.IsLit() && dreamLanternController.IsFocused(_lanternFocusThreshold) && dreamLanternController.CheckIlluminationAtPoint(vector, _sensorRadius, _maxDistance) && !CheckOcclusion(dreamLanternController.GetLightPosition(), vector, sensorWorldDir))
				{
					_illuminatingDreamLanternList.Add(dreamLanternController);
					_illuminated = true;
				}
				break;
			}
			case LightSourceType.UNDEFINED:
				oWLight = _lightSources[i] as OWLight2;
				flag = oWLight.GetLight().shadows != 0 && oWLight.GetLight().shadowStrength > 0.5f;
				if (oWLight.CheckIlluminationAtPoint(vector, _sensorRadius, _maxDistance) && !CheckOcclusion(oWLight.transform.position, vector, sensorWorldDir, flag))
				{
					_illuminated = true;
				}
				break;
			case LightSourceType.VOLUME_ONLY:
				_illuminated = true;
				break;
			}
		}
	}

	private bool CheckOcclusion(Vector3 lightSource, Vector3 sensorWorldPos, Vector3 sensorWorldDir, bool occludableLight = true)
	{
		Vector3 vector = sensorWorldPos - lightSource;
		if (_directionalSensor && Vector3.Angle(sensorWorldDir, -vector) > _detectionAngle)
		{
			return true;
		}
		if (_checkForOcclusion && occludableLight)
		{
			float num = Mathf.Max(0f, vector.magnitude - _sensorRadius);
			int num2 = Physics.RaycastNonAlloc(lightSource, vector, s_raycastHitBuffer, num, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < num2; i++)
			{
				if (!s_raycastHitBuffer[i].collider.CompareTag("Player") && s_raycastHitBuffer[i].distance < num)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 vector = base.transform.TransformPoint(_localSensorOffset);
		Vector3 direction = base.transform.TransformDirection(_localDirection);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(vector, Mathf.Max(0.1f, _sensorRadius));
		if (_directionalSensor)
		{
			Gizmos.DrawRay(vector, direction);
		}
	}
}
