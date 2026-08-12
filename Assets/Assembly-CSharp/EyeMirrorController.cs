using UnityEngine;

public class EyeMirrorController : MonoBehaviour
{
	[Header("Hide In Tomb")]
	[SerializeField]
	private OWLight2 _planetFillLight;

	[Header("Portraits")]
	[SerializeField]
	private AbstractDoor _door;

	[SerializeField]
	private EyeTombPortrait[] _portraits;

	[Header("Volumes")]
	[SerializeField]
	private OWTriggerVolume _tombVolume;

	[SerializeField]
	private OWTriggerVolume _probeDestructionVolume;

	[SerializeField]
	private EndlessCylinder _forestCylinder;

	[SerializeField]
	private OWTriggerVolume _closeDoorTrigger;

	[Header("Signals")]
	[SerializeField]
	private AudioSignal _buriedSignal;

	[SerializeField]
	private AudioSignal _instrumentSignal;

	[Header("Mirror")]
	[SerializeField]
	private DreamCandle _dreamCandle;

	[SerializeField]
	private OWFlameController _mirrorCandle;

	[SerializeField]
	private GameObject _mirrorPlayer;

	[SerializeField]
	private VisibilityObject _mirrorVisibilityObject;

	[SerializeField]
	private GameObject _mirrorProbe;

	[SerializeField]
	private Transform _mirrorProbeScaleRoot;

	[SerializeField]
	private Transform _mirrorProbeCenterBone;

	[Header("Mummy")]
	[SerializeField]
	private GameObject _mummyObject;

	[SerializeField]
	private GameObject _tunnelBlocker;

	[Header("Instrument")]
	[SerializeField]
	private VisibilityObject _instrumentVisibilityObject;

	[SerializeField]
	private OWRigidbody _instrumentBody;

	[SerializeField]
	private OWAudioSource _instrumentImpactAudio;

	private bool _hasExtinguishedCandle;

	private bool _hasMummyAppeared;

	private bool _hasInstrumentAppeared;

	private bool _hasInstrumentImpactPlayed;

	private bool _probeLaunchedInTomb;

	private int _numSwappedPortraits;

	private float _mummySpawnTime;

	private ProbeAnimatorController _probeAnimator;

	private void Awake()
	{
		_tombVolume.OnEntry += OnEnterTomb;
		_tombVolume.OnExit += OnExitTomb;
		_closeDoorTrigger.OnEntry += OnEnterCloseDoorTrigger;
		_dreamCandle.OnLitStateChanged += new OWEvent.OWCallback(OnLitStateChanged);
		for (int i = 0; i < _portraits.Length; i++)
		{
			_portraits[i].OnSwapPortrait += new OWEvent.OWCallback(OnSwapPortrait);
		}
	}

	private void Start()
	{
		_mummyObject.SetActive(value: false);
		_tunnelBlocker.SetActive(value: false);
		_mirrorPlayer.SetActive(value: false);
		_mirrorProbe.SetActive(value: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		_tombVolume.OnEntry -= OnEnterTomb;
		_tombVolume.OnExit -= OnExitTomb;
		_closeDoorTrigger.OnEntry -= OnEnterCloseDoorTrigger;
		_dreamCandle.OnLitStateChanged -= new OWEvent.OWCallback(OnLitStateChanged);
		for (int i = 0; i < _portraits.Length; i++)
		{
			_portraits[i].OnSwapPortrait -= new OWEvent.OWCallback(OnSwapPortrait);
		}
	}

	private void OnSwapPortrait()
	{
		Locator.GetFlashlight().TurnOn(playAudio: false);
		_numSwappedPortraits++;
		if (_numSwappedPortraits == _portraits.Length)
		{
			_door.Open();
		}
	}

	private void OnEnterCloseDoorTrigger(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_door.Close();
		}
	}

	private void OnLitStateChanged()
	{
		_hasExtinguishedCandle = true;
		_mirrorCandle.FadeTo(0f, 0.5f);
		_mummySpawnTime = Time.time + 0.5f;
		_tunnelBlocker.SetActive(value: true);
		GlobalMessenger<float, float>.FireEvent("FlickerOffAndOn", 0.5f, 1f);
	}

	private void Update()
	{
		if (_hasExtinguishedCandle && !_hasMummyAppeared && Time.time > _mummySpawnTime)
		{
			Locator.GetFlashlight().TurnOn(playAudio: false);
			_hasMummyAppeared = true;
			_mummyObject.SetActive(value: true);
		}
		if (_hasMummyAppeared && !_hasInstrumentAppeared && _instrumentVisibilityObject.IsVisible() && _instrumentVisibilityObject.IsIlluminated())
		{
			_hasInstrumentAppeared = true;
			_tunnelBlocker.SetActive(value: false);
			_mummyObject.SetActive(value: false);
			_instrumentBody.gameObject.SetActive(value: true);
			_instrumentSignal.SetSignalActivation(active: true);
			_instrumentBody.SetVelocity(Vector3.zero);
			_instrumentBody.SetAngularVelocity(Vector3.zero);
		}
		if (_hasInstrumentAppeared && !_hasInstrumentImpactPlayed && Vector3.Angle(_instrumentBody.transform.forward, base.transform.up) < 10f)
		{
			_hasInstrumentImpactPlayed = true;
			_instrumentImpactAudio.PlayOneShot(AudioType.DefaultPropImpact);
		}
	}

	private void FixedUpdate()
	{
		Transform playerTransform = Locator.GetPlayerTransform();
		Vector3 localPosition = base.transform.InverseTransformPoint(playerTransform.position);
		_mirrorPlayer.transform.localPosition = localPosition;
		Quaternion localRotation = Quaternion.Inverse(base.transform.rotation) * playerTransform.rotation;
		_mirrorPlayer.transform.localRotation = localRotation;
		if (_probeLaunchedInTomb)
		{
			Transform transform = Locator.GetProbe().transform;
			Vector3 localPosition2 = base.transform.InverseTransformPoint(transform.position);
			_mirrorProbe.transform.localPosition = localPosition2;
			Quaternion localRotation2 = Quaternion.Inverse(base.transform.rotation) * transform.rotation;
			_mirrorProbe.transform.localRotation = localRotation2;
			_mirrorProbeCenterBone.localRotation = _probeAnimator.GetCenterBoneLocalRotation();
			_mirrorProbeScaleRoot.localScale = _probeAnimator.transform.parent.localScale;
		}
	}

	private void OnEnterTomb(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_planetFillLight.SetActivation(active: false);
			_forestCylinder.SetActivation(active: false);
			_mirrorPlayer.SetActive(value: true);
			_buriedSignal.SetSignalActivation(active: false);
			_probeDestructionVolume.SetTriggerActivation(active: false);
			if (_probeLaunchedInTomb)
			{
				_mirrorProbe.SetActive(value: true);
			}
			Locator.GetToolModeSwapper().GetProbeLauncher().OnLaunchProbe += OnLaunchProbe;
			base.enabled = true;
		}
	}

	private void OnExitTomb(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_forestCylinder.SetActivation(active: true);
			_mirrorPlayer.SetActive(value: false);
			if (!_hasInstrumentAppeared)
			{
				_planetFillLight.SetActivation(active: true);
				_buriedSignal.SetSignalActivation(active: true);
			}
			_mirrorProbe.SetActive(value: false);
			Locator.GetToolModeSwapper().GetProbeLauncher().OnLaunchProbe -= OnLaunchProbe;
			base.enabled = false;
		}
	}

	private void OnLaunchProbe(SurveyorProbe probe)
	{
		_probeLaunchedInTomb = true;
		_mirrorProbe.SetActive(value: true);
		_mirrorProbeScaleRoot.localScale = Vector3.one;
		Locator.GetProbe().OnRetrieveProbe += OnRetrieveProbe;
		_probeAnimator = Locator.GetProbe().GetComponentInChildren<ProbeAnimatorController>();
	}

	private void OnRetrieveProbe()
	{
		_probeLaunchedInTomb = false;
		_mirrorProbe.SetActive(value: false);
		Locator.GetProbe().OnRetrieveProbe -= OnRetrieveProbe;
	}
}
