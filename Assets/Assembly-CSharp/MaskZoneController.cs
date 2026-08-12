using UnityEngine;

public class MaskZoneController : MonoBehaviour
{
	[SerializeField]
	private OWTriggerVolume _maskZoneTrigger;

	[SerializeField]
	private EyeShuttleController _shuttle;

	[SerializeField]
	private GameObject _whiteSphere;

	[SerializeField]
	private VisibilityTracker _sphereVisibilityTracker;

	[SerializeField]
	private QuantumSkeletonTower _skeletonTower;

	[SerializeField]
	private QuantumInstrument _maskInstrument;

	[SerializeField]
	private Transform _returnSocket;

	[SerializeField]
	private AudioSignal _groundSignal;

	[SerializeField]
	private AudioSignal _skySignal;

	private bool _hasPlayerLookedAtSky;

	private void Awake()
	{
		_maskZoneTrigger.OnEntry += OnEnterMaskZone;
		_maskZoneTrigger.OnExit += OnExitMaskZone;
		_maskInstrument.OnFinishGather += OnFinishGather;
	}

	private void Start()
	{
		_skeletonTower.SetIsQuantum(isQuantum: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_maskZoneTrigger.OnEntry -= OnEnterMaskZone;
		_maskZoneTrigger.OnExit -= OnExitMaskZone;
		_maskInstrument.OnFinishGather -= OnFinishGather;
	}

	private void OnFinishGather()
	{
		_shuttle.OnFinishGather();
		Locator.GetPlayerBody().SetPosition(_returnSocket.position);
		Locator.GetPlayerBody().SetRotation(_returnSocket.rotation);
		Locator.GetPlayerBody().SetVelocity(Vector3.zero);
		PlayerCameraController component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
		component.SetDegreesY(component.GetMinDegreesY());
		base.enabled = false;
	}

	private void Update()
	{
		if (!_hasPlayerLookedAtSky && _sphereVisibilityTracker.IsVisible())
		{
			_hasPlayerLookedAtSky = true;
			_skeletonTower.SetIsQuantum(isQuantum: true);
		}
	}

	private void OnEnterMaskZone(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_whiteSphere.SetActive(value: true);
			_groundSignal.SetSignalActivation(active: false);
			_skySignal.SetSignalActivation(active: true);
			_skeletonTower.SetIsQuantum(_hasPlayerLookedAtSky);
			base.enabled = true;
		}
	}

	private void OnExitMaskZone(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector") && !_shuttle.HasLaunched())
		{
			_whiteSphere.SetActive(value: false);
			_skeletonTower.SetIsQuantum(isQuantum: false);
			_groundSignal.SetSignalActivation(active: true);
			_skySignal.SetSignalActivation(active: false);
			base.enabled = false;
		}
	}
}
