using UnityEngine;

public class NomaiShuttleController : MonoBehaviour
{
	public enum ShuttleID
	{
		HourglassShuttle = 0,
		BrittleHollowShuttle = 1
	}

	[SerializeField]
	private ShuttleID _id;

	[SerializeField]
	private float _retrievalLength = 1f;

	[SerializeField]
	private NomaiInterfaceSlot _launchSlot;

	[SerializeField]
	private NomaiInterfaceSlot _retrieveSlot;

	[SerializeField]
	private NomaiInterfaceSlot _landSlot;

	[SerializeField]
	private NomaiInterfaceOrb _orb;

	[SerializeField]
	private OWTriggerVolume _triggerVolume;

	[SerializeField]
	private OWTriggerVolume _beamResetVolume;

	[SerializeField]
	private TractorBeamController _tractorBeam;

	[SerializeField]
	private OWRigidbody _shuttleBody;

	[SerializeField]
	private GameObject _detectorObj;

	[SerializeField]
	private ForceVolume _forceVolume;

	[SerializeField]
	private GameObject _exteriorColliderRoot;

	[SerializeField]
	private CollisionGroup _exteriorCollisionGroup;

	[SerializeField]
	private ImpactSensor _impactSensor;

	[SerializeField]
	private GameObject _landingBeamRoot;

	[Space]
	[SerializeField]
	private GameObject _exteriorRendererObj;

	[Space]
	[SerializeField]
	private OWCollider[] _exteriorLegColliders = new OWCollider[0];

	private ForceApplier _forceApplier;

	private GravityCannonController _cannon;

	private PlanetoidRuleset _targetPlanetoid;

	private SingularityWarpEffect _warpEffect;

	private bool _isPlayerInside;

	private bool _isLanding;

	private bool _isRetrieving;

	private bool _retrievingWithPlayer;

	private bool _allowLanding;

	private int _framesToReposition;

	private void Awake()
	{
		Locator.RegisterNomaiShuttle(this);
		_warpEffect = GetComponentInChildren<SingularityWarpEffect>();
		_triggerVolume.OnEntry += OnEntry;
		_triggerVolume.OnExit += OnExit;
		_beamResetVolume.OnExit += OnExitBeamReset;
		_launchSlot.OnSlotActivated += OnLaunchSlotActivated;
		_launchSlot.OnSlotDeactivated += OnLaunchSlotDeactivated;
		_retrieveSlot.OnSlotActivated += OnRetrieveSlotActivated;
		_landSlot.OnSlotActivated += OnLandSlotActivated;
		_landSlot.OnSlotDeactivated += OnLandSlotDeactivated;
		_impactSensor.OnImpact += OnImpact;
		_landingBeamRoot.SetActive(value: false);
		if (_id == ShuttleID.BrittleHollowShuttle)
		{
			_exteriorRendererObj.SetActive(value: false);
			GlobalMessenger.AddListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
			GlobalMessenger.AddListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
		}
	}

	private void Start()
	{
		_cannon = Locator.GetGravityCannon(_id);
		if (_cannon == null)
		{
			Debug.LogError("Failed to locate gravity cannon " + _id);
		}
		_shuttleBody.Suspend();
		_forceApplier = _detectorObj.GetComponent<ForceApplier>();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_triggerVolume.OnEntry -= OnEntry;
		_triggerVolume.OnExit -= OnExit;
		_beamResetVolume.OnExit -= OnExitBeamReset;
		_launchSlot.OnSlotActivated -= OnLaunchSlotActivated;
		_launchSlot.OnSlotDeactivated -= OnLaunchSlotDeactivated;
		_retrieveSlot.OnSlotActivated -= OnRetrieveSlotActivated;
		_landSlot.OnSlotActivated -= OnLandSlotActivated;
		_landSlot.OnSlotDeactivated -= OnLandSlotDeactivated;
		_impactSensor.OnImpact -= OnImpact;
		Locator.UnregisterNomaiShuttle(this);
		if (_id == ShuttleID.BrittleHollowShuttle)
		{
			GlobalMessenger.RemoveListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
			GlobalMessenger.RemoveListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
		}
	}

	public bool IsPlayerInside()
	{
		return _isPlayerInside;
	}

	public void Retrieve()
	{
		if (!_isRetrieving && !(_cannon == null) && _cannon.AllowShuttleRetrieval(base.transform.position))
		{
			if (_id == ShuttleID.BrittleHollowShuttle && _shuttleBody.IsSuspended())
			{
				_exteriorRendererObj.SetActive(value: true);
				GlobalMessenger.RemoveListener("PlayerEnterQuantumMoon", OnPlayerEnterQuantumMoon);
				GlobalMessenger.RemoveListener("PlayerExitQuantumMoon", OnPlayerExitQuantumMoon);
			}
			_cannon.SetGravityActivation(activate: false);
			if (_isPlayerInside)
			{
				_retrievingWithPlayer = true;
				_warpEffect.singularityController.OnCreation += StartReposition;
				_warpEffect.singularityController.Create();
				_cannon.PlayRecallEffect(_retrievalLength, playerInsideShuttle: true);
			}
			else
			{
				_retrievingWithPlayer = false;
				_warpEffect.OnWarpComplete += StartReposition;
				_warpEffect.WarpObjectOut(_retrievalLength);
				_cannon.PlayRecallEffect(_retrievalLength, playerInsideShuttle: false);
			}
			_orb.AddLock();
			_isRetrieving = true;
			_allowLanding = false;
		}
	}

	public void OnImmobilizationComplete()
	{
		_forceApplier.SetApplyFluids(applyFluids: true);
		_forceApplier.SetApplyForces(applyForces: true);
	}

	private void StartReposition()
	{
		if (_shuttleBody.IsSuspended())
		{
			UnsuspendShuttle();
		}
		_framesToReposition = 2;
		base.enabled = true;
		if (_retrievingWithPlayer)
		{
			_warpEffect.singularityController.OnCreation -= StartReposition;
			_warpEffect.singularityController.CollapseImmediate();
			_cannon.PlayEndOfRecallEffect();
		}
		else
		{
			_warpEffect.OnWarpComplete -= StartReposition;
		}
	}

	private void CompleteReposition()
	{
		if (_cannon != null)
		{
			_forceApplier.SetApplyFluids(applyFluids: false);
			_forceApplier.SetApplyForces(applyForces: false);
			_cannon.MoveShuttleToSocket(_shuttleBody);
			if (_isLanding)
			{
				StopLanding();
			}
		}
		for (int i = 0; i < _exteriorLegColliders.Length; i++)
		{
			_exteriorLegColliders[i].SetActivation(active: true);
		}
		_orb.RemoveLock();
		if (_tractorBeam != null && !_isPlayerInside)
		{
			_tractorBeam.SetActivation(active: true);
		}
		_isRetrieving = false;
	}

	private void UnsuspendShuttle()
	{
		_shuttleBody.Unsuspend(restoreCachedVelocity: false);
		_shuttleBody.transform.parent = null;
		_shuttleBody.transform.position = base.transform.position;
		_shuttleBody.transform.rotation = base.transform.rotation;
		base.transform.parent = _shuttleBody.transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		EffectVolume[] componentsInChildren = GetComponentsInChildren<EffectVolume>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetAttachedBody();
		}
		_orb.SetParentBody(_shuttleBody);
		_orb.GetComponent<ConstantForceDetector>().AddConstantVolume(_forceVolume, inheritForceAcceleration: true, clearOtherFields: true);
		if (_isPlayerInside)
		{
			DynamicForceDetector component = Locator.GetPlayerDetector().GetComponent<DynamicForceDetector>();
			component.RemoveVolume(_forceVolume);
			component.AddVolume(_forceVolume);
		}
		OWCollider[] componentsInChildren2 = _exteriorColliderRoot.GetComponentsInChildren<OWCollider>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			_exteriorCollisionGroup.RemoveCollider(componentsInChildren2[j]);
		}
		for (int k = 0; k < _exteriorLegColliders.Length; k++)
		{
			_exteriorLegColliders[k].SetActivation(active: false);
		}
	}

	private void AttemptLanding()
	{
		PlanetoidRuleset[] array = Object.FindObjectsOfType<PlanetoidRuleset>();
		PlanetoidRuleset planetoidRuleset = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = Vector3.Distance(array[i].transform.position, base.transform.position) - array[i].GetShuttleLandingRadius();
			if (num2 < num)
			{
				num = num2;
				planetoidRuleset = array[i];
			}
		}
		if (planetoidRuleset != null && num < 1000f)
		{
			_targetPlanetoid = planetoidRuleset;
			_isLanding = true;
			_landSlot.SetAttractive(attractive: true);
			_landingBeamRoot.SetActive(value: true);
			base.enabled = true;
			MonoBehaviour.print("LANDING SEQUENCE: Distance to " + _targetPlanetoid.name + ": " + num);
		}
	}

	private void StopLanding()
	{
		_isLanding = false;
		_targetPlanetoid = null;
		_landingBeamRoot.SetActive(value: false);
	}

	public ShuttleID GetID()
	{
		return _id;
	}

	public OWRigidbody GetOWRigidbody()
	{
		return _shuttleBody;
	}

	private void FixedUpdate()
	{
		if (_isLanding)
		{
			Vector3 toDirection = base.transform.position - _targetPlanetoid.transform.position;
			float num = toDirection.magnitude - _targetPlanetoid.GetShuttleLandingRadius();
			if (_targetPlanetoid == null || num > 2000f)
			{
				StopLanding();
				return;
			}
			Vector3 vector = OWPhysics.FromToAngularVelocity(base.transform.up, toDirection);
			_shuttleBody.SetAngularVelocity(Vector3.zero);
			_shuttleBody.AddAngularVelocityChange(vector * 0.01f);
			OWRigidbody attachedOWRigidbody = _targetPlanetoid.GetAttachedOWRigidbody();
			float t = Mathf.InverseLerp(100f, 0f, num);
			float num2 = Mathf.Lerp(10f, 1f, t);
			Vector3 vector2 = attachedOWRigidbody.GetVelocity() - toDirection.normalized * num2 - _shuttleBody.GetVelocity();
			MonoBehaviour.print("approach speed: " + num2 + "   delta velocity: " + vector2.magnitude);
			Vector3 velocityChange = vector2 * Time.deltaTime * 1f;
			_shuttleBody.AddVelocityChange(velocityChange);
			if (_isPlayerInside)
			{
				Locator.GetPlayerBody().AddVelocityChange(velocityChange);
			}
		}
		if (_framesToReposition > 0)
		{
			_framesToReposition--;
			if (_framesToReposition == 0)
			{
				CompleteReposition();
			}
		}
		if (!_isLanding && _framesToReposition == 0)
		{
			base.enabled = false;
		}
	}

	private void OnImpact(ImpactData impact)
	{
		if (impact.otherBody.GetMass() > 100f && _isLanding)
		{
			Debug.Log("Shuttle impact with " + impact.otherCollider.name, impact.otherCollider);
			StopLanding();
		}
	}

	private void OnEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = true;
			GlobalMessenger.FireEvent("EnterShuttle");
			_tractorBeam.SetActivation(active: false);
		}
	}

	private void OnExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = false;
			GlobalMessenger.FireEvent("ExitShuttle");
		}
	}

	private void OnExitBeamReset(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && !_isPlayerInside && !_tractorBeam.IsActive())
		{
			_tractorBeam.SetActivation(active: true);
		}
	}

	private void OnLaunchSlotActivated(NomaiInterfaceSlot slot)
	{
		_cannon.SetGravityActivation(activate: true);
		_allowLanding = true;
	}

	private void OnLaunchSlotDeactivated(NomaiInterfaceSlot slot)
	{
		_cannon.SetGravityActivation(activate: false);
	}

	private void OnRetrieveSlotActivated(NomaiInterfaceSlot slot)
	{
		Retrieve();
	}

	private void OnLandSlotActivated(NomaiInterfaceSlot slot)
	{
		if (_allowLanding && !_shuttleBody.IsSuspended())
		{
			AttemptLanding();
		}
	}

	private void OnLandSlotDeactivated(NomaiInterfaceSlot slot)
	{
		if (_isLanding)
		{
			StopLanding();
			_landSlot.SetAttractive(attractive: false);
		}
	}

	private void OnPlayerEnterQuantumMoon()
	{
		_exteriorRendererObj.SetActive(value: true);
	}

	private void OnPlayerExitQuantumMoon()
	{
		_exteriorRendererObj.SetActive(value: false);
	}
}
