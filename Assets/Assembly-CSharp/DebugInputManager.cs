using System;
using UnityEngine;

public class DebugInputManager : MonoBehaviour
{
	private enum DebugInputMode
	{
		OFF = 0,
		NORMAL = 1,
		SHIP_DAMAGE = 2,
		CONSOLE = 3
	}

	private struct InputSequenceItem
	{
		public IInputCommands inputCommand;

		public bool pressed;
	}

	private RelativeLocationData _relativeData;

	private OWRigidbody _relativeBody;

	private bool _gotoWarpPointNextFrame;

	private bool _hasSetWarpPoint;

	private bool _revealRumorsOnly = true;

	private ShipDamageController _shipDamageController;

	private InputSequenceItem[] _inputModeChangeSequence;

	private DebugInputMode _debugInputMode;

	private int _debugInputModeIndex;

	private SurveyorProbe _probe;

	private ProbeLauncher _probeLauncher;

	private GameObject _solarSystemRoot;

	private TimeLoop _timeLoop;

	private PlayerSpawner _spawner;

	private DreamWorldController _dwController;

	private RingWorldController _rwController;

	private void Awake()
	{
		UnityEngine.Object.Destroy(this);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
	}

	private void Start()
	{
		_probe = Locator.GetProbe();
		GameObject obj = GameObject.FindGameObjectWithTag("Root");
		_timeLoop = obj.GetRequiredComponent<TimeLoop>();
		GameObject obj2 = GameObject.FindGameObjectWithTag("Player");
		_spawner = obj2.GetRequiredComponent<PlayerSpawner>();
		_dwController = Locator.GetDreamWorldController();
		_rwController = Locator.GetRingWorldController();
	}

	private void OnDestroy()
	{
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherEquipped", OnProbeLauncherEquipped);
		GlobalMessenger<ProbeLauncher>.RemoveListener("ProbeLauncherUnequipped", OnProbeLauncherUnequipped);
	}

	private void OnProbeLauncherEquipped(ProbeLauncher pl)
	{
		_probeLauncher = pl;
	}

	private void OnProbeLauncherUnequipped(ProbeLauncher pl)
	{
		_probeLauncher = null;
	}

	private void FixedUpdate()
	{
		if (_gotoWarpPointNextFrame)
		{
			_gotoWarpPointNextFrame = false;
			Locator.GetPlayerBody().MoveToRelativeLocation(_relativeData, _relativeBody);
		}
	}

	private void CheckDebugInputMode()
	{
		if (OWInput.GetNewlyReleasedKeyCode() == KeyCode.None)
		{
			return;
		}
		if (_inputModeChangeSequence[_debugInputModeIndex].inputCommand.IsNewlyReleased())
		{
			_debugInputModeIndex++;
			if (_debugInputModeIndex < _inputModeChangeSequence.Length)
			{
				return;
			}
			int num = (int)(_debugInputMode + 1);
			if (!Enum.IsDefined(typeof(DebugInputMode), num))
			{
				num = 0;
			}
			_debugInputMode = (DebugInputMode)num;
			Debug.Log("New Debug Input Mode: " + _debugInputMode);
			if (_debugInputMode == DebugInputMode.SHIP_DAMAGE)
			{
				if (Locator.GetShipBody() != null)
				{
					_shipDamageController = Locator.GetShipBody().GetComponentInChildren<ShipDamageController>();
				}
				else
				{
					_shipDamageController = null;
				}
			}
			_debugInputModeIndex = 0;
		}
		else
		{
			_debugInputModeIndex = 0;
		}
	}

	private void Update()
	{
		CheckDebugInputMode();
		switch (_debugInputMode)
		{
		case DebugInputMode.SHIP_DAMAGE:
			UpdateShipDamageInputMode();
			break;
		case DebugInputMode.NORMAL:
			UpdateNormalInputMode();
			break;
		case DebugInputMode.CONSOLE:
			UpdateConsoleInputMode();
			break;
		}
	}

	private void UpdateNormalInputMode()
	{
		if (_spawner != null && DebugKeyCode.GetKey(DebugKeyCode.warpAllowed))
		{
			if (DebugKeyCode.GetKeyDown(DebugKeyCode.cometWarp))
			{
				if (DebugKeyCode.GetKey(DebugKeyCode.altWarp))
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.SunStation));
				}
				_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.Comet));
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.hourglassTwinsWarp))
			{
				if (DebugKeyCode.GetKey(DebugKeyCode.altWarp))
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.HourglassTwin_2));
				}
				else
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.HourglassTwin_1));
				}
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.homePlanetWarp))
			{
				if (DebugKeyCode.GetKey(DebugKeyCode.altWarp))
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.TimberHearth_Alt));
				}
				else
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.TimberHearth));
				}
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.brittleHollowWarp))
			{
				_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.BrittleHollow));
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.gasGiantWarp))
			{
				_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.GasGiant));
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.darkBrambleWarp))
			{
				_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.DarkBramble));
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.shipWarp))
			{
				_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.Ship));
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.quantumWarp))
			{
				_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.QuantumMoon));
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.IPWarp))
			{
				if (DebugKeyCode.GetKey(DebugKeyCode.altWarp))
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.InvisiblePlanet_Alt));
				}
				else
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.InvisiblePlanet));
				}
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.moonWarp))
			{
				if (DebugKeyCode.GetKey(DebugKeyCode.altWarp))
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.SignalDish));
				}
				else
				{
					_spawner.DebugWarp(_spawner.GetSpawnPoint(SpawnLocation.LunarLookout));
				}
			}
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.setWarpPoint) && Locator.GetPlayerSectorDetector().GetLastEnteredSector() != null)
		{
			_hasSetWarpPoint = true;
			_relativeBody = Locator.GetPlayerSectorDetector().GetLastEnteredSector().GetOWRigidbody();
			_relativeData = new RelativeLocationData(Locator.GetPlayerBody(), _relativeBody);
		}
		if (_hasSetWarpPoint && DebugKeyCode.GetKeyDown(DebugKeyCode.gotoWarpPoint))
		{
			_gotoWarpPointNextFrame = true;
		}
		if (DebugKeyCode.GetKey(KeyCode.E) && DebugKeyCode.GetKey(KeyCode.U))
		{
			GlobalMessenger.FireEvent("DebugWarpVessel");
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.revealAllShipLogFacts))
		{
			Locator.GetShipLogManager().RevealAllFacts(_revealRumorsOnly);
			_revealRumorsOnly = false;
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.destroyShip))
		{
			UnityEngine.Object.Destroy(Locator.GetShipTransform().gameObject);
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.learnForgetFrequencies))
		{
			if (PlayerData.KnowsMultipleFrequencies())
			{
				PlayerData.ForgetFrequency(SignalFrequency.Quantum);
				PlayerData.ForgetFrequency(SignalFrequency.EscapePod);
				PlayerData.ForgetFrequency(SignalFrequency.HideAndSeek);
				PlayerData.ForgetFrequency(SignalFrequency.Radio);
			}
			else
			{
				PlayerData.LearnFrequency(SignalFrequency.Quantum);
				PlayerData.LearnFrequency(SignalFrequency.EscapePod);
				PlayerData.LearnFrequency(SignalFrequency.HideAndSeek);
				PlayerData.LearnFrequency(SignalFrequency.Radio);
			}
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.triggerSupernova))
		{
			GlobalMessenger.FireEvent("TriggerSupernova");
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.destroyTimeline))
		{
			Debug.Log("Try DestroyTimeline (Requires NomaiExperimentBlackHole)");
			GlobalMessenger.FireEvent("DebugTimelineDestroyed");
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.suitUp))
		{
			if (!Locator.GetPlayerSuit().IsWearingSuit())
			{
				Locator.GetPlayerSuit().SuitUp();
			}
			else
			{
				Locator.GetPlayerSuit().RemoveSuit();
			}
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.refillResources) || (InputLibrary.rollMode.IsPressed() && InputLibrary.toolOptionDown.IsNewlyPressed()))
		{
			Locator.GetPlayerTransform().GetComponent<PlayerResources>().DebugRefillResources();
			if ((bool)Locator.GetShipTransform())
			{
				ShipComponent[] componentsInChildren = Locator.GetShipTransform().GetComponentsInChildren<ShipComponent>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].SetDamaged(damaged: false);
				}
			}
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.powerOverwhelming))
		{
			Locator.GetPlayerTransform().GetComponent<PlayerResources>().ToggleInvincibility();
			Locator.GetDeathManager().ToggleInvincibility();
			Transform shipTransform = Locator.GetShipTransform();
			if ((bool)shipTransform)
			{
				shipTransform.GetComponentInChildren<ShipDamageController>().ToggleInvincibility();
			}
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.putOnHelmet))
		{
			Locator.GetPlayerSuit().PutOnHelmet();
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.uiTestAndSuicide))
		{
			if (PlayerState.AtFlightConsole())
			{
				Locator.GetShipTransform().GetComponent<ShipResources>().SetDebugKillResources(killResources: true);
			}
			else
			{
				Locator.GetPlayerTransform().GetComponent<PlayerResources>().SetDebugKillResources(killResources: true);
			}
		}
		if (DebugKeyCode.GetKeyUp(DebugKeyCode.uiTestAndSuicide))
		{
			if (PlayerState.AtFlightConsole())
			{
				Locator.GetShipTransform().GetComponent<ShipResources>().SetDebugKillResources(killResources: false);
			}
			else
			{
				Locator.GetPlayerTransform().GetComponent<PlayerResources>().SetDebugKillResources(killResources: false);
			}
		}
		if ((DebugKeyCode.GetKeyDown(DebugKeyCode.learnLaunchCodes) || (InputLibrary.rollMode.IsPressed() && InputLibrary.toolOptionDown.IsNewlyPressed())) && !PlayerData.KnowsLaunchCodes())
		{
			PlayerData.LearnLaunchCodes();
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.timeLapse))
		{
			Time.timeScale = 10f;
		}
		else if (DebugKeyCode.GetKeyUp(DebugKeyCode.timeLapse))
		{
			Time.timeScale = 1f;
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.breakAllFragments))
		{
			FragmentIntegrity[] array = UnityEngine.Object.FindObjectsOfType<FragmentIntegrity>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].AddDamage(10000f);
			}
		}
		if (DebugKeyCode.GetKeyDown(DebugKeyCode.cycleUiSize))
		{
			UITextSize uiSizeSetting = UITextSize.AUTO;
			switch (PlayerData.GetTextSize())
			{
			case UITextSize.AUTO:
				uiSizeSetting = UITextSize.SMALL;
				break;
			case UITextSize.SMALL:
				uiSizeSetting = UITextSize.LARGE;
				break;
			case UITextSize.LARGE:
				uiSizeSetting = UITextSize.AUTO;
				break;
			}
			Debug.Log("[Debug]Set New UI Size Setting: " + uiSizeSetting);
			PlayerData.SetUiSizeSetting(uiSizeSetting);
			Locator.GetUISizeManager().OnUiSizeSettingChanged();
		}
	}

	private void UpdateConsoleInputMode()
	{
		if (InputLibrary.rollMode.IsPressed() && InputLibrary.toolActionPrimary.IsPressed())
		{
			InputLibrary.toolOptionDown.IsNewlyPressed();
		}
	}

	private void UpdateShipDamageInputMode()
	{
		if (_shipDamageController == null)
		{
			return;
		}
		if (DebugKeyCode.GetKey(DebugKeyCode.damageModifierHull))
		{
			if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageLanding))
			{
				_shipDamageController.DebugDamageShip(UITextType.ShipPartLanding);
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageTop))
			{
				_shipDamageController.DebugDamageShip(UITextType.ShipPartTop);
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damagePort))
			{
				_shipDamageController.DebugDamageShip(UITextType.ShipPartPort);
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageStarboard))
			{
				_shipDamageController.DebugDamageShip(UITextType.ShipPartStarboard);
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageForward))
			{
				_shipDamageController.DebugDamageShip(UITextType.ShipPartForward);
			}
			else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageAft))
			{
				_shipDamageController.DebugDamageShip(UITextType.ShipPartAft);
			}
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageAutopilot))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartAutopilot);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageLights))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartLights);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageCamera))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartCamera);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageFuel))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartFuel);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageOxygen))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartO2);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageGravity))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartGravity);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageElectric))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartElectric);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageLeftThrust))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartLeftThrust);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageRightThrust))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartRightThrust);
		}
		else if (DebugKeyCode.GetKeyDown(DebugKeyCode.damageReactor))
		{
			_shipDamageController.DebugDamageShip(UITextType.ShipPartReactor);
		}
	}
}
