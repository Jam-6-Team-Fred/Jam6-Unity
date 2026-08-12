using UnityEngine;

public class GravityCannonController : MonoBehaviour
{
	[SerializeField]
	private NomaiShuttleController.ShuttleID _shuttleID;

	[SerializeField]
	private ForceVolume _forceVolume;

	[SerializeField]
	private Transform _shuttleSocket;

	[SerializeField]
	private NomaiInterfaceSlot _launchSlot;

	[SerializeField]
	private NomaiInterfaceSlot _retrieveSlot;

	[SerializeField]
	private SingularityWarpEffect _warpEffect;

	[SerializeField]
	private GameObject _recallProxyGeometry;

	[SerializeField]
	private OWTriggerVolume _platformTrigger;

	[Space]
	[SerializeField]
	private OWAudioSource _ambientAudioSrc;

	[SerializeField]
	private OWAudioSource _oneShotAudioSrc;

	[SerializeField]
	private float _fadeLength = 0.5f;

	[Space]
	[SerializeField]
	private NomaiComputer _nomaiComputer;

	[SerializeField]
	private string _retrieveShipLogFact;

	[SerializeField]
	private string _launchShipLogFact;

	private bool _isGravityActive;

	private bool _waitForProxySwap;

	private bool _playerShipOnPlatform;

	private bool _allowRevealLaunchFact;

	private SandLevelController _sandController;

	private OWRigidbody _shuttleBody;

	private int _immobilizeShuttleFrameCounter;

	private void Awake()
	{
		Locator.RegisterGravityCannon(this);
		_platformTrigger.OnEntry += OnPlatformEntry;
		_platformTrigger.OnExit += OnPlatformExit;
	}

	private void Start()
	{
		AstroObject componentInParent = GetComponentInParent<AstroObject>();
		if (componentInParent != null)
		{
			_sandController = componentInParent.GetSandLevelController();
		}
		_recallProxyGeometry.SetActive(value: false);
		_forceVolume.SetVolumeActivation(active: false);
		_launchSlot.OnSlotActivated += OnLaunchSlotActivated;
		_launchSlot.OnSlotDeactivated += OnLaunchSlotDeactivated;
		_retrieveSlot.OnSlotActivated += OnRetrieveSlotActivated;
		_ambientAudioSrc.loop = true;
		_ambientAudioSrc.AssignAudioLibraryClip(AudioType.NomaiGravityCannonAmbient_LP);
		_ambientAudioSrc.SetLocalVolume(0f);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_launchSlot.OnSlotActivated -= OnLaunchSlotActivated;
		_launchSlot.OnSlotDeactivated -= OnLaunchSlotDeactivated;
		_retrieveSlot.OnSlotActivated -= OnRetrieveSlotActivated;
		_platformTrigger.OnEntry -= OnPlatformEntry;
		_platformTrigger.OnExit -= OnPlatformExit;
		Locator.UnregisterGravityCannon(this);
	}

	public bool IsGravityActive()
	{
		return _isGravityActive;
	}

	public bool IsPlayerShipOnPlatform()
	{
		return _playerShipOnPlatform;
	}

	public NomaiShuttleController.ShuttleID GetID()
	{
		return _shuttleID;
	}

	public bool AllowShuttleRetrieval(Vector3 shuttlePosition)
	{
		NomaiShuttleController nomaiShuttle = Locator.GetNomaiShuttle(_shuttleID);
		if (nomaiShuttle != null && nomaiShuttle.GetOWRigidbody().gameObject.activeSelf && Vector3.Distance(shuttlePosition, _shuttleSocket.position) > 1f)
		{
			if (!(_sandController == null))
			{
				return !_sandController.IsPointBuried(_shuttleSocket.position);
			}
			return true;
		}
		return false;
	}

	public void PlayRecallEffect(float effectLength, bool playerInsideShuttle)
	{
		if (!playerInsideShuttle)
		{
			_recallProxyGeometry.SetActive(value: true);
			_warpEffect.WarpObjectIn(effectLength);
		}
		else
		{
			_warpEffect.singularityController.Create();
		}
	}

	public void PlayEndOfRecallEffect()
	{
		_warpEffect.singularityController.Collapse();
	}

	public void MoveShuttleToSocket(OWRigidbody shuttleBody)
	{
		if (_playerShipOnPlatform)
		{
			Locator.GetShipBody().GetComponent<ShipDamageController>().Explode();
		}
		shuttleBody.SetRotation(_shuttleSocket.rotation);
		shuttleBody.SetPosition(_shuttleSocket.position);
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
		OWRigidbody componentInParent = GetComponentInParent<OWRigidbody>();
		shuttleBody.SetVelocity(componentInParent.GetPointVelocity(_shuttleSocket.position));
		shuttleBody.SetAngularVelocity(componentInParent.GetAngularVelocity());
		_waitForProxySwap = true;
		_nomaiComputer.ClearAllEntries();
		if (_retrieveShipLogFact.Length > 0)
		{
			Locator.GetShipLogManager().RevealFact(_retrieveShipLogFact);
		}
		_shuttleBody = shuttleBody;
		_immobilizeShuttleFrameCounter = 5;
		_allowRevealLaunchFact = true;
		base.enabled = true;
	}

	public void SetGravityActivation(bool activate)
	{
		_forceVolume.SetVolumeActivation(activate);
		if (_isGravityActive != activate)
		{
			if (activate)
			{
				_ambientAudioSrc.FadeIn(_fadeLength);
			}
			else
			{
				_ambientAudioSrc.FadeOut(_fadeLength);
			}
		}
		_isGravityActive = activate;
		if (activate && _allowRevealLaunchFact && _launchShipLogFact.Length > 0)
		{
			Locator.GetShipLogManager().RevealFact(_launchShipLogFact);
		}
	}

	private void Update()
	{
		if (_waitForProxySwap)
		{
			_waitForProxySwap = false;
		}
		else
		{
			_recallProxyGeometry.SetActive(value: false);
		}
	}

	private void FixedUpdate()
	{
		_shuttleBody.transform.rotation = _shuttleSocket.rotation;
		_shuttleBody.transform.position = _shuttleSocket.position;
		OWRigidbody componentInParent = GetComponentInParent<OWRigidbody>();
		_shuttleBody.SetVelocity(componentInParent.GetPointVelocity(_shuttleSocket.position));
		_shuttleBody.SetAngularVelocity(componentInParent.GetAngularVelocity());
		_immobilizeShuttleFrameCounter--;
		if (_immobilizeShuttleFrameCounter <= 0 && !_recallProxyGeometry.activeSelf)
		{
			base.enabled = false;
			_shuttleBody = null;
			Locator.GetNomaiShuttle(_shuttleID).OnImmobilizationComplete();
		}
	}

	private void OnLaunchSlotActivated(NomaiInterfaceSlot slot)
	{
		_allowRevealLaunchFact = true;
		SetGravityActivation(activate: true);
	}

	private void OnLaunchSlotDeactivated(NomaiInterfaceSlot slot)
	{
		SetGravityActivation(activate: false);
	}

	private void OnRetrieveSlotActivated(NomaiInterfaceSlot slot)
	{
		NomaiShuttleController nomaiShuttle = Locator.GetNomaiShuttle(_shuttleID);
		if (nomaiShuttle != null)
		{
			nomaiShuttle.Retrieve();
		}
		else
		{
			Debug.Log("Failed to locate Nomai shuttle");
		}
	}

	private void OnPlatformEntry(GameObject hitObj)
	{
		if (hitObj.CompareTag("ShipDetector"))
		{
			_playerShipOnPlatform = true;
		}
	}

	private void OnPlatformExit(GameObject hitObj)
	{
		if (hitObj.CompareTag("ShipDetector"))
		{
			_playerShipOnPlatform = false;
		}
	}
}
