using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneIntegrator : MonoBehaviour
{
	[SerializeField]
	private bool _enableCenterOfTheUniverse = true;

	[SerializeField]
	private Vector3 _startPosition;

	[SerializeField]
	private Vector3 _startRotation;

	[SerializeField]
	private AstroObject _primaryAstroObject;

	[SerializeField]
	private InitialMotion _rootInitialMotion;

	[SerializeField]
	private ConstantForceDetector _rootForceDetector;

	[SerializeField]
	private bool _spawnPlayer;

	[SerializeField]
	private bool _spawnShip;

	[SerializeField]
	private bool _useDebugLoopCount;

	[SerializeField]
	private int _debugLoopCount = 2;

	[SerializeField]
	private int _debugStartAtMinute;

	[SerializeField]
	private bool _spawnDLCPhotographer;

	[SerializeField]
	private SpawnPoint _defaultPlayerSpawnPoint;

	[SerializeField]
	private SpawnPoint _defaultShipSpawnPoint;

	private GameObject _solarSystemRoot;

	private GameObject _sun;

	private GameObject _player;

	private GameObject _playerShip;

	public void SetDefaultSpawnPoint(SpawnPoint spawn, bool isShipSpawn)
	{
		if (isShipSpawn)
		{
			_defaultShipSpawnPoint = spawn;
		}
		else
		{
			_defaultPlayerSpawnPoint = spawn;
		}
	}

	private void OnValidate()
	{
		if (_defaultPlayerSpawnPoint == _defaultShipSpawnPoint)
		{
			_defaultPlayerSpawnPoint = null;
		}
	}

	private void Awake()
	{
		if (SceneManager.GetActiveScene().name.Contains("SolarSystem"))
		{
			InitGlobalSceneObjects();
			return;
		}
		if (Application.isEditor && _useDebugLoopCount)
		{
			if (_debugLoopCount > 1)
			{
				PlayerData.LearnLaunchCodes();
			}
			PlayerData.SaveLoopCount(_debugLoopCount);
		}
		SpawnPlanetSceneObjects();
	}

	private void Start()
	{
		if (SceneManager.GetActiveScene().name.Contains("SolarSystem"))
		{
			SyncGlobalSceneObjects();
			Object.Destroy(base.gameObject);
		}
		else
		{
			if (Application.isEditor && _debugStartAtMinute > 0)
			{
				TimeLoop.SetSecondsRemaining(TimeLoop.GetSecondsRemaining() - (float)(_debugStartAtMinute * 60));
			}
			SyncPlanetSceneObjects();
		}
		if (!Physics.autoSyncTransforms)
		{
			Physics.SyncTransforms();
		}
	}

	private void LateUpdate()
	{
		if (!SceneManager.GetActiveScene().name.Contains("SolarSystem") && (bool)_primaryAstroObject && _primaryAstroObject.GetCustomName() == "FocalBody" && _spawnPlayer)
		{
			_ = (bool)_player;
		}
		Object.Destroy(base.gameObject);
	}

	private void SpawnPlanetSceneObjects()
	{
		if ((bool)_primaryAstroObject && ((_primaryAstroObject.GetAstroObjectName() == AstroObject.Name.WhiteHole && SceneManager.GetActiveScene().name.Contains("BrittleHollow")) || (_primaryAstroObject.GetAstroObjectName() == AstroObject.Name.BrittleHollow && SceneManager.GetActiveScene().name.Contains("WhiteHole")) || (_primaryAstroObject.GetAstroObjectName() == AstroObject.Name.DreamWorld && SceneManager.GetActiveScene().name.Contains("RingWorld")) || (_primaryAstroObject.GetAstroObjectName() == AstroObject.Name.RingWorld && SceneManager.GetActiveScene().name.Contains("DreamWorld"))))
		{
			_solarSystemRoot = GameObject.FindGameObjectWithTag("Root");
			base.transform.position = _startPosition;
			base.transform.eulerAngles = _startRotation;
			while (base.transform.childCount > 0)
			{
				base.transform.GetChild(0).SetParent(_solarSystemRoot.transform, worldPositionStays: true);
			}
			_sun = GameObject.FindGameObjectWithTag("Sun");
			_spawnPlayer = false;
			_spawnShip = false;
			return;
		}
		_solarSystemRoot = new GameObject("SolarSystemRoot");
		_solarSystemRoot.tag = "Root";
		_solarSystemRoot.AddComponent<CenterOfTheUniverse>().enabled = _enableCenterOfTheUniverse;
		_solarSystemRoot.AddComponent<TimeLoop>();
		_solarSystemRoot.AddComponent<SectorManager>();
		base.transform.position = _startPosition;
		base.transform.eulerAngles = _startRotation;
		_sun = SpawnPrefab("environments/Sun/Sun_Body");
		if (_spawnPlayer)
		{
			if (_spawnDLCPhotographer)
			{
				_player = SpawnPrefab("Player/DLCPhotographer_Body");
			}
			else
			{
				_player = SpawnPrefab("Player/Player_Body");
			}
			SpawnPrefab("Player/Probe_Body");
		}
		SpawnPrefab("Effects/FlashbackCamera");
		SpawnPrefab("Player/MapCamera");
		if (_spawnShip)
		{
			_playerShip = SpawnPrefab("Ship/Ship_Body");
			if ((bool)_playerShip)
			{
				_playerShip.transform.position = _defaultShipSpawnPoint.transform.position;
				_playerShip.transform.rotation = _defaultShipSpawnPoint.transform.rotation;
			}
		}
		SpawnPrefab("Skybox");
		SpawnUI();
		SpawnPrefab("GlobalManagers");
		SpawnPrefab("DistantProxyManager");
		if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.BrittleHollow)
		{
			SceneManager.LoadScene("WhiteHole", LoadSceneMode.Additive);
		}
		if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.WhiteHole)
		{
			SceneManager.LoadScene("BrittleHollow", LoadSceneMode.Additive);
		}
		if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.RingWorld)
		{
			SceneManager.LoadScene("DreamWorld", LoadSceneMode.Additive);
		}
		if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.DreamWorld)
		{
			SceneManager.LoadScene("RingWorld", LoadSceneMode.Additive);
		}
	}

	private void SpawnUI()
	{
		SpawnPrefab("UI/PauseBackdropCanvas");
		SpawnPrefab("UI/DialogueCanvas");
		SpawnPrefab("UI/PauseMenu");
		SpawnPrefab("UI/ScreenPromptCanvas");
		SpawnPrefab("UI/Reticule");
		SpawnPrefab("UI/CanvasMarkerManager");
	}

	private void SyncPlanetSceneObjects()
	{
		if ((bool)_sun)
		{
			_solarSystemRoot.GetComponent<CenterOfTheUniverse>().SetStaticReferenceFrame(_sun.GetAttachedOWRigidbody());
			if ((bool)_primaryAstroObject)
			{
				_primaryAstroObject.SetPrimaryBody(_sun.GetComponent<AstroObject>());
			}
			if ((bool)_rootInitialMotion)
			{
				_rootInitialMotion.SetPrimaryBody(_sun.GetAttachedOWRigidbody());
			}
			if ((bool)_rootForceDetector)
			{
				_rootForceDetector.AddConstantVolume(_sun.GetComponent<AstroObject>().GetGravityVolume());
			}
			Locator.RegisterAstroObject(_sun.GetComponent<AstroObject>());
			if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.Comet)
			{
				_primaryAstroObject.GetRequiredComponent<AlignWithTargetBody>().SetTargetBody(_sun.GetAttachedOWRigidbody());
			}
		}
		if (_spawnPlayer && (bool)_player)
		{
			_player.GetRequiredComponent<PlayerSpawner>().SetInitialSpawnPoint(_defaultPlayerSpawnPoint);
			AstroObject.Name name = (_primaryAstroObject ? _primaryAstroObject.GetAstroObjectName() : AstroObject.Name.None);
			if (name == AstroObject.Name.QuantumMoon || name == AstroObject.Name.WhiteHole)
			{
				_player.GetRequiredComponent<MatchInitialMotion>().enabled = false;
			}
		}
		if (_spawnShip && (bool)_playerShip)
		{
			_playerShip.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(_defaultShipSpawnPoint.GetAttachedOWRigidbody());
		}
	}

	private GameObject SpawnPrefab(string path)
	{
		return null;
	}

	private void InitGlobalSceneObjects()
	{
		SceneLoader sceneLoader = Object.FindObjectOfType<SceneLoader>();
		InitPositionRotation(sceneLoader);
		if ((bool)_primaryAstroObject)
		{
			_primaryAstroObject.SetPrimaryBody(sceneLoader.GetSun().GetComponent<AstroObject>());
		}
		if ((bool)_rootInitialMotion)
		{
			_rootInitialMotion.SetPrimaryBody(sceneLoader.GetSun().GetAttachedOWRigidbody());
		}
		if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.Comet)
		{
			_primaryAstroObject.GetRequiredComponent<AlignWithTargetBody>().SetTargetBody(sceneLoader.GetSun().GetAttachedOWRigidbody());
		}
	}

	private void InitPositionRotation(SceneLoader sceneLoader)
	{
		if (_primaryAstroObject != null && _primaryAstroObject.GetAstroObjectName() != AstroObject.Name.Comet && _primaryAstroObject.GetAstroObjectName() != AstroObject.Name.WhiteHole)
		{
			float num = 350f;
			switch (_primaryAstroObject.GetAstroObjectName())
			{
			case AstroObject.Name.HourglassTwins:
				num += 20f;
				break;
			case AstroObject.Name.BrittleHollow:
				num += 20f;
				break;
			}
			Quaternion quaternion = Quaternion.AngleAxis(num, Vector3.up);
			_startPosition = quaternion * _startPosition;
			_startRotation += Vector3.up * num;
		}
		base.transform.position = _startPosition;
		base.transform.eulerAngles = _startRotation;
		while (base.transform.childCount > 0)
		{
			base.transform.GetChild(0).SetParent(sceneLoader.GetSolarSystemRoot(), worldPositionStays: true);
		}
	}

	private void SyncGlobalSceneObjects()
	{
		SceneLoader sceneLoader = Object.FindObjectOfType<SceneLoader>();
		if ((bool)_rootForceDetector)
		{
			_rootForceDetector.AddConstantVolume(sceneLoader.GetSun().GetComponent<AstroObject>().GetGravityVolume());
		}
	}

	public void InitBuildSceneObjects()
	{
		SceneLoader sceneLoader = Object.FindObjectOfType<SceneLoader>();
		InitPositionRotation(sceneLoader);
		if ((bool)_primaryAstroObject)
		{
			_primaryAstroObject.SetPrimaryBody(sceneLoader.GetSun().GetComponent<AstroObject>());
		}
		if ((bool)_rootInitialMotion)
		{
			_rootInitialMotion.SetPrimaryBody(sceneLoader.GetSun().GetAttachedOWRigidbody());
		}
		if ((bool)_rootForceDetector)
		{
			_rootForceDetector.AddConstantVolumeInEditor(sceneLoader.GetSun().GetComponent<AstroObject>().GetGravityVolume());
		}
		if ((bool)_primaryAstroObject && _primaryAstroObject.GetAstroObjectName() == AstroObject.Name.Comet)
		{
			_primaryAstroObject.GetRequiredComponent<AlignWithTargetBody>().SetTargetBody(sceneLoader.GetSun().GetAttachedOWRigidbody());
		}
	}
}
