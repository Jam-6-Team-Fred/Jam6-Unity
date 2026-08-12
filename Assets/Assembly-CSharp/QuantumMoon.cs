using UnityEngine;

public class QuantumMoon : QuantumObject
{
	public const int EYE_INDEX = 5;

	private const int CHECK_DEPTH = 10;

	private const int MAX_STATE_SKIPS = 10;

	[SerializeField]
	private float _fogRadius = 105f;

	[SerializeField]
	private float _fogThickness = 5f;

	[SerializeField]
	private float _fogRolloffDistance = 5f;

	[SerializeField]
	private float _eyeStateFogOffset;

	[Space]
	[SerializeField]
	private GameObject[] _deactivateAtEye;

	[SerializeField]
	private OWCollider _noStateGroundCollider;

	[SerializeField]
	private Transform _outerCloudTransform;

	[SerializeField]
	private FogOverrideVolume _fogOverride;

	[SerializeField]
	private SunOverrideVolume _sunOverride;

	[SerializeField]
	private AudioSignal _quantumSignal;

	[SerializeField]
	private ReferenceFrameVolume _referenceFrameVolume;

	[SerializeField]
	private Transform _vortexReturnPivot;

	[SerializeField]
	private SphereShape _interiorVolumeShape;

	[SerializeField]
	private OWAudioSource _vortexAudio;

	[Space]
	[SerializeField]
	private Transform _probeAnchorPoint;

	[SerializeField]
	private OWTriggerVolume _probeShipTrigger;

	[SerializeField]
	private SphereShape _sectorShape;

	[SerializeField]
	private VisibilityTracker _visibilityTracker;

	[SerializeField]
	private QuantumShrine _shrine;

	[SerializeField]
	private GameObject[] _quantumShrineProxyRenderers;

	[SerializeField]
	private float _sphereCheckRadius = 500f;

	[SerializeField]
	private GameObject[] _states;

	[SerializeField]
	private QuantumDarkTrigger[] _darkTriggers;

	[SerializeField]
	private string _revealFactID;

	private QuantumOrbit[] _orbits;

	private int _stateIndex = -1;

	private int _lastStateIndex = -1;

	private int[] _stateSkipCounts;

	private OWRigidbody _moonBody;

	private ConstantForceDetector _constantForceDetector;

	private QuantumFogEffectBubbleController _playerFogBubble;

	private QuantumFogEffectBubbleController _shipLandingCamFogBubble;

	private bool _isPlayerInside;

	private bool _isShipInside;

	private bool _isProbeInside;

	private bool _hasSunCollapsed;

	private bool _waitForEyeStateDeath;

	private float _sunCollapseTime;

	private int _collapseToIndex = -1;

	private float _playerWarpTime = -1000f;

	private float _origSectorRadius;

	private float _origFogOverrideRadius;

	private float _origSunOverrideRadius;

	private float _origInteriorVolumeRadius;

	private bool _useInitialMotion;

	private void OnValidate()
	{
		SphereShape component = _probeShipTrigger.GetComponent<SphereShape>();
		if (component != null && !OWMath.ApproxEquals(component.radius, _fogRadius))
		{
			component.radius = _fogRadius;
		}
	}

	protected override void Awake()
	{
		_visibilityTrackers = new VisibilityTracker[1] { _visibilityTracker };
		base.Awake();
		_origSectorRadius = _sectorShape.radius;
		_origFogOverrideRadius = _fogOverride.radius;
		_origSunOverrideRadius = _sunOverride.radius;
		_origInteriorVolumeRadius = _interiorVolumeShape.radius;
		_stateSkipCounts = new int[_states.Length];
		for (int i = 0; i < _stateSkipCounts.Length; i++)
		{
			_stateSkipCounts[i] = 0;
		}
		_moonBody = base.gameObject.GetAttachedOWRigidbody();
		_constantForceDetector = (ConstantForceDetector)_moonBody.GetAttachedForceDetector();
		_outerCloudTransform.parent = null;
		_probeShipTrigger.OnEntry += OnProbeShipEntry;
		_probeShipTrigger.OnExit += OnProbeShipExit;
		GlobalMessenger.AddListener("WarpPlayer", OnWarpPlayer);
		GlobalMessenger.AddListener("PlayerBlink", OnPlayerBlink);
	}

	protected override void Start()
	{
		_orbits = Object.FindObjectsOfType(typeof(QuantumOrbit)) as QuantumOrbit[];
		_playerFogBubble = Locator.GetPlayerCamera().GetRequiredComponentInChildren<QuantumFogEffectBubbleController>();
		_shipLandingCamFogBubble = Locator.GetShipTransform().GetRequiredComponentInChildren<QuantumFogEffectBubbleController>();
		_vortexAudio.SetLocalVolume(0f);
		_useInitialMotion = true;
		base.Start();
		if (!_collapseOnStart)
		{
			Debug.LogWarning("QUANTUM MOON DID NOT COLLAPSE ON START");
			OWRigidbody component = Locator.GetSunTransform().GetComponent<OWRigidbody>();
			_moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(component, _moonBody));
		}
	}

	private void OnDisable()
	{
		_outerCloudTransform.gameObject.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_probeShipTrigger.OnEntry -= OnProbeShipEntry;
		_probeShipTrigger.OnExit -= OnProbeShipExit;
		GlobalMessenger.RemoveListener("WarpPlayer", OnWarpPlayer);
		GlobalMessenger.RemoveListener("PlayerBlink", OnPlayerBlink);
	}

	public int GetStateIndex()
	{
		return _stateIndex;
	}

	public int GetLastStateIndex()
	{
		return _lastStateIndex;
	}

	public bool IsPlayerInside()
	{
		return _isPlayerInside;
	}

	public bool IsProbeInside()
	{
		return _isProbeInside;
	}

	public bool IsShipInside()
	{
		return _isShipInside;
	}

	public bool IsPlayerInsideShrine()
	{
		return _shrine.IsPlayerInside();
	}

	public void OnSunCollapse(bool handlePlayerDeath)
	{
		_hasSunCollapsed = true;
		_sunCollapseTime = Time.time;
		_waitForEyeStateDeath = handlePlayerDeath;
	}

	public override bool IsPlayerEntangled()
	{
		return _isPlayerInside;
	}

	protected override bool CheckIllumination()
	{
		for (int i = 0; i < _darkTriggers.Length; i++)
		{
			if (_darkTriggers[i].IsPlayerInDarkness())
			{
				return false;
			}
		}
		if (_shrine != null)
		{
			return !_shrine.IsPlayerInDarkness();
		}
		return true;
	}

	protected override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
	{
		bool flag = false;
		if (_isPlayerInside && _hasSunCollapsed)
		{
			return false;
		}
		if (Time.time - _playerWarpTime < 1f)
		{
			return false;
		}
		if (_stateIndex == 5 && _isPlayerInside && !IsPlayerEntangled())
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			int num = ((_collapseToIndex != -1) ? _collapseToIndex : GetRandomStateIndex());
			int num2 = -1;
			for (int j = 0; j < _orbits.Length; j++)
			{
				if (_orbits[j].GetStateIndex() == num)
				{
					num2 = j;
					break;
				}
			}
			if (num2 == -1)
			{
				Debug.LogError("QUANTUM MOON FAILED TO FIND ORBIT FOR STATE " + num);
			}
			float num3 = ((num2 != -1) ? _orbits[num2].GetOrbitRadius() : 10000f);
			OWRigidbody oWRigidbody = ((num2 != -1) ? _orbits[num2].GetAttachedOWRigidbody() : Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody());
			Vector3 onUnitSphere = Random.onUnitSphere;
			if (num == 5)
			{
				onUnitSphere.y = 0f;
				onUnitSphere.Normalize();
			}
			Vector3 position = onUnitSphere * num3 + oWRigidbody.GetWorldCenterOfMass();
			if (!Physics.CheckSphere(position, _sphereCheckRadius, OWLayerMask.physicalMask) || _collapseToIndex != -1)
			{
				_visibilityTracker.transform.position = position;
				if (!Physics.autoSyncTransforms)
				{
					Physics.SyncTransforms();
				}
				if (skipInstantVisibilityCheck || IsPlayerEntangled() || !CheckVisibilityInstantly())
				{
					_moonBody.transform.position = position;
					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}
					_visibilityTracker.transform.localPosition = Vector3.zero;
					_constantForceDetector.AddConstantVolume(oWRigidbody.GetAttachedGravityVolume(), inheritForceAcceleration: true, clearOtherFields: true);
					Vector3 vector = oWRigidbody.GetVelocity();
					if (_useInitialMotion)
					{
						InitialMotion component = oWRigidbody.GetComponent<InitialMotion>();
						vector = ((component != null) ? component.GetInitVelocity() : Vector3.zero);
						_useInitialMotion = false;
					}
					_moonBody.SetVelocity(OWPhysics.CalculateOrbitVelocity(oWRigidbody, _moonBody, Random.Range(0, 360)) + vector);
					_useInitialMotion = false;
					_lastStateIndex = _stateIndex;
					_stateIndex = num;
					_collapseToIndex = -1;
					flag = true;
					for (int k = 0; k < _stateSkipCounts.Length; k++)
					{
						_stateSkipCounts[k] = ((k != _stateIndex) ? (_stateSkipCounts[k] + 1) : 0);
					}
					break;
				}
				_visibilityTracker.transform.localPosition = Vector3.zero;
			}
			else
			{
				Debug.LogError("Quantum moon orbit position occupied! Aborting collapse.");
			}
		}
		if (flag)
		{
			if (_isPlayerInside)
			{
				SetSurfaceState(_stateIndex);
			}
			else
			{
				SetSurfaceState(-1);
				_quantumSignal.SetSignalActivation(_stateIndex != 5);
			}
			_referenceFrameVolume.gameObject.SetActive(_stateIndex != 5);
			_moonBody.SetIsTargetable(_stateIndex != 5);
			for (int l = 0; l < _deactivateAtEye.Length; l++)
			{
				_deactivateAtEye[l].SetActive(_stateIndex != 5);
			}
			GlobalMessenger<OWRigidbody>.FireEvent("QuantumMoonChangeState", _moonBody);
			return true;
		}
		return false;
	}

	private int GetRandomStateIndex()
	{
		int num = ((TimeLoop.GetSecondsRemaining() < 420f) ? 1 : 0);
		int result = -1;
		int num2 = 0;
		for (int i = num; i < _stateSkipCounts.Length; i++)
		{
			if (_stateSkipCounts[i] > num2)
			{
				num2 = _stateSkipCounts[i];
				result = i;
			}
		}
		if (num2 > 10)
		{
			return result;
		}
		int num3 = Random.Range(num, _states.Length - 1);
		if (num3 >= _stateIndex)
		{
			num3++;
		}
		return num3;
	}

	private void SetSurfaceState(int index)
	{
		if (_isProbeInside && Locator.GetProbe() != null && Locator.GetProbe().IsLaunched())
		{
			Locator.GetProbe().Unanchor();
			Locator.GetProbe().GetAnchor().AnchorToSocket(_probeAnchorPoint);
			if (_isPlayerInside)
			{
				Locator.GetProbe().Unanchor();
				Locator.GetProbe().GetOWRigidbody().SetVelocity(_moonBody.GetPointVelocity(_probeAnchorPoint.position));
			}
		}
		for (int i = 0; i < _states.Length; i++)
		{
			_states[i].SetActive(index == i);
		}
		_noStateGroundCollider.SetActivation(index == -1);
		_sectorShape.radius = ((index != 5) ? _origSectorRadius : 250f);
		_fogOverride.radius = ((index != 5) ? _origFogOverrideRadius : 250f);
		_sunOverride.radius = ((index != 5) ? _origSunOverrideRadius : 250f);
		_interiorVolumeShape.radius = ((index != 5) ? _origInteriorVolumeRadius : 250f);
		if (_shrine != null)
		{
			_shrine.OnQuantumMoonSurfaceStateChanged(index);
		}
		if (index == 5)
		{
			_vortexAudio.FadeIn(1f);
		}
		else
		{
			_vortexAudio.FadeOut(1f);
		}
		for (int j = 0; j < _quantumShrineProxyRenderers.Length; j++)
		{
			_quantumShrineProxyRenderers[j].SetActive(index != -1);
		}
	}

	private void CheckPlayerFogProximity()
	{
		float num = Vector3.Distance(base.transform.position, Locator.GetPlayerCamera().transform.position);
		float num2 = ((_stateIndex == 5) ? _eyeStateFogOffset : 0f);
		float num3 = num - (_fogRadius + num2);
		float fogAlpha = 0f;
		if (!_isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(_fogThickness + _fogRolloffDistance, _fogThickness, num3);
			if (num3 < 0f)
			{
				if (IsLockedByProbeSnapshot())
				{
					_isPlayerInside = true;
					SetSurfaceState(_stateIndex);
					Locator.GetShipLogManager().RevealFact(_revealFactID);
					GlobalMessenger.FireEvent("PlayerEnterQuantumMoon");
				}
				else
				{
					Collapse(skipInstantVisibilityCheck: true);
				}
			}
		}
		else if (_isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(0f - _fogThickness - _fogRolloffDistance, 0f - _fogThickness, num3);
			if (num3 >= 0f)
			{
				if (_stateIndex != 5)
				{
					_isPlayerInside = false;
					if (!IsLockedByProbeSnapshot())
					{
						Collapse(skipInstantVisibilityCheck: true);
					}
					SetSurfaceState(-1);
					GlobalMessenger.FireEvent("PlayerExitQuantumMoon");
				}
				else
				{
					bool num4 = _hasSunCollapsed || IsLockedByProbeSnapshot();
					Vector3 vector = Locator.GetPlayerTransform().position - base.transform.position;
					Locator.GetPlayerBody().SetVelocity(_moonBody.GetPointVelocity(Locator.GetPlayerTransform().position) - vector.normalized * 5f);
					float num5 = (num4 ? 80f : (_fogRadius - 1f));
					Locator.GetPlayerBody().SetPosition(base.transform.position + _vortexReturnPivot.up * num5);
					Locator.GetPlayerBody().SetRotation(_vortexReturnPivot.rotation);
					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}
					PlayerCameraController component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
					component.SetDegreesY(component.GetMinDegreesY());
					_vortexAudio.SetLocalVolume(0f);
					if (!num4)
					{
						_collapseToIndex = 1;
						Collapse(skipInstantVisibilityCheck: true);
					}
					else
					{
						_vortexAudio.FadeIn(1f);
					}
				}
			}
		}
		_playerFogBubble.SetFogAlpha(fogAlpha);
		_shipLandingCamFogBubble.SetFogAlpha(fogAlpha);
	}

	private void FixedUpdate()
	{
		CheckPlayerFogProximity();
		if (!_isPlayerInside)
		{
			Vector3 toDirection = Locator.GetPlayerTransform().position - _moonBody.GetPosition();
			Quaternion quaternion = Quaternion.FromToRotation(-_moonBody.transform.up, toDirection);
			_moonBody.GetRigidbody().rotation = quaternion * _moonBody.GetRotation();
		}
	}

	private void LateUpdate()
	{
		_outerCloudTransform.position = base.transform.position;
		if (!_waitForEyeStateDeath || !(Time.time > _sunCollapseTime + 20f))
		{
			return;
		}
		_waitForEyeStateDeath = false;
		if (!TimeLoopCoreController.ParadoxExists())
		{
			if (TimeLoop.IsTimeLoopEnabled())
			{
				Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
			}
			else
			{
				Locator.GetDeathManager().BeginEscapedTimeLoopSequence(TimeloopEscapeType.Quantum);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position, _fogRadius);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, _sphereCheckRadius);
		Gizmos.color = new ColorHSV(277f, 1f, 1f).ToColorRGB();
		OWGizmos.DrawWireCircle(base.transform.position, Vector3.up, _maxSnapshotLockRange);
		OWGizmos.DrawBillboardedWireCircle(base.transform.position, _maxSnapshotLockRange);
	}

	private void OnProbeShipEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("ProbeDetector"))
		{
			_isProbeInside = true;
			GlobalMessenger.FireEvent("ProbeEnterQuantumMoon");
			if (!_isPlayerInside)
			{
				Locator.GetProbe().GetAnchor().AnchorToSocket(_probeAnchorPoint);
			}
		}
		else if (hitObj.CompareTag("ShipDetector"))
		{
			_isShipInside = true;
			GlobalMessenger.FireEvent("ShipEnterQuantumMoon");
		}
	}

	private void OnProbeShipExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("ProbeDetector"))
		{
			_isProbeInside = false;
			GlobalMessenger.FireEvent("ProbeExitQuantumMoon");
		}
		else if (hitObj.CompareTag("ShipDetector"))
		{
			_isShipInside = false;
			GlobalMessenger.FireEvent("ShipExitQuantumMoon");
		}
	}

	private void OnWarpPlayer()
	{
		bool isPlayerInside = _isPlayerInside;
		_isPlayerInside = Vector3.Distance(base.transform.position, Locator.GetPlayerCamera().transform.position) < _fogRadius;
		if (_isPlayerInside && !isPlayerInside)
		{
			SetSurfaceState(_stateIndex);
			_playerWarpTime = Time.time;
			GlobalMessenger.FireEvent("PlayerEnterQuantumMoon");
		}
		else if (!_isPlayerInside && isPlayerInside)
		{
			SetSurfaceState(-1);
			GlobalMessenger.FireEvent("PlayerExitQuantumMoon");
		}
	}

	private void OnPlayerBlink()
	{
		if (IsVisible())
		{
			Collapse(skipInstantVisibilityCheck: true);
		}
	}
}
