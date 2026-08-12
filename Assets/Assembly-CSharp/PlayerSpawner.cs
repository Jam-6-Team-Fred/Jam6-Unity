using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
	private SpawnPoint _initialSpawnPoint;

	private SpawnPoint[] _spawnList;

	private OWRigidbody _playerBody;

	private PlayerCameraController _cameraController;

	private bool _hasMovedCamera;

	private bool _lookPromptAdded;

	private ScreenPrompt _lookPrompt;

	private ScreenPrompt _movePrompt;

	private SpawnPoint _debugWarpPoint;

	private bool _debugWarpNextUpdate;

	private bool _firstFrame = true;

	private bool _finishSpawnNextUpdate = true;

	private static Vector3 _localResetPos;

	private static Quaternion _localResetRotation;

	private static float _resetCamaDegreesY;

	private void Awake()
	{
		_playerBody = base.gameObject.GetTaggedComponent<OWRigidbody>("Player");
		_cameraController = base.gameObject.GetTaggedComponent<PlayerCameraController>("MainCamera");
		_lookPrompt = new ScreenPrompt(InputLibrary.look, UITextLibrary.GetString(UITextType.LookPrompt));
		_movePrompt = new ScreenPrompt(InputLibrary.moveXZ, UITextLibrary.GetString(UITextType.MovePrompt));
		GlobalMessenger.AddListener("ResetSimulation", OnResetSimulation);
		GlobalMessenger.AddListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger<int>.AddListener("StartOfTimeLoop", OnStartOfTimeLoop);
	}

	private void OnDestroy()
	{
		GlobalMessenger.RemoveListener("ResetSimulation", OnResetSimulation);
		GlobalMessenger.RemoveListener("ResumeSimulation", OnResumeSimulation);
		GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", OnStartOfTimeLoop);
	}

	private void OnStartOfTimeLoop(int loopCount)
	{
		FindPlanetSpawns();
		SpawnPlayer();
	}

	private void OnResetSimulation()
	{
		Transform obj = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
		_localResetPos = obj.InverseTransformPoint(_playerBody.transform.position);
		_localResetRotation = Quaternion.Inverse(obj.rotation) * _playerBody.transform.rotation;
		_resetCamaDegreesY = _cameraController.GetDegreesY();
	}

	private void OnResumeSimulation()
	{
		OWRigidbody attachedOWRigidbody = Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetAttachedOWRigidbody();
		_playerBody.transform.position = attachedOWRigidbody.transform.TransformPoint(_localResetPos);
		_playerBody.transform.rotation = attachedOWRigidbody.transform.rotation * _localResetRotation;
		_cameraController.SetDegreesY(_resetCamaDegreesY);
		_playerBody.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(attachedOWRigidbody);
		FindPlanetSpawns();
	}

	private void FindPlanetSpawns()
	{
		_spawnList = Object.FindObjectsOfType(typeof(SpawnPoint)) as SpawnPoint[];
	}

	public SpawnPoint GetInitialSpawnPoint()
	{
		return _initialSpawnPoint;
	}

	public void SetInitialSpawnPoint(SpawnPoint spawnPoint)
	{
		_initialSpawnPoint = spawnPoint;
	}

	public SpawnPoint GetSpawnPoint(SpawnLocation location)
	{
		for (int i = 0; i < _spawnList.Length; i++)
		{
			SpawnPoint spawnPoint = _spawnList[i];
			if (spawnPoint.GetSpawnLocation() == location && spawnPoint.IsShipSpawn() == PlayerState.IsInsideShip())
			{
				return spawnPoint;
			}
		}
		return null;
	}

	public void DebugWarp(SpawnPoint spawnPoint)
	{
		if (spawnPoint != null)
		{
			_debugWarpPoint = spawnPoint;
			_debugWarpNextUpdate = true;
		}
	}

	private void SpawnPlayer()
	{
		if (PlayerData.GetWarpedToTheEye())
		{
			Debug.Log("Abort player spawn. Vessel will handle it.");
			return;
		}
		if (_initialSpawnPoint == null)
		{
			if (LoadManager.GetCurrentScene() == OWScene.SolarSystem || SceneManager.GetActiveScene().name.Contains("TimberHearth") || SceneManager.GetActiveScene().name.Contains("TH_BeautifulCorner") || SceneManager.GetActiveScene().name.Contains("Test"))
			{
				_initialSpawnPoint = GetSpawnPoint(SpawnLocation.TimberHearth);
			}
			if (!_initialSpawnPoint)
			{
				for (int i = 0; i < _spawnList.Length; i++)
				{
					if (_spawnList[i].GetSpawnLocation() != SpawnLocation.Ship && !_spawnList[i].IsShipSpawn())
					{
						_initialSpawnPoint = _spawnList[i];
						break;
					}
				}
			}
		}
		if (_initialSpawnPoint != null)
		{
			if (LoadManager.GetCurrentScene() == OWScene.SolarSystem)
			{
				_cameraController.SetDegreesY(80f);
			}
			OWRigidbody attachedOWRigidbody = _initialSpawnPoint.gameObject.GetAttachedOWRigidbody();
			_playerBody.transform.position = _initialSpawnPoint.transform.position;
			_playerBody.transform.rotation = _initialSpawnPoint.transform.rotation;
			_playerBody.GetRequiredComponent<MatchInitialMotion>().SetBodyToMatch(attachedOWRigidbody);
			if (!Physics.autoSyncTransforms)
			{
				Physics.SyncTransforms();
			}
			_finishSpawnNextUpdate = true;
			MonoBehaviour.print("SPAWN PLAYER");
		}
		else
		{
			Debug.LogError("Spawn point is null!");
		}
	}

	private void FixedUpdate()
	{
		if (_firstFrame)
		{
			_firstFrame = false;
		}
		else if (_finishSpawnNextUpdate && _initialSpawnPoint != null)
		{
			_finishSpawnNextUpdate = false;
			_initialSpawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
			_initialSpawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject);
			_initialSpawnPoint.OnSpawnPlayer();
		}
		if (_debugWarpNextUpdate)
		{
			_debugWarpNextUpdate = false;
			OWRigidbody obj = (PlayerState.IsInsideShip() ? Locator.GetShipBody() : _playerBody);
			obj.WarpToPositionRotation(_debugWarpPoint.transform.position, _debugWarpPoint.transform.rotation);
			obj.SetVelocity(_debugWarpPoint.GetPointVelocity());
			if (obj == _playerBody)
			{
				_debugWarpPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
				_debugWarpPoint.AddObjectToTriggerVolumes(Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject);
				_debugWarpPoint.OnSpawnPlayer();
				MonoBehaviour.print("DEBUG PLAYER WARP");
			}
		}
	}

	private void Update()
	{
		if (_hasMovedCamera)
		{
			return;
		}
		if (!_lookPromptAdded && Time.timeSinceLevelLoad > 10f)
		{
			Locator.GetPromptManager().AddScreenPrompt(_lookPrompt, PromptPosition.UpperRight, makeVisible: true);
			Locator.GetPromptManager().AddScreenPrompt(_movePrompt, PromptPosition.UpperRight, makeVisible: true);
			_lookPromptAdded = true;
		}
		if (_cameraController.GetDegreesY() < 45f)
		{
			_hasMovedCamera = true;
			if (_lookPromptAdded)
			{
				Locator.GetPromptManager().RemoveScreenPrompt(_lookPrompt);
				Locator.GetPromptManager().RemoveScreenPrompt(_movePrompt);
			}
		}
	}
}
