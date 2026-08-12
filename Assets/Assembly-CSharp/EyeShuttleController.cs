using UnityEngine;

public class EyeShuttleController : MonoBehaviour
{
	[SerializeField]
	private GameObject _shuttleObject;

	[SerializeField]
	private EndlessCylinder _forestCylinder;

	[SerializeField]
	private EndlessTriggerVolume _endlessShuttleVolume;

	[SerializeField]
	private GameObject _blockExitObject;

	[Space]
	[SerializeField]
	private ParticleSystem _starParticles;

	[SerializeField]
	private ParticleSystem _slowParticles;

	[SerializeField]
	private GameObject _whiteSphere;

	[SerializeField]
	private GameObject _blackTunnel;

	[SerializeField]
	private GameObject _maskObject;

	[SerializeField]
	private Transform _playerMaskSocket;

	[Space]
	[SerializeField]
	private NomaiInterfaceSlot _launchSlot;

	[SerializeField]
	private NomaiInterfaceSlot _retrieveSlot;

	[SerializeField]
	private NomaiInterfaceSlot _landSlot;

	[SerializeField]
	private NomaiInterfaceOrb _orb;

	[SerializeField]
	private OWTriggerVolume _shuttleVolume;

	[SerializeField]
	private OWTriggerVolume _beamResetVolume;

	[SerializeField]
	private TractorBeamController _tractorBeam;

	[Space]
	[SerializeField]
	private OWAudioSource _musicSource;

	[SerializeField]
	private OWAudioSource _oneShotSource;

	private ParticleSystem.MainModule _starParticlesMain;

	private ParticleSystem.EmissionModule _starParticlesEmission;

	private bool _isPlayerInside;

	private bool _hasLaunched;

	private bool _hasArrivedAtMask;

	private bool _spawnPlayerAtMaskNextUpdate;

	private bool _hasPlayedOneShot;

	private float _spawnShuttleTime;

	private float _launchTime;

	private float _whiteoutFade;

	private const float FLIGHT_DURATION = 21f;

	private void Awake()
	{
		_starParticlesMain = _starParticles.main;
		_starParticlesEmission = _starParticles.emission;
		_shuttleVolume.OnEntry += OnEnterShuttle;
		_shuttleVolume.OnExit += OnExitShuttle;
		_beamResetVolume.OnExit += OnExitBeamReset;
		_launchSlot.OnSlotActivated += OnLaunchSlotActivated;
		_launchSlot.OnSlotDeactivated += OnLaunchSlotDeactivated;
		_retrieveSlot.OnSlotActivated += OnRetrieveSlotActivated;
		_landSlot.OnSlotActivated += OnLandSlotActivated;
		_landSlot.OnSlotDeactivated += OnLandSlotDeactivated;
		_blockExitObject.SetActive(value: false);
	}

	private void Start()
	{
		_whiteSphere.SetActive(value: false);
		_endlessShuttleVolume.SetActivation(active: false);
		_blackTunnel.SetActive(value: false);
		_shuttleObject.SetActive(value: false);
		_maskObject.SetActive(value: false);
		base.enabled = false;
	}

	public bool HasLaunched()
	{
		return _hasLaunched;
	}

	public void SpawnShuttle()
	{
		_shuttleObject.SetActive(value: true);
		_tractorBeam.SetActivation(active: false);
		_spawnShuttleTime = Time.time;
		base.enabled = true;
	}

	public void OnFinishGather()
	{
		_forestCylinder.SetActivation(active: true);
		_endlessShuttleVolume.SetActivation(active: false);
	}

	private void OnDestroy()
	{
		_shuttleVolume.OnEntry -= OnEnterShuttle;
		_shuttleVolume.OnExit -= OnExitShuttle;
		_beamResetVolume.OnExit -= OnExitBeamReset;
		_launchSlot.OnSlotActivated -= OnLaunchSlotActivated;
		_launchSlot.OnSlotDeactivated -= OnLaunchSlotDeactivated;
		_retrieveSlot.OnSlotActivated -= OnRetrieveSlotActivated;
		_landSlot.OnSlotActivated -= OnLandSlotActivated;
		_landSlot.OnSlotDeactivated -= OnLandSlotDeactivated;
	}

	private void Launch()
	{
		if (!_hasLaunched)
		{
			_hasLaunched = true;
			_launchTime = Time.time;
			Locator.GetToolModeSwapper().UnequipTool();
			_forestCylinder.SetActivation(active: false);
			_endlessShuttleVolume.SetActivation(active: true);
			_blackTunnel.SetActive(value: true);
			_blockExitObject.SetActive(value: true);
			Vector3 vector = Vector3.up * 1000f;
			base.transform.position += vector;
			Locator.GetPlayerBody().SetPosition(Locator.GetPlayerBody().GetPosition() + vector);
			_orb.transform.position += vector;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_starParticles.Play();
			_musicSource.Play();
		}
	}

	private void Update()
	{
		if (!_isPlayerInside && Time.time > _spawnShuttleTime + 2f && !_tractorBeam.IsActive())
		{
			_tractorBeam.SetActivation(active: true);
		}
		if (_hasLaunched && !_hasArrivedAtMask)
		{
			float num = Mathf.InverseLerp(_launchTime, _launchTime + 21f, Time.time);
			_starParticlesMain.startSpeed = Mathf.Lerp(200f, 2000f, num * num);
			_starParticlesMain.startLifetime = 400f / _starParticlesMain.startSpeed.constant;
			_starParticlesEmission.rateOverTime = Mathf.Lerp(50f, 200f, num * num);
		}
	}

	private void FixedUpdate()
	{
		if (_spawnPlayerAtMaskNextUpdate)
		{
			Locator.GetFlashlight().TurnOn(playAudio: false);
			Locator.GetPlayerBody().SetPosition(_playerMaskSocket.position);
			Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SetDegreesY(0f);
			Locator.GetPlayerBody().SetVelocity(Vector3.zero);
			_spawnPlayerAtMaskNextUpdate = false;
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
		}
		if (_hasLaunched && !_hasArrivedAtMask)
		{
			float num = _launchTime + 21f;
			float num2 = Mathf.InverseLerp(_launchTime, num, Time.time);
			float num3 = Mathf.Lerp(2000f, 30f, num2);
			_whiteoutFade = Mathf.InverseLerp(num - 1f, num, Time.time);
			_whiteSphere.transform.localPosition = Vector3.up * num3;
			if (!_hasPlayedOneShot && Time.time > _launchTime + 21f - 0.4f)
			{
				_oneShotSource.PlayOneShot(AudioType.EyeShuttleIntoLight);
				_hasPlayedOneShot = true;
			}
			if (num2 >= 1f)
			{
				_hasArrivedAtMask = true;
				if (!_hasPlayedOneShot)
				{
					_oneShotSource.PlayOneShot(AudioType.EyeShuttleIntoLight);
				}
				_shuttleVolume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
				_shuttleVolume.RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
				_isPlayerInside = false;
				_spawnPlayerAtMaskNextUpdate = true;
				_whiteSphere.SetActive(value: false);
				_shuttleObject.SetActive(value: false);
				_maskObject.SetActive(value: true);
				_starParticles.Stop();
				_starParticles.Clear();
				_slowParticles.Play();
			}
		}
		else if (_hasArrivedAtMask)
		{
			_whiteoutFade = Mathf.MoveTowards(_whiteoutFade, 0f, Time.deltaTime * 0.2f);
			if (_whiteoutFade <= 0f)
			{
				base.enabled = false;
			}
		}
	}

	private void OnEnterShuttle(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = true;
			Locator.GetFlashlight().TurnOff(playAudio: false);
		}
	}

	private void OnExitShuttle(GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			_isPlayerInside = false;
		}
	}

	private void OnExitBeamReset(GameObject hitObj)
	{
	}

	private void OnLaunchSlotActivated(NomaiInterfaceSlot slot)
	{
		Launch();
	}

	private void OnLaunchSlotDeactivated(NomaiInterfaceSlot slot)
	{
	}

	private void OnRetrieveSlotActivated(NomaiInterfaceSlot slot)
	{
	}

	private void OnLandSlotActivated(NomaiInterfaceSlot slot)
	{
	}

	private void OnLandSlotDeactivated(NomaiInterfaceSlot slot)
	{
	}
}
