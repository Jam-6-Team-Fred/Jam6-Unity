using UnityEngine;

public class GalaxyMapController : MonoBehaviour
{
	[SerializeField]
	private Transform _playerWarpPoint;

	[SerializeField]
	private SingleInteractionVolume _interactVolume;

	[SerializeField]
	private Transform _eyeTransform;

	[SerializeField]
	private Transform _galaxyTransform;

	[SerializeField]
	private Transform _galaxyStartPos;

	[SerializeField]
	private Transform _galaxyEndPos;

	[SerializeField]
	private GameObject _forestObj;

	[SerializeField]
	private MiniGalaxyController _miniGalaxyController;

	[SerializeField]
	private OWTriggerVolume _observatoryVolume;

	[SerializeField]
	private OWAudioSource _audioSource;

	[SerializeField]
	private EndlessTriggerVolume _endlessObservatoryVolume;

	[SerializeField]
	private OWTriggerVolume _forestOfGalaxiesVolume;

	[Space]
	[SerializeField]
	private GameObject _supernovaPrefab;

	[SerializeField]
	private Shape _supernovaSpawnShape;

	[SerializeField]
	private float _supernovaSpawnRate = 10f;

	[SerializeField]
	private int _maxSupernovaCount = 100;

	[SerializeField]
	private float _supernovaSpeed = 200f;

	[Space]
	[SerializeField]
	private ParticleSystem[] _starParticles;

	[SerializeField]
	private float _starParticleDelay = 1f;

	[SerializeField]
	private float _exitGalaxyDelay = 4f;

	private MiniGalaxy _miniGalaxy;

	private bool _hasWarpedPlayer;

	private bool _zoomOutEye;

	private bool _spawnStars;

	private bool _exitGalaxy;

	private bool _hasPlayerRegainedControl;

	private Vector3 _origEyePos;

	private float _zoomSpeed;

	private float _startZoomTime;

	private float _spawnStarTime;

	private float _exitGalaxyTime;

	private float _spawnSupernovaTime;

	private GameObject[] _supernovas;

	private int _supernovaCount;

	private void Awake()
	{
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract += OnPressInteract;
		}
		_supernovas = new GameObject[_maxSupernovaCount];
		_supernovaCount = 0;
		_miniGalaxy = _galaxyTransform.GetComponentInChildren<MiniGalaxy>();
		_eyeTransform.gameObject.SetActive(value: true);
		GlobalMessenger<EyeState>.AddListener("EyeStateChanged", OnEyeStateChanged);
	}

	private void Start()
	{
		_eyeTransform.gameObject.SetActive(value: false);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (_interactVolume != null)
		{
			_interactVolume.OnPressInteract -= OnPressInteract;
		}
		GlobalMessenger<EyeState>.RemoveListener("EyeStateChanged", OnEyeStateChanged);
	}

	private void OnEyeStateChanged(EyeState newState)
	{
		if (newState == EyeState.ForestOfGalaxies)
		{
			_miniGalaxyController.TurnOnGalaxies();
		}
	}

	private void OnPressInteract()
	{
		base.enabled = true;
		Locator.GetPlayerController().SetColliderActivation(active: false);
		Locator.GetPlayerBody().GetComponent<HighSpeedImpactSensor>().enabled = false;
		_endlessObservatoryVolume.SetActivation(active: false);
		_forestOfGalaxiesVolume.SetTriggerActivation(active: false);
		ReticleController.Hide();
		_zoomSpeed = 50f;
		Locator.GetPlayerBody().AddVelocityChange(-Locator.GetPlayerCamera().transform.forward * _zoomSpeed);
		_origEyePos = _eyeTransform.localPosition;
		_audioSource.Play();
		RumbleManager.PlayGalaxyZoom();
		Locator.GetEyeStateManager().SetState(EyeState.ZoomOut);
	}

	private void FixedUpdate()
	{
		if (!_hasWarpedPlayer)
		{
			if (Vector3.Distance(Locator.GetPlayerTransform().position, _interactVolume.transform.position) > 10f)
			{
				WarpPlayerToZoomSequence();
			}
			return;
		}
		Vector3 vector = _playerWarpPoint.position - Locator.GetPlayerTransform().position;
		Locator.GetPlayerBody().SetVelocity(vector * Time.deltaTime);
		if (_zoomOutEye)
		{
			Vector3 localPosition = _eyeTransform.localPosition;
			localPosition.z += _zoomSpeed * Time.deltaTime;
			_eyeTransform.localPosition = localPosition;
			float num = 500f;
			_zoomSpeed += num * Time.deltaTime;
			float num2 = Mathf.InverseLerp(_origEyePos.z + 500f, _origEyePos.z + 20000f, _eyeTransform.localPosition.z);
			num2 = 1f - (num2 - 1f) * (num2 - 1f);
			_eyeTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, num2);
			if (!_spawnStars && Time.time > _startZoomTime + _starParticleDelay)
			{
				_spawnStars = true;
				_spawnStarTime = Time.time;
				for (int i = 0; i < _starParticles.Length; i++)
				{
					_starParticles[i].Play();
				}
			}
			if (num2 >= 1f)
			{
				_zoomOutEye = false;
				Locator.GetPlayerBody().SetVelocity(Locator.GetPlayerTransform().forward * 0.01f);
			}
		}
		if (_spawnStars)
		{
			if (!_exitGalaxy && Time.time > _spawnSupernovaTime)
			{
				_spawnSupernovaTime = Time.time + 1f / _supernovaSpawnRate;
				SpawnSupernova();
			}
			if (!_exitGalaxy && Time.time > _spawnStarTime + _exitGalaxyDelay)
			{
				_miniGalaxy.AppearAfterSeconds(0f);
				for (int j = 0; j < _starParticles.Length; j++)
				{
					_starParticles[j].Stop();
				}
				_exitGalaxy = true;
				_exitGalaxyTime = Time.time;
			}
		}
		for (int k = 0; k < _supernovaCount; k++)
		{
			_supernovas[k].transform.position += base.transform.forward * _supernovaSpeed * Time.deltaTime;
		}
		if (!_exitGalaxy)
		{
			return;
		}
		float num3 = Mathf.InverseLerp(_exitGalaxyTime, _exitGalaxyTime + 6f, Time.time);
		_galaxyTransform.position = Vector3.Lerp(_galaxyStartPos.position, _galaxyEndPos.position, Mathf.SmoothStep(0f, 1f, num3));
		if (!_hasPlayerRegainedControl && num3 >= 1f)
		{
			_hasPlayerRegainedControl = true;
			_miniGalaxy.DieAfterSeconds(1f, playDeathParticles: true, AudioType.EyeBigGalaxyBurn);
			OWInput.ChangeInputMode(InputMode.Character);
			ReticleController.Show();
			Locator.GetPlayerController().SetColliderActivation(active: true);
			Locator.GetPlayerBody().GetComponent<HighSpeedImpactSensor>().enabled = true;
			Locator.GetPlayerBody().SetVelocity(Vector3.zero);
			Locator.GetEyeStateManager().SetState(EyeState.ForestOfGalaxies);
			_forestOfGalaxiesVolume.SetTriggerActivation(active: true);
			for (int l = 0; l < _supernovaCount; l++)
			{
				_supernovas[l].SetActive(value: false);
			}
			_exitGalaxy = false;
			base.enabled = false;
			if (Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe() != null)
			{
				Object.Destroy(Locator.GetToolModeSwapper().GetProbeLauncher().GetActiveProbe()
					.gameObject);
					Debug.Log("PROBE DESTROYED (LEFT BEHIND)");
				}
			}
		}

		private void SpawnSupernova()
		{
			if (_supernovaCount < _maxSupernovaCount)
			{
				Vector3 randomPointInsideShape = _supernovaSpawnShape.GetRandomPointInsideShape();
				_supernovas[_supernovaCount] = Object.Instantiate(_supernovaPrefab, randomPointInsideShape, Random.rotation, base.transform);
				_supernovaCount++;
			}
		}

		private void WarpPlayerToZoomSequence()
		{
			_eyeTransform.gameObject.SetActive(value: true);
			OWRigidbody playerBody = Locator.GetPlayerBody();
			OWRigidbody attachedOWRigidbody = _playerWarpPoint.GetAttachedOWRigidbody();
			playerBody.WarpToPositionRotation(_playerWarpPoint.position, _playerWarpPoint.rotation);
			playerBody.SetVelocity(attachedOWRigidbody.GetVelocity());
			Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().SetDegreesY(0f);
			OWInput.ChangeInputMode(InputMode.None);
			_observatoryVolume.RemoveObjectFromVolume(Locator.GetPlayerDetector());
			_observatoryVolume.RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
			_hasWarpedPlayer = true;
			_zoomOutEye = true;
			_startZoomTime = Time.time;
			_spawnSupernovaTime = Time.time;
		}
	}
